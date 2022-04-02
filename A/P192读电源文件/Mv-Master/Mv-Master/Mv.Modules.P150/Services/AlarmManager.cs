using DataService;
using Mv.Modules.P150.ViewModels.Messages;
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
namespace Mv.Modules.P150.Services
{
    public class PowerRecord
    {
        [Index(1)]
        public string Station { get; set; }
        [Index(2)]
        public string Date { get; set; }
        [Index(3)]
        public string Time { get; set; }
        [Index(4)]
        public string Rstart { get; set; }
        [Index(5)]
        public string Current { get; set; }
        [Index(6)]
        public string Voltage { get; set; }
        [Index(7)]
        public string Power { get; set; }
        [Index(8)]
        public string Rend { get; set; }
        [Index(9)]
        public string Bonding_Temp { get; set; }
        [Index(10)]
        public string Bonding_Time { get; set; }
        [Index(11)]
        public string Tool_Temp { get; set; }
        [Index(12)]
        public string Bonding_Method { get; set; }
        [Index(13)]
        public string RC1 { get; set; }
        [Index(14)]
        public string RC2 { get; set; }
        [Index(15)]
        public string Result { get; set; }
    }
    public class PowerInfo
    {
        public string Station { get; set; }
        public string Date { get; set; }
        public string Time { get; set; }
        public string Rstart { get; set; }
        public string Current { get; set; }
        public string Voltage { get; set; }
        public string Power { get; set; }
        public string Rend { get; set; }
        public string Bonding_Temp { get; set; }
        public string Bonding_Time { get; set; }
        public string Tool_Temp { get; set; }
        public string Bonding_Method { get; set; }
        public string RC1 { get; set; }
        public string RC2 { get; set; }
        public string Result { get; set; }
    }
    public interface IAlarmManager
    { }
    public class AlarmManager : IAlarmManager, IDisposable
    {
        private readonly IDataServer server;
        private readonly IEventAggregator @event;
        private readonly ILoggerFacade logger;
        private readonly IConfigureFile _configure;
        private readonly string _sw_version = "V1.1.23";
        private TraceConfig _config;
        private bool isError;
        private string message = "";
        private string code = "";
        private string severity = "";
        private string occurrence_time = "";

        private string snA="N/A";
        private string snB = "N/A";

        private string messageA = "";
        private string codeA = "";
        private string severityA = "";
        private DateTime occurrence_timeA;
        private DateTime resolved_timeA;

        private string messageB = "";
        private string codeB = "";
        private string severityB = "";
        private DateTime occurrence_timeB;
        private DateTime resolved_timeB;



        private Edge[] edges = new Edge[9] { new Edge(), new Edge(), new Edge(), new Edge(), new Edge(), new Edge(), new Edge(), new Edge(), new Edge() };
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
            Task.Factory.StartNew(() => {
                while (true)
                {
                    var TAG = server["HANDSHAKE"];
                    TAG?.Write((short)1);//加？表示可为空值
                    Thread.Sleep(1000);
                    TAG?.Write((short)0);//加？表示可为空值
                    Thread.Sleep(1000);
                }
            }, TaskCreationOptions.LongRunning);//心跳
            Task.Factory.StartNew(() => {
                while (true)
                {
                    //var TAG_Year = server["Year"];
                    //var TAG_Month = server["Month"];
                    //var TAG_Day = server["Day"];
                    var TAG_Hour = server["Hour"];
                    var TAG_Min = server["Min"];
                    var TAG_Sec = server["Sec"];
                    var TAG = server[GetTagName(TagNames.Gettime)];
                    if (TAG != null)
                    {
                        edges[3].CurrentValue = TAG.ToString();
                        if (edges[3].ValueChanged && edges[3].CurrentValue == "1")
                        {
                            //TAG_Year?.Write((short)DateTime.Now.Year);
                            //TAG_Month?.Write((short)DateTime.Now.Month);
                            //TAG_Day?.Write((short)DateTime.Now.Day);
                            var currentTime = DateTime.Now;
                            TAG_Hour?.Write((short)currentTime.Hour);
                            TAG_Min?.Write((short)currentTime.Minute);
                            TAG_Sec?.Write((short)currentTime.Second);
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

            Task.Factory.StartNew(() => {
                var TAG = server[GetTagName(TagNames.Save1)];
                var TAG_OK = server[GetTagName(TagNames.SaveOK1)];
                while (true)
                {
                    if (TAG != null)
                    {
                        edges[0].CurrentValue = TAG.ToString();
                        if (edges[0].ValueChanged && edges[0].CurrentValue == "True")
                        {
                            TAG_OK?.Write((bool)false);
                            Thread.Sleep(300);
                            try
                            {
                                var shiftnow = DateTime.Now;
                                var power = GetPowerRecord();
                                MachineData rt = new MachineData();
                                rt.sequence = 1;
                                rt.unit_sn = System.Guid.NewGuid().ToString() + "-A";
                                snA = rt.unit_sn;
                                MachineData.Serials serials = new MachineData.Serials();
                                rt.serials = serials;
                                rt.pass = "true";
                                try
                                {
                                 //string Tag_indate = server[GetTagName(TagNames.INDATEA)].ToString();
                                 //Tag_indate = Tag_indate.Insert(4, "/").Insert(7, "/");
                                 //string Tag_outdate = server[GetTagName(TagNames.OUTDATEA)].ToString();
                                 //Tag_outdate = Tag_outdate.Insert(4, "/").Insert(7, "/");

                                string Tag_intime = server[GetTagName(TagNames.INTIMEA)].ToString();
                                Tag_intime = Tag_intime.Substring(1).Insert(2, ":").Insert(5, ":");
                                string Tag_outime = server[GetTagName(TagNames.OUTTIMEA)].ToString();
                                Tag_outime = Tag_outime.Substring(1).Insert(2, ":").Insert(5, ":");
                                    //var input_tmie = DateTime.Parse(Tag_indate+" "+Tag_intime);
                                    //var output_tmie = DateTime.Parse(Tag_outdate+" "+Tag_outime);
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
                                    if (interval > 0 && interval <= 120)
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
                                catch(Exception ex)
                                {
                                    rt.input_time = DateTime.Now.AddSeconds(-40).ToString("yyyy-MM-ddTHH:mm:ss.ffzz00");
                                    rt.output_time = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.ffzz00");
                                    logger.Log($"A轴生成时间失败" + ex.Message, Category.Info, Priority.None);
                                }
                                MachineData.Data data = new MachineData.Data();
                                #region Data
                                data.Spindle = "A";
                                if (_config.isFA1)
                                    data.CTQ_FAI1 = _config.FA1_A;
                                if (_config.isFA2)
                                    data.CTQ_FAI2 = _config.FA2_A;
                                if (_config.isFA3)
                                    data.CTQ_FAI3 = _config.FA3_A;
                                if (_config.isFA4)
                                    data.CTQ_FAI4 = _config.FA4_A;
                                if (_config.isFA5)
                                    data.CTQ_FAI5 = _config.FA5_A;
                                if (_config.isFA6)
                                    data.CTQ_FAI6 = _config.FA6_A;
                                if (_config.isFA7)
                                    data.CTQ_FAI7 = _config.FA7_A;
                                if (_config.isFA8)
                                    data.CTQ_FAI8 = _config.FA8_A;
                                if (_config.isFA9)
                                    data.CTQ_FAI9 = _config.FA9_A;
                                if (_config.isFA10)
                                    data.CTQ_FAI10 = _config.FA10_A;
                                if (_config.isFA11)
                                    data.CTQ_FAI11 = _config.FA11_A;
                                if (_config.isFA12)
                                    data.CTQ_FAI12 = _config.FA12_A;
                                if (_config.isFA13)
                                    data.CTQ_FAI13 = _config.FA13_A;
                                if (_config.isFA14)
                                    data.CTQ_FAI14 = _config.FA14_A;
                                if (_config.isFA15)
                                    data.CTQ_FAI15 = _config.FA15_A;
                                if (_config.isFA16)
                                    data.CTQ_FAI16 = _config.FA16_A;
                                if (_config.isFA17)
                                    data.CTQ_FAI17 = _config.FA17_A;
                                if (_config.isFA18)
                                    data.CTQ_FAI18 = _config.FA18_A;
                                if (_config.isFA19)
                                    data.CTQ_FAI19 = _config.FA19_A;
                                if (_config.isFA20)
                                    data.CTQ_FAI20 = _config.FA20_A;
                                if (_config.isFA21)
                                    data.CTQ_FAI21 = _config.FA21_A;
                                if (_config.isFA22)
                                    data.CTQ_FAI22 = _config.FA22_A;
                                if (_config.isFA23)
                                    data.CTQ_FAI23 = _config.FA23_A;
                                if (_config.isFA24)
                                    data.CTQ_FAI24 = _config.FA24_A;
                                if (_config.isFA25)
                                    data.CTQ_FAI25 = _config.FA25_A;
                                if (_config.isParallelism)
                                    data.Parallelism_between_upper_and_lower_mandrel = _config.Parallelism_A;
                                if (_config.isBendingPin)
                                    data.Bending_Pin_Position = _config.BendingPin_A;
                                if (_config.isUsageC && server[GetTagName(TagNames.FMA)] != null)
                                    data.Tool_cycles = server[GetTagName(TagNames.FMA)].Value.Int32.ToString();
                                if (_config.isUsageM && server[GetTagName(TagNames.PMA)] != null)
                                    data.PM_cycles = server[GetTagName(TagNames.PMA)].Value.Int16.ToString();
                                if (_config.isOD)
                                    data.Wire_OD = _config.OD_A;
                                if (_config.isLower && server[GetTagName(TagNames.LMTA)] != null)
                                    data.Lower_Mandrel_temperature = (server[GetTagName(TagNames.LMTA)].Value.Int32 / 10.0).ToString("F1");
                                if (_config.isWireTen)
                                {
                                    data.Wire_Tension1 = _config.WireTen_A1;
                                    data.Wire_Tension2 = _config.WireTen_A2;
                                    data.Wire_Tension3 = _config.WireTen_A3;
                                    data.Wire_Tension4 = _config.WireTen_A4;
                                    data.Wire_Tension5 = _config.WireTen_A5;
                                    data.Wire_Tension6 = _config.WireTen_A6;
                                    data.Wire_Tension7 = _config.WireTen_A7;
                                }
                                if (_config.isGrap1 && server[GetTagName(TagNames.MG1A)] != null)
                                    data.Mandrel_gap1 = (server[GetTagName(TagNames.MG1A)].Value.Int32 / 10000.0).ToString("F4");
                                if (_config.isGrap2 && server[GetTagName(TagNames.MG2A)] != null)
                                    data.Mandrel_gap2 = (server[GetTagName(TagNames.MG2A)].Value.Int32 / 10000.0).ToString("F4");
                                if (_config.isGrap3 && server[GetTagName(TagNames.MG3A)] != null)
                                    data.Mandrel_gap3 = (server[GetTagName(TagNames.MG3A)].Value.Int32 / 10000.0).ToString("F4");
                                if (_config.isWspeed1 && server[GetTagName(TagNames.WS1A)] != null)
                                    data.Winding_speed1 = (server[GetTagName(TagNames.WS1A)].Value.Int32 / 100.0).ToString("F2");
                                if (_config.isWspeed2 && server[GetTagName(TagNames.WS2A)] != null)
                                    data.Winding_speed2 = (server[GetTagName(TagNames.WS2A)].Value.Int32 / 100.0).ToString("F2");
                                if (_config.isWspeed3 && server[GetTagName(TagNames.WS3A)] != null)
                                    data.Winding_speed3 = (server[GetTagName(TagNames.WS3A)].Value.Int32 / 100.0).ToString("F2");
                                if (_config.isWspeedB && server[GetTagName(TagNames.WSBA)] != null)
                                    data.Bending_speed = (server[GetTagName(TagNames.WSBA)].Value.Int32 / 100.0).ToString("F2");
                                if (_config.isIspeed && server[GetTagName(TagNames.ISA)] != null)
                                    data.Iron_speed = (server[GetTagName(TagNames.ISA)].Value.Int32 / 10000.0).ToString("F4");

                                data.sw_version = _sw_version;
                                #endregion
                                #region PowerData
                                if (_config.isRstart)
                                    data.Rstart = power.Item2.Rstart;
                                if (_config.isCurrent)
                                    data.Current = power.Item2.Current;
                                if (_config.isVoltage)
                                    data.Voltage = power.Item2.Voltage;
                                if (_config.isPower)
                                    data.Power = power.Item2.Power;
                                if (_config.isRend)
                                    data.Rend = power.Item2.Rend;
                                if (_config.isBonding_Temp)
                                    data.Bonding_Temp = power.Item2.Bonding_Temp;
                                if (_config.isBonding_Time)
                                    data.Bonding_Time = power.Item2.Bonding_Time;
                                if (_config.isTool_Temp)
                                    data.Tool_Temp = power.Item2.Tool_Temp;
                                if (_config.isBonding_Method)
                                    data.Bonding_Method = power.Item2.Bonding_Method;
                                if (_config.isRC1)
                                    data.RC1 = power.Item2.RC1;
                                if (_config.isRC2)
                                    data.RC2 = power.Item2.RC2;
                                #endregion
                                #region 班次
                                data.shift = "";
                                var time1 = shiftnow.Date.AddHours(8);
                                var time2 = shiftnow.Date.AddHours(20);

                                if (shiftnow >= time1 && shiftnow < time2)
                                    data.shift = shiftnow.Day.ToString() + "-D";
                                if (shiftnow < time1)
                                    data.shift = shiftnow.AddDays(-1).Day.ToString() + "-N";
                                if (shiftnow >= time2)
                                    data.shift = shiftnow.Day.ToString() + "-D";
                                #endregion
                                rt.data = data;
                                var jsonSetting = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
                                var json = JsonConvert.SerializeObject(rt, Formatting.Indented, jsonSetting);
                                JsonHelper.WriteJsonFile($"./Hive/Json/MachineData/{DateTime.Now:yyyyMMddhhmmss.fffffff}+A.json", json);
                                TAG_OK?.Write((bool)true);
                            }
                            catch (Exception ex)
                            {
                                logger.Log(ex.Message, Category.Info, Priority.None);
                            }
                        }
                    }
                    Thread.Sleep(100);
                }
            }, TaskCreationOptions.LongRunning);//A轴数据上传
            Task.Factory.StartNew(() => {
                var TAG = server[GetTagName(TagNames.Save2)];
                var TAG_OK = server[GetTagName(TagNames.SaveOK2)];
                while (true)
                {
                    if (TAG != null)
                    {
                        edges[1].CurrentValue = TAG.ToString();
                        if (edges[1].ValueChanged && edges[1].CurrentValue == "True")
                        {
                            TAG_OK?.Write((bool)false);
                            Thread.Sleep(200);
                            try
                            {
                                var shiftnow = DateTime.Now;
                                var power = GetPowerRecord();
                                MachineData rt = new MachineData();
                                rt.sequence = 2;
                                rt.unit_sn = System.Guid.NewGuid().ToString() + "-B";
                                snB = rt.unit_sn;
                                MachineData.Serials serials = new MachineData.Serials();
                                rt.serials = serials;
                                rt.pass = "true";
                                try
                                {
                                    //string Tag_indate = server[GetTagName(TagNames.INDATEB)].ToString();
                                    //Tag_indate = Tag_indate.Insert(4, "/").Insert(7, "/");
                                    //string Tag_outdate = server[GetTagName(TagNames.OUTDATEB)].ToString();
                                    //Tag_outdate = Tag_outdate.Insert(4, "/").Insert(7, "/");

                                    string Tag_intime = server[GetTagName(TagNames.INTIMEB)].ToString();
                                    Tag_intime = Tag_intime.Substring(1).Insert(2, ":").Insert(5, ":");
                                    string Tag_outime = server[GetTagName(TagNames.OUTTIMEB)].ToString();
                                    Tag_outime = Tag_outime.Substring(1).Insert(2, ":").Insert(5, ":");
                                    //var input_tmie = DateTime.Parse(Tag_indate+" "+Tag_intime);
                                    //var output_tmie = DateTime.Parse(Tag_outdate+" "+Tag_outime);
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
                                        if (interval > 0 && interval <= 120)
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
                                #region Data
                                data.Spindle = "B";
                                if (_config.isFA1)
                                    data.CTQ_FAI1 = _config.FA1_B;
                                if (_config.isFA2)
                                    data.CTQ_FAI2 = _config.FA2_B;
                                if (_config.isFA3)
                                    data.CTQ_FAI3 = _config.FA3_B;
                                if (_config.isFA4)
                                    data.CTQ_FAI4 = _config.FA4_B;
                                if (_config.isFA5)
                                    data.CTQ_FAI5 = _config.FA5_B;
                                if (_config.isFA6)
                                    data.CTQ_FAI6 = _config.FA6_B;
                                if (_config.isFA7)
                                    data.CTQ_FAI7 = _config.FA7_B;
                                if (_config.isFA8)
                                    data.CTQ_FAI8 = _config.FA8_B;
                                if (_config.isFA9)
                                    data.CTQ_FAI9 = _config.FA9_B;
                                if (_config.isFA10)
                                    data.CTQ_FAI10 = _config.FA10_B;
                                if (_config.isFA11)
                                    data.CTQ_FAI11 = _config.FA11_B;
                                if (_config.isFA12)
                                    data.CTQ_FAI12 = _config.FA12_B;
                                if (_config.isFA13)
                                    data.CTQ_FAI13 = _config.FA13_B;
                                if (_config.isFA14)
                                    data.CTQ_FAI14 = _config.FA14_B;
                                if (_config.isFA15)
                                    data.CTQ_FAI15 = _config.FA15_B;
                                if (_config.isFA16)
                                    data.CTQ_FAI16 = _config.FA16_B;
                                if (_config.isFA17)
                                    data.CTQ_FAI17 = _config.FA17_B;
                                if (_config.isFA18)
                                    data.CTQ_FAI18 = _config.FA18_B;
                                if (_config.isFA19)
                                    data.CTQ_FAI19 = _config.FA19_B;
                                if (_config.isFA20)
                                    data.CTQ_FAI20 = _config.FA20_B;
                                if (_config.isFA21)
                                    data.CTQ_FAI21 = _config.FA21_B;
                                if (_config.isFA22)
                                    data.CTQ_FAI22 = _config.FA22_B;
                                if (_config.isFA23)
                                    data.CTQ_FAI23 = _config.FA23_B;
                                if (_config.isFA24)
                                    data.CTQ_FAI24 = _config.FA24_B;
                                if (_config.isFA25)
                                    data.CTQ_FAI25 = _config.FA25_B;
                                if (_config.isParallelism)
                                    data.Parallelism_between_upper_and_lower_mandrel = _config.Parallelism_B;
                                if (_config.isBendingPin)
                                    data.Bending_Pin_Position = _config.BendingPin_B;
                                if (_config.isUsageC && server[GetTagName(TagNames.FMB)] != null)
                                    data.Tool_cycles = server[GetTagName(TagNames.FMB)].Value.Int32.ToString();
                                if (_config.isUsageM && server[GetTagName(TagNames.PMB)] != null)
                                    data.PM_cycles = server[GetTagName(TagNames.PMB)].Value.Int16.ToString();
                                if (_config.isOD)
                                    data.Wire_OD = _config.OD_B;
                                if (_config.isLower && server[GetTagName(TagNames.LMTB)] != null)
                                    data.Lower_Mandrel_temperature = (server[GetTagName(TagNames.LMTB)].Value.Int32 / 10.0).ToString("F1");
                                if (_config.isWireTen)
                                {
                                    data.Wire_Tension1 = _config.WireTen_B1;
                                    data.Wire_Tension2 = _config.WireTen_B2;
                                    data.Wire_Tension3 = _config.WireTen_B3;
                                    data.Wire_Tension4 = _config.WireTen_B4;
                                    data.Wire_Tension5 = _config.WireTen_B5;
                                    data.Wire_Tension6 = _config.WireTen_B6;
                                    data.Wire_Tension7 = _config.WireTen_B7;

                                }
                                if (_config.isGrap1 && server[GetTagName(TagNames.MG1B)] != null)
                                    data.Mandrel_gap1 = (server[GetTagName(TagNames.MG1B)].Value.Int32 / 10000.0).ToString("F4");
                                if (_config.isGrap2 && server[GetTagName(TagNames.MG2B)] != null)
                                    data.Mandrel_gap2 = (server[GetTagName(TagNames.MG2B)].Value.Int32 / 10000.0).ToString("F4");
                                if (_config.isGrap3 && server[GetTagName(TagNames.MG3B)] != null)
                                    data.Mandrel_gap3 = (server[GetTagName(TagNames.MG3B)].Value.Int32 / 10000.0).ToString("F4");
                                if (_config.isWspeed1 && server[GetTagName(TagNames.WS1B)] != null)
                                    data.Winding_speed1 = (server[GetTagName(TagNames.WS1B)].Value.Int32 / 100.0).ToString("F2");
                                if (_config.isWspeed2 && server[GetTagName(TagNames.WS2B)] != null)
                                    data.Winding_speed2 = (server[GetTagName(TagNames.WS2B)].Value.Int32 / 100.0).ToString("F2");
                                if (_config.isWspeed3 && server[GetTagName(TagNames.WS3B)] != null)
                                    data.Winding_speed3 = (server[GetTagName(TagNames.WS3B)].Value.Int32 / 100.0).ToString("F2");
                                if (_config.isWspeedB && server[GetTagName(TagNames.WSBB)] != null)
                                    data.Bending_speed = (server[GetTagName(TagNames.WSBB)].Value.Int32 / 100.0).ToString("F2");
                                if (_config.isIspeed && server[GetTagName(TagNames.ISB)] != null)
                                    data.Iron_speed = (server[GetTagName(TagNames.ISB)].Value.Int32 / 10000.0).ToString("F4");
                                data.sw_version = _sw_version;
                                #endregion
                                #region PowerData
                                if (_config.isRstart)
                                    data.Rstart = power.Item2.Rstart;
                                if (_config.isCurrent)
                                    data.Current = power.Item2.Current;
                                if (_config.isVoltage)
                                    data.Voltage = power.Item2.Voltage;
                                if (_config.isPower)
                                    data.Power = power.Item2.Power;
                                if (_config.isRend)
                                    data.Rend = power.Item2.Rend;
                                if (_config.isBonding_Temp)
                                    data.Bonding_Temp = power.Item2.Bonding_Temp;
                                if (_config.isBonding_Time)
                                    data.Bonding_Time = power.Item2.Bonding_Time;
                                if (_config.isTool_Temp)
                                    data.Tool_Temp = power.Item2.Tool_Temp;
                                if (_config.isBonding_Method)
                                    data.Bonding_Method = power.Item2.Bonding_Method;
                                if (_config.isRC1)
                                    data.RC1 = power.Item2.RC1;
                                if (_config.isRC2)
                                    data.RC2 = power.Item2.RC2;
                                #endregion
                                #region 班次
                                data.shift = "";
                                var time1 = shiftnow.Date.AddHours(8);
                                var time2 = shiftnow.Date.AddHours(20);

                                if (shiftnow >= time1 && shiftnow < time2)
                                    data.shift = shiftnow.Day.ToString() + "-D";
                                if (shiftnow < time1)
                                    data.shift = shiftnow.AddDays(-1).Day.ToString() + "-N";
                                if (shiftnow >= time2)
                                    data.shift = shiftnow.Day.ToString() + "-D";
                                #endregion
                                rt.data = data;
                                TAG_OK?.Write((bool)true);
                                var jsonSetting = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
                                var json = JsonConvert.SerializeObject(rt, Formatting.Indented, jsonSetting);
                                JsonHelper.WriteJsonFile($"./Hive/Json/MachineData/{DateTime.Now:yyyyMMddhhmmss.fffffff}+B.json", json);
                            }
                            catch (Exception ex)
                            {
                                logger.Log(ex.Message, Category.Info, Priority.None);
                            }

                        }
                    }
                    Thread.Sleep(100);
                }
            }, TaskCreationOptions.LongRunning);//B轴数据上传


            Task.Factory.StartNew(() => {
                var TAG = server[GetTagName(TagNames.State)];
                var TAG_StateA = server[GetTagName(TagNames.StateA)];
                var TAG_StateB = server[GetTagName(TagNames.StateB)];
                while (true)
                {
                    if (TAG != null)
                    {
                        edges[2].CurrentValue = TAG.ToString();
                        if (edges[2].ValueChanged && edges[2].CurrentValue != "0")
                        {
                            var nowtime = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.ffzz00");
                            MachineState rt = new MachineState();
                            MachineState.Data dt = new MachineState.Data();

                            if (TAG_StateA != null)
                                dt.State_A = TAG_StateA.ToString();
                            if (TAG_StateB != null)
                                dt.State_B = TAG_StateB.ToString();
                            #region 状态切换成uDT5后遍寻所有报警信息,只能从状态Runing1切换成uDT5,报警或按下停止按钮
                            if (edges[2].CurrentValue == "5")
                            {
                                Thread.Sleep(1000);
                                occurrence_time = nowtime;
                                code = "O99EECE-01-03";
                                message = "The stop button is pressed";
                                if (alarmGroup != null)
                                {
                                    foreach (var item in alarmGroup.Items)
                                    {
                                        if (item.GetMetaData().Severity.ToString() != "local" && item.Value.Boolean == true)
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
                                if (edges[2].OldValue == "0")
                                    dt.state_change_reason = "User open hive software";
                            }
                            #endregion

                            #region 状态切换成Runing1,只能从状态IDLE2切换成Runing1，按下开始按钮
                            if (edges[2].CurrentValue == "1")
                            {
                                dt.previous_state = "2";
                                dt.sw_version = _sw_version;
                                if (edges[2].OldValue == "0")
                                    dt.state_change_reason = "User open hive software";
                                else
                                    dt.state_change_reason = "User pressed start button";
                            }
                            #endregion

                            #region 状态切换成Engineering3,只能从IDLE2切入，按下工程师按钮
                            if (edges[2].CurrentValue == "3")
                            {
                                dt.previous_state = "2";
                                if (edges[2].OldValue == "0")
                                    dt.state_change_reason = "User open hive software";
                                else
                                    dt.state_change_reason = "User pressed Engineering button";
                            }
                            #endregion

                            #region 状态切换成pDT4,只能从状态IDLE2切入，按下维修按钮
                            if (edges[2].CurrentValue == "4")
                            {
                                dt.previous_state = "2";
                                if (edges[2].OldValue == "0")
                                    dt.state_change_reason = "User open hive software";
                                else
                                    dt.state_change_reason = "User pressed Maintenance button";
                            }
                            #endregion

                            #region 状态切换成IDLE2,其他任意状态都可以切入，查询上次状态，如果是上次是pDT4状态(遍寻修改的维修内容)
                            if (edges[2].CurrentValue == "2")
                            {
                                if (edges[2].OldValue == "1")
                                {
                                    dt.previous_state = "1";
                                    dt.state_change_reason = "User pressed pause button";
                                }
                                else if (edges[2].OldValue == "3")
                                {
                                    dt.previous_state = "3";
                                    dt.state_change_reason = "User pressed Exit button";
                                }
                                else if (edges[2].OldValue == "4")
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
                                else if (edges[2].OldValue == "5")
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
                                    //JsonHelper.WriteJsonFile($"./Hive/Json/ErrorData/{DateTime.Now:yyyyMMddhhmmss.fffffff}+{code}.json", error_json);
                                    #endregion
                                }
                                else
                                {
                                    dt.previous_state = "5";
                                    dt.state_change_reason = "User open hive software";
                                }
                            }
                            #endregion
                            rt.machine_state = edges[2].CurrentValue;
                            rt.state_change_time = nowtime;
                            rt.data = dt;
                            var jsonSetting = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
                            var json = JsonConvert.SerializeObject(rt, Formatting.Indented, jsonSetting);
                            //JsonHelper.WriteJsonFile($"./Hive/Json/StateData/{DateTime.Now:yyyyMMddhhmmss.fffffff}.json", json);

                        }
                    }
                    Thread.Sleep(100);
                }
            }, TaskCreationOptions.LongRunning);//设备总状态上传


            Task.Factory.StartNew(() => {
                var TAG_State = server[GetTagName(TagNames.State)];
                var TAG_StateA = server[GetTagName(TagNames.StateA)];
                var TAG_StateB = server[GetTagName(TagNames.StateB)];
                while (true)
                {
                    if (TAG_StateA != null && TAG_State != null && TAG_StateB != null)
                    {
                        edges[4].CurrentValue = TAG_StateA.ToString();
                        if (edges[4].ValueChanged && edges[4].CurrentValue != "0")
                        {
                            var nowtime = DateTime.Now;
                            MachineState rt = new MachineState();
                            rt.sequence = 1;
                            MachineState.Data dt = new MachineState.Data();
                            if (edges[4].CurrentValue == "5")
                            {
                                #region A轴报警索引
                                codeA = "O99EECE-01-06";
                                messageA = "Not retrieve A-Axis error list";
                                severityA = "critical";
                                occurrence_timeA = nowtime;
                                if (alarmGroup != null)
                                {
                                    foreach (var item in alarmGroup.Items)
                                    {
                                        if (item.GetMetaData().Severity.ToString() != "local" && item.Value.Boolean == true && item.GetMetaData().Axis.ToString() != "B")
                                        {
                                            messageA = item.GetMetaData().Description;
                                            codeA = item.GetMetaData().Code;
                                            severityA = item.GetMetaData().Severity.ToString();
                                        }
                                    }
                                }
                                #endregion
                                dt.code = codeA;
                                dt.error_message = messageA;
                                dt.previous_state = edges[4].OldValue;
                                if (codeA.Contains("O99EECE-01-03"))
                                    dt.state_change_reason = "User pressed stop button";
                                else
                                    dt.state_change_reason = "Machine alarm with description";
                                if (edges[4].OldValue == "0")
                                    dt.state_change_reason = "User open hive software";
                            }
                            else
                            {
                                #region 状态切换成Runing1,只能从状态IDLE2切换成Runing1，按下开始按钮
                                if (edges[4].CurrentValue == "1")
                                {
                                    dt.previous_state = edges[4].OldValue;
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
                                    dt.previous_state = edges[4].OldValue;
                                    if (edges[4].OldValue == "0")
                                        dt.state_change_reason = "User open hive software";
                                    else
                                        dt.state_change_reason = "User pressed Engineering button";
                                }
                                #endregion
                                #region 状态切换成pDT4,只能从状态IDLE2切入，按下维修按钮
                                if (edges[4].CurrentValue == "4")
                                {
                                    dt.previous_state = edges[4].OldValue;
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
                                    }
                                    else
                                    {
                                        dt.previous_state = "5";
                                        dt.state_change_reason = "User open hive software";
                                    }
                                }
                                #endregion
                                if (edges[4].OldValue == "5")
                                {
                                    #region A轴报警生成
                                    resolved_timeA = nowtime;
                                    ErrorData machineerror = new ErrorData();
                                    machineerror.sequence = 1;
                                    machineerror.code = "O99EECE-01-06";
                                    machineerror.message = "Not retrieve A-Axis error list";
                                    machineerror.severity = "critical";
                                    machineerror.occurrence_time = occurrence_timeA.ToString("yyyy-MM-ddTHH:mm:ss.ffzz00");
                                    //判断当前的状态是否为IDLE2
                                    if (edges[4].CurrentValue == "2")
                                    {
                                        machineerror.resolved_time = nowtime.ToString("yyyy-MM-ddTHH:mm:ss.ffzz00");
                                    }
                                    else//当前状态不为待机的状态，则补状态为待机的状态
                                    {
                                        machineerror.resolved_time = nowtime.AddMilliseconds(-10).ToString("yyyy-MM-ddTHH:mm:ss.ffzz00");
                                        MachineState rt2 = new MachineState();
                                        MachineState.Data dt2 = new MachineState.Data();
                                        dt2.previous_state = "5";
                                        dt2.state_change_reason = "User pressed reset button";
                                        rt2.machine_state = "2";
                                        rt2.sequence = 1;
                                        rt2.state_change_time = nowtime.AddMilliseconds(-10).ToString("yyyy-MM-ddTHH:mm:ss.ffzz00");
                                        rt2.data = dt2;
                                        var jsonSetting2 = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
                                        var json2 = JsonConvert.SerializeObject(rt2, Formatting.Indented, jsonSetting2);
                                        JsonHelper.WriteJsonFile($"./Hive/Json/StateData/{nowtime.AddMilliseconds(-10):yyyyMMddhhmmss.fffffff}A.json", json2);
                                    }
                                    if (!string.IsNullOrEmpty(messageA) && !string.IsNullOrEmpty(codeA) && !string.IsNullOrEmpty(severityA))
                                    {
                                        machineerror.message = messageA;
                                        machineerror.code = codeA;
                                        machineerror.severity = severityA;
                                    }//无数据丢失，则上传轮询到的报警
                                    try
                                    {
                                        var a = (nowtime - occurrence_timeA).TotalSeconds;
                                        if (a <= 0)
                                            machineerror.occurrence_time = nowtime.AddSeconds(-10).ToString("yyyy-MM-ddTHH:mm:ss.ffzz00");
                                    }
                                    catch
                                    {
                                        machineerror.occurrence_time = nowtime.AddSeconds(-10).ToString("yyyy-MM-ddTHH:mm:ss.ffzz00");
                                    }
                                    messageA = "";
                                    codeA = "";
                                    severityA = "";
                                    ErrorData.Data error_dt = new ErrorData.Data();
                                    machineerror.data = error_dt;
                                    var error_jsonSetting = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
                                    var error_json = JsonConvert.SerializeObject(machineerror, Formatting.Indented, error_jsonSetting);
                                    JsonHelper.WriteJsonFile($"./Hive/Json/ErrorData/{DateTime.Now:yyyyMMddhhmmss.fffffff}+{code}A.json", error_json);
                                    #endregion

                                }
                            }

                            dt.State_A = TAG_StateA.ToString();
                            dt.State_A_change_time = nowtime.ToString("yyyy-MM-ddTHH:mm:ss.ffzz00");
                            dt.State_B = TAG_StateB.ToString();
                            rt.machine_state = edges[4].CurrentValue;
                            rt.state_change_time = nowtime.ToString("yyyy-MM-ddTHH:mm:ss.ffzz00");
                            rt.data = dt;
                            var jsonSetting = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
                            var json = JsonConvert.SerializeObject(rt, Formatting.Indented, jsonSetting);
                            JsonHelper.WriteJsonFile($"./Hive/Json/StateData/{DateTime.Now:yyyyMMddhhmmss.fffffff}A.json", json);

                        }
                    }
                    Thread.Sleep(100);
                }
            }, TaskCreationOptions.LongRunning);//设备A状态上传
            Task.Factory.StartNew(() => {
                var TAG_State = server[GetTagName(TagNames.State)];
                var TAG_StateB = server[GetTagName(TagNames.StateB)];
                var TAG_StateA = server[GetTagName(TagNames.StateA)];
                while (true)
                {
                    if (TAG_StateB != null && TAG_State != null && TAG_StateA != null)
                    {
                        edges[5].CurrentValue = TAG_StateB.ToString();
                        if (edges[5].ValueChanged && edges[5].CurrentValue != "0")
                        {
                            var nowtime = DateTime.Now;
                            MachineState rt = new MachineState();
                            rt.sequence = 2;//B轴数据
                            MachineState.Data dt = new MachineState.Data();
                            if (edges[5].CurrentValue == "5")
                            {
                                #region B轴报警索引
                                codeB = "O99EECE-01-07";
                                messageB = "Not retrieve B-Axis error list";
                                severityB = "critical";
                                occurrence_timeB = nowtime;
                                if (alarmGroup != null)
                                {
                                    foreach (var item in alarmGroup.Items)
                                    {
                                        if (item.GetMetaData().Severity.ToString() != "local" && item.Value.Boolean == true && item.GetMetaData().Axis.ToString() != "A")
                                        {
                                            messageB = item.GetMetaData().Description;
                                            codeB = item.GetMetaData().Code;
                                            severityB = item.GetMetaData().Severity.ToString();
                                        }
                                    }
                                }
                                #endregion
                                dt.code = codeB;
                                dt.error_message = messageB;
                                dt.previous_state = edges[5].OldValue;
                                if (codeB.Contains("O99EECE-01-03"))
                                    dt.state_change_reason = "User pressed stop button";
                                else
                                    dt.state_change_reason = "Machine alarm with description";
                                if (edges[5].OldValue == "0")
                                    dt.state_change_reason = "User open hive software";
                            }
                            else
                            {
                                #region 状态切换成Runing1,只能从状态IDLE2切换成Runing1，按下开始按钮
                                if (edges[5].CurrentValue == "1")
                                {
                                    dt.previous_state = edges[5].OldValue;
                                    dt.sw_version = _sw_version;
                                    if (edges[5].OldValue == "0")
                                        dt.state_change_reason = "User open hive software";
                                    else
                                        dt.state_change_reason = "User pressed start button";
                                }
                                #endregion
                                #region 状态切换成Engineering3,只能从IDLE2切入，按下工程师按钮
                                if (edges[5].CurrentValue == "3")
                                {
                                    dt.previous_state = edges[5].OldValue;
                                    if (edges[5].OldValue == "0")
                                        dt.state_change_reason = "User open hive software";
                                    else
                                        dt.state_change_reason = "User pressed Engineering button";
                                }
                                #endregion
                                #region 状态切换成pDT4,只能从状态IDLE2切入，按下维修按钮
                                if (edges[5].CurrentValue == "4")
                                {
                                    dt.previous_state = edges[5].OldValue;
                                    if (edges[5].OldValue == "0")
                                        dt.state_change_reason = "User open hive software";
                                    else
                                        dt.state_change_reason = "User pressed Maintenance button";
                                }
                                #endregion
                                #region 状态切换成IDLE2,其他任意状态都可以切入，查询上次状态，如果是上次是pDT4状态(遍寻修改的维修内容)
                                if (edges[5].CurrentValue == "2")
                                {
                                    if (edges[5].OldValue == "1")
                                    {
                                        dt.previous_state = "1";
                                        dt.state_change_reason = "User pressed pause button";
                                    }
                                    else if (edges[5].OldValue == "3")
                                    {
                                        dt.previous_state = "3";
                                        dt.state_change_reason = "User pressed Exit button";
                                    }
                                    else if (edges[5].OldValue == "4")
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
                                    else if (edges[5].OldValue == "5")
                                    {
                                        dt.previous_state = "5";
                                        dt.state_change_reason = "User pressed reset button";
                                    }
                                    else
                                    {
                                        dt.previous_state = "5";
                                        dt.state_change_reason = "User open hive software";
                                    }
                                }
                                #endregion
                                if (edges[5].OldValue == "5")
                                {
                                    #region B轴报警生成
                                    resolved_timeB = nowtime;
                                    ErrorData machineerror = new ErrorData();
                                    machineerror.sequence = 2;
                                    machineerror.code = "O99EECE-01-07";
                                    machineerror.message = "Not retrieve B-Axis error list";
                                    machineerror.severity = "critical";
                                    machineerror.occurrence_time = occurrence_timeB.ToString("yyyy-MM-ddTHH:mm:ss.ffzz00");
                                    //判断当前的状态是否为IDLE2
                                    if (edges[5].CurrentValue == "2")
                                    {
                                        machineerror.resolved_time = nowtime.ToString("yyyy-MM-ddTHH:mm:ss.ffzz00");
                                    }
                                    else//当前状态不为待机的状态，则补状态为待机的状态
                                    {
                                        machineerror.resolved_time = nowtime.AddMilliseconds(-10).ToString("yyyy-MM-ddTHH:mm:ss.ffzz00");
                                        MachineState rt2 = new MachineState();
                                        MachineState.Data dt2 = new MachineState.Data();
                                        dt2.previous_state = "5";
                                        dt2.state_change_reason = "User pressed reset button";
                                        rt2.machine_state = "2";
                                        rt2.sequence = 2;
                                        rt2.state_change_time = nowtime.AddMilliseconds(-10).ToString("yyyy-MM-ddTHH:mm:ss.ffzz00");
                                        rt2.data = dt2;
                                        var jsonSetting2 = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
                                        var json2 = JsonConvert.SerializeObject(rt2, Formatting.Indented, jsonSetting2);
                                        JsonHelper.WriteJsonFile($"./Hive/Json/StateData/{nowtime.AddMilliseconds(-10):yyyyMMddhhmmss.fffffff}B.json", json2);
                                    }
                                    if (!string.IsNullOrEmpty(messageB) && !string.IsNullOrEmpty(codeB) && !string.IsNullOrEmpty(severityB))
                                    {
                                        machineerror.message = messageB;
                                        machineerror.code = codeB;
                                        machineerror.severity = severityB;
                                    }//无数据丢失，则上传轮询到的报警
                                    try
                                    {
                                        var a = (nowtime - occurrence_timeB).TotalSeconds;
                                        if (a <= 0)
                                            machineerror.occurrence_time = nowtime.AddSeconds(-10).ToString("yyyy-MM-ddTHH:mm:ss.ffzz00");
                                    }
                                    catch
                                    {
                                        machineerror.occurrence_time = nowtime.AddSeconds(-10).ToString("yyyy-MM-ddTHH:mm:ss.ffzz00");
                                    }
                                    messageB = "";
                                    codeB = "";
                                    severityB = "";
                                    ErrorData.Data error_dt = new ErrorData.Data();
                                    machineerror.data = error_dt;
                                    var error_jsonSetting = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
                                    var error_json = JsonConvert.SerializeObject(machineerror, Formatting.Indented, error_jsonSetting);
                                    JsonHelper.WriteJsonFile($"./Hive/Json/ErrorData/{DateTime.Now:yyyyMMddhhmmss.fffffff}+{code}B.json", error_json);
                                    #endregion

                                }
                            }
                            dt.State_B = TAG_StateB.ToString();
                            dt.State_B_change_time = nowtime.ToString("yyyy-MM-ddTHH:mm:ss.ffzz00");
                            dt.State_A = TAG_StateA.ToString();
                            rt.machine_state = edges[5].CurrentValue;
                            rt.state_change_time = nowtime.ToString("yyyy-MM-ddTHH:mm:ss.ffzz00");
                            rt.data = dt;
                            var jsonSetting = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
                            var json = JsonConvert.SerializeObject(rt, Formatting.Indented, jsonSetting);
                            JsonHelper.WriteJsonFile($"./Hive/Json/StateData/{DateTime.Now:yyyyMMddhhmmss.fffffff}B.json", json);

                        }
                    }
                    Thread.Sleep(100);
                }
            }, TaskCreationOptions.LongRunning);//设备B状态上传

            Task.Factory.StartNew(() => {
                while (true)
                {
                    var TAG = server[GetTagName(TagNames.GetCT1)];
                    if (TAG != null)
                    {
                        edges[6].CurrentValue = TAG.ToString();
                        if (edges[6].ValueChanged && edges[6].CurrentValue == "1")
                        {
                            Dictionary<string, string> dictionary = new Dictionary<string, string>();
                            dictionary["Time"] = DateTime.Now.ToString("yyyy/MM/ddTHH:mm:ss:ff");
                            dictionary["SN"] = snA;
                            foreach (var item in PARASGroup.Items)
                            {
                                if (item.GetMetaData().Code == "CT1")
                                {
                                    if(item.GetMetaData().DataType == DataType.SHORT)
                                        dictionary[item.GetTagName()] = (item.Value.Int16 / 10.0).ToString("F1");
                                    else
                                        dictionary[item.GetTagName()] = item.ToString();

                                }
                            }
                           bool res=CsvHelper.WriteCsv($"./RunLog/CT/{DateTime.Now:yyyyMM}/A/{DateTime.Now:yyyyMMdd}.csv", dictionary);
                            if(res)
                                TAG?.Write((short)11);
                            else
                                TAG?.Write((short)12);
                        }
                    }
                    Thread.Sleep(200);
                }
            }, TaskCreationOptions.LongRunning);//A轴CT时间采集(傻逼客户天天改,加奇葩功能，程序都成一坨屎了)
            Task.Factory.StartNew(() => {
                while (true)
                {
                    var TAG = server[GetTagName(TagNames.GetCT2)];
                    if (TAG != null)
                    {
                        edges[7].CurrentValue = TAG.ToString();
                        if (edges[7].ValueChanged && edges[7].CurrentValue == "1")
                        {

                            Dictionary<string, string> dictionary = new Dictionary<string, string>();
                            dictionary["Time"] = DateTime.Now.ToString("yyyy/MM/ddTHH:mm:ss:ff");
                            dictionary["SN"] = snB;
                            foreach (var item in PARASGroup.Items)
                            {
                                if (item.GetMetaData().Code == "CT2")
                                {
                                    if(item.GetMetaData().DataType==DataType.SHORT)
                                        dictionary[item.GetTagName()] =(item.Value.Int16/10.0).ToString("F1");
                                    else
                                        dictionary[item.GetTagName()] = item.ToString();
                                }
                            }
                            bool res=CsvHelper.WriteCsv($"./RunLog/CT/{DateTime.Now:yyyyMM}/B/{DateTime.Now:yyyyMMdd}.csv", dictionary);
                            if (res)
                                TAG?.Write((short)11);
                            else
                                TAG?.Write((short)12);
                        }
                    }
                    Thread.Sleep(200);
                }
            }, TaskCreationOptions.LongRunning);//B轴CT时间采集

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
                var name = tag.GetMetaData().Name;
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
                if (!value1)
                {
                      Dictionary<string, string> dictionary = new Dictionary<string, string>();
                      dictionary["Time"] = DateTime.Now.ToString("yyyy/MM/ddTHH:mm:ss:ff");
                      dictionary["message"] = message;
                      dictionary["severity"] = severity;
                      dictionary["code"] = code;
                      dictionary["occurrence_time"] = oldtimestamp;
                      dictionary["resolved_time"] = timeStamp;
                      CsvHelper.WriteCsv($"./RunLog/alarms/{DateTime.Now:yyyyMM}/{DateTime.Now:yyyyMMdd}.csv", dictionary);
                }
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
        private (bool, PowerInfo) GetPowerRecord()
        {
            try
            {
                var powerInfo = new PowerInfo();
                FileStream fs = new FileStream($"C:/SCHLEICH/CSV/{DateTime.Today:yyyy_MM_dd}.csv", FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using var reader = new StreamReader(fs, encoding: Encoding.UTF8);
                using var csv = new CsvReader(reader, CultureInfo.CurrentCulture);
                var records = csv.GetRecords<PowerRecord>()
                .Where(a => !string.IsNullOrEmpty(a.Station))
                .Where(b => !string.IsNullOrEmpty(b.Date))
                .Where(b => !string.IsNullOrEmpty(b.Time))
                .Where(b => !string.IsNullOrEmpty(b.Rstart))
                .Where(b => !string.IsNullOrEmpty(b.Current))
                .Where(b => !string.IsNullOrEmpty(b.Voltage))
                .Where(b => !string.IsNullOrEmpty(b.Power))
                .Where(b => !string.IsNullOrEmpty(b.Rend))
                .Where(b => !string.IsNullOrEmpty(b.Bonding_Temp))
                .Where(b => !string.IsNullOrEmpty(b.Bonding_Time))
                .Where(b => !string.IsNullOrEmpty(b.Tool_Temp))
                .Where(b => !string.IsNullOrEmpty(b.Bonding_Method))
                .Where(b => !string.IsNullOrEmpty(b.RC1))
                .Where(b => !string.IsNullOrEmpty(b.RC2))
                .Where(b => !string.IsNullOrEmpty(b.Result))
                .Distinct().ToList();
                var count = records.Count - 1;
                var a = (DateTime.Now - DateTime.Parse(records[count].Time)).TotalSeconds;
                if (a >= 0.0 && a <= 5.0)
                {
                    powerInfo.Station = records[count].Station;
                    powerInfo.Date = records[count].Date;
                    powerInfo.Time = records[count].Time;
                    powerInfo.Rstart = records[count].Rstart;
                    powerInfo.Current = records[count].Current;
                    powerInfo.Voltage = records[count].Voltage;
                    powerInfo.Power = records[count].Power;
                    powerInfo.Rend = records[count].Rend;
                    powerInfo.Bonding_Temp = records[count].Bonding_Temp;
                    powerInfo.Bonding_Time = records[count].Bonding_Time;
                    powerInfo.Tool_Temp = records[count].Tool_Temp;
                    powerInfo.Bonding_Method = records[count].Bonding_Method;
                    powerInfo.RC1 = records[count].RC1;
                    powerInfo.RC2 = records[count].RC2;
                    powerInfo.Result = records[count].Result;
                }
                else
                {
                    powerInfo.Station = "N/A";
                    powerInfo.Date = "N/A";
                    powerInfo.Time = "N/A";
                    powerInfo.Rstart = "N/A";
                    powerInfo.Current = "N/A";
                    powerInfo.Voltage = "N/A";
                    powerInfo.Power = "N/A";
                    powerInfo.Rend = "N/A";
                    powerInfo.Bonding_Temp = "N/A";
                    powerInfo.Bonding_Time = "N/A";
                    powerInfo.Tool_Temp = "N/A";
                    powerInfo.Bonding_Method = "N/A";
                    powerInfo.RC1 = "N/A";
                    powerInfo.RC2 = "N/A";
                    powerInfo.Result = "N/A";
                }
                return (true, powerInfo);
            }
            catch (Exception ex)
            {
                logger.Log(ex.Message + Environment.NewLine + ex.StackTrace, Category.Exception, Priority.None);
                var ngpowerInfo = new PowerInfo();
                ngpowerInfo.Station = "N/A";
                ngpowerInfo.Date = "N/A";
                ngpowerInfo.Time = "N/A";
                ngpowerInfo.Rstart = "N/A";
                ngpowerInfo.Current = "N/A";
                ngpowerInfo.Voltage = "N/A";
                ngpowerInfo.Power = "N/A";
                ngpowerInfo.Rend = "N/A";
                ngpowerInfo.Bonding_Temp = "N/A";
                ngpowerInfo.Bonding_Time = "N/A";
                ngpowerInfo.Tool_Temp = "N/A";
                ngpowerInfo.Bonding_Method = "N/A";
                ngpowerInfo.RC1 = "N/A";
                ngpowerInfo.RC2 = "N/A";
                ngpowerInfo.Result = "N/A";
                return (false, ngpowerInfo);
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
