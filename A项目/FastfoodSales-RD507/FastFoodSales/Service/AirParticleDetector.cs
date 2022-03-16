using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Communication.ModBus;
using Communication.Core;
using System.Threading;
using DAQ.Core.Log;
using DAQ.Core;
using Stylet;
using LiveCharts;

namespace DAQ.Service
{

    public class AirDataVM : PropertyChangedBase
    {
        public short RunStatus { get; set; } = -1; //-1未练级 0仪器停止 1仪器运行
        public string Message { get; set; } = "Ofline"; //消息提示
        public float CurrentFlow { get; set; } = 0f;
        public int CurrentValue5 { get; set; } = 0;
        public int CurrentValue10 { get; set; } = 0;
        public int UpperLimit { get; set; } = 0;
        public int Lowerlimit { get; set; } = 0;
        public short Result { get; set; } = 0;
        public ChartValues<double> Values
        {
            get;
            set;
        } = new ChartValues<double>();
        public string[] TimeStamp { get; set; }
    }
    public interface IAirParticleDetector
    {
        AirDataVM AirDataVm { get; set; }
        short GetShort(int index);
        int GetInt(int index);
        bool SetRun();
        bool SetStop();
        bool Connect();
    }
   public class AirParticleDetector:IAirParticleDetector
   {
        private readonly IDeviceReadWriter device;
        private readonly IConfigureFile configure;
        private Config config;
        private ModbusRtu modbusRtu;
        byte[] rbs = new byte[92 * 2];
        private AirDataVM airDataVM = new AirDataVM();
        private System.Timers.Timer timer = new System.Timers.Timer();
        public AirParticleDetector(IDeviceReadWriter device,IConfigureFile configureFile)
        {
            this.device = device;
            this.configure = configureFile;
            config = configure.Load().GetValue<Config>(nameof(Config)) ?? new Config();
            configure.ValueChanged += configure_ValueChanged;
            timer.Interval = config.AirScanInterval*1000;
            airDataVM.Lowerlimit = config.AirValueL;
            airDataVM.UpperLimit = config.AirValueH;
            timer.Enabled = true;
            timer.Elapsed += timer_Tick;
            modbusRtu?.Close();
            modbusRtu = new ModbusRtu();
            modbusRtu.AddressStartWithZero = true;
            modbusRtu.IsStringReverse = true;
            modbusRtu.DataFormat = DataFormat.CDAB;
            Connect();
            SetRun();
            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    var rr = modbusRtu.Read("0", (ushort)(rbs.Length / 2));
                    if (rr.IsSuccess)
                    {
                        Buffer.BlockCopy(rr.Content, 0, rbs, 0, rr.Content.Length);
                        airDataVM.RunStatus = GetShort(2);
                        airDataVM.CurrentFlow =(float)(GetShort(40)/100.0);
                        airDataVM.CurrentValue5 = GetInt(30);
                        airDataVM.CurrentValue10 = GetInt(32);
                        if(airDataVM.RunStatus==1)
                            airDataVM.Message = "Runing";
                        else
                            airDataVM.Message = "Stop";
                    }
                    else
                    {
                        airDataVM.RunStatus = -1;
                        airDataVM.CurrentFlow = 0;
                        airDataVM.CurrentValue5 = -1;
                        airDataVM.CurrentValue10 = -1;
                        airDataVM.Message = "Ofline";
                        LogHelper.Info("AirParticleDetector:" + rr.Message);
                    }
                    device.SetShort(81, airDataVM.RunStatus);//告诉PLC当前仪器的连接状态
                    device.SetInt(82, airDataVM.CurrentValue5);//告诉PLC当前值
                    device.SetInt(84, airDataVM.Lowerlimit);//告诉PLC当前下限
                    device.SetInt(86, airDataVM.UpperLimit);//告诉PLC当前上限
                    device.SetShort(88, GetShort(40));//告诉PLC当前流量
                    Thread.Sleep(500);
                }
            }, TaskCreationOptions.LongRunning);
            timer.Start();
        }
        //刷新状态
        private void timer_Tick(object sender, EventArgs e)
        {
            try
            {
                timer.Enabled = false;
                if(airDataVM.CurrentValue5>airDataVM.UpperLimit&&airDataVM.CurrentFlow<airDataVM.Lowerlimit)
                {
                    airDataVM.Result= -1;
                }
                else
                {
                    airDataVM.Result = 1;
                }
                airDataVM.Values.Add(airDataVM.CurrentValue5);
                if (airDataVM.Values.Count > 40)
                {
                    airDataVM.Values.RemoveAt(0);
                }
                var UpdateData = new Dictionary<string, string>();
                UpdateData["Time"] = DateTime.Now.ToString();
                UpdateData["CurrentValue"] = airDataVM.CurrentValue5.ToString();
                UpdateData["UpperLimit"] = airDataVM.UpperLimit.ToString();
                UpdateData["Lowerlimit"] = airDataVM.Lowerlimit.ToString();
                UpdateData["Flow"] = airDataVM.CurrentFlow.ToString();
                var res = Helper.SaveFile(Path.Combine(config.FileDir + @"\ParticleC", DateTime.Now.ToString("yyyyMMdd")+ ".csv"), UpdateData); 
            }
            catch 
            {
                airDataVM.Result = -1;
            }
            timer.Enabled = true;
        }
        public AirDataVM AirDataVm
        {
            get => airDataVM;
            set => airDataVM = value;
        }
        private void configure_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            if (e.KeyName != nameof(Config)) return;
            var _config = configure.GetValue<Config>(nameof(Config));
            config = _config;
            timer.Interval = config.AirScanInterval * 1000;
            airDataVM.Lowerlimit = config.AirValueL;
            airDataVM.UpperLimit = config.AirValueH;
        }
        public bool Connect()
        {
            try
            {
                config = configure.Load().GetValue<Config>(nameof(Config)) ?? new Config();
                modbusRtu.SerialPortInni(sp =>
                {
                    sp.PortName = config.AirComPort;
                    sp.BaudRate = 19200;
                    sp.DataBits = 8;
                    sp.StopBits = System.IO.Ports.StopBits.One;
                    sp.Parity = System.IO.Ports.Parity.None;
                });
                modbusRtu.Open(); 
                return true; 
            }
            catch
            {
                return false;
            }
        }
        public bool SetRun()
        {
            return SetShort("2", 1);
        }
        public bool SetStop()
        {
            return SetShort("2", 0);
        }
        public short GetShort(int index)
        {
            return modbusRtu.ByteTransform.TransInt16(rbs, index * 2);
        }
        public int GetInt(int index)
        {
            return modbusRtu.ByteTransform.TransInt32(rbs, index * 2);
        }
        public bool SetShort(string address,short value)
        {
            try
            {
               return modbusRtu.Write(address, value).IsSuccess;
            }
            catch(Exception ex)
            {
                LogHelper.Error(ex.Message);
                return false;
            }
        }
        public bool SetInt(string address, int value)
        {
            try
            {
                return modbusRtu.Write(address, value).IsSuccess;
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.Message);
                return false;
            }
        }
    }
}
