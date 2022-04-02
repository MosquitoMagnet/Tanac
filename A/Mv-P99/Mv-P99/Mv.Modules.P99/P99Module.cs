using Mv.Modules.P99.Service;
using Mv.Modules.P99.Views;
using Mv.Ui.Core;
using Mv.Ui.Core.Modularity;
using Prism.Events;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using Unity;

namespace Mv.Modules.P99
{
    public class P99Module : ModuleBase
    {
        private readonly IRegionManager _regionManager;

        public P99Module(IUnityContainer container, IRegionManager regionManager) : base(container)
        {
            _regionManager = regionManager;       
        }

        public override void RegisterTypes(IContainerRegistry containerRegistry)
        {       
            containerRegistry.RegisterSingleton<IDeviceReadWriter, ModbusDeviceReadWriter>();
            containerRegistry.RegisterSingleton<IPlcCognexComm, PlcCognexComm>();
            containerRegistry.RegisterSingleton<IPlcScannerComm, PlcScannerComm>();
            containerRegistry.RegisterSingleton<ICognexCommunication, CognexCommunication>();
            containerRegistry.RegisterSingleton<IAlarmService, AlarmService>();
            containerRegistry.RegisterSingleton<IRunTimeService, RunTimeService>();
            containerRegistry.RegisterSingleton<IScannerComm, ScannerComm>();
            containerRegistry.RegisterSingleton<IEpson2Cognex, Epson2Cognex>();
            containerRegistry.RegisterSingleton<IOPTLight, OPTLight>();

            containerRegistry.Register<IFactoryInfo, ICTFactory>("ICT");
            containerRegistry.Register<IFactoryInfo, LinYiFactory>("LinYi");

            _regionManager.RegisterViewWithRegion(RegionNames.MainTabRegion, typeof(P99Component));
            _regionManager.RegisterViewWithRegion(RegionNames.SettingsTabRegion, typeof(P99Settings));
            _regionManager.RegisterViewWithRegion(RegionNames.MainTabRegion, typeof(Cognex));
            _regionManager.RegisterViewWithRegion(RegionNames.MainTabRegion, typeof(Alarms));
        }
    }
}