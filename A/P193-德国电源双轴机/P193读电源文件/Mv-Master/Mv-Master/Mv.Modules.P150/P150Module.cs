using BatchCoreService;
using DataService;
using Mv.Modules.P150.Services;
using Mv.Modules.P150.Views;
using Mv.Modules.P150.Views.Dialogs;
using Mv.Modules.P150.Views.Messages;
using Mv.Ui.Core;
using Mv.Ui.Core.Modularity;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using Unity;


namespace Mv.Modules.P150
{
    [Module(ModuleName = "P150")]
    public class P150Module : ModuleBase
    {
        private readonly IRegionManager _regionManager;

        public P150Module(IUnityContainer container, IRegionManager regionManager) : base(container)
        {

            _regionManager = regionManager;
        }

        public override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            this.Container.RegisterType<IDriverDataContext, DriverDataContext>();
            this.Container.RegisterSingleton<IDataServer, DAService>();
            this.Container.RegisterSingleton<IAlarmManager, AlarmManager>();
            this.Container.RegisterSingleton<IUpload, Upload>();

            containerRegistry.RegisterForNavigation<DriverConfiger>();
            containerRegistry.RegisterForNavigation<GroupMonitor>();
            containerRegistry.RegisterForNavigation<DriverMonitor>();
            containerRegistry.RegisterForNavigation<DriverEditer>();
            _regionManager.RegisterViewWithRegion(RegionNames.MainTabRegion, typeof(TagEditor));//加载View
            _regionManager.RegisterViewWithRegion(RegionNames.MainTabRegion, typeof(MessageCenter));
            _regionManager.RegisterViewWithRegion(RegionNames.SettingsTabRegion, typeof(Setting));
        }
        public override void OnInitialized(IContainerProvider containerProvider)
        {
            base.OnInitialized(containerProvider);
             //var dataServer = containerProvider.Resolve<IDataServer>();
             var alarmmanager = containerProvider.Resolve<IAlarmManager>();
             var upload = containerProvider.Resolve<IUpload>();
        }
    }
}
