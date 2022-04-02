using DataService;
using Mv.Modules.RD402.Hive.ViewModels.Messages;
using Prism.Events;
using Prism.Logging;
using PropertyTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Reactive.Linq;
using MV.Core.Events;
using Mv.Core.Interfaces;
using Newtonsoft.Json;
using CsvHelper;
using CsvHelper.Configuration.Attributes;
using System.IO;
using System.Globalization;
using System.Threading;

namespace Mv.Modules.RD402.Hive.Services
{
    public interface IAlarmManager
    { }
    public class AlarmManager : IAlarmManager, IDisposable
    {
        private readonly IDataServer server;
        private readonly IEventAggregator @event;
        private readonly ILoggerFacade logger;
        private readonly string _sw_version = "V1.0.9";
        private string message = "";
        private string code = "";
        private string severity = "";
        private string occurrence_time = "";
        private Edge[] edges = new Edge[7] { new Edge(), new Edge(), new Edge(), new Edge(), new Edge(), new Edge(), new Edge() };
        public AlarmManager(IDataServer server, IEventAggregator @event, ILoggerFacade logger)
        {
            this.server = server;
            this.@event = @event;
            this.logger = logger;           
            var alarmGroup = server.GetGroupByName("alarms");//PLC报警
            if (alarmGroup != null)
            {
                foreach (var item in alarmGroup.Items)
                {
                    item.ValueChanged += Item_ValueChanged;
                }
            }
            var PARASGroup = server.GetGroupByName("PARAS");//PLC数据
            if (PARASGroup != null)
            {
                foreach (var item in PARASGroup.Items)
                {
                    item.ValueChanged += PARA_ValueChanged;
                }
            }
            #region 心跳线程
            Task.Factory.StartNew(() => {
                while (true)
                {
                    var TAG = server["HANDSHAKE"];
                    TAG?.Write((short)1);//加？表示可为空值
                    Thread.Sleep(1000);
                    TAG?.Write((short)0);//加？表示可为空值
                    Thread.Sleep(1000);
                }
            }, TaskCreationOptions.LongRunning);
            #endregion
            #region 设备状态上传
            Task.Factory.StartNew(() => {
                var TAG = server["State"];
                while (true)
                {
                    if (TAG != null)
                    {
                        edges[4].CurrentValue = TAG.ToString();
                        if (edges[4].ValueChanged && edges[4].CurrentValue != "0")
                        {
                            var nowtime = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.ffzz00");
                            MachineState rt = new MachineState();
                            MachineState.Data dt = new MachineState.Data();
                            #region 状态切换成uDT5后遍寻所有报警信息,只能从状态Runing1切换成uDT5,报警或按下停止按钮
                            if (edges[4].CurrentValue == "5")
                            {
                                Thread.Sleep(1000);
                                occurrence_time = nowtime;
                                code = "O99EECE-01-03";
                                message = "The stop button is pressed";
                                if (alarmGroup != null)
                                {
                                    foreach (var item in alarmGroup.Items)
                                    {
                                        if (item.Value.Boolean == true)
                                        {
                                            message = item.GetMetaData().Description;
                                            code = item.GetMetaData().Code;
                                            severity = item.GetMetaData().Severity.ToString();
                                        }
                                    }
                                }
                                dt.previous_state = "1";
                                dt.code = code;
                                dt.error_message = message;
                                if (message == "The stop button is pressed")
                                    dt.state_change_reason = "User pressed stop button";
                                else
                                    dt.state_change_reason = "Machine alarm with description";
                                if (edges[4].OldValue == "0")
                                    dt.state_change_reason = "User open hive software";
                            }
                            #endregion

                            #region 状态切换成Runing1,只能从状态IDLE2切换成Runing1，按下开始按钮
                            if (edges[4].CurrentValue == "1")
                            {
                                dt.previous_state = "2";
                                dt.sw_version = _sw_version;
                                if (edges[4].OldValue == "0")
                                    dt.state_change_reason = "User open hive software";
                                else
                                    dt.state_change_reason = "User pressed start button";
                            }
                            #endregion

                            #region 状态切换成Engineering3,只能从IDLE2切入，按下工程师按钮
                            if (edges[4].CurrentValue == "3")
                            {
                                dt.previous_state = "2";
                                if (edges[4].OldValue == "0")
                                    dt.state_change_reason = "User open hive software";
                                else
                                    dt.state_change_reason = "User pressed Engineering button";
                            }
                            #endregion

                            #region 状态切换成pDT4,只能从状态IDLE2切入，按下维修按钮
                            if (edges[4].CurrentValue == "4")
                            {
                                dt.previous_state = "2";
                                if (edges[4].OldValue == "0")
                                    dt.state_change_reason = "User open hive software";
                                else
                                    dt.state_change_reason = "User pressed Maintenance button";
                            }
                            #endregion

                            #region 状态切换成IDLE2,其他任意状态都可以切入，查询上次状态，如果是上次是pDT4状态(遍寻修改的维修内容)
                            if (edges[4].CurrentValue == "2")
                            {
                                if (edges[4].OldValue == "1")
                                {
                                    dt.previous_state = "1";
                                    dt.state_change_reason = "User pressed pause button";
                                }
                                else if (edges[4].OldValue == "3")
                                {
                                    dt.previous_state = "3";
                                    dt.state_change_reason = "User pressed Exit button";
                                }
                                else if (edges[4].OldValue == "4")
                                {
                                    dt.previous_state = "4";
                                    dt.state_change_reason = "Exit Maintenance";
                                    #region 遍寻维修模式下修改的内容
                                    int i = 0;
                                    if (PARASGroup != null)
                                    {
                                        foreach (var item in PARASGroup.Items)
                                        {
                                            if (item.Value.Boolean == true && item.GetMetaData().Code == "PM")
                                            {
                                                dt.state_change_reason += ",";
                                                dt.state_change_reason += item.GetMetaData().Description;
                                                i++;
                                            }
                                        }
                                    }
                                    if (i == 0)
                                    {
                                        dt.state_change_reason = "Exit Maintenance,other";
                                    }
                                    #endregion
                                }
                                else if (edges[4].OldValue == "5")
                                {
                                    dt.previous_state = "5";
                                    dt.state_change_reason = "User pressed reset button";
                                    #region 产生报警信息
                                    ErrorData machineerror = new ErrorData();
                                    machineerror.message = "The stop button is pressed";
                                    machineerror.code = "O99EECE-01-03";
                                    machineerror.severity = "critical";
                                    machineerror.occurrence_time = occurrence_time;
                                    machineerror.resolved_time = nowtime;
                                    if (!string.IsNullOrEmpty(message) && !string.IsNullOrEmpty(code) && !string.IsNullOrEmpty(severity))
                                    {
                                        machineerror.message = message;
                                        machineerror.code = code;
                                        machineerror.severity = severity;
                                    }//无数据丢失，则上传轮询到的报警
                                    if (string.IsNullOrEmpty(occurrence_time))
                                    {
                                        machineerror.occurrence_time = DateTime.Now.AddSeconds(-10).ToString("yyyy-MM-ddTHH:mm:ss.ffzz00");
                                    }
                                    else
                                    {
                                        try
                                        {
                                            var a = (DateTime.Parse(nowtime) - DateTime.Parse(occurrence_time)).TotalSeconds;
                                            if (a <= 0)
                                                machineerror.occurrence_time = DateTime.Now.AddSeconds(-10).ToString("yyyy-MM-ddTHH:mm:ss.ffzz00");
                                        }
                                        catch
                                        {
                                            machineerror.occurrence_time = DateTime.Now.AddSeconds(-10).ToString("yyyy-MM-ddTHH:mm:ss.ffzz00");
                                        }
                                    }
                                    message = "";
                                    code = "";
                                    severity = "";
                                    occurrence_time = "";
                                    ErrorData.Data error_dt = new ErrorData.Data();
                                    machineerror.data = error_dt;
                                    var error_jsonSetting = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
                                    var error_json = JsonConvert.SerializeObject(machineerror, Formatting.Indented, error_jsonSetting);
                                    JsonHelper.WriteJsonFile($"./Hive/Json/ErrorData/{DateTime.Now:yyyyMMddhhmmssFFFFF}+{code}.json", error_json);
                                    #endregion
                                }
                                else
                                {
                                    dt.previous_state = "5";
                                    dt.state_change_reason = "User open hive software";
                                }
                            }
                            #endregion
                            rt.machine_state = edges[4].CurrentValue;
                            rt.state_change_time = nowtime;
                            rt.data = dt;
                            var jsonSetting = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
                            var json = JsonConvert.SerializeObject(rt, Formatting.Indented, jsonSetting);
                            JsonHelper.WriteJsonFile($"./Hive/Json/StateData/{DateTime.Now:yyyyMMddhhmmss.fffffff}.json", json);

                        }
                    }
                    Thread.Sleep(100);
                }
            }, TaskCreationOptions.LongRunning);
            #endregion
        }

        private void PARA_ValueChanged(object sender, DataService.ValueChangedEventArgs e)
        {
            if (sender is ITag tag)
            {
                var value = tag.GetValue(e.Value).ToString();
                var oldvalue = tag.GetValue(e.OldValue).ToString();
                var timeStamp = tag.TimeStamp.ToString("yyyy-MM-ddTHH:mm:ss.ffzz00");
                var meta = tag.GetMetaData();
                var msg = $"{meta.Name}\t{meta.Address}\t{meta.Description}\t{"From_" + oldvalue + "_to_" + value}";
                this.@event.GetEvent<UserMessageEvent>().Publish(new UserMessage { Content = msg, Level = 0, Source = "PARAS" });                
            }

        }
        private void Item_ValueChanged(object sender, DataService.ValueChangedEventArgs e)
        {
            if (sender is BoolTag tag)
            {
                var value = (bool)tag.GetValue(e.Value) ? "From_OFF_to_ON" : "From_ON_to_OFF";
                var message = tag.GetMetaData().Description;
                var code = tag.GetMetaData().Code;
                var severity = tag.GetMetaData().Severity.ToString();
                var timeStamp = tag.TimeStamp.ToString("yyyy-MM-ddTHH:mm:ss.ffzz00");
                var oldtimestamp = tag.OldTimeStamp.ToString("yyyy-MM-ddTHH:mm:ss.ffzz00");
                var meta = tag.GetMetaData();
                var msg = $"{meta.Name}\t{meta.Address}\t{meta.Description}\t{value}";
                this.@event.GetEvent<UserMessageEvent>().Publish(new UserMessage { Content = msg, Level = 0, Source = "alarms" });               
                var value1 = (bool)tag.GetValue(e.Value);
                var minimum = tag.GetMetaData().Minimum;
                if (!value1)
                {
                   if(minimum>0.0)
                   { 
                    ErrorData rt = new ErrorData();
                    rt.message = message;
                    rt.code = code;
                    rt.severity = severity;
                    rt.occurrence_time = oldtimestamp;
                    rt.resolved_time = timeStamp;
                    ErrorData.Data dt = new ErrorData.Data();
                    rt.data = dt;
                    var jsonSetting = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
                    var json = JsonConvert.SerializeObject(rt, Formatting.Indented, jsonSetting);
                    JsonHelper.WriteJsonFile($"./Hive/Json/ErrorData/{DateTime.Now:yyyyMMddhhmmssFFFFF}+{code}.json", json);
                    }
                }
            }

        }      
        #region IDisposable Support
        private bool disposedValue = false; // 要检测冗余调用

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    var alarmGroup = server?.GetGroupByName("alarms");
                    if (alarmGroup != null)
                    {
                        foreach (var item in alarmGroup.Items)
                        {
                            item.ValueChanged -= Item_ValueChanged;
                        }
                    }
                    var PARASGroup = server.GetGroupByName("PARAS");
                    if (PARASGroup != null)
                    {
                        foreach (var item in PARASGroup.Items)
                        {
                            item.ValueChanged -= PARA_ValueChanged;
                        }
                    }
                    // TODO: 释放托管状态(托管对象)。
                }

                // TODO: 释放未托管的资源(未托管的对象)并在以下内容中替代终结器。
                // TODO: 将大型字段设置为 null。

                disposedValue = true;
            }
        }

        // TODO: 仅当以上 Dispose(bool disposing) 拥有用于释放未托管资源的代码时才替代终结器。
        // ~AlarmManager()
        // {
        //   // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
        //   Dispose(false);
        // }

        // 添加此代码以正确实现可处置模式。
        public void Dispose()
        {
            // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
            Dispose(true);
            // TODO: 如果在以上内容中替代了终结器，则取消注释以下行。
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
