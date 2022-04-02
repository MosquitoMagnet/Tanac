using Mv.Core;
using Mv.Modules.P99.Service;
using Mv.Ui.Core;
using Mv.Ui.Mvvm;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Documents;
using Unity;

namespace Mv.Modules.P99.ViewModels
{
    public class CognexValue : BindableBase
    {
        private double trigger;

        public double Trigger
        {
            get { return trigger; }
            set { SetProperty(ref trigger, value); }
        }

        private double x;

        public double X
        {
            get { return x; }
            set { SetProperty(ref x, value); }
        }

        private double y;

        public double Y
        {
            get { return y; }
            set { SetProperty(ref y, value); }
        }

        private double xf;

        public double Xf
        {
            get { return xf; }
            set { SetProperty(ref xf, value); }
        }

        private double yf;

        public double Yf
        {
            get { return yf; }
            set { SetProperty(ref yf, value); }
        }
    }

    public class CognexViewModel : ViewModelBase
    {
        private const int ADDR_CMD = 0;
        private const int ADDR_DONE = 0;
        private readonly IPlcCognexComm device;
        private readonly ICognexCommunication cognex;
        private readonly IOPTLight light;


        public ObservableCollection<BindableWrapper<short>> UVCurrents { get; set; } = new ObservableCollection<BindableWrapper<short>>(Enumerable.Repeat(new BindableWrapper<short>(), 4));
        public ObservableCollection<string> Messages { get; set; } = new ObservableCollection<string>();
        public BindableWrapper<bool> IsConnected { get; set; } = new BindableWrapper<bool>();
        public ObservableCollection<CognexValue> CognexValues { get; set; } 
        Subject<(short, short, short, short)> CurrentSubject = new Subject<(short, short, short, short)>();
        public CognexViewModel(IUnityContainer container, IPlcCognexComm plcCognex, ICognexCommunication cognex,IOPTLight light) : base(container)
        {
            this.device = plcCognex;
            this.cognex = cognex;
            this.light = light;
            Observable.Interval(TimeSpan.FromMilliseconds(1000)).Subscribe(x =>
            {
                device.SetInt(0, 20, (x % 2 == 0) ? 0 : 1);
                device.SetInt(1, 20, (x % 2 == 0) ? 0 : 1);
            }); //心跳

            CognexValues = new ObservableCollection<CognexValue>(new CognexValue[] { new CognexValue(), new CognexValue() });
            Task.Factory.StartNew(() =>{
                while (true)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        var m = (i + 1);
                        short current = light.GetCurrent(m);
                        device.SetShort(0, 22 + i, current);
                        UVCurrents[i].Value = current;                   
                    }
                    CurrentSubject.OnNext((UVCurrents[0], UVCurrents[1], UVCurrents[2], UVCurrents[3]));
                    Thread.Sleep(100);
                }
            },TaskCreationOptions.LongRunning);
            CurrentSubject.DistinctUntilChanged().Timestamp().Buffer(timeSpan:TimeSpan.FromMilliseconds(1000)).Subscribe(ms => {
                var list = new List<Dictionary<string, string>>();
                foreach (var m in ms)
                {
                    var dictionary = new Dictionary<string, string>();
                    dictionary["DateTime"] = m.Timestamp.DateTime.ToString();
                    dictionary["UV1"] = m.Value.Item1.ToString();
                    dictionary["UV2"] = m.Value.Item2.ToString();
                    dictionary["UV3"] = m.Value.Item3.ToString();
                    dictionary["UV4"] = m.Value.Item4.ToString();

                    list.Add(dictionary);

                }
                var path = Path.Combine(MvFolders.MainProgram, "UVLight", $"{ DateTime.Today:yyyyMMdd}.csv");
                Helper.SaveFile(path, list);

            });
            Task.Factory.StartNew(async () =>
            {
                while (true)
                {
                    try
                    {
                        for (int id = 0; id < 2; id++)
                        {
                            var cmd = device.GetInt(id, ADDR_CMD);
                            var x = device.GetInt(id, 2);
                            var y = device.GetInt(id, 4);
                            var xf = device.GetInt(id, 6);
                            var yf = device.GetInt(id, 8);
                            // CognexValues[id].X = x;
                            var mid = id;
                            Invoke(() =>
                            {
                                IsConnected.Value = device.IsConnected;
                                CognexValues[mid].X = x / 1000d;
                                CognexValues[mid].Y = y / 1000d;
                                CognexValues[mid].Xf = xf / 1000d;
                                CognexValues[mid].Yf = yf / 1000d;
                                CognexValues[mid].Trigger = cmd;
                            });

                            if (cmd == 111)
                            {//开始示教
                                AddMessage($"开始示教[{id}]");
                                var result1 = cognex.CalibrationAsync(id, 0, (0, 0, 0));
                                var result = await cognex.CalibrationAsync(id, 0, (0, 0, 0));
                                AddMessage(result ? "Success" : "Fail");
                                var res = result ? cmd : 999;
                                device.SetInt(id, ADDR_DONE, res);
                                AddMessage("等待命令清除");
                                SpinWait.SpinUntil(() => device.GetInt(id, ADDR_CMD) != 111, 1000);
                                device.SetInt(id, ADDR_DONE, 0);
                                AddMessage("命令执行完毕");
                            }
                            if (cmd == 222)
                            {//9点
                                AddMessage($"9点示教[{id}]");
                                var result = await cognex.CalibrationAsync(id, 1, (x / 1000d, y / 1000d, 0));
                                var res = result ? cmd : 999;
                                AddMessage(result ? "Success" : "Fail");
                                device.SetInt(id, ADDR_DONE, res);
                                AddMessage("等待命令清除");
                                SpinWait.SpinUntil(() => device.GetInt(id, ADDR_CMD) != 222, 1000);
                                device.SetInt(id, ADDR_DONE, 0);
                                AddMessage("命令执行完毕");
                            }
                            if (cmd == 333)
                            {//结束示教
                                AddMessage($"结束示教[{id}]");
                                var result = await cognex.CalibrationAsync(id, 2, (0, 0, 0));
                                AddMessage(result ? "PASS" : "Fail");
                                var res = result ? cmd : 999;
                                device.SetInt(id, ADDR_DONE, res);
                                AddMessage("等待命令清除");
                                SpinWait.SpinUntil(() => device.GetInt(id, ADDR_CMD) != 333, 1000);
                                device.SetInt(id, ADDR_DONE, 0);
                                AddMessage("命令执行完毕");
                            }
                            if (cmd == 444)
                            {//训练
                                AddMessage("开始训练");
                                var result = await cognex.TrainCameraAsync(id, (x / 1000d, y / 1000d, xf / 1000d, yf / 1000d));
                                AddMessage($"收到数据：{result.Item2}");
                                var resContent = result.Item1 ? "PASS" : "FAIL";
                                AddMessage($"训练结果:{resContent},偏移量：{result.Item3},{result.Item4}");
                                // device.SetInt
                                device.SetInt(id, ADDR_DONE, 444);
                                AddMessage("等待命令清除");
                                SpinWait.SpinUntil(() => device.GetInt(id, ADDR_CMD) != 444, 1000);
                                device.SetInt(id, ADDR_DONE, 0);
                            }
                            if (cmd == 555)
                            {//
                                AddMessage("拍照");
                                var result = await cognex.TakePhotoAsync(id, x / 1000d, y / 1000d);
                                AddMessage($"收到数据：{result.Item2}");
                                var resContent = result.Item1 ? "PASS" : "FAIL";
                                AddMessage($"拍照结果:{resContent},拍照数据：{result.Item3},{result.Item4},{result.Item5},{result.Item6},{result.Item7},{result.Item8},{result.Item9},{result.Item10}");
                                device.SetInt(id, ADDR_DONE, result.Item1 ? 555 : 999);
                                device.SetInt(id, 2, result.Item3);
                                device.SetInt(id, 4, result.Item4);
                                device.SetInt(id, 6, result.Item5);
                                device.SetInt(id, 8, result.Item6);
                                device.SetInt(id, 10, result.Item7);
                                device.SetInt(id, 12, result.Item8);
                                device.SetInt(id, 14, result.Item9);
                                device.SetInt(id, 16, result.Item10);
                                AddMessage("等待命令清除");
                                SpinWait.SpinUntil(() => device.GetInt(id, ADDR_CMD) != 555, 1000);
                                device.SetInt(id, ADDR_DONE, 0);
                                AddMessage("命令执行完毕");
                            }
                            //   device.SetInt(id, ADDR_DONE, 444);
                        }
                    }
                    catch (Exception ex)
                    {
                        AddMessage(ex.Message);
                        Logger.Log($"{ex.Message}{Environment.NewLine}{ex.StackTrace}", Prism.Logging.Category.Exception, Prism.Logging.Priority.None);
                        //   throw;
                    }
                    Thread.Sleep(1);
                }
            },TaskCreationOptions.LongRunning);
        }

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
    }
}