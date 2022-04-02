using BatchCoreService;
using DataService;
using Mv.Modules.RD402.Hive.Services;
using Mv.Modules.RD402.Hive.Views;
using Mv.Modules.RD402.Hive.Views.Dialogs;
using Mv.Modules.RD402.Hive.Views.Messages;
using Mv.Ui.Core;
using Mv.Ui.Core.Modularity;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using Unity;

namespace Mv.Modules.RD402.Hive
{
    [Module(ModuleName = "RD402Hive")]
    public class RD402HiveModule : ModuleBase
    {
        private readonly IRegionManager _regionManager;

        public RD402HiveModule(IUnityContainer container, IRegionManager regionManager) : base(container)
        {

            _regionManager = regionManager;
        }

        public override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            this.Container.RegisterType<IDriverDataContext, DriverDataContext>();
            this.Container.RegisterSingleton<IDataServer, DAService>();
            this.Container.RegisterSingleton<IAlarmManager, AlarmManager>();
            this.Container.RegisterSingleton<IHiveUpload, HiveUpload>();
            containerRegistry.RegisterForNavigation<DriverConfiger>();
            containerRegistry.RegisterForNavigation<GroupMonitor>();
            containerRegistry.RegisterForNavigation<DriverMonitor>();
            containerRegistry.RegisterForNavigation<DriverEditer>();
            _regionManager.RegisterViewWithRegion(RegionNames.MainTabRegion, typeof(TagEditor));//加载View
            _regionManager.RegisterViewWithRegion(RegionNames.SettingsTabRegion, typeof(HiveSetting));
        }
        public override void OnInitialized(IContainerProvider containerProvider)
        {
            base.OnInitialized(containerProvider);
             //var dataServer = containerProvider.Resolve<IDataServer>();
             var alarmmanager = containerProvider.Resolve<IAlarmManager>();
             var upload = containerProvider.Resolve<IHiveUpload>();
        }
    }
}
