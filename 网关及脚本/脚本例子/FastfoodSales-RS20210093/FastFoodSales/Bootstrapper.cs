using System;
using Stylet;
using StyletIoC;
using System.IO.Ports;
using DAQ.Service;
using System.Threading.Tasks;
using DAQ.Pages;
using HslCommunication.Core;
using HslCommunication.Profinet.Omron;
using HslCommunication.Profinet.Siemens;
using HslCommunication.Profinet.Melsec;
using Script.Methods;
using System.Threading;
using System.Windows;

namespace DAQ
{

    public class Bootstrapper : Bootstrapper<MainWindowViewModel>
    {

        private static System.Threading.Mutex mutex;
        protected override void ConfigureIoC(IStyletIoCBuilder builder)//2
        {
 
            // Configure the IoC container in here
            builder.Bind<IEventAggregator>().To<EventAggregator>().InSingletonScope();
            builder.Bind<TH2829Port>().ToSelf().InSingletonScope();
            builder.Bind<TH9320PortA>().ToSelf().InSingletonScope();
            builder.Bind<TH9320PortB>().ToSelf().InSingletonScope();
            builder.Bind<TH9320PortC>().ToSelf().InSingletonScope();

            builder.Bind<ScriptMgr>().ToSelf().InSingletonScope();

            builder.Bind<HomeViewModel>().ToSelf().InSingletonScope();
            builder.Bind<MsgViewModel>().ToSelf().InSingletonScope();
            builder.Bind<SettingsViewModel>().ToSelf().InSingletonScope();
            builder.Bind<MsgFileSaver<TLog>>().ToSelf();
            builder.Bind<PlcService>().ToSelf().InSingletonScope();
            builder.Bind<PLCViewModel>().ToSelf();
            builder.Bind<MainWindowViewModel>().ToSelf().InSingletonScope();
            builder.Bind<AlarmService>().ToSelf().InSingletonScope();
            builder.Autobind();
        }
        protected override void Configure()//3
        {

            // Perform any other configuration before the application starts
        }
        protected override void OnStart()//1
        {

            mutex = new System.Threading.Mutex(true, "OnlyRun_CRNS");
            if (mutex.WaitOne(0, false))
            {
                base.OnStart();
            }
            else
            {
                MessageBox.Show("程序已经在运行！", "提示");
                Application.Current.Shutdown();
            }
        }
        protected override void OnLaunch()//4
        {
            base.OnLaunch();
            var a = Container.Get<PlcService>();
            var e = Container.Get<ScriptMgr>();
            Task.Run(() => a.Connect());
            Task.Run(() => e.StartRun());        
        }

    }
}
