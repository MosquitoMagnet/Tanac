using System;
using Stylet;
using StyletIoC;
using System.IO.Ports;
using System.Threading.Tasks;
using DAQ.Pages;
using DAQ.Service;
using Communication.Core;
using Communication.Profinet.Omron;
using Communication.Profinet.Siemens;

namespace DAQ
{
    public class Bootstrapper : Bootstrapper<MainWindowViewModel>
    {
        protected override void ConfigureIoC(IStyletIoCBuilder builder)//2
        {
            // Configure the IoC container in here
            builder.Bind<IEventAggregator>().To<EventAggregator>().InSingletonScope();
            builder.Bind<IConfigureFile>().To<ConfigureFile>().InSingletonScope();
            builder.Bind<IAlarmService>().To<AlarmService>().InSingletonScope();         
            builder.Bind<IDeviceReadWriter>().To<ModbusDeviceReadWriter>().InSingletonScope();
            builder.Bind<IAirParticleDetector>().To<AirParticleDetector>().InSingletonScope();
            builder.Bind<IInkPrinter>().To<InkPrinter>().InSingletonScope();
            builder.Bind<IRotbotService>().To<RotbotService>().InSingletonScope();
            builder.Bind<IFactoryInfo>().To<DebugFactory>().InSingletonScope();
            builder.Bind<HomeViewModel>().ToSelf().InSingletonScope();
            builder.Bind<MsgViewModel>().ToSelf().InSingletonScope();
            builder.Bind<SettingsViewModel>().ToSelf().InSingletonScope();
            builder.Bind<PLCViewModel>().ToSelf();
            builder.Bind<MainWindowViewModel>().ToSelf().InSingletonScope();
            builder.Autobind();

        }

        protected override void Configure()//3
        {

            // Perform any other configuration before the application starts
        }
        protected override void OnStart()//1
        {

            base.OnStart();
        }
        protected override void OnLaunch()//4
        {

            base.OnLaunch();
        }

    }
}
