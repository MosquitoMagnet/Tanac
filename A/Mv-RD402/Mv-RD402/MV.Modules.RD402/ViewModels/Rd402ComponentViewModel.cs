using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Packaging;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using Mv.Core.Interfaces;
using Mv.Modules.RD402.Service;
using Mv.Modules.RD402.Views;
using Mv.Ui.Core;
using Mv.Ui.Mvvm;
using Prism.Commands;
using Prism.Logging;
using Unity;
using ZXing;
using ZXing.Common;
using ZXing.Presentation;
using System.Linq;
using MV.Core.Events;

namespace Mv.Modules.RD402.ViewModels
{

    public static class RD402Helper
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
                //    AddMsg(ex.Message);
                return false;
            }
        }
    }


    public class Rd402ComponentViewModel : ViewModelBase
    {
        private readonly IConfigureFile _configure;
        private IFactoryInfo factoryInfo;
        private readonly IDeviceReadWriter _device;
        private readonly IInkPrinter inkPrinter;
        private readonly IEpsonCommunication epsonCommunication;
        private string _2dcode;

        private RD402Config _config;
        private string _dayOfWeek;
        private bool _isConnected;
        private string _linecode;
        private string _machineCode;
        private string _spindle;
        private string _vendor;
        private string _wireConfig;
        private string barcode;
        private string coilWinding;
        private readonly Dispatcher dispatcher = Dispatcher.CurrentDispatcher;
        private string station;

        public Rd402ComponentViewModel(IUnityContainer container, IDeviceReadWriter device, IInkPrinter inkPrinter, IConfigureFile configure, IEpsonCommunication epsonCommunication
        ) : base(container)
        {
            _device = device;
            this.inkPrinter = inkPrinter;
            this.epsonCommunication = epsonCommunication;
            _configure = configure;
            _configure.ValueChanged += _configure_ValueChanged;
            _config = configure.GetValue<RD402Config>(nameof(RD402Config)) ?? new RD402Config();

            this.factoryInfo = Container.Resolve<IFactoryInfo>(_config.Factory);

            CancellationTokenSource cancellationToken = new CancellationTokenSource();

            EventAggregator.GetEvent<UserMessageEvent>().Subscribe(x =>
            {

                Invoke(() =>
                {
                    AddMsg(x.Content);
                });
            });


            Task.Factory.StartNew(() =>
                {
                    while (true)
                    {
                        if (Application.Current != null)
                            Application.Current.Dispatcher?.BeginInvoke(() =>
                            {
                                for (var i = 0; i < 16; i++) Obs[i] = device.GetBit(0, i);
                                for (var i = 0; i < 16; i++) Outs[i] = device.GetSetBit(0, i);
                                IsConnected = device.IsConnected;
                                Spindle = factoryInfo.GetSpindle(device.GetWord(1));
                            });

                        Thread.Sleep(100);
                    }
                },
          cancellationToken.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
            short tick = 0;
            Task.Factory.StartNew(
                async () =>
                {
                    while (true)
                    {
                        if (_device.GetBit(0, 0))
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
                                if (_config.Factory=="ICT"&& _config.isQRCheck ==true)
                                { 
                                AddMsg($"开始比对二维码: {MatrixCode}");
                                    
                                   if(MatrixCode.Length==17)
                                    {
                                        string revision = MatrixCode.Substring(15, 1);
                                        string eeCode = MatrixCode.Substring(11, 4);
                                        string vendorCode = MatrixCode.Substring(0, 3);

                                        if (revision==Revision&&eeCode==EECode&&VendorCode==vendorCode)
                                        {
                                            AddMsg($"二维码比对成功");
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
                                        else
                                        {
                                            AddMsg($"二维码比对失败,{MatrixCode},EECode:{eeCode}--{EECode},Apple版次代码:{revision}--{Revision},工厂代码:{vendorCode}--{VendorCode}");
                                            _device.SetBit(0, 2, false);
                                        }

                                }
                                   else
                                   {
                                    AddMsg($"二维码位数错误");
                                    _device.SetBit(0, 2, false);
                                   }
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
                            }
                            _device.SetBit(0, 0, true);
                            SpinWait.SpinUntil(() => !_device.GetBit(0, 0), 1000);
                            _device.SetBit(0, 0, false);
                            _device.SetBit(0, 2, false);
                        }
                        if (_device.GetBit(0, 1))
                        {                          
                            barcode = factoryInfo.GetBarcode(MatrixCode);
                            AddMsg($"开始设置明码 {barcode}...");
                            _device.SetString(23, barcode);
                            if (await inkPrinter.WritePrinterTextAsync(barcode).ConfigureAwait(false))
                            {
                                _device.SetBit(0, 3, true);
                                AddMsg("Success!");
                            }
                            else
                            {
                                _device.SetBit(0, 3, false);
                                AddMsg("Fail!");
                            }
                            AddMsg("条码设置完毕...");
                            _device.SetBit(0, 1, true);
                            SpinWait.SpinUntil(() => !_device.GetBit(0, 1), 1000);
                            _device.SetBit(0, 1, false);
                            _device.SetBit(0, 3, false);

                        }
                        if (_device.GetBit(0, 4)&& _config.Factory != "信维")
                        {
                            AddMsg("开始保存文件...");
                            //var result = factoryInfo.UploadFile(_device.GetBit(0, 5), factoryInfo.GetSpindle(_device.GetWord(1)), MatrixCode);
                            _device.SetBit(0, 5, true);
                            AddMsg("文件保存完毕");
                            _device.SetBit(0, 4, true);
                            SpinWait.SpinUntil(() => !_device.GetBit(0, 4), 1000);
                            _device.SetBit(0, 4, false);
                            _device.SetBit(0, 5, false);
                        }
                        if (tick >= 15) tick = 0;
                        _device.SetShort(10, tick++);
                        Thread.Sleep(1);
                        //    AddMsg(tick.ToString());
                    }
                }, cancellationToken.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        public ObservableCollection<string> Msg { get; } = new ObservableCollection<string>();

        public ObservableCollection<BindableWrapper<bool>> Obs { get; } =
            new ObservableCollection<BindableWrapper<bool>>(Enumerable.Repeat(new BindableWrapper<bool>() { Value = false }, 16));

        public ObservableCollection<BindableWrapper<bool>> Outs { get; } =
            new ObservableCollection<BindableWrapper<bool>>(Enumerable.Repeat(new BindableWrapper<bool>() { Value = false }, 16));

        public string LineCode
        {
            get => _linecode;
            set
            {
                SetProperty(ref _linecode, value);
                Barcode = $"{LineCode}{MachineCode}{Spindle}{DayOfWeek}{Vendor}{WireConfig}";
            }
        }

        public string Factory => _config.Factory;

        public Geometry Codeimage { get; private set; }

        public bool IsConnected
        {
            get => _isConnected;
            set => SetProperty(ref _isConnected, value);
        }

        public string MachineCode
        {
            get => string.IsNullOrEmpty(_machineCode) ? _config.MachineCode : _machineCode;
            set
            {
                if (SetProperty(ref _machineCode, value) && _config.Factory == "ICT")
                {
                    _config.MachineCode = value;
                    Barcode = $"{LineCode}{MachineCode}{Spindle}{DayOfWeek}{Vendor}{WireConfig}";
                }
            }
        }

        public string Vendor
        {
            get => string.IsNullOrEmpty(_vendor) ? _config.WireVendor : _vendor;
            set
            {
                if (SetProperty(ref _vendor, value) && _config.Factory == "ICT")
                    Barcode = $"{LineCode}{MachineCode}{Spindle}{DayOfWeek}{Vendor}{WireConfig}";
            }
        }

        public string MatrixCode
        {
            get => _2dcode;
            set
            {
                if (!SetProperty(ref _2dcode, value)) return;
                Barcode = factoryInfo.GetBarcode(_2dcode, _config, _device.GetWord(1));
                dispatcher.BeginInvoke(() =>
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
                    RaisePropertyChanged(nameof(Codeimage));
                });
            }
        }

        public string WireConfig
        {
            get => string.IsNullOrEmpty(_wireConfig) ? _config.WireConfig : _wireConfig;
            set
            {
                if (SetProperty(ref _wireConfig, value))
                    Barcode = $"{LineCode}{MachineCode}{Spindle}{DayOfWeek}{Vendor}{WireConfig}";
            }
        }

        public string Mo
        {
            get => _config.Mo;
        }

        public string CoilWinding
        {
            get => _config.CoilWinding;
        }

        public string Station
        {
            get => _config.Station;
        }


        public string Spindle
        {
            get => string.IsNullOrEmpty(_spindle) ? "0" : _spindle;
            set
            {
                SetProperty(ref _spindle, value);
            }
        }

        public string DayOfWeek
        {
            get => _dayOfWeek;
        }

        public string Barcode
        {
            get => barcode;
            set => SetProperty(ref barcode, value);
        }

        public string LineNumber
        {
            get => _config.LineNumber;
        }

        public string QRCheck
        {
            get => _config.QRCheck;
        }
        /// <summary>
        /// Apple版次代码
        /// </summary>
        /// <param name="message"></param>
        public string Revision
        {
            get => _config.Revision;
        }
        /// <summary>
        /// Engineering Reference Code
        /// </summary>
        /// <param name="message"></param>
        public string EECode
        {
            get => _config.EECode;
        }
        /// <summary>
        /// 工厂代码
        /// </summary>
        public string VendorCode
        {
            get => _config.VendorCode;
        }

        private void AddMsg(string message)
        {
            var msg = $"{DateTime.Now.ToString()} {message}";
            dispatcher.Invoke(() =>
            {
                if (Msg.Count > 100) Msg.RemoveAt(0);
                Msg.Add(msg);
            });
            Logger.Log(msg, Category.Info, Priority.None);
        }

        private void _configure_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            if (e.KeyName != nameof(RD402Config)) return;
            var config = _configure.GetValue<RD402Config>(nameof(RD402Config));
            _config = config;
            factoryInfo = Container.Resolve<IFactoryInfo>(config.Factory);
          
            RaisePropertyChanged(nameof(LineNumber));
            RaisePropertyChanged(nameof(Mo));
            RaisePropertyChanged(nameof(MachineCode));
            RaisePropertyChanged(nameof(Factory));
            Barcode = factoryInfo.GetBarcode(MatrixCode, config, _device.GetWord(1));
           
            RaisePropertyChanged(nameof(Factory));
        }

        #region 设置条码命令

        private DelegateCommand _setbarcodeCommand;

        public DelegateCommand SetBarcodeCommand =>
            _setbarcodeCommand ??= new DelegateCommand(async () =>
                await SetBarcode().ConfigureAwait(false));

        private async Task SetBarcode()
        {
            if (await inkPrinter.WritePrinterTextAsync(Barcode).ConfigureAwait(false))
            {
                AddMsg($"{MvUser.Username}:设置条码成功.");
            }
            else
            {
                var msg = $"{MvUser.Username}:设置条码成功.";
                AddMsg(msg);
            }
        }

        #endregion

        #region 获取二维码命令

        private DelegateCommand _get2dcodeCommand;

        public DelegateCommand Get2DodeCommand =>
            _get2dcodeCommand ??= new DelegateCommand(ExecuteGet2DodeCommand);

        private async void ExecuteGet2DodeCommand()
        {
            var result = await GetMatrixCode().ConfigureAwait(false);
        }




        private async Task<bool> GetMatrixCode()
        {
            var result = await Task.Run(factoryInfo.GetSn).ConfigureAwait(false);
            AddMsg($"{MvUser.Username}:获取二维码:{result.Item2}.");
            if (!result.Item1) return false;
            dispatcher.Invoke(() => { MatrixCode = result.Item2; });
            AddMsg($"{MvUser.Username}:二维码:{MatrixCode}.");
            return true;
        }

        #endregion


        #region 设置PLC位输出状态切换命令

        private DelegateCommand<string> _setOutputCommand;

        public DelegateCommand<string> SetOutputCommand =>
            _setOutputCommand ??= new DelegateCommand<string>(ExecuteSetOutputCommand,
                m => { return (int)MvUser.Role >= (int)MvRole.Admin; });

        private void ExecuteSetOutputCommand(string index)
        {
            _device.SetBit(0, int.Parse(index), !_device.GetSetBit(0, int.Parse(index)));
        }
        #endregion

        #region 设置二维码到PLC命令

        private DelegateCommand _set2dcodeCommand;

        public DelegateCommand Set2DodeCommand =>
            _set2dcodeCommand ??= new DelegateCommand(async () => await ExecuteSet2DodeCommand().ConfigureAwait(false));

        private async Task ExecuteSet2DodeCommand()
        {
            await WritePrinterCode().ConfigureAwait(false);
        }

        private async Task<bool> WritePrinterCode()
        {
            if (await inkPrinter.WritePrinterCodeAsync(MatrixCode).ConfigureAwait(false))
            {
                AddMsg($"{MvUser.Username}: set matrix code {MatrixCode},ok");
                return true;
            }

            AddMsg($"{MvUser.Username}: set matrix code {MatrixCode},fail");
            return false;
        }

        #endregion

    }


}