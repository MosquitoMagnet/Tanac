using System;
using System.IO;
using System.Collections.Generic;
using Mv.Core.Interfaces;
using Prism.Events;
using MV.Core.Events;
using System.Reactive;
using System.Reactive.Linq;
using MaterialDesignThemes.Wpf;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using SimpleTCP;
using Newtonsoft.Json;
using Prism.Logging;
using DataService;

namespace Mv.Modules.RD402.Service
{
    public enum TagNames
    {
        Save1,    //	A轴二维码保存
        Save2,    //	B轴二维码保存
        Save3,    //	C轴二维码保存
        Save4,    //	D轴二维码保存
        State,//设备状态
        FMA,//A轴换模后使用次数D1021
        FMB,//B轴换模后使用次数D1025
        PMA,//A轴维修后使用次数D1023
        PMB,//B轴维修后使用次数D1027
        LMTA,//A轴下模温度
        LMTB,//B轴下模温度
        MG1A, //A轴Grap1速度D0010544 4
        MG2A,//A轴Grap2速度D0010554  4
        MG3A, //A轴Grap3速度D0010519 4
        MG1B,//B轴Grap1速度D0011144  4
        MG2B,//B轴Grap2速度D0011154  4
        MG3B,//B轴Grap3速度D0011119  4
        WS1A,//A轴第1段速度D0010424  2
        WS2A,//A轴第2段速度D0010434  2  
        WS3A,//A轴第3段速度D0010454  2
        WS1B,//B轴第1段速度D11024    2
        WS2B,//B轴第2段速度D11034    2
        WS3B,//B轴第3段速度D11054    2
        WSBA,//A轴折弯速度D0010444   2
        WSBB,//B轴折弯速度D0011044   2
        ISA,//A轴烫线速度D0010624    4
        ISB,//B轴烫线速度D0011224     4
        Update//更新时间
    }
    public interface IEpsonCommunication
    {
        bool ServerConnected { get; }
    }
    public class EpsonCommunication: IEpsonCommunication, IDisposable
    {
        SimpleTcpServer server;
        RD402Config _config;
        RD402HiveDataConfig _hiveDataconfig;
        private readonly string _sw_version = "V1.0.7";
        private readonly ILoggerFacade _logger;
        private readonly ISnackbarMessageQueue _messageQueue;
        private readonly IConfigureFile _configureFile;
        private readonly IEventAggregator @event;
        private readonly IUpload uploader;
        private readonly IDataServer dataserver;
        private readonly IDeviceReadWriter _device;
        private string Spindle="N/A";
        private string Unit_sn= "N/A";
        public static DateTime Input_time;
        public bool ServerConnected { get; private set; }
        public EpsonCommunication(IDataServer dataserver, ISnackbarMessageQueue messageQueue,ILoggerFacade logger,IConfigureFile configureFile, IEventAggregator @event, IUpload uploader, IDeviceReadWriter device)
        {
            this.dataserver = dataserver;
            this._device = device;
            _configureFile = configureFile.Load();
            this._configureFile.ValueChanged += _configure_ValueChanged;
            _config = _configureFile.GetValue<RD402Config>("RD402Config");
            _hiveDataconfig = _configureFile.GetValue<RD402HiveDataConfig>("RD402HiveDataConfig");
            server = new SimpleTcpServer();      
            server.DataReceived += Server_DataReceived;
            var PARASGroup = dataserver.GetGroupByName("PARAS");//PLC数据
            if (PARASGroup != null)
            {
                foreach (var item in PARASGroup.Items)
                {
                    item.ValueChanged += PARA_ValueChanged;
                }
            }
            Input_time = DateTime.Now;
            _messageQueue = messageQueue;
            this.@event = @event;
            this._logger = logger;
            this.uploader = uploader;
            server.Start(6666);
        }
        private void Server_DataReceived(object sender, Message e)
        {
            string msg = "Epson to IPC:Send Fail  "+ e.MessageString;
            byte[] byteArray = System.Text.Encoding.Default.GetBytes(e.MessageString);          
            var data = e.MessageString;
          if(data.Length>3)
          {
            if (data[0] == 'S'& data[1] == 'N'& data[2] == ',')
            {
                string[] data2 = data.Split("\r\n".ToCharArray());
                string data3 = data2[0].ToString();
                string data1 = data3.Substring(3);
                if (data1.IndexOf(',') < 0)
                {
                   
                }
                else
                {
                    string[] sn = data1.Split(',');
                    if(sn[1]==""|| sn[1] == " ")
                    {
                       

                    }
                    else
                    {
                            Spindle = sn[0];
                            Unit_sn = sn[1];
                            msg = "Epson to IPC Send Success,Mandrel:" + sn[0] + ",SN:" + sn[1];
                            this.@event.GetEvent<UserMessageEvent>().Publish(new UserMessage { Content = msg, Level = 0, Source = "Epson" });
                            if (sn[1].Length>5)
                            {
                                if (_config.Factory == "ICT")
                                {
                                    var ICTWireVendor = "N/A";
                                    if (Spindle == "A")
                                    {
                                        if (_config.isWireVendorA == true)
                                            ICTWireVendor = _config.WireVendorCodeB;
                                        else
                                            ICTWireVendor = _config.WireVendorCodeA;


                                    }
                                    if (Spindle == "B")
                                    {
                                        if (_config.isWireVendorB == true)
                                            ICTWireVendor = _config.WireVendorCodeB;
                                        else
                                            ICTWireVendor = _config.WireVendorCodeA;
                                    }
                                    ICTUploadFile(true, sn[0], sn[1], ICTWireVendor);
                                }
                                if (_config.Factory == "信维")
                                {
                                    msg = "开始上传数据...";
                                    this.@event.GetEvent<UserMessageEvent>().Publish(new UserMessage { Content = msg, Level = 0, Source = "Epson" });
                                    var tick2 = Environment.TickCount;
                                    var result = uploader.Upload(sn[0], sn[1]);
                                    msg = $"数据上传耗时:{Environment.TickCount - tick2} ms";
                                    this.@event.GetEvent<UserMessageEvent>().Publish(new UserMessage { Content = msg, Level = 0, Source = "Epson" });
                                    var strRes = $"数据上传:{result.Item2}";
                                    _logger.Log(msg, Category.Info, Priority.None);
                                    msg = strRes;
                                    bool upOK = result.Item2.Contains("数据上传成功");
                                    if (upOK)
                                    {
                                        _device.SetBit(0, 4, false);
                                    }
                                    else
                                    {
                                        _device.SetBit(0, 4, true);
                                    }

                                    if (!result.Item1 || !upOK)
                                    {
                                        var WireVendor = "N/A";
                                        if (Spindle == "A")
                                        {
                                            if (_config.isWireVendorA == true)
                                                WireVendor = _config.WireVendorCodeB;
                                            else
                                                WireVendor = _config.WireVendorCodeA;
                                        }
                                        if (Spindle == "B")
                                        {
                                            if (_config.isWireVendorB == true)
                                                WireVendor = _config.WireVendorCodeB;
                                            else
                                                WireVendor = _config.WireVendorCodeA;
                                        }
                                        ICTUploadFile(true, sn[0], sn[1], WireVendor);
                                    }

                                }
                                #region 上传HIVE系统
                                try
                                {
                                    var Output_time = DateTime.Now;
                                    var a = (Output_time - Input_time).TotalSeconds;
                                    if (a <= 0)
                                    {
                                        Input_time = Output_time.AddSeconds(-60);
                                    }
                                    if (Spindle == "A")
                                    {
                                        MachineData rt = new MachineData();
                                        rt.unit_sn = Unit_sn;
                                        MachineData.Serials serials = new MachineData.Serials();
                                        rt.serials = serials;
                                        rt.pass = "true";
                                        rt.input_time = Input_time.ToString("yyyy-MM-ddTHH:mm:ss.ffzz00");
                                        rt.output_time = Output_time.ToString("yyyy-MM-ddTHH:mm:ss.ffzz00");
                                        MachineData.Data Hivedata = new MachineData.Data();
                                        #region Data
                                        Hivedata.Spindle = Spindle;
                                        if (_hiveDataconfig.isFA1)
                                            Hivedata.CTQ_FAI1 = _hiveDataconfig.FA1_A;
                                        if (_hiveDataconfig.isFA2)
                                            Hivedata.CTQ_FAI2 = _hiveDataconfig.FA2_A;
                                        if (_hiveDataconfig.isFA3)
                                            Hivedata.CTQ_FAI3 = _hiveDataconfig.FA3_A;
                                        if (_hiveDataconfig.isFA4)
                                            Hivedata.CTQ_FAI4 = _hiveDataconfig.FA4_A;
                                        if (_hiveDataconfig.isFA5)
                                            Hivedata.CTQ_FAI5 = _hiveDataconfig.FA5_A;
                                        if (_hiveDataconfig.isFA6)
                                            Hivedata.CTQ_FAI6 = _hiveDataconfig.FA6_A;
                                        if (_hiveDataconfig.isFA7)
                                            Hivedata.CTQ_FAI7 = _hiveDataconfig.FA7_A;
                                        if (_hiveDataconfig.isFA8)
                                            Hivedata.CTQ_FAI8 = _hiveDataconfig.FA8_A;
                                        if (_hiveDataconfig.isFA9)
                                            Hivedata.CTQ_FAI9 = _hiveDataconfig.FA9_A;
                                        if (_hiveDataconfig.isFA10)
                                            Hivedata.CTQ_FAI10 = _hiveDataconfig.FA10_A;
                                        if (_hiveDataconfig.isFA11)
                                            Hivedata.CTQ_FAI11 = _hiveDataconfig.FA11_A;
                                        if (_hiveDataconfig.isFA12)
                                            Hivedata.CTQ_FAI12 = _hiveDataconfig.FA12_A;
                                        if (_hiveDataconfig.isFA13)
                                            Hivedata.CTQ_FAI13 = _hiveDataconfig.FA13_A;
                                        if (_hiveDataconfig.isFA14)
                                            Hivedata.CTQ_FAI14 = _hiveDataconfig.FA14_A;
                                        if (_hiveDataconfig.isFA15)
                                            Hivedata.CTQ_FAI15 = _hiveDataconfig.FA15_A;
                                        if (_hiveDataconfig.isFA16)
                                            Hivedata.CTQ_FAI16 = _hiveDataconfig.FA16_A;
                                        if (_hiveDataconfig.isFA17)
                                            Hivedata.CTQ_FAI17 = _hiveDataconfig.FA17_A;
                                        if (_hiveDataconfig.isFA18)
                                            Hivedata.CTQ_FAI18 = _hiveDataconfig.FA18_A;
                                        if (_hiveDataconfig.isFA19)
                                            Hivedata.CTQ_FAI19 = _hiveDataconfig.FA19_A;
                                        if (_hiveDataconfig.isFA20)
                                            Hivedata.CTQ_FAI20 = _hiveDataconfig.FA20_A;
                                        if (_hiveDataconfig.isFA21)
                                            Hivedata.CTQ_FAI21 = _hiveDataconfig.FA21_A;
                                        if (_hiveDataconfig.isFA22)
                                            Hivedata.CTQ_FAI22 = _hiveDataconfig.FA22_A;
                                        if (_hiveDataconfig.isFA23)
                                            Hivedata.CTQ_FAI23 = _hiveDataconfig.FA23_A;
                                        if (_hiveDataconfig.isFA24)
                                            Hivedata.CTQ_FAI24 = _hiveDataconfig.FA24_A;
                                        if (_hiveDataconfig.isFA25)
                                            Hivedata.CTQ_FAI25 = _hiveDataconfig.FA25_A;
                                        if (_hiveDataconfig.isParallelism)
                                            Hivedata.Parallelism_between_upper_and_lower_mandrel = _hiveDataconfig.Parallelism_A;
                                        if (_hiveDataconfig.isBendingPin)
                                            Hivedata.Bending_Pin_Position = _hiveDataconfig.BendingPin_A;
                                        if (_hiveDataconfig.isUsageC && dataserver[GetTagName(TagNames.FMA)] != null)
                                            Hivedata.Tool_cycles = dataserver[GetTagName(TagNames.FMA)].Value.Int32.ToString();
                                        if (_hiveDataconfig.isUsageM && dataserver[GetTagName(TagNames.PMA)] != null)
                                            Hivedata.PM_cycles = dataserver[GetTagName(TagNames.PMA)].Value.Int16.ToString();
                                        if (_hiveDataconfig.isOD)
                                            Hivedata.Wire_OD = _hiveDataconfig.OD_A;
                                        if (_hiveDataconfig.isLower && dataserver[GetTagName(TagNames.LMTA)] != null)
                                            Hivedata.Lower_Mandrel_temperature = (dataserver[GetTagName(TagNames.LMTA)].Value.Int32 / 10.0).ToString("F1");
                                        if (_hiveDataconfig.isWireTen)
                                            Hivedata.Wire_Tension = _hiveDataconfig.WireTen_A;
                                        if (_hiveDataconfig.isGrap1 && dataserver[GetTagName(TagNames.MG1A)] != null)
                                            Hivedata.Mandrel_gap1 = (dataserver[GetTagName(TagNames.MG1A)].Value.Int32 / 10000.0).ToString("F4");
                                        if (_hiveDataconfig.isGrap2 && dataserver[GetTagName(TagNames.MG2A)] != null)
                                            Hivedata.Mandrel_gap2 = (dataserver[GetTagName(TagNames.MG2A)].Value.Int32 / 10000.0).ToString("F4");
                                        if (_hiveDataconfig.isGrap3 && dataserver[GetTagName(TagNames.MG3A)] != null)
                                            Hivedata.Mandrel_gap3 = (dataserver[GetTagName(TagNames.MG3A)].Value.Int32 / 10000.0).ToString("F4");
                                        if (_hiveDataconfig.isWspeed1 && dataserver[GetTagName(TagNames.WS1A)] != null)
                                            Hivedata.Winding_speed1 = (dataserver[GetTagName(TagNames.WS1A)].Value.Int32 / 100.0).ToString("F2");
                                        if (_hiveDataconfig.isWspeed2 && dataserver[GetTagName(TagNames.WS2A)] != null)
                                            Hivedata.Winding_speed2 = (dataserver[GetTagName(TagNames.WS2A)].Value.Int32 / 100.0).ToString("F2");
                                        if (_hiveDataconfig.isWspeed3 && dataserver[GetTagName(TagNames.WS3A)] != null)
                                            Hivedata.Winding_speed3 = (dataserver[GetTagName(TagNames.WS3A)].Value.Int32 / 100.0).ToString("F2");
                                        if (_hiveDataconfig.isWspeedB && dataserver[GetTagName(TagNames.WSBA)] != null)
                                            Hivedata.Bending_speed = (dataserver[GetTagName(TagNames.WSBA)].Value.Int32 / 100.0).ToString("F2");
                                        if (_hiveDataconfig.isIspeed && dataserver[GetTagName(TagNames.ISA)] != null)
                                            Hivedata.Iron_speed = (dataserver[GetTagName(TagNames.ISA)].Value.Int32 / 10000.0).ToString("F4");
                                        Hivedata.sw_version = _sw_version;
                                        #endregion
                                        rt.data = Hivedata;
                                        var jsonSetting = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
                                        var json = JsonConvert.SerializeObject(rt, Formatting.Indented, jsonSetting);
                                        JsonHelper.WriteJsonFile($"./Hive/Json/MachineData/{DateTime.Now:yyyyMMddhhmmss.fffffff}+A.json", json);
                                    }
                                    if (Spindle == "B")
                                    {
                                        MachineData rt = new MachineData();
                                        rt.unit_sn = Unit_sn;
                                        MachineData.Serials serials = new MachineData.Serials();
                                        rt.serials = serials;
                                        rt.pass = "true";
                                        rt.input_time = Input_time.ToString("yyyy-MM-ddTHH:mm:ss.ffzz00");
                                        rt.output_time = Output_time.ToString("yyyy-MM-ddTHH:mm:ss.ffzz00");
                                        MachineData.Data Hivedata = new MachineData.Data();
                                        #region Data
                                        Hivedata.Spindle = Spindle;
                                        if (_hiveDataconfig.isFA1)
                                            Hivedata.CTQ_FAI1 = _hiveDataconfig.FA1_B;
                                        if (_hiveDataconfig.isFA2)
                                            Hivedata.CTQ_FAI2 = _hiveDataconfig.FA2_B;
                                        if (_hiveDataconfig.isFA3)
                                            Hivedata.CTQ_FAI3 = _hiveDataconfig.FA3_B;
                                        if (_hiveDataconfig.isFA4)
                                            Hivedata.CTQ_FAI4 = _hiveDataconfig.FA4_B;
                                        if (_hiveDataconfig.isFA5)
                                            Hivedata.CTQ_FAI5 = _hiveDataconfig.FA5_B;
                                        if (_hiveDataconfig.isFA6)
                                            Hivedata.CTQ_FAI6 = _hiveDataconfig.FA6_B;
                                        if (_hiveDataconfig.isFA7)
                                            Hivedata.CTQ_FAI7 = _hiveDataconfig.FA7_B;
                                        if (_hiveDataconfig.isFA8)
                                            Hivedata.CTQ_FAI8 = _hiveDataconfig.FA8_B;
                                        if (_hiveDataconfig.isFA9)
                                            Hivedata.CTQ_FAI9 = _hiveDataconfig.FA9_B;
                                        if (_hiveDataconfig.isFA10)
                                            Hivedata.CTQ_FAI10 = _hiveDataconfig.FA10_B;
                                        if (_hiveDataconfig.isFA11)
                                            Hivedata.CTQ_FAI11 = _hiveDataconfig.FA11_B;
                                        if (_hiveDataconfig.isFA12)
                                            Hivedata.CTQ_FAI12 = _hiveDataconfig.FA12_B;
                                        if (_hiveDataconfig.isFA13)
                                            Hivedata.CTQ_FAI13 = _hiveDataconfig.FA13_B;
                                        if (_hiveDataconfig.isFA14)
                                            Hivedata.CTQ_FAI14 = _hiveDataconfig.FA14_B;
                                        if (_hiveDataconfig.isFA15)
                                            Hivedata.CTQ_FAI15 = _hiveDataconfig.FA15_B;
                                        if (_hiveDataconfig.isFA16)
                                            Hivedata.CTQ_FAI16 = _hiveDataconfig.FA16_B;
                                        if (_hiveDataconfig.isFA17)
                                            Hivedata.CTQ_FAI17 = _hiveDataconfig.FA17_B;
                                        if (_hiveDataconfig.isFA18)
                                            Hivedata.CTQ_FAI18 = _hiveDataconfig.FA18_B;
                                        if (_hiveDataconfig.isFA19)
                                            Hivedata.CTQ_FAI19 = _hiveDataconfig.FA19_B;
                                        if (_hiveDataconfig.isFA20)
                                            Hivedata.CTQ_FAI20 = _hiveDataconfig.FA20_B;
                                        if (_hiveDataconfig.isFA21)
                                            Hivedata.CTQ_FAI21 = _hiveDataconfig.FA21_B;
                                        if (_hiveDataconfig.isFA22)
                                            Hivedata.CTQ_FAI22 = _hiveDataconfig.FA22_B;
                                        if (_hiveDataconfig.isFA23)
                                            Hivedata.CTQ_FAI23 = _hiveDataconfig.FA23_B;
                                        if (_hiveDataconfig.isFA24)
                                            Hivedata.CTQ_FAI24 = _hiveDataconfig.FA24_B;
                                        if (_hiveDataconfig.isFA25)
                                            Hivedata.CTQ_FAI25 = _hiveDataconfig.FA25_B;
                                        if (_hiveDataconfig.isParallelism)
                                            Hivedata.Parallelism_between_upper_and_lower_mandrel = _hiveDataconfig.Parallelism_B;
                                        if (_hiveDataconfig.isBendingPin)
                                            Hivedata.Bending_Pin_Position = _hiveDataconfig.BendingPin_B;
                                        if (_hiveDataconfig.isUsageC && dataserver[GetTagName(TagNames.FMB)] != null)
                                            Hivedata.Tool_cycles = dataserver[GetTagName(TagNames.FMB)].Value.Int32.ToString();
                                        if (_hiveDataconfig.isUsageM && dataserver[GetTagName(TagNames.PMB)] != null)
                                            Hivedata.PM_cycles = dataserver[GetTagName(TagNames.PMB)].Value.Int16.ToString();
                                        if (_hiveDataconfig.isOD)
                                            Hivedata.Wire_OD = _hiveDataconfig.OD_B;
                                        if (_hiveDataconfig.isLower && dataserver[GetTagName(TagNames.LMTB)] != null)
                                            Hivedata.Lower_Mandrel_temperature = (dataserver[GetTagName(TagNames.LMTB)].Value.Int32 / 10.0).ToString("F1");
                                        if (_hiveDataconfig.isWireTen)
                                            Hivedata.Wire_Tension = _hiveDataconfig.WireTen_B;
                                        if (_hiveDataconfig.isGrap1 && dataserver[GetTagName(TagNames.MG1B)] != null)
                                            Hivedata.Mandrel_gap1 = (dataserver[GetTagName(TagNames.MG1B)].Value.Int32 / 10000.0).ToString("F4");
                                        if (_hiveDataconfig.isGrap2 && dataserver[GetTagName(TagNames.MG2B)] != null)
                                            Hivedata.Mandrel_gap2 = (dataserver[GetTagName(TagNames.MG2B)].Value.Int32 / 10000.0).ToString("F4");
                                        if (_hiveDataconfig.isGrap3 && dataserver[GetTagName(TagNames.MG3B)] != null)
                                            Hivedata.Mandrel_gap3 = (dataserver[GetTagName(TagNames.MG3B)].Value.Int32 / 10000.0).ToString("F4");
                                        if (_hiveDataconfig.isWspeed1 && dataserver[GetTagName(TagNames.WS1B)] != null)
                                            Hivedata.Winding_speed1 = (dataserver[GetTagName(TagNames.WS1B)].Value.Int32 / 100.0).ToString("F2");
                                        if (_hiveDataconfig.isWspeed2 && dataserver[GetTagName(TagNames.WS2B)] != null)
                                            Hivedata.Winding_speed2 = (dataserver[GetTagName(TagNames.WS2B)].Value.Int32 / 100.0).ToString("F2");
                                        if (_hiveDataconfig.isWspeed3 && dataserver[GetTagName(TagNames.WS3B)] != null)
                                            Hivedata.Winding_speed3 = (dataserver[GetTagName(TagNames.WS3B)].Value.Int32 / 100.0).ToString("F2");
                                        if (_hiveDataconfig.isWspeedB && dataserver[GetTagName(TagNames.WSBB)] != null)
                                            Hivedata.Bending_speed = (dataserver[GetTagName(TagNames.WSBB)].Value.Int32 / 100.0).ToString("F2");
                                        if (_hiveDataconfig.isIspeed && dataserver[GetTagName(TagNames.ISB)] != null)
                                            Hivedata.Iron_speed = (dataserver[GetTagName(TagNames.ISB)].Value.Int32 / 10000.0).ToString("F4");
                                        Hivedata.sw_version = _sw_version;
                                        #endregion
                                        rt.data = Hivedata;
                                        var jsonSetting = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
                                        var json = JsonConvert.SerializeObject(rt, Formatting.Indented, jsonSetting);
                                        JsonHelper.WriteJsonFile($"./Hive/Json/MachineData/{DateTime.Now:yyyyMMddhhmmss.fffffff}+A.json", json);
                                    }
                                    Input_time = Output_time;
                                }
                                catch (Exception ex)
                                {
                                    _logger.Log(ex.Message, Category.Info, Priority.None);
                                }//上传HIVE系统
                                #endregion
                            }
                        }
                    }            
            }           
            }//上传数据
            
            this.@event.GetEvent<UserMessageEvent>().Publish(new UserMessage { Content = msg, Level = 0, Source = "Epson" });
        }
        public bool ICTUploadFile(bool result, string Spindle, string MatrixCode,string WireVendor)
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            dictionary["SN"] = MatrixCode;
            dictionary["Time"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            dictionary["Machine"] = _config.MachineNumber;
            dictionary["Mandrel_number"] = Spindle;
            dictionary["WireVendor"] = WireVendor;
            dictionary["Result"] = "PASS";
            return Helper.SaveFile(Path.Combine(_config.FileDir, MatrixCode + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".csv"), dictionary);
        }
        private void PARA_ValueChanged(object sender, DataService.ValueChangedEventArgs e)
        {
            if (sender is BoolTag booltag)
            {
                var Save = booltag.GetMetaData();
                if (Save.Name == "Update")
                {
                    if (booltag.Value.Boolean == true)
                    {

                        try
                        {
                            Input_time= DateTime.Now;
                        }
                        catch (Exception ex)
                        {
                            _logger.Log(ex.Message, Category.Info, Priority.None);
                        }
                    }
                }

            }

        }
        private string GetTagName(TagNames tagenum)
        {
            return Enum.GetName(typeof(TagNames), tagenum);
        }      
        private void _configure_ValueChanged(object sender, Mv.Core.Interfaces.ValueChangedEventArgs e)
        {
            if (e.KeyName == nameof(RD402Config))
            { 
            var config = _configureFile.GetValue<RD402Config>(nameof(RD402Config));
            _config = config;
            }
            if (e.KeyName == nameof(RD402HiveDataConfig))
            {
            var hiveDataconfig = _configureFile.GetValue<RD402HiveDataConfig>(nameof(RD402HiveDataConfig));
            _hiveDataconfig = hiveDataconfig;
            }
        }     
        public static class Helper
        {
            public static bool SaveFile(string fileName, Dictionary<string, string> hashtable)
            {
                try
                {
                    var dir = Path.GetDirectoryName(fileName);
                    if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                    if (!File.Exists(fileName))
                    {
                        var header = string.Join(',', hashtable.Keys).Trim(',') + Environment.NewLine;
                        var content = string.Join(',', hashtable.Values).Trim(',') + Environment.NewLine;
                        File.AppendAllText(fileName, header + content);
                    }
                    else
                    {
                        var content = string.Join(',', hashtable.Values).Trim(',');
                        File.AppendAllText(fileName, content + Environment.NewLine);
                    }
                    return true;
                }
                catch (Exception ex)
                {                  
                    return false;
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
                    var PARASGroup = dataserver.GetGroupByName("PARAS");
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
