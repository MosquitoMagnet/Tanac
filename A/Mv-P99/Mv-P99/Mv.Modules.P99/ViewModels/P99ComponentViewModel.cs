using Microsoft.Xaml.Behaviors.Media;
using Mv.Core.Interfaces;
using Mv.Modules.P99.Service;
using Mv.Ui.Core;
using Mv.Ui.Mvvm;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Unity;
using DataService;
using Newtonsoft.Json;


namespace Mv.Modules.P99.ViewModels
{
    public class P99ComponentViewModel : ViewModelBase
    {
        private readonly IPlcScannerComm plcScanner;
        private readonly IConfigureFile configureFile;
        private readonly IScannerComm scannerComm;
        private readonly IEpson2Cognex epson2Cognex;
        private IFactoryInfo factoryInfo;

        public ObservableCollection<BindableWrapper<string>> SupportRingSNs { get; set; } = new ObservableCollection<BindableWrapper<string>>(Enumerable.Repeat(new BindableWrapper<string>() { Value = "" }, 4));
        public ObservableCollection<BindableWrapper<string>> MandrelNO { get; set; } = new ObservableCollection<BindableWrapper<string>>(Enumerable.Repeat(new BindableWrapper<string>() { Value = "" }, 4));
        public ObservableCollection<BindableWrapper<int>> InputTime { get; set; } = new ObservableCollection<BindableWrapper<int>>(Enumerable.Repeat(new BindableWrapper<int>() { Value =0 }, 4));
        public ObservableCollection<BindableWrapper<int>> OutputTime { get; set; } = new ObservableCollection<BindableWrapper<int>>(Enumerable.Repeat(new BindableWrapper<int>() { Value = 0 }, 4));
        public BindableWrapper<bool> IsConnected { get; set; } = new BindableWrapper<bool>();
        public BindableWrapper<bool> IsConnected2 { get; set; } = new BindableWrapper<bool>();
        public ObservableCollection<string> Messages { get; set; } = new ObservableCollection<string>();
        public BindableWrapper<bool> Trigger { get; set; } = new BindableWrapper<bool>();

        private DelegateCommand saveData;
        public DelegateCommand SaveCmd =>
            saveData ?? (saveData = new DelegateCommand(SaveData));

        private void AddMessage(string Msg)
        {
            this.Invoke(() =>
            {
                Logger.Log(Msg, Prism.Logging.Category.Debug, Prism.Logging.Priority.None);
                Msg = DateTime.Now.ToString() + " " + Msg;
                Messages.Add(Msg);
                if (Messages.Count > 1000)
                {
                    Messages.RemoveAt(0);
                }
            });

        }

        private bool VerifyCode(string code)
        {
            if (string.IsNullOrEmpty(code.Trim('\0')) || code.StartsWith("NG"))
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        

        public P99ComponentViewModel(IUnityContainer container, IDeviceReadWriter device,IPlcScannerComm plcScanner, IConfigureFile configureFile,IScannerComm scannerComm, IEpson2Cognex epson2Cognex) : base(container)
        {
            this.plcScanner = plcScanner;
            this.configureFile = configureFile;
            this.scannerComm = scannerComm;
            this.epson2Cognex = epson2Cognex;
            EventAggregator.GetEvent<MessageEvent>().Subscribe((x) => Invoke(() => {
                AddMessage(x);
            }));
            var m = configureFile.GetValue<P99Config>(nameof(P99Config));
            if (m == null)
            {
                configureFile.SetValue(nameof(P99Config), new P99Config());
            }
            this.factoryInfo = Container.Resolve<IFactoryInfo>(m.Factory);

            Task.Factory.StartNew(() =>
            {
                short itick = 0;
                P99Config p99Config = configureFile.GetValue<P99Config>(nameof(P99Config));
                while (true)
                {
                    this.Invoke(() =>
                    {

                        for (int i = 0; i < 4; i++)
                        {
                            if (p99Config.Factory == "LinYi")
                                SupportRingSNs[i] = device.GetString(90*2 + i * 40, 40).Trim('\0');
                            else
                                SupportRingSNs[i] = device.GetString(20 + i * 20, 20).Trim('\0');
                            MandrelNO[i] = device.GetString(100 + i * 20, 20).Trim('\0');
                            InputTime[i] = device.GetInt(304+i*2);
                            OutputTime[i] = device.GetInt(312 + i * 2);
                        }
                        Trigger = (device.GetWord(0) > 0);
                        IsConnected.Value = device.IsConnected;
                        IsConnected2.Value = plcScanner.IsConnected;
                        device.SetShort(1, itick++);
                        if (itick >= 10)
                            itick = 0;
                    });
                    Thread.Sleep(1);
                }
            }, TaskCreationOptions.LongRunning);
            Task.Factory.StartNew(() =>
            {
              
                while (true)
                {
                    if (Trigger)
                    {
                        AddMessage("收到保存数据信号");
                        device.SetShort(0, 1);
                        AddMessage("置位保存数据输出");
                        try
                        {
                            SaveData();
                        }
                        catch (Exception ex)
                        {
                            AddMessage(ex.Message);
                         //   throw;
                        }
                        AddMessage("数据保存完成，等待触发数据保存信号输入关闭");
                        SpinWait.SpinUntil(() => (Trigger == false), 2000);
                        AddMessage("数据保存信号输入已关闭，关闭数据保存信号输出");
                        device.SetShort(0, 0);
                    }
                    Thread.Sleep(1);
                }
            });

        }

        private void SaveData()
        {
            P99Config p99Config = configureFile.GetValue<P99Config>(nameof(P99Config));
            for (int i = 0; i < 4; i++)
            {
                var dic = new Dictionary<string, string>();
                dic["Time"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                dic["Support ring SN"] = SupportRingSNs[i].Value;
                dic["Spindle NO."] = p99Config.MachineNo + "_" + (i + 1).ToString();
                dic["Mandrel NO."] = MandrelNO[i].Value;
                dic["Result"] = ((VerifyCode(SupportRingSNs[i].Value) && VerifyCode(MandrelNO[i].Value))) ? "PASS" : "FAIL";
                var content = string.Join(',', dic.Values).Trim(',');
                string fileName= Path.Combine(p99Config.SaveDir, (VerifyCode(SupportRingSNs[i].Value) ? SupportRingSNs[i].Value : "Empty" + Helper.GetTimeStamp().ToString()) + ".csv");
                if (p99Config.Factory == "LinYi")
                {
                    fileName = Path.Combine(p99Config.SaveDir, DateTime.Today.ToString("yyyyMMdd") + ".csv");
                    Helper.SaveFile(fileName, dic);
                }
                AddMessage(content);
                var res=factoryInfo.Upload(SupportRingSNs[i].Value, fileName, dic);
                AddMessage(res);
                #region Hive MachinceData
                if(SupportRingSNs[i].Value.Length>5)
                {
                MachineData rt = new MachineData();
                rt.unit_sn = SupportRingSNs[i].Value;
                MachineData.Serials serials = new MachineData.Serials();
                rt.serials = serials;
                rt.pass = "true";
                    #region 产品输入输出时间处理
                    try
                    {
                        string Tag_intime = InputTime[i].Value.ToString();
                        Tag_intime = Tag_intime.Substring(1).Insert(2, ":").Insert(5, ":");
                        string Tag_outime = OutputTime[i].Value.ToString();
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
                            if (interval > 0 && interval <= 620)
                            {
                                rt.input_time = input_tmie2.ToString("yyyy-MM-ddTHH:mm:ss.ffzz00");
                                rt.output_time = output_tmie.ToString("yyyy-MM-ddTHH:mm:ss.ffzz00");
                            }
                            else
                            {
                                rt.input_time = output_tmie.AddSeconds(-40).ToString("yyyy-MM-ddTHH:mm:ss.ffzz00");
                                rt.output_time = output_tmie.ToString("yyyy-MM-ddTHH:mm:ss.ffzz00");                               
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        rt.input_time = DateTime.Now.AddSeconds(-40).ToString("yyyy-MM-ddTHH:mm:ss.ffzz00");
                        rt.output_time = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss.ffzz00");
                    }
                    #endregion
                MachineData.Data Hivedata = new MachineData.Data();
                Hivedata.Spindle_NO = p99Config.MachineNo + "_" + (i + 1).ToString();
                Hivedata.Mandrel_NO= MandrelNO[i].Value;
                rt.data = Hivedata;
                var jsonSetting = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };
                var json = JsonConvert.SerializeObject(rt, Formatting.Indented, jsonSetting);
                JsonHelper.WriteJsonFile($"./Hive/Json/MachineData/{DateTime.Now:yyyyMMddhhmmss.fffffff}+{i.ToString()}.json", json);
                }
                #endregion
            }
        }
    }
}
