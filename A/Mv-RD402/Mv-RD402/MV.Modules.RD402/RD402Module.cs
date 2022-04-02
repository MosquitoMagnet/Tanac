using Mv.Modules.RD402.Service;
using Mv.Modules.RD402.ViewModels;
using Mv.Modules.RD402.Views;
using Mv.Ui.Core;
using Mv.Ui.Core.Modularity;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using Unity;

namespace Mv.Modules.RD402
{
    public class Rd402Module : ModuleBase
    {
        private readonly IRegionManager _regionManager;

        public override void OnInitialized(IContainerProvider containerProvider)
        {
            base.OnInitialized(containerProvider);
        }

        public override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton(typeof(DeviceReadWriter));
            containerRegistry.RegisterSingleton<IEpsonCommunication, EpsonCommunication>();                        
            containerRegistry.Register<IGetSn, DebugGetSn>("调试");
            containerRegistry.Register<IGetSn, checkmes>("ICT");
            containerRegistry.Register<IGetSn, CE023>("信维");
            containerRegistry.Register<IUpload, XinweiUpload>();
            containerRegistry.Register<IFactoryInfo, ICTFactory>("ICT");
            containerRegistry.Register<IFactoryInfo, SunwayFactory>("信维");
            containerRegistry.Register<IFactoryInfo, DebugFactory>("调试");
            containerRegistry.RegisterSingleton<IInkPrinter, InkPrinter>();
            containerRegistry.Register<IDeviceReadWriter, DeviceReadWriter>();
            _regionManager.RegisterViewWithRegion(RegionNames.MainTabRegion, typeof(Rd402Component));
            _regionManager.RegisterViewWithRegion(RegionNames.SettingsTabRegion, typeof(Rd402Setting));
            _regionManager.RegisterViewWithRegion(RegionNames.SettingsTabRegion, typeof(HiveDataSetting));
        }

        public Rd402Module(IUnityContainer container, IRegionManager regionManager) : base(container)
        {
            _regionManager = regionManager;
        }
    }
}