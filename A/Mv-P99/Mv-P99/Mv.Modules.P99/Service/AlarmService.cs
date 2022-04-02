using CsvHelper;
using CsvHelper.Configuration.Attributes;
using Prism.Events;
using Prism.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Concurrent;
using Newtonsoft.Json;
using DataService;
using System.Threading.Tasks;
using System.Threading;

namespace Mv.Modules.P99.Service
{
    public class AlarmItem
    {
        public int Id { get; set; }
        public string Address { get; set; }
        public string Message { get; set; }
        public string MessageHive { get; set; }
        public string ErrorCode { get; set; }
        public string Severity { get; set; }
        public bool Hive { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime StopTime { get; set; }
        public TimeSpan TimeSpan { get; set; }
    }

    public class AlarmService : IAlarmService
    {
        internal class AlarmInfoRecord
        {
            [Index(2)]
            public string Address { get; set; }
            [Index(4)]
            public string Message { get; set; }
            [Index(5)]
            public string Hive { get; set; }
            [Index(6)]
            public string MessageHive { get; set; }
            [Index(7)]
            public string ErrorCode { get; set; }
            [Index(8)]
            public string Severity { get; set; }

        }
        internal class AlarmInfo
        {
            public string Address { get; set; }

            public string Message { get; set; }
            public string MessageHive { get; set; }
            public string ErrorCode { get; set; }
            public string Severity { get; set; }
            public bool Hive { get; set; }
            public int AddressOffset { get; set; }

            public int BitIndex { get; set; }
        }
        private readonly ILoggerFacade logger;
        private readonly IEventAggregator eventAggregator;
        private readonly IDeviceReadWriter device;
        private int addressOffset = 3700;
        private int localOffset = 200;
        private string message = "";
        private string code = "";
        private string severity = "";
        private string occurrence_time = "";
        private Edge[] edges = new Edge[7] { new Edge(), new Edge(), new Edge(), new Edge(), new Edge(), new Edge(), new Edge() };
        Subject<AlarmItem> subjectAlarmItem = new Subject<AlarmItem>();
        Subject<AlarmInfo> subjectNewAlarm = new Subject<AlarmInfo>();
        private IEnumerable<AlarmInfoRecord> GetAlarmInfos(FileInfo file)
        {
            try
            {
                using var reader = new StreamReader(file.FullName, encoding: Encoding.UTF8);
                using var csv = new CsvReader(reader, CultureInfo.CurrentCulture);
                var records = csv.GetRecords<AlarmInfoRecord>()
                .Where(x => !string.IsNullOrEmpty(x.Message))
                .Where(X => regex.IsMatch(X.Address))
                .Distinct().ToList();
                return records;
            }
            catch (Exception ex)
            {
                logger.Log(ex.Message + Environment.NewLine + ex.StackTrace, Category.Exception, Priority.None);
                return new List<AlarmInfoRecord>();
            }
        }

        Regex regex = new Regex(@"^MB\d{4}[A-D0-9]{1}");
        public AlarmService(ILoggerFacade logger, IEventAggregator eventAggregator, IDeviceReadWriter device)
        {
            this.logger = logger;
            this.eventAggregator = eventAggregator;
            this.device = device;
            LoadAlarmInfos();
            
            Observable.Interval(TimeSpan.FromMilliseconds(100)).ObserveOnDispatcher().Subscribe(ObserveAlarms);
            subjectNewAlarm.Subscribe(m =>
            {
                logger.Log($"{m.Address}:{m.Message}", Category.Debug, Priority.None);
                var dictionary = new Dictionary<string, string>();
                dictionary["地址"] = m.Address;
                dictionary["开始时间"] = DateTime.Now.ToString();
                dictionary["信息"] = m.Message;
                if (m.Message.Contains("按钮"))
                    dictionary["类型"] = "U";
                else
                    dictionary["类型"] = "A";
                Helper.SaveFile($"./SFC/{DateTime.Today:yyyyMMdd}.csv", dictionary);
            });
            subjectAlarmItem.Buffer(TimeSpan.FromSeconds(1)).Subscribe((n) =>
            {
                n.ForEach(m =>
                {
                    var dictionary = new Dictionary<string, string>();
                    dictionary["开始时间"] = m.StartTime.ToString();
                    dictionary["结束时间"] = m.StopTime.ToString();
                    dictionary["报警地址"] = m.Address;
                    dictionary["报警信息"] = m.Message;
                    dictionary["持续时间"] = m.TimeSpan.ToString();
                    Helper.SaveFile($"./报警信息/{DateTime.Today:yyyyMMdd}.csv", dictionary);
                });
            });
            #region 设备状态上传
            Task.Factory.StartNew(() => {
                while (true)
                {
                  try
                  {
                        edges[2].CurrentValue = device.GetShort(301);
                        if (edges[2].ValueChanged && edges[2].CurrentValue!= 0)
                        {
                            var nowtime = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.ffzz00");
                            MachineState rt = new MachineState();
                            MachineState.Data dt = new MachineState.Data();
                            #region 状态切换成uDT5后遍寻所有报警信息,只能从状态Runing1切换成uDT5,报警或按下停止按钮
                            if (edges[2].CurrentValue == 5)
                            {
                                Thread.Sleep(500);
                                occurrence_time = nowtime;
                                code = "O99EECE-01-03";
                                message = "The stop button is pressed";
                                currentAlarmItems.Values.ForEach(x =>
                                {
                                    if (x.Hive == true)
                                    {
                                        message = x.MessageHive;
                                        code = x.ErrorCode;
                                        severity = x.Severity;
                                    }
                                });
                                dt.previous_state = "1";
                                dt.code = code;
                                dt.error_message = message;
                                if (message == "The stop button is pressed")
                                    dt.state_change_reason = "User pressed stop button";
                                else
                                    dt.state_change_reason = "Machine alarm with description";
                                if (edges[2].OldValue ==0)
                                    dt.state_change_reason = "User open hive software";
                            }
                            #endregion

                            #region 状态切换成Runing1,只能从状态IDLE2切换成Runing1，按下开始按钮
                            if (edges[2].CurrentValue == 1)
                            {
                                dt.previous_state = "2";
                                if (edges[2].OldValue == 0)
                                    dt.state_change_reason = "User open hive software";
                                else
                                    dt.state_change_reason = "User pressed start button";
                            }
                            #endregion

                            #region 状态切换成Engineering3,只能从IDLE2切入，按下工程师按钮
                            if (edges[2].CurrentValue == 3)
                            {
                                dt.previous_state = "2";
                                if (edges[2].OldValue == 0)
                                    dt.state_change_reason = "User open hive software";
                                else
                                    dt.state_change_reason = "User pressed Engineering button";
                            }
                            #endregion

                            #region 状态切换成pDT4,只能从状态IDLE2切入，按下维修按钮
                            if (edges[2].CurrentValue == 4)
                            {
                                dt.previous_state = "2";
                                if (edges[2].OldValue == 0)
                                    dt.state_change_reason = "User open hive software";
                                else
                                    dt.state_change_reason = "User pressed Maintenance button";
                            }
                            #endregion

                            #region 状态切换成IDLE2,其他任意状态都可以切入，查询上次状态，如果是上次是pDT4状态(遍寻修改的维修内容)
                            if (edges[2].CurrentValue == 2)
                            {
                                if (edges[2].OldValue == 1)
                                {
                                    dt.previous_state = "1";
                                    dt.state_change_reason = "User pressed pause button";
                                }
                                else if (edges[2].OldValue == 3)
                                {
                                    dt.previous_state = "3";
                                    dt.state_change_reason = "User pressed Exit button";
                                }
                                else if (edges[2].OldValue == 4)
                                {
                                    dt.previous_state = "4";
                                    dt.state_change_reason = "Exit Maintenance";
                                }
                                else if (edges[2].OldValue == 5)
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
                            rt.machine_state = edges[2].CurrentValue.ToString();
                            rt.state_change_time = nowtime;
                            rt.data = dt;
                            var jsonSetting = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
                            var json = JsonConvert.SerializeObject(rt, Formatting.Indented, jsonSetting);
                            JsonHelper.WriteJsonFile($"./Hive/Json/StateData/{DateTime.Now:yyyyMMddhhmmss.fffffff}.json", json);
                        }
                  }
                    catch
                  {
                   }
                    Thread.Sleep(300);
                }
            }, TaskCreationOptions.LongRunning);
            #endregion
            #region 设备获取时间
            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    edges[3].CurrentValue = device.GetShort(302);
                    if (edges[3].ValueChanged && edges[3].CurrentValue == 1)
                    {
                        device.SetShort(10,0);
                        Thread.Sleep(500);
                        device.SetShort(11, (short)DateTime.Now.Hour);
                        device.SetShort(12, (short)DateTime.Now.Minute);
                        device.SetShort(13, (short)DateTime.Now.Second);
                        device.SetShort(10, 1);
                    }
                  Thread.Sleep(300);
                }
            }, TaskCreationOptions.LongRunning);
            #endregion
        }
        ConcurrentDictionary<string, AlarmItem> currentAlarmItems = new ConcurrentDictionary<string, AlarmItem>();
        //bool localvalue;
        private void ObserveAlarms(long m)
        {
            //localvalue = !localvalue;
            //device.SetBit(0, 1, localvalue);
            //device.GetBit(200, 1);
            var inalarms = alarms.Where(x => device.GetBit(x.AddressOffset - addressOffset + localOffset, x.BitIndex));
            var noalarms = alarms.Except(inalarms);
            //添加新的报警信息
            var newalarms = inalarms.Where(x => !currentAlarmItems.ContainsKey(x.Address)).ToList();
            newalarms.ForEach((newAlarm) => currentAlarmItems[newAlarm.Address] = new AlarmItem
            {
                Address = newAlarm.Address,
                Message = newAlarm.Message,
                Severity = newAlarm.Severity,
                Hive = newAlarm.Hive,
                ErrorCode = newAlarm.ErrorCode,
                MessageHive = newAlarm.MessageHive,
                StartTime = DateTime.Now,
                TimeSpan = TimeSpan.FromSeconds(0)
            });
            newalarms.ForEach(x => subjectNewAlarm.OnNext(x));


            //删除旧的报警信息
            noalarms.Where(x => currentAlarmItems.ContainsKey(x.Address)).ToList()
            .ForEach((solvedAlarm) =>
            {
                AlarmItem value = currentAlarmItems[solvedAlarm.Address];
                value.StopTime = DateTime.Now;
                value.TimeSpan = value.StopTime - value.StartTime;
                subjectAlarmItem.OnNext(value);
                currentAlarmItems.TryRemove(solvedAlarm.Address, out var removed);
              //  currentAlarmItems.Remove(solvedAlarm.Address);
                logger.Log($"{value.Address}:{value.Message}\t开始时间：{value.StartTime:HH:mm:ss}，结束时间：{value.StopTime:HH:mm:ss},持续时间：{value.TimeSpan.TotalSeconds}S"
                    , Category.Debug
                    , Priority.None);
            });
            //更新现有报警
            currentAlarmItems.Values.ForEach(x =>
            {
                x.StopTime = DateTime.Now;
                x.TimeSpan = x.StopTime - x.StartTime;
            });

        }

        public List<AlarmItem> GetAlarmItems()
        {
            return currentAlarmItems.Values.ToList();
        }
        List<AlarmInfo> alarms = new List<AlarmInfo>();
        public void LoadAlarmInfos(string filePath = @"./Alarms/Alarms.csv")
        {
            alarms = GetAlarmInfos(new FileInfo(filePath))
                .Select(x => new AlarmInfo
                {
                    Address = x.Address,
                    Message = x.Message,
                    Hive = bool.Parse(x.Hive),
                    MessageHive = x.MessageHive,
                    ErrorCode = x.ErrorCode,
                    Severity = x.Severity,
                    AddressOffset = int.Parse(x.Address.Substring(2, 4)),
                    BitIndex = Convert.ToInt32(x.Address.Substring(6, 1), 16)
                }).ToList();
        }
    }
}
