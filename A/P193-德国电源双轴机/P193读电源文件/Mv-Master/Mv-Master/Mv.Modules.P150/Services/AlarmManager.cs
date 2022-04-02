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
using NPOI.HSSF.UserModel;//引用NPOI的dll
using NPOI.SS.UserModel;
using NPOI.DDF;



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
        [Index(16)]
        public string TimeStamp { get; set; }
        [Index(17)]
        public string SetValue_WireDiameter { get; set; }
        [Index(18)]
        public string SetValue_NoParallWires { get; set; }
        [Index(19)]
        public string SetValue_CurrentDensity { get; set; }
        [Index(20)]
        public string SetValue_BondingTemperature { get; set; }


    }
    public class PowerInfo
    {
        public string Station { get; set; } = "N/A";
        public string Date { get; set; } = "N/A";
        public string Time { get; set; } = "N/A";
        public double Rstart { get; set; }
        public double Current { get; set; }
        public double Voltage { get; set; }
        public double Power { get; set; }
        public double Rend { get; set; }
        public double Bonding_Temp { get; set; }
        public double Bonding_Time { get; set; }
        public double Tool_Temp { get; set; }
        public string Bonding_Method { get; set; } = "N/A";
        public double RC1 { get; set; }
        public double RC2 { get; set; }
        public string Result { get; set; } = "N/A";
        public DateTime TimeStamp { get; set; }
        public double SetValue_WireDiameter { get; set; }
        public double SetValue_NoParallWires { get; set; }
        public double SetValue_CurrentDensity { get; set; }
        public double SetValue_BondingTemperature { get; set; }
    }
    public interface IAlarmManager
    { }
    public class AlarmManager : IAlarmManager, IDisposable
    {

        private readonly IDataServer server;
        private readonly IEventAggregator @event;
        private readonly ILoggerFacade logger;
        private readonly IConfigureFile _configure;
        private readonly string _sw_version = "V1.1.17";
        private TraceConfig _config;
        private bool isError;
        private string message = "";
        private string code = "";
        private string severity = "";
        private string occurrence_time = "";

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


        private Edge[] edges = new Edge[7] { new Edge(), new Edge(), new Edge(), new Edge(), new Edge(), new Edge(), new Edge() };
        public AlarmManager(IDataServer server, IEventAggregator @event, ILoggerFacade logger, IConfigureFile configure)
        {
            this.server = server;
            this.@event = @event;
            this.logger = logger;
            this._configure = configure;
            this._configure.ValueChanged += _configure_ValueChanged;
            _config = _configure.GetValue<TraceConfig>("TraceConfig");
            var alarmGroup = server.GetGroupByName("alarms");//PLC报警
            var PARASGroup = server.GetGroupByName("PARAS");//PLC数据
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
                                var power = GetPowerRecord();
                                if (power.Item1)
                                    SaveParameterData(false, power.Item2);
                                MachineData rt = new MachineData();
                                rt.unit_sn = System.Guid.NewGuid().ToString() + "-A";
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
                                    data.Rstart = power.Item2.Rstart.ToString();
                                if (_config.isCurrent)
                                    data.Current = power.Item2.Current.ToString();
                                if (_config.isVoltage)
                                    data.Voltage = power.Item2.Voltage.ToString();
                                if (_config.isPower)
                                    data.Power = power.Item2.Power.ToString();
                                if (_config.isRend)
                                    data.Rend = power.Item2.Rend.ToString();
                                if (_config.isBonding_Temp)
                                    data.Bonding_Temp = power.Item2.Bonding_Temp.ToString();
                                if (_config.isBonding_Time)
                                    data.Bonding_Time = power.Item2.Bonding_Time.ToString();
                                if (_config.isTool_Temp)
                                    data.Tool_Temp = power.Item2.Tool_Temp.ToString();
                                if (_config.isBonding_Method)
                                    data.Bonding_Method = power.Item2.Bonding_Method;
                                if (_config.isRC1)
                                    data.RC1 = power.Item2.RC1.ToString();
                                if (_config.isRC2)
                                    data.RC2 = power.Item2.RC2.ToString();
                                #endregion
                                rt.data = data;
                                var jsonSetting = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
                                var json = JsonConvert.SerializeObject(rt, Formatting.Indented, jsonSetting);
                                JsonHelper.WriteJsonFile($"./Hive/Json/MachineData/{DateTime.Now:yyyyMMddhhmmss.fffffff}+A.json", json);

                                #region A轴数据保存到本地
                                Dictionary<string, string> dictionary = new Dictionary<string, string>();
                                dictionary["Unit_Sn"] = rt.unit_sn;
                                dictionary["Input_Time"] = rt.input_time;
                                dictionary["Output_Time"] = rt.output_time;
                                var savetmie = DateTime.Parse(rt.output_time);
                                CsvHelper.WriteCsv($"D:/Data/{savetmie:yyyyMM}/MachineDataA/{savetmie:yyyyMMdd}.csv", dictionary);
                                #endregion


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
                                var power = GetPowerRecord();
                                if (power.Item1)
                                    SaveParameterData(true, power.Item2);
                                MachineData rt = new MachineData();
                                rt.unit_sn = System.Guid.NewGuid().ToString() + "-B";
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
                                    data.Rstart = power.Item2.Rstart.ToString();
                                if (_config.isCurrent)
                                    data.Current = power.Item2.Current.ToString();
                                if (_config.isVoltage)
                                    data.Voltage = power.Item2.Voltage.ToString();
                                if (_config.isPower)
                                    data.Power = power.Item2.Power.ToString();
                                if (_config.isRend)
                                    data.Rend = power.Item2.Rend.ToString();
                                if (_config.isBonding_Temp)
                                    data.Bonding_Temp = power.Item2.Bonding_Temp.ToString();
                                if (_config.isBonding_Time)
                                    data.Bonding_Time = power.Item2.Bonding_Time.ToString();
                                if (_config.isTool_Temp)
                                    data.Tool_Temp = power.Item2.Tool_Temp.ToString();
                                if (_config.isBonding_Method)
                                    data.Bonding_Method = power.Item2.Bonding_Method;
                                if (_config.isRC1)
                                    data.RC1 = power.Item2.RC1.ToString();
                                if (_config.isRC2)
                                    data.RC2 = power.Item2.RC2.ToString();
                                #endregion
                                rt.data = data;
                                TAG_OK?.Write((bool)true);
                                var jsonSetting = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
                                var json = JsonConvert.SerializeObject(rt, Formatting.Indented, jsonSetting);
                                JsonHelper.WriteJsonFile($"./Hive/Json/MachineData/{DateTime.Now:yyyyMMddhhmmss.fffffff}+B.json", json);
                                #region B轴数据保存到本地
                                Dictionary<string, string> dictionary = new Dictionary<string, string>();
                                dictionary["Unit_Sn"] = rt.unit_sn;
                                dictionary["Input_Time"] = rt.input_time;
                                dictionary["Output_Time"] = rt.output_time;
                                var savetmie = DateTime.Parse(rt.output_time);
                                CsvHelper.WriteCsv($"D:/Data/{savetmie:yyyyMM}/MachineDataB/{savetmie:yyyyMMdd}.csv", dictionary);
                                #endregion

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
                var TAG_StateA= server[GetTagName(TagNames.StateA)];
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
                                        if (item.GetMetaData().Severity.ToString()!= "local" && item.Value.Boolean == true)
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
                                if(edges[2].OldValue=="0")
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
                                            if (item.Value.Boolean == true&&item.GetMetaData().Code=="PM")
                                            {
                                                dt.state_change_reason += ",";
                                                dt.state_change_reason+=item.GetMetaData().Description;
                                                i++;
                                            }
                                        }
                                    }
                                    if(i==0)
                                    {
                                        dt.state_change_reason = "Exit Maintenance,other";
                                    }
                                    #endregion
                                }
                                else if (edges[2].OldValue == "5")
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
                            #region ErrorData的生成
                            if (edges[2].OldValue == "5")
                            {
                                #region 产生报警信息
                                ErrorData machineerror = new ErrorData();
                                machineerror.message = "The stop button is pressed";
                                machineerror.code = "O99EECE-01-03";
                                machineerror.severity = "critical";
                                machineerror.occurrence_time = occurrence_time;

                                //判断当前的状态是否为IDLE2
                                if (edges[2].CurrentValue == "2")
                                {
                                    machineerror.resolved_time = nowtime;
                                }
                                else//当前状态不为待机的状态，则补状态为待机的状态
                                {
                                    machineerror.resolved_time = DateTime.Parse(nowtime).AddMilliseconds(-10).ToString("yyyy-MM-ddTHH:mm:ss.ffzz00");
                                    MachineState rt2 = new MachineState();
                                    MachineState.Data dt2 = new MachineState.Data();
                                    dt2.previous_state = "5";
                                    dt2.state_change_reason = "User pressed reset button";
                                    rt2.machine_state = "2";
                                    rt2.state_change_time = DateTime.Parse(nowtime).AddMilliseconds(-10).ToString("yyyy-MM-ddTHH:mm:ss.ffzz00");
                                    rt2.data = dt2;
                                    var jsonSetting2 = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
                                    var json2 = JsonConvert.SerializeObject(rt2, Formatting.Indented, jsonSetting2);
                                    JsonHelper.WriteJsonFile($"./Hive/Json/StateData/{DateTime.Parse(nowtime).AddMilliseconds(-10):yyyyMMddhhmmss.fffffff}.json", json2);
                                }
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
                                JsonHelper.WriteJsonFile($"./Hive/Json/ErrorData/{DateTime.Now:yyyyMMddhhmmss.fffffff}+{code}.json", error_json);
                                #endregion
                            }
                            #endregion



                            rt.machine_state = edges[2].CurrentValue;
                            rt.state_change_time = nowtime;
                            rt.data = dt;
                            var jsonSetting = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
                            var json = JsonConvert.SerializeObject(rt, Formatting.Indented, jsonSetting);
                            JsonHelper.WriteJsonFile($"./Hive/Json/StateData/{DateTime.Now:yyyyMMddhhmmss.fffffff}.json", json);

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
                    if (TAG_StateA != null&& TAG_State!= null&& TAG_StateB!=null)
                    {
                        edges[4].CurrentValue = TAG_StateA.ToString();
                        if (edges[4].ValueChanged && edges[4].CurrentValue != "0")
                        {
                            var nowtime = DateTime.Now;
                            MachineState rt = new MachineState();
                            MachineState.Data dt = new MachineState.Data();
                            if (edges[4].CurrentValue == "5")
                            {
                                dt.code = "O99EECE-01-04";
                                dt.error_message = "A axis state change";

                                #region A轴报警索引
                                codeA = "O99EECE-01-06";
                                messageA = "Not retrieve A-Axis error list";
                                severityA = "critical";
                                occurrence_timeA = nowtime;
                                if (alarmGroup != null)
                                {
                                    foreach (var item in alarmGroup.Items)
                                    {
                                        if (item.GetMetaData().Severity.ToString() != "local" && item.Value.Boolean == true&& item.GetMetaData().Axis.ToString()!="B")
                                        {
                                            messageA = item.GetMetaData().Description;
                                            codeA = item.GetMetaData().Code;
                                            severityA = item.GetMetaData().Severity.ToString();
                                        }
                                    }
                                }
                                #endregion
                            }
                            else
                            {
                                if(edges[4].OldValue=="5")
                                {
                                    #region A轴报警保存
                                       resolved_timeA = nowtime;
                                       Dictionary<string, string> dictionary = new Dictionary<string, string>();
                                       dictionary["Occurrence_Time"] = occurrence_timeA.ToString("yyyy-MM-ddTHH:mm:ss.ff");
                                       dictionary["Resolved_Time"] = resolved_timeA.ToString("yyyy-MM-ddTHH:mm:ss.ff");
                                       dictionary["Code"] = codeA;
                                       dictionary["Message"] = messageA;
                                       dictionary["Severity"] = severityA;
                                       CsvHelper.WriteCsv($"D:/Data/{occurrence_timeA:yyyyMM}/ErrorDataA/{occurrence_timeA:yyyyMMdd}.csv", dictionary);
                                    #endregion
                                }
                            }
                            dt.state_change_reason = "A axis state change";
                            dt.State_A = TAG_StateA.ToString();
                            dt.State_A_change_time= nowtime.ToString("yyyy-MM-ddTHH:mm:ss.ffzz00");
                            dt.State_B = TAG_StateB.ToString();
                            rt.machine_state = edges[4].CurrentValue;
                            rt.state_change_time = nowtime.ToString("yyyy-MM-ddTHH:mm:ss.ffzz00");
                            rt.data = dt;
                            //var jsonSetting = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
                            //var json = JsonConvert.SerializeObject(rt, Formatting.Indented, jsonSetting);
                            //JsonHelper.WriteJsonFile($"./Hive/Json/StateData/{DateTime.Now:yyyyMMddhhmmss.fffffff}A.json", json);

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
                    if (TAG_StateB != null && TAG_State != null&& TAG_StateA!=null)
                    {
                        edges[5].CurrentValue = TAG_StateB.ToString();
                        if (edges[5].ValueChanged && edges[5].CurrentValue != "0")
                        {
                            var nowtime = DateTime.Now;
                            MachineState rt = new MachineState();
                            MachineState.Data dt = new MachineState.Data();
                            if(edges[5].CurrentValue == "5")
                            {
                                dt.code = "O99EECE-01-05";
                                dt.error_message = "B axis state change";
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
                            }
                            else
                            {
                                if (edges[5].OldValue == "5")
                                {
                                    #region B轴报警保存
                                    resolved_timeB = nowtime;
                                    Dictionary<string, string> dictionary = new Dictionary<string, string>();
                                    dictionary["Occurrence_Time"] = occurrence_timeB.ToString("yyyy-MM-ddTHH:mm:ss.ff");
                                    dictionary["Resolved_Time"] = resolved_timeB.ToString("yyyy-MM-ddTHH:mm:ss.ff");
                                    dictionary["Code"] = codeB;
                                    dictionary["Message"] = messageB;
                                    dictionary["Severity"] = severityB;
                                    CsvHelper.WriteCsv($"D:/Data/{occurrence_timeB:yyyyMM}/ErrorDataB/{occurrence_timeB:yyyyMMdd}.csv", dictionary);
                                    #endregion
                                }
                            }
                            dt.state_change_reason = "B axis state change";
                            dt.State_B = TAG_StateB.ToString();
                            dt.State_B_change_time = nowtime.ToString("yyyy-MM-ddTHH:mm:ss.ffzz00");
                            dt.State_A = TAG_StateA.ToString();
                            rt.machine_state = edges[5].CurrentValue;
                            rt.state_change_time = nowtime.ToString("yyyy-MM-ddTHH:mm:ss.ffzz00");
                            rt.data = dt;
                            //var jsonSetting = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
                            //var json = JsonConvert.SerializeObject(rt, Formatting.Indented, jsonSetting);
                            //JsonHelper.WriteJsonFile($"./Hive/Json/StateData/{DateTime.Now:yyyyMMddhhmmss.fffffff}B.json", json);

                        }
                    }
                    Thread.Sleep(100);
                }
            }, TaskCreationOptions.LongRunning);//设备B状态上传

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
        /// <summary>
        /// 保存参数设置记录
        /// </summary>
        /// <param name="Ret">1.False A轴 2.True B轴 </param>
        /// <param name="powerInfo"></param>
        /// <returns></returns>
        private bool SaveParameterData(bool Ret,PowerInfo powerInfo)
        {
            try
            {
                DateTime nowtime = DateTime.Now;
                string Gap1A = (server["D5402"]?.Value.Int32 / 10000.0)?.ToString("F4");
                string Gap2A = (server["D5404"]?.Value.Int32 / 10000.0)?.ToString("F4");
                string Gap3A = (server["D5406"]?.Value.Int32 / 10000.0)?.ToString("F4");
                string CoolA1 = (server["D462"]?.Value.Int16 / 10.0)?.ToString("F1");
                string CoolA2 = (server["D463"]?.Value.Int16 / 10.0)?.ToString("F1");
                string Iron_TempA= server["D1100"]?.Value.Int16.ToString();
                string TensionA= server["D1106"]?.Value.Int16.ToString();
                string Iron_SpeedA= (server["D5040"]?.Value.Int32 / 10000.0)?.ToString("F4");


                string Gap1B = (server["D5412"]?.Value.Int32 / 10000.0)?.ToString("F4");
                string Gap2B = (server["D5414"]?.Value.Int32 / 10000.0)?.ToString("F4");
                string Gap3B = (server["D5416"]?.Value.Int32 / 10000.0)?.ToString("F4");
                string CoolB1 = (server["D482"]?.Value.Int16 / 10.0)?.ToString("F1");
                string CoolB2 = (server["D483"]?.Value.Int16 / 10.0)?.ToString("F1");
                string Iron_TempB = server["D1101"]?.Value.Int16.ToString();
                string TensionB = "N/A";
                string Iron_SpeedB = (server["D5045"]?.Value.Int32 / 10000.0)?.ToString("F4");



                string fileStr = Ret ? "MachineDataB" : $"MachineDataA";
                Dictionary<string, string> dictionary = new Dictionary<string, string>();
                dictionary["TimeStamp"] = DateTime.Now.ToString("u");
                dictionary["Iron_temperature"] = Ret? Iron_TempB:Iron_TempA;
                dictionary["Current_Density"] =powerInfo.SetValue_CurrentDensity.ToString();
                dictionary["Bonding_temperature"] = powerInfo.Bonding_Temp.ToString();
                dictionary["Mandrel_highest_temperature"] = powerInfo.Tool_Temp.ToString();
                dictionary["Tension"] = Ret?TensionB:TensionA;
                dictionary["Gap1"] = Ret ?Gap1B:Gap1A;
                dictionary["Gap2"] = Ret ? Gap2B : Gap3A;
                dictionary["Gap3"] = Ret ? Gap3B : Gap3A;
                dictionary["Iron_speed"] = Ret? Iron_SpeedB:Iron_SpeedA;
                dictionary["Cooling_time1"] = Ret?CoolB1:CoolA1;
                dictionary["Cooling_time2"] = Ret ? CoolB2 : CoolA2;
                dictionary["Bonding_time"] = powerInfo.Bonding_Time.ToString();
                var res=CsvHelper.WriteCsv($"D:/ActualData/{nowtime:yyyy}/{nowtime:MM}/{nowtime:dd}/{fileStr}.csv", dictionary);
                return res;
            }
            catch(Exception ex)
            {
                logger.Log(ex.Message + Environment.NewLine + ex.StackTrace, Category.Exception, Priority.None);
                return false;
            }
        }
        /// <summary>
        /// 获取德国电源CSV文件内的最后一条数据
        /// </summary>
        /// <returns></returns>
        private (bool, PowerInfo) GetPowerRecord()
        {
            try
            {
                var powerInfo = new PowerInfo();
                DateTime dateTime = DateTime.Now;
                string file = $"C:/SCHLEICH/CSV/{dateTime:yyyy_MM_dd}.csv";
                if(!File.Exists(file))
                {
                   
                    file = $"C:/SCHLEICH/CSV/{dateTime.AddDays(-1):yyyy_MM_dd}.csv";
                    if (!File.Exists(file))
                        return (false, powerInfo);
                }
                FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using var reader = new StreamReader(fs, encoding: Encoding.UTF8);
                using var csv = new CsvReader(reader, CultureInfo.CurrentCulture);
                var record = csv.GetRecords<PowerRecord>().LastOrDefault();
                if (record != null)
                {
                    powerInfo.Station = record.Station;
                    powerInfo.Date = record.Date;
                    powerInfo.Time = record.Time;
                    powerInfo.Rstart = double.Parse(record.Rstart);
                    powerInfo.Current = double.Parse(record.Current);
                    powerInfo.Voltage = double.Parse(record.Voltage);
                    powerInfo.Power = double.Parse(record.Power);
                    powerInfo.Rend = double.Parse(record.Rend);
                    powerInfo.Bonding_Temp = double.Parse(record.Bonding_Temp);
                    powerInfo.Bonding_Time = double.Parse(record.Bonding_Time);
                    powerInfo.Tool_Temp = double.Parse(record.Tool_Temp);
                    powerInfo.Bonding_Method =record.Bonding_Method;
                    powerInfo.RC1 = double.Parse(record.RC1);
                    powerInfo.RC2 = double.Parse(record.RC2);
                    powerInfo.Result = record.Result;
                    powerInfo.TimeStamp =DateTime.Parse(record.TimeStamp);
                    powerInfo.SetValue_WireDiameter = double.Parse(record.SetValue_WireDiameter);
                    powerInfo.SetValue_NoParallWires = double.Parse(record.SetValue_NoParallWires);
                    powerInfo.SetValue_CurrentDensity = double.Parse(record.SetValue_CurrentDensity);
                    powerInfo.SetValue_BondingTemperature = double.Parse(record.SetValue_BondingTemperature);
                    return (true, powerInfo);
                }
                else
                {
                    return (false, powerInfo);
                }
            }
            catch (Exception ex)
            {
                logger.Log(ex.Message + Environment.NewLine + ex.StackTrace, Category.Exception, Priority.None);
                return (false, new PowerInfo());
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
