using Prism.Events;
using Prism.Logging;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Text;

namespace Mv.Modules.P99.Service
{
    public class RunTimeService : IRunTimeService
    {
        private readonly ILoggerFacade logger;
        private readonly IEventAggregator eventAggregator;
        private readonly IDeviceReadWriter device;

        public int LoadCount { get; private set; }
        public int UnloadCount { get; private set; }
        public int LoadCameraNg { get; private set; }
        public int GlueCameraNg { get; private set; }
        public int ScanCodeNg { get; private set; }
        public double Worktime { get; private set; }
        public double Runtime { get; private set; }
        public double Downtime { get; private set; }
        public double Looptime { get; private set; }
        public double Idletime { get; private set; }
        public RunTimeService(ILoggerFacade logger, IEventAggregator eventAggregator, IDeviceReadWriter device)
        {
            this.logger = logger;
            this.eventAggregator = eventAggregator;
            this.device = device;
            Observable.Interval(TimeSpan.FromSeconds(60*5)).Subscribe(x =>
            {
                LoadCount = device.GetInt(270);
                UnloadCount = device.GetInt(272);
                LoadCameraNg = device.GetInt(274);
                GlueCameraNg = device.GetInt(276);
                ScanCodeNg = device.GetInt(278);
                Worktime = Math.Round(device.GetInt(282) * 1.0 / 60,2);
                Runtime = Math.Round(device.GetInt(284) * 1.0 / 60,2);
                Downtime =Math.Round(device.GetInt(286) * 1.0 / 60,2);
                Looptime = device.GetInt(288) * 1.0 / 100;
                Idletime =Math.Round(device.GetInt(292) * 1.0 / 60,2);

                var dic = new Dictionary<string, string>();
                dic["时间"] = DateTime.Now.ToString();
                dic["上料数量"] = LoadCount.ToString();
                dic["下料数量"] = UnloadCount.ToString();
                dic["上料相机NG"] = LoadCameraNg.ToString();
                dic["点胶相机NG"] = GlueCameraNg.ToString();
                dic["扫码NG"] = ScanCodeNg.ToString();
                dic["运行信息"] = Runtime.ToString();
                dic["停机时间"] = Downtime.ToString();
                dic["周期时间"] = Looptime.ToString();
                dic["待机时间"] = Idletime.ToString();
                Helper.SaveFile($"./生产信息/{DateTime.Now:yyyyMMdd}.csv",dic);
            });

        }
    }
}
