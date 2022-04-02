using DataService;
using Mv.Modules.P91.Hive.ViewModels.Messages;
using Prism.Events;
using Prism.Logging;
using System.Collections;
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

namespace Mv.Modules.P91.Hive.Services
{
    public interface IAlarmManager
    { }
    public class AlarmManager : IAlarmManager, IDisposable
    {
        private readonly IDataServer server;
        private readonly IEventAggregator @event;
        private readonly ILoggerFacade logger;
        private readonly IConfigureFile _configure;
        private readonly string _sw_version = "V1.1.3";
        private TraceConfig _config;
        private string message = "";
        private string code = "";
        private string severity = "";
        private string occurrence_time = "";
        private string messageDT = "";
        private string codeDT = "";
        private string severityDT = "";
        private DateTime occurrence_timeDT;
        private DateTime resolved_timeDT;

        private Edge[] edges = new Edge[8] { new Edge(), new Edge(), new Edge(), new Edge(), new Edge(), new Edge(), new Edge(), new Edge() };
        public AlarmManager(IDataServer server, IEventAggregator @event, ILoggerFacade logger, IConfigureFile configure)
        {
            this.server = server;
            this.@event = @event;
            this.logger = logger;
            this._configure = configure;
            this._configure.ValueChanged += _configure_ValueChanged;
            _config = _configure.GetValue<TraceConfig>("TraceConfig");
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
            #region 写入时间
            Task.Factory.StartNew(() => {
                while (true)
                {
                    var TAG_Hour = server["Hour"];
                    var TAG_Min = server["Min"];
                    var TAG_Sec = server["Sec"];
                    var TAG = server[GetTagName(TagNames.Gettime)];
                    if (TAG != null)
                    {
                        edges[3].CurrentValue = TAG.ToString();
                        if (edges[3].CurrentValue == "1")
                        {
                            var date = DateTime.Now;
                            TAG_Hour?.Write((short)date.Hour);
                            TAG_Min?.Write((short)date.Minute);
                            TAG_Sec?.Write((short)date.Second);
                            if (TAG_Hour != null && TAG_Min != null && TAG_Sec != null)
                            {
                                TAG?.Write((short)11);
                            }
                            else
                            {
                                TAG?.Write((short)12);
                            }
                        }
                    }
                    Thread.Sleep(200);
                }
            }, TaskCreationOptions.LongRunning);//写入时间
            #endregion
            #region 设备状态上传
            Task.Factory.StartNew(() => {
                var TAG = server[GetTagName(TagNames.State)];
                while (true)
                {
                    if (TAG != null)
                    {
                        edges[4].CurrentValue = TAG.ToString();
                        if (edges[4].ValueChanged && edges[4].CurrentValue != "0")
                        {
                            #region 界面颜色切换
                            if(edges[4].CurrentValue == "1")
                            {
                                Global.HiveColour = "#00FF00";
                            }
                            else if (edges[4].CurrentValue == "2")
                            {
                                Global.HiveColour = "#FFFF00";
                            }
                            else if (edges[4].CurrentValue == "3")
                            {
                                Global.HiveColour = "#2C2CFF";
                            }
                            else if (edges[4].CurrentValue == "4")
                            {
                                Global.HiveColour = "#8120FF";
                            }
                            else if (edges[4].CurrentValue == "5")
                            {
                                Global.HiveColour = "#FF0000";
                            }
                            else
                            {
                                Global.HiveColour = "#AAAAAAAA";
                            }

                            #endregion
                            var nowtime = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.ffzz00");
                            MachineState rt = new MachineState();
                            MachineState.Data dt = new MachineState.Data();
                            #region 状态切换成uDT5后遍寻所有报警信息,只能从状态Runing1切换成uDT5,报警或按下停止按钮
                            if (edges[4].CurrentValue == "5")
                            {
                                Thread.Sleep(500);
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
            #region A轴数据上传
            Task.Factory.StartNew(() => {
                var TAG = server[GetTagName(TagNames.Save1)];
                while (true)
                {
                    if (TAG != null)
                    {
                        edges[0].CurrentValue = TAG.ToString();
                        if (edges[0].ValueChanged && edges[0].CurrentValue == "1")
                        {
                            try
                            {                             
                                MachineData rt = new MachineData();
                                rt.unit_sn = System.Guid.NewGuid().ToString() + "-A";
                                MachineData.Serials serials = new MachineData.Serials();
                                rt.serials = serials;
                                rt.pass = "true";
                                try
                                {
                                    string Tag_intime = server[GetTagName(TagNames.INTIME1)].ToString();
                                    Tag_intime = Tag_intime.Substring(1).Insert(2, ":").Insert(5, ":");
                                    string Tag_outime = server[GetTagName(TagNames.OUTTIME1)].ToString();
                                    Tag_outime = Tag_outime.Substring(1).Insert(2, ":").Insert(5, ":");
                                    var input_tmie = DateTime.Parse(Tag_intime);
                                    var output_tmie = DateTime.Parse(Tag_outime);
                                    var a = (output_tmie - input_tmie).TotalSeconds;
                                    if (a > 0)
                                    {
                                        rt.input_time = input_tmie.ToString("yyyy-MM-ddTHH:mm:ss.ffzz00");
                                        rt.output_time = output_tmie.ToString("yyyy-MM-ddTHH:mm:ss.ffzz00");
                                    }
                                    else
                                    {
                                        var input_tmie2 = input_tmie.AddDays(-1);
                                        var interval = (output_tmie - input_tmie2).TotalSeconds;
                                        if (interval > 0 && interval <= 420)
                                        {
                                            rt.input_time = input_tmie2.ToString("yyyy-MM-ddTHH:mm:ss.ffzz00");
                                            rt.output_time = output_tmie.ToString("yyyy-MM-ddTHH:mm:ss.ffzz00");
                                        }
                                        else
                                        {
                                            rt.input_time = output_tmie.AddSeconds(-40).ToString("yyyy-MM-ddTHH:mm:ss.ffzz00");
                                            rt.output_time = output_tmie.ToString("yyyy-MM-ddTHH:mm:ss.ffzz00");
                                            logger.Log($"A轴时间失败比对+{rt.unit_sn},input_time:{input_tmie},output_time:{output_tmie}", Category.Info, Priority.None);
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    rt.input_time = DateTime.Now.AddSeconds(-40).ToString("yyyy-MM-ddTHH:mm:ss.ffzz00");
                                    rt.output_time = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.ffzz00");
                                    logger.Log($"A轴生成时间失败" + ex.Message, Category.Info, Priority.None);
                                }
                                MachineData.Data data = new MachineData.Data();
                                data.Spindle = "D";
                                rt.data = data;
                                var jsonSetting = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
                                var json = JsonConvert.SerializeObject(rt, Formatting.Indented, jsonSetting);
                                #region Data
                                var UpdateData = new Hashtable();
                                UpdateData["sw_version"] = _sw_version;
                                UpdateData["Spindle"] = "A";
                                if (PARASGroup != null)
                                {
                                    foreach (var item in PARASGroup.Items)
                                    {
                                        if (item.GetMetaData().Code == "A" && item.Active)
                                        {
                                            UpdateData[item.GetTagName()] = item.ToString();
                                        }
                                    }
                                }
                                StringBuilder sb = new StringBuilder();
                                foreach (string k in UpdateData.Keys)
                                {
                                    if (sb.Length > 0)
                                    {
                                        sb.Append(",\r\n    ");
                                    }
                                    sb.Append("\"" + k + "\":" + "\"" + (UpdateData[k].ToString() + "\""));
                                }

                                #endregion
                                json = json.Replace("\"Spindle\": \"D\"", sb.ToString());
                                JsonHelper.WriteJsonFile($"./Hive/Json/MachineData/{DateTime.Now:yyyyMMddhhmmss.fffffff}+A.json", json);
                                TAG?.Write((short)Feedback.数据上传OK);
                            }
                            catch (Exception ex)
                            {
                                logger.Log(ex.Message, Category.Info, Priority.None);
                                TAG?.Write((short)Feedback.数据上传NG);
                            }
                        }
                    }
                    Thread.Sleep(100);
                }
            }, TaskCreationOptions.LongRunning);//A轴数据上传
            #endregion
            #region B轴数据上传
            Task.Factory.StartNew(() => {
                var TAG = server[GetTagName(TagNames.Save2)];
                while (true)
                {
                    if (TAG != null)
                    {
                        edges[1].CurrentValue = TAG.ToString();
                        if (edges[1].ValueChanged && edges[1].CurrentValue == "1")
                        {
                            try
                            {                               
                                MachineData rt = new MachineData();
                                rt.unit_sn = System.Guid.NewGuid().ToString() + "-B";
                                MachineData.Serials serials = new MachineData.Serials();
                                rt.serials = serials;
                                rt.pass = "true";
                                try
                                {
                                    string Tag_intime = server[GetTagName(TagNames.INTIME2)].ToString();
                                    Tag_intime = Tag_intime.Substring(1).Insert(2, ":").Insert(5, ":");
                                    string Tag_outime = server[GetTagName(TagNames.OUTTIME2)].ToString();
                                    Tag_outime = Tag_outime.Substring(1).Insert(2, ":").Insert(5, ":");
                                    var input_tmie = DateTime.Parse(Tag_intime);
                                    var output_tmie = DateTime.Parse(Tag_outime);
                                    var a = (output_tmie - input_tmie).TotalSeconds;
                                    if (a > 0)
                                    {
                                        rt.input_time = input_tmie.ToString("yyyy-MM-ddTHH:mm:ss.ffzz00");
                                        rt.output_time = output_tmie.ToString("yyyy-MM-ddTHH:mm:ss.ffzz00");
                                    }
                                    else
                                    {
                                        var input_tmie2 = input_tmie.AddDays(-1);
                                        var interval = (output_tmie - input_tmie2).TotalSeconds;
                                        if (interval > 0 && interval <= 420)
                                        {
                                            rt.input_time = input_tmie2.ToString("yyyy-MM-ddTHH:mm:ss.ffzz00");
                                            rt.output_time = output_tmie.ToString("yyyy-MM-ddTHH:mm:ss.ffzz00");
                                        }
                                        else
                                        {
                                            rt.input_time = output_tmie.AddSeconds(-40).ToString("yyyy-MM-ddTHH:mm:ss.ffzz00");
                                            rt.output_time = output_tmie.ToString("yyyy-MM-ddTHH:mm:ss.ffzz00");
                                            logger.Log($"B轴时间失败比对+{rt.unit_sn},input_time:{input_tmie},output_time:{output_tmie}", Category.Info, Priority.None);
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    rt.input_time = DateTime.Now.AddSeconds(-40).ToString("yyyy-MM-ddTHH:mm:ss.ffzz00");
                                    rt.output_time = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.ffzz00");
                                    logger.Log($"B轴生成时间失败" + ex.Message, Category.Info, Priority.None);
                                }
                                MachineData.Data data = new MachineData.Data();
                                data.Spindle = "D";
                                rt.data = data;
                                var jsonSetting = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
                                var json = JsonConvert.SerializeObject(rt, Formatting.Indented, jsonSetting);
                                #region Data
                                var UpdateData = new Hashtable();
                                UpdateData["sw_version"] = _sw_version;
                                UpdateData["Spindle"] = "B";
                                if (PARASGroup != null)
                                {
                                    foreach (var item in PARASGroup.Items)
                                    {
                                        if (item.GetMetaData().Code == "B" && item.Active)
                                        {
                                            UpdateData[item.GetTagName()] = item.ToString();
                                        }
                                    }
                                }
                                StringBuilder sb = new StringBuilder();
                                foreach (string k in UpdateData.Keys)
                                {
                                    if (sb.Length > 0)
                                    {
                                        sb.Append(",\r\n    ");
                                    }
                                    sb.Append("\"" + k + "\":" + "\"" + (UpdateData[k].ToString() + "\""));
                                }

                                #endregion
                                json = json.Replace("\"Spindle\": \"D\"", sb.ToString());
                                JsonHelper.WriteJsonFile($"./Hive/Json/MachineData/{DateTime.Now:yyyyMMddhhmmss.fffffff}+B.json", json);
                                TAG?.Write((short)Feedback.数据上传OK);
                            }
                            catch (Exception ex)
                            {
                                TAG?.Write((short)Feedback.数据上传NG);
                                logger.Log(ex.Message, Category.Info, Priority.None);
                            }

                        }
                    }
                    Thread.Sleep(100);
                }
            }, TaskCreationOptions.LongRunning);//B轴数据上传
            #endregion
            #region C轴数据上传
            Task.Factory.StartNew(() => {
                var TAG = server[GetTagName(TagNames.Save3)];
                while (true)
                {
                    if (TAG != null)
                    {
                        edges[2].CurrentValue = TAG.ToString();
                        if (edges[2].ValueChanged && edges[2].CurrentValue == "1")
                        {
                            try
                            {
                                MachineData rt = new MachineData();
                                rt.unit_sn = System.Guid.NewGuid().ToString() + "-C";
                                MachineData.Serials serials = new MachineData.Serials();
                                rt.serials = serials;
                                rt.pass = "true";
                                try
                                {
                                    string Tag_intime = server[GetTagName(TagNames.INTIME3)].ToString();
                                    Tag_intime = Tag_intime.Substring(1).Insert(2, ":").Insert(5, ":");
                                    string Tag_outime = server[GetTagName(TagNames.OUTTIME3)].ToString();
                                    Tag_outime = Tag_outime.Substring(1).Insert(2, ":").Insert(5, ":");
                                    var input_tmie = DateTime.Parse(Tag_intime);
                                    var output_tmie = DateTime.Parse(Tag_outime);
                                    var a = (output_tmie - input_tmie).TotalSeconds;
                                    if (a > 0)
                                    {
                                        rt.input_time = input_tmie.ToString("yyyy-MM-ddTHH:mm:ss.ffzz00");
                                        rt.output_time = output_tmie.ToString("yyyy-MM-ddTHH:mm:ss.ffzz00");
                                    }
                                    else
                                    {
                                        var input_tmie2 = input_tmie.AddDays(-1);
                                        var interval = (output_tmie - input_tmie2).TotalSeconds;
                                        if (interval > 0 && interval <= 420)
                                        {
                                            rt.input_time = input_tmie2.ToString("yyyy-MM-ddTHH:mm:ss.ffzz00");
                                            rt.output_time = output_tmie.ToString("yyyy-MM-ddTHH:mm:ss.ffzz00");
                                        }
                                        else
                                        {
                                            rt.input_time = output_tmie.AddSeconds(-40).ToString("yyyy-MM-ddTHH:mm:ss.ffzz00");
                                            rt.output_time = output_tmie.ToString("yyyy-MM-ddTHH:mm:ss.ffzz00");
                                            logger.Log($"C轴时间失败比对+{rt.unit_sn},input_time:{input_tmie},output_time:{output_tmie}", Category.Info, Priority.None);
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    rt.input_time = DateTime.Now.AddSeconds(-40).ToString("yyyy-MM-ddTHH:mm:ss.ffzz00");
                                    rt.output_time = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.ffzz00");
                                    logger.Log($"C轴生成时间失败" + ex.Message, Category.Info, Priority.None);
                                }
                                MachineData.Data data = new MachineData.Data();
                                data.Spindle = "D";
                                rt.data = data;
                                var jsonSetting = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
                                var json = JsonConvert.SerializeObject(rt, Formatting.Indented, jsonSetting);
                                #region Data
                                var UpdateData = new Hashtable();
                                UpdateData["sw_version"] = _sw_version;
                                UpdateData["Spindle"] = "C";
                                if (PARASGroup != null)
                                {
                                    foreach (var item in PARASGroup.Items)
                                    {
                                        if (item.GetMetaData().Code == "C" && item.Active)
                                        {
                                            UpdateData[item.GetTagName()] = item.ToString();
                                        }
                                    }
                                }
                                StringBuilder sb = new StringBuilder();
                                foreach (string k in UpdateData.Keys)
                                {
                                    if (sb.Length > 0)
                                    {
                                        sb.Append(",\r\n    ");
                                    }
                                    sb.Append("\"" + k + "\":" + "\"" + (UpdateData[k].ToString() + "\""));
                                }

                                #endregion
                                json = json.Replace("\"Spindle\": \"D\"", sb.ToString());
                                JsonHelper.WriteJsonFile($"./Hive/Json/MachineData/{DateTime.Now:yyyyMMddhhmmss.fffffff}+C.json", json);
                                TAG?.Write((short)Feedback.数据上传OK);
                            }
                            catch (Exception ex)
                            {
                                TAG?.Write((short)Feedback.数据上传NG);
                                logger.Log(ex.Message, Category.Info, Priority.None);
                            }

                        }
                    }
                    Thread.Sleep(100);
                }
            }, TaskCreationOptions.LongRunning);//C轴数据上传
            #endregion
            #region D轴数据上传
            Task.Factory.StartNew(() => {
                var TAG = server[GetTagName(TagNames.Save4)];
                while (true)
                {
                    if (TAG != null)
                    {
                        edges[3].CurrentValue = TAG.ToString();
                        if (edges[3].ValueChanged && edges[3].CurrentValue == "1")
                        {
                            try
                            {
                                MachineData rt = new MachineData();
                                rt.unit_sn = System.Guid.NewGuid().ToString() + "-D";
                                MachineData.Serials serials = new MachineData.Serials();
                                rt.serials = serials;
                                rt.pass = "true";
                                try
                                {
                                    string Tag_intime = server[GetTagName(TagNames.INTIME4)].ToString();
                                    Tag_intime = Tag_intime.Substring(1).Insert(2, ":").Insert(5, ":");
                                    string Tag_outime = server[GetTagName(TagNames.OUTTIME4)].ToString();
                                    Tag_outime = Tag_outime.Substring(1).Insert(2, ":").Insert(5, ":");
                                    var input_tmie = DateTime.Parse(Tag_intime);
                                    var output_tmie = DateTime.Parse(Tag_outime);
                                    var a = (output_tmie - input_tmie).TotalSeconds;
                                    if (a > 0)
                                    {
                                        rt.input_time = input_tmie.ToString("yyyy-MM-ddTHH:mm:ss.ffzz00");
                                        rt.output_time = output_tmie.ToString("yyyy-MM-ddTHH:mm:ss.ffzz00");
                                    }
                                    else
                                    {
                                        var input_tmie2 = input_tmie.AddDays(-1);
                                        var interval = (output_tmie - input_tmie2).TotalSeconds;
                                        if (interval > 0 && interval <= 420)
                                        {
                                            rt.input_time = input_tmie2.ToString("yyyy-MM-ddTHH:mm:ss.ffzz00");
                                            rt.output_time = output_tmie.ToString("yyyy-MM-ddTHH:mm:ss.ffzz00");
                                        }
                                        else
                                        {
                                            rt.input_time = output_tmie.AddSeconds(-40).ToString("yyyy-MM-ddTHH:mm:ss.ffzz00");
                                            rt.output_time = output_tmie.ToString("yyyy-MM-ddTHH:mm:ss.ffzz00");
                                            logger.Log($"D轴时间失败比对+{rt.unit_sn},input_time:{input_tmie},output_time:{output_tmie}", Category.Info, Priority.None);
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    rt.input_time = DateTime.Now.AddSeconds(-40).ToString("yyyy-MM-ddTHH:mm:ss.ffzz00");
                                    rt.output_time = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.ffzz00");
                                    logger.Log($"D轴生成时间失败" + ex.Message, Category.Info, Priority.None);
                                }
                                MachineData.Data data = new MachineData.Data();
                                data.Spindle = "D";
                                rt.data = data;
                                var jsonSetting = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
                                var json = JsonConvert.SerializeObject(rt, Formatting.Indented, jsonSetting);
                                #region Data
                                var UpdateData = new Hashtable();
                                UpdateData["sw_version"] = _sw_version;
                                UpdateData["Spindle"] = "D";
                                if (PARASGroup != null)
                                {
                                    foreach (var item in PARASGroup.Items)
                                    {
                                        if (item.GetMetaData().Code == "D" && item.Active)
                                        {
                                            UpdateData[item.GetTagName()] = item.ToString();
                                        }
                                    }
                                }
                                StringBuilder sb = new StringBuilder();
                                foreach (string k in UpdateData.Keys)
                                {
                                    if (sb.Length > 0)
                                    {
                                        sb.Append(",\r\n    ");
                                    }
                                    sb.Append("\"" + k + "\":" + "\"" + (UpdateData[k].ToString() + "\""));
                                }

                                #endregion
                                json = json.Replace("\"Spindle\": \"D\"", sb.ToString());
                                JsonHelper.WriteJsonFile($"./Hive/Json/MachineData/{DateTime.Now:yyyyMMddhhmmss.fffffff}+D.json", json);
                                TAG?.Write((short)Feedback.数据上传OK);
                            }
                            catch (Exception ex)
                            {
                                TAG?.Write((short)Feedback.数据上传NG);
                                logger.Log(ex.Message, Category.Info, Priority.None);
                            }
                        }
                    }
                    Thread.Sleep(100);
                }
            }, TaskCreationOptions.LongRunning);//D轴数据上传
            #endregion
            //#region ICT AutoDT上传
            //Task.Factory.StartNew(() =>
            //{
            //    var TAG = server[GetTagName(TagNames.StateDT)];
            //    var TAG_Res = server[GetTagName(TagNames.DTRes)];
            //    while (true)
            //    {
            //        if (TAG != null)
            //        {
            //            edges[5].CurrentValue = TAG.ToString();
            //            if (edges[5].ValueChanged && edges[5].CurrentValue != "0")
            //            {
            //                var now= DateTime.Now;
            //                TAG_Res?.Write((short)0);
            //                var data=new AutoDtData();
            //                #region 状态切换成异常停机
            //                if (edges[5].CurrentValue == "5")
            //                {
            //                    Thread.Sleep(100);
            //                    codeDT = "E001";
            //                    messageDT = "Not retrieve plc error list";
            //                    severityDT = "critical";
            //                    occurrence_timeDT= now;
            //                    if (alarmGroup != null)
            //                    {
            //                        foreach (var item in alarmGroup.Items)
            //                        {
            //                            if (item.Value.Boolean == true)
            //                            {
            //                                if (item.GetMetaData().DtCode != null)
            //                                {
            //                                    codeDT = item.GetMetaData().DtCode;
            //                                    messageDT = item.GetMetaData().Description;
            //                                    severityDT = item.GetMetaData().Severity.ToString();
            //                                }
            //                            }
            //                        }
            //                    }
            //                    data.code= codeDT;
            //                    data.status = "-1";
            //                }
            //                #endregion

            //                #region 状态切换成正常运行
            //                else if (edges[5].CurrentValue == "1")
            //                {
            //                    data.status = "0";
            //                    data.code = "OK";
            //                }
            //                #endregion
            //                else if(edges[5].CurrentValue == "2")
            //                {
            //                    data.status = "0";
            //                    data.code = "IDLE";

            //                }
            //                else if (edges[5].CurrentValue == "3")
            //                {
            //                    data.status = "0";
            //                    data.code = "Engineering";

            //                }
            //                else if (edges[5].CurrentValue == "4")
            //                {
            //                    data.status = "0";
            //                    data.code = "END";

            //                }

            //                if (edges[5].OldValue == "5")
            //                {
            //                    resolved_timeDT = now;
            //                    try
            //                    {
            //                        Dictionary<string, string> dictionary = new Dictionary<string, string>();
            //                        dictionary["Time"] = DateTime.Now.ToString("yyyy/MM/ddTHH:mm:ss:ff");
            //                        dictionary["message"] = messageDT;
            //                        dictionary["severity"] = severityDT;
            //                        dictionary["code"] = codeDT;
            //                        dictionary["occurrence_time"] = occurrence_timeDT.ToString("yyyy/MM/ddTHH:mm:ss:ff");
            //                        dictionary["resolved_time"] = resolved_timeDT.ToString("yyyy/MM/ddTHH:mm:ss:ff");
            //                        CsvHelper.WriteCsv($"./AutoDtLog/alarms/{DateTime.Now:yyyyMM}/{DateTime.Now:yyyyMMdd}.csv", dictionary);
            //                    }
            //                    catch (Exception ex)
            //                    {

            //                    }
            //                }
            //                try
            //                    {
            //                        var json = JsonConvert.SerializeObject(data);
            //                        string url= $"http://10.33.30.122/equipment/kssfclisa/machine/{_config.LineId}/{_config.MachineId}";
            //                        if (_config.isDtUpload)
            //                        {
            //                            string responseString = "";
            //                            var res = PostHelper.DtPost(url, json, ref responseString);
            //                            CsvHelper.Writelog($"./AutoDtLog/Upload/{DateTime.Now:yyyyMM}/{DateTime.Now:yyyyMMdd}", "IPC send:" + json);
            //                            CsvHelper.Writelog($"./AutoDtLog/Upload/{DateTime.Now:yyyyMM}/{DateTime.Now:yyyyMMdd}", "server send:" + responseString);
            //                        if (responseString.Contains("0"))
            //                        {
            //                            TAG_Res?.Write((short)1);
            //                            this.@event.GetEvent<UserMessageEvent>().Publish(new UserMessage { Content ="DT上传成功", Level = 0, Source = "AutoDT" });
            //                        }
            //                        else
            //                        {
            //                            TAG_Res?.Write((short)2);
            //                            this.@event.GetEvent<UserMessageEvent>().Publish(new UserMessage { Content = "DT上传失败", Level = 0, Source = "AutoDT" });
            //                        }

            //                    }
            //                    }
            //                    catch (Exception ex)
            //                    {
            //                        TAG_Res?.Write((short)2);
            //                        CsvHelper.Writelog($"./AutoDtLog/Upload/{DateTime.Now:yyyyMM}/{DateTime.Now:yyyyMMdd}", $"Error:Error");
            //                    }                               
            //            }
            //        }
            //        Thread.Sleep(100);
            //    }
            //}, TaskCreationOptions.LongRunning);
            //#endregion
            Task.Factory.StartNew(() => {
                while (true)
                {
                    var GetTAG = server["DNBSJSC"];
                    if (GetTAG != null)
                    {
                        edges[6].CurrentValue = GetTAG.ToString();
                        if (edges[6].ValueChanged && edges[6].CurrentValue == "1")
                        {
                            try
                            {
                                Dictionary<string, string> dictionary = new Dictionary<string, string>();
                                dictionary["Time"] = DateTime.Now.ToString("yyyy/MM/ddTHH:mm:ss:ff");
                                dictionary["Current1(A)"] =Math.Round(server["A1"].Value.Int32/1000.0,3).ToString();
                                dictionary["Voltage1(V)"] = Math.Round(server["V1"].Value.Int32 / 100.0, 2).ToString();
                                dictionary["Energy1(kW*h)"] = Math.Round(server["LJGL1"].Value.Int32 / 1000.0, 3).ToString();
                                dictionary["Power factor1"] = Math.Round(server["NUM1"].Value.Int32 / 1000.0,3).ToString();
                                dictionary["Active power1(kW)"] = Math.Round(server["YG1"].Value.Int32 / 100000000.0, 8).ToString();
                                dictionary["Current2(A)"] = Math.Round(server["A2"].Value.Int32 / 1000.0, 3).ToString();
                                dictionary["Voltage2(V)"] = Math.Round(server["V2"].Value.Int32 / 100.0, 2).ToString();
                                dictionary["Energy2(kW*h)"] = Math.Round(server["LJGL2"].Value.Int32 / 1000.0, 3).ToString();
                                dictionary["Power factor2"] = Math.Round(server["NUM2"].Value.Int32 / 1000.0, 3).ToString();
                                dictionary["Active power2(kW)"] = Math.Round(server["YG2"].Value.Int32 / 100000000.0, 8).ToString();
                                dictionary["Current3(A)"] = Math.Round(server["A3"].Value.Int32 / 1000.0, 3).ToString();
                                dictionary["Voltage3(V)"] = Math.Round(server["V3"].Value.Int32 / 100.0, 2).ToString();
                                dictionary["Energy3(kW*h)"] = Math.Round(server["LJGL3"].Value.Int32 / 1000.0, 3).ToString();
                                dictionary["Power factor3"] = Math.Round(server["NUM3"].Value.Int32 / 1000.0, 3).ToString();
                                dictionary["Active power3(kW)"] = Math.Round(server["YG3"].Value.Int32 / 100000000.0, 8).ToString();
                                dictionary["Current4(A)"] = Math.Round(server["A4"].Value.Int32 / 1000.0, 3).ToString();
                                dictionary["Voltage4(V)"] = Math.Round(server["V4"].Value.Int32 / 100.0, 2).ToString();
                                dictionary["Energy4(kW*h)"] = Math.Round(server["LJGL4"].Value.Int32 / 1000.0, 3).ToString();
                                dictionary["Power factor4"] = Math.Round(server["NUM4"].Value.Int32 / 1000.0, 3).ToString();
                                dictionary["Active power4(kW)"] = Math.Round(server["YG4"].Value.Int32 / 100000000.0,8).ToString();
                                bool res = CsvHelper.WriteCsv($"./RunLog/EE/{DateTime.Now:yyyyMM}/{DateTime.Now:yyyyMMdd}.csv", dictionary);
                                if (res)
                                    GetTAG?.Write((short)11);
                                else
                                    GetTAG?.Write((short)12);
                            }
                            catch (Exception ex)
                            {
                                GetTAG?.Write((short)12);
                            }
                        }
                    }
                    Thread.Sleep(200);
                }
            }, TaskCreationOptions.LongRunning);//电能表数据上传(傻逼客户天天改,加奇葩功能，程序都成一坨屎了)
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
                logger.Log(msg, Category.Info, Priority.None);
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
                var minimum = tag.GetMetaData().Minimum;
                this.@event.GetEvent<UserMessageEvent>().Publish(new UserMessage { Content = msg, Level = 0, Source = "alarms" });
                logger.Log(msg, Category.Info, Priority.None);
                var value1 = (bool)tag.GetValue(e.Value);
            }

        }      
        private void _configure_ValueChanged(object sender, Mv.Core.Interfaces.ValueChangedEventArgs e)
        {
            if (e.KeyName != nameof(TraceConfig)) return;
            var config = _configure.GetValue<TraceConfig>(nameof(TraceConfig));
            _config = config;

        }
        private string GetTagName(TagNames tagenum)
        {
            return Enum.GetName(typeof(TagNames), tagenum);
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

    public class AutoDtData
    {
        public string status { get; set; }
        public string code { get; set; }
    }
}
