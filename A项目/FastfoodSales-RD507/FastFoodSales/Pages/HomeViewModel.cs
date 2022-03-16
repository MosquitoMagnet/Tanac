using Stylet;
using DAQ.Pages;
using StyletIoC;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using DAQ.Service;
using ZXing;
using ZXing.Common;
using ZXing.Presentation;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using DAQ.Core.Log;
using LiveCharts;
using System.Text.RegularExpressions;
using Communication.BasicFramework;

namespace DAQ
{
    public class HomeViewModel : Screen
    {

        IEventAggregator _eventAggregator;
        MsgViewModel _msg;
        private IConfigureFile configure;
        private readonly IDeviceReadWriter _device;
        private readonly IInkPrinter inkPrinter;
        private readonly IRotbotService rotbotService;
        private readonly IFactoryInfo factoryInfo;
        private readonly IAirParticleDetector airParticle;
        private string _2dcode;
        private string _machineCode;
        private Config config;
        private string barcode;
        private readonly Dispatcher dispatcher = Dispatcher.CurrentDispatcher;
        private Edge[] edges = new Edge[10] { new Edge(), new Edge(), new Edge(), new Edge(), new Edge(), new Edge(), new Edge(), new Edge(), new Edge(), new Edge() };

        public HomeViewModel(MsgViewModel msg, IEventAggregator eventAggregator, IConfigureFile configure, 
            IDeviceReadWriter device, IInkPrinter inkPrinter, IFactoryInfo factoryInfo,
            IRotbotService rotbotService,IAirParticleDetector airParticle)
        {

            _msg = msg;
            this.configure = configure;
            this.rotbotService = rotbotService;
            this._device = device;
            this.inkPrinter = inkPrinter;
            this.factoryInfo = factoryInfo;
            this.airParticle = airParticle;

            configure.ValueChanged += configure_ValueChanged;
            config = configure.Load().GetValue<Config>(nameof(Config)) ?? new Config();
            CancellationTokenSource cancellationToken = new CancellationTokenSource();
            rotbotService.CodeDataReceived += codeDataReceived;

            short tick = 0;
            Task.Factory.StartNew(
                async () =>
                {
                    while (true)
                    {
                        CoilModule = _device.GetShort(2).ToString();
                        edges[0].CurrentValue = _device.GetBit(0,0);
                        edges[1].CurrentValue = _device.GetBit(0,1);                  
                        if (edges[0].CurrentValue&&edges[0].ValueChanged)
                        {
                            AddMsg("获取二维码...");
                            var tick2 = Environment.TickCount;
                            var result = await GetMatrixCode().ConfigureAwait(false);
                            AddMsg($"耗时:{Environment.TickCount - tick2} ms");
                            var strRes = result ? "Success" : "Fail";
                            AddMsg(strRes);
                            if (!result)
                            {
                                _device.SetBit(0, 2, false);
                            }
                            else
                            {  
                                    AddMsg($"开始写入二维码: {MatrixCode}");
                                    tick2 = Environment.TickCount;
                                    if (!await WritePrinterCode().ConfigureAwait(false))
                                    {
                                        AddMsg("Fail");
                                        _device.SetBit(0, 2, false);
                                    }
                                    else
                                    {
                                        _device.SetString(11, MatrixCode);
                                        AddMsg("Success");
                                        _device.SetBit(0, 2, true);
                                    }
                                    AddMsg($"耗时 {Environment.TickCount - tick2} ms");
                                    AddMsg("二维码设置完成...");
                            }
                            _device.SetBit(0, 0, true);
                            SpinWait.SpinUntil(() => !_device.GetBit(0, 0), 1000);
                            _device.SetBit(0, 0, false);
                            _device.SetBit(0, 2, false);
                        }
                        if (edges[1].CurrentValue && edges[1].ValueChanged)
                        {
                            AddMsg($"开始读取NFC...");
                            var tick2 = Environment.TickCount;
                            var result = await inkPrinter.ReadNfcCodeAsync().ConfigureAwait(false);
                            AddMsg($"耗时:{Environment.TickCount - tick2} ms");
                            if(result!=null)
                            {
                                byte[] wr = SoftBasic.BytesReverseByWord(result);//颠倒字节组
                                AddMsg($"读取NFC成功...");
                                _device.SetBytes(23, wr);
                                _device.SetBit(0,3, true);

                            }
                            else
                            {
                                AddMsg($"读取NFC失败...");
                                _device.SetBit(0,3, false);
                            }
                            _device.SetBit(0, 1, true);
                            SpinWait.SpinUntil(() => !_device.GetBit(0, 1), 1000);
                            _device.SetBit(0, 1, false);
                            _device.SetBit(0, 3, false);

                        }
                        if (tick >= 15) tick = 0;
                        _device.SetShort(10, tick++);
                        Thread.Sleep(1);
                    }
                }, cancellationToken.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
            Task.Factory.StartNew(() =>
            {
                while(true)
                {
                    edges[2].CurrentValue = _device.GetBit(0, 4);
                    edges[3].CurrentValue = _device.GetBit(0, 5);
                    //产品A数据保存
                    if (edges[2].CurrentValue && edges[2].ValueChanged)
                    {
                        AddMsg("A开始保存文件...");
                        string code = _device.GetString(40, 40).Trim('\0');
                        AddMsg($"A上传的二维码为{code}");
                        //var result = factoryInfo.UploadFile(_device.GetBit(0, 5), factoryInfo.GetSpindle(_device.GetWord(1)), MatrixCode);

                        try
                        {
                            float od = _device.GetShort(20) / 1000f;
                            int type = _device.GetShort(22);
                            float gap = _device.GetShort(24) / 1000f;
                            string intime = _device.GetShort(26).ToString("x4").Insert(2, ":") + ":" + _device.GetShort(27).ToString("x2");
                            string outtime = _device.GetShort(28).ToString("x4").Insert(2, ":") + ":" + _device.GetShort(29).ToString("x2");
                            int temp = _device.GetShort(30);
                            int lspeed = _device.GetShort(32);
                            int mspeed = _device.GetShort(34);
                            int hspeed = _device.GetShort(36);
                            float ct = _device.GetFloat(38);
                            string res = "NG";
                            if (_device.GetBit(10, 0))
                                res = "OK";

                            var dictionary = new Dictionary<string, string>();
                            dictionary["Measured Wire OD on the Spool(mm)"] = od.ToString("f4");
                            dictionary["Mandrel S/N"] = type.ToString();
                            dictionary["Mandrel Gap(mm)"] = gap.ToString("f3");
                            dictionary["Time In"] = intime;
                            dictionary["Time Out"] = outtime;
                            dictionary["Heat Gun Temperature(°C)"] = temp.ToString();
                            dictionary["Spindle Speed(RPM)"] = $"{lspeed};{mspeed};{hspeed}";
                            dictionary["Lead Position"] = res;
                            dictionary["Cycle time"] = ct.ToString("f1");
                            dictionary["Coil+Liner S/N"] = code;
                            bool saveRes = Helper.SaveFile(Path.Combine(config.FileDir + @"\MachineDataA", DateTime.Now.ToString("yyyyMMdd") + ".csv"), dictionary);
                            if (saveRes == false)
                            {
                                bool saveRes2 = Helper.SaveFile(Path.Combine(config.FileDir + @"\MachineDataA", DateTime.Now.ToString("yyyyMMdd") + "_1.csv"), dictionary);
                                _device.SetBit(0, 5, saveRes2);
                            }
                            else
                            {
                                _device.SetBit(0, 5, true);
                            }
                        }
                        catch
                        {
                            _device.SetBit(0, 5, false);
                        }
                        AddMsg("A文件保存完毕");
                        _device.SetBit(0, 4, true);
                        SpinWait.SpinUntil(() => !_device.GetBit(0, 4), 1000);
                        _device.SetBit(0, 4, false);
                        _device.SetBit(0, 5, false);
                    }
                    //产品B数据保存
                    if (edges[3].CurrentValue && edges[3].ValueChanged)
                    {
                        AddMsg("开始保存B文件...");
                        string code = _device.GetString(140, 40).Trim('\0');
                        AddMsg($"上传的B二维码为{code}");
                        //var result = factoryInfo.UploadFile(_device.GetBit(0, 5), factoryInfo.GetSpindle(_device.GetWord(1)), MatrixCode);

                        try
                        {
                            float od = _device.GetShort(120) / 1000f;
                            int type = _device.GetShort(122);
                            float gap = _device.GetShort(124) / 1000f;
                            string intime = _device.GetShort(126).ToString("x4").Insert(2, ":") + ":" + _device.GetShort(127).ToString("x2");
                            string outtime = _device.GetShort(128).ToString("x4").Insert(2, ":") + ":" + _device.GetShort(129).ToString("x2");
                            int temp = _device.GetShort(130);
                            int lspeed = _device.GetShort(132);
                            int mspeed = _device.GetShort(134);
                            int hspeed = _device.GetShort(136);
                            float ct = _device.GetFloat(138);
                            string res = "NG";
                            if (_device.GetBit(10, 1))
                                res = "OK";

                            var dictionary = new Dictionary<string, string>();
                            dictionary["Measured Wire OD on the Spool(mm)"] = od.ToString("f4");
                            dictionary["Mandrel S/N"] = type.ToString();
                            dictionary["Mandrel Gap(mm)"] = gap.ToString("f3");
                            dictionary["Time In"] = intime;
                            dictionary["Time Out"] = outtime;
                            dictionary["Heat Gun Temperature(°C)"] = temp.ToString();
                            dictionary["Spindle Speed(RPM)"] = $"{lspeed};{mspeed};{hspeed}";
                            dictionary["Lead Position"] = res;
                            dictionary["Cycle time"] = ct.ToString("f1");
                            dictionary["Coil+Liner S/N"] = code;
                            bool saveRes = Helper.SaveFile(Path.Combine(config.FileDir + @"\MachineDataB", DateTime.Now.ToString("yyyyMMdd") + ".csv"), dictionary);
                            if (saveRes == false)
                            {
                                bool saveRes2 = Helper.SaveFile(Path.Combine(config.FileDir + @"\MachineDataB", DateTime.Now.ToString("yyyyMMdd") + "_1.csv"), dictionary);
                                _device.SetBit(0, 7, saveRes2);
                            }
                            else
                            {
                                _device.SetBit(0, 7, true);
                            }
                        }
                        catch
                        {
                            _device.SetBit(0, 7, false);
                        }
                        AddMsg("B文件保存完毕");
                        _device.SetBit(0, 6, true);
                        SpinWait.SpinUntil(() => !_device.GetBit(0, 6), 1000);
                        _device.SetBit(0, 6, false);
                        _device.SetBit(0, 7, false);
                    }
                }

            });
        }

        private void codeDataReceived(object sender,string e)
        {

            try
            {           
                var arr1 = e.Split(Environment.NewLine.ToCharArray());
                var arr2 = arr1[0].Split(',');
                if(arr2[0]=="SN")
                {
                    string code = arr2[1];
                    AddMsg($"rotbot send Data,code,{code}");
                    rotbotService.BroadcastRobot("SN,1\r\n");
                }
                else
                {
                    LogHelper.Error($"Invalid data format,{e}");
                    AddMsg($"rotbot send Data,Invalid data format,{e}");
                    rotbotService.BroadcastRobot("SN,3\r\n");
                }
            }
            catch(Exception ex)
            {
                LogHelper.Error(ex.Message);
                AddMsg($"rotbot send Data,Invalid data format,{e}");
                rotbotService.BroadcastRobot("SN,3\r\n");
            }
        }
        private void configure_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            if (e.KeyName != nameof(Config)) return;
            var _config = configure.GetValue<Config>(nameof(Config));
            config = _config;
            OnPropertyChanged(nameof(SI));
            OnPropertyChanged(nameof(Station));
            OnPropertyChanged(nameof(LineNumber));
            OnPropertyChanged(nameof(Spindle));
        }
        protected override void OnInitialActivate()
        {
            base.OnInitialActivate();
        }

        protected override void OnViewLoaded()
        {
            base.OnViewLoaded();
        }
        protected override void OnClose()
        {
            base.OnClose();
        }

        public BindableCollection<string> Msg { get; } = new BindableCollection<string>();
        public RotbotVM RotbotVm
        {
            get => rotbotService.rotbotVm;
            set => rotbotService.rotbotVm = value;
        }
        public AirDataVM AirDataVm
        {
            get => airParticle.AirDataVm;
            set => airParticle.AirDataVm = value;
        }
        public ModbusDeviceVM ModbusDeviceVm
        {
            get => _device.ModbusDeviceVm;
            set => _device.ModbusDeviceVm = value;
        }
        public string Barcode
        {
            get => barcode;
            set => barcode=value;
        }
        public string MatrixCode
        {
            get => _2dcode;
            set
            {
                _2dcode = value;               
                 dispatcher.BeginInvoke((Action)delegate ()
                    {
                        var writer = new BarcodeWriterGeometry
                        {
                            Format = BarcodeFormat.DATA_MATRIX,
                            Options = new EncodingOptions
                            {
                                Height = 80,
                                Width = 80,
                                Margin = 0
                            }
                        };
                        var image = writer.Write(_2dcode);
                        Codeimage = image;
                    });               
                }
            
        }


        private string coilModule;
        public string CoilModule
        {
            get => coilModule;
            set
            {
                coilModule = value;
                OnPropertyChanged(nameof(CoilModule));
            }
        }
        public string SI
        {
            get => config.SI;
        }
        public string LineNumber
        {
            get => config.LineNumber;
        }
        public string Station
        {
            get => config.Station;
        }
        public string Spindle
        {
            get => config.Spindle;
        }



        public Geometry Codeimage { get; private set; }
        private void AddMsg(string message)
        {
            var msg = $"{DateTime.Now.ToString()} {message}";
            dispatcher.Invoke(() =>
            {
                if (Msg.Count > 100) Msg.RemoveAt(0);
                Msg.Add(msg);
            });
        }

        #region 设置条码命令

        public async Task SetBarcodeCommand()
        {
            await WritePrinterText().ConfigureAwait(false);
        }

        private async Task<bool> WritePrinterText()
        {
            if (await inkPrinter.WritePrinterTextAsync(Barcode).ConfigureAwait(false))
            {
                AddMsg($"set matrix txt {Barcode},ok");
                return true;
            }

            AddMsg($"set matrix txt {Barcode},fail");
            return false;
        }






        #endregion

        #region 获取二维码命令
        public async void Get2DodeCommand()
        {
            var result = await GetMatrixCode().ConfigureAwait(false);
        }
        private async Task<bool> GetMatrixCode()
        {
            var result = await Task.Run<(bool,string)>(new Func<(bool,string)>(factoryInfo.GetSn)).ConfigureAwait(false);

            AddMsg($"获取二维码:{result.Item2}.");
            if (!result.Item1) return false;
            dispatcher.Invoke(() => { MatrixCode = result.Item2; });
            AddMsg($"二维码:{MatrixCode}.");
            return true;
        }
        #endregion

        #region 设置二维码到PLC命令
        public async Task Set2DodeCommand()
        {
            await WritePrinterCode().ConfigureAwait(false);
        }

        private async Task<bool> WritePrinterCode()
        {
            if (await inkPrinter.WritePrinterCodeAsync(MatrixCode).ConfigureAwait(false))
            {
                AddMsg($"set matrix code {MatrixCode},ok");
                return true;
            }

            AddMsg($"set matrix code {MatrixCode},fail");
            return false;
        }

        #endregion

        public async void SetAirStartCommand()
        {
            var result = await SetAirStart().ConfigureAwait(false);
        }
        private async Task<bool> SetAirStart()
        {
            var result = await Task.Run<bool>(new Func<bool>(airParticle.SetRun)).ConfigureAwait(false);

            AddMsg($"Start sending particle counter start signal");
            if (!result) return false;                    
            AddMsg($"Particle counter start signal sent successfully");
            return true;
        }

        public async void SetAirStopCommand()
        {
            var result = await SetAirStop().ConfigureAwait(false);
        }
        private async Task<bool> SetAirStop()
        {
            var result = await Task.Run<bool>(new Func<bool>(airParticle.SetStop)).ConfigureAwait(false);

            AddMsg($"Start sending particle counter stop signal");
            if (!result) return false;
            AddMsg($"Particle counter stop signal sent successfully");
            return true;
        }
        public async void AirConnectCommand()
        {
            var result = await AirConnect().ConfigureAwait(false);
        }
        private async Task<bool> AirConnect()
        {
            var result = await Task.Run<bool>(new Func<bool>(airParticle.Connect)).ConfigureAwait(false);

            AddMsg($"Start connecting the particle counter");
            if (!result)
            {
                AddMsg($"Failed to connect the particle counter");
                return false;
            }
            AddMsg($"Connected particle counter successfully");
            return true;
        }
    }
}

