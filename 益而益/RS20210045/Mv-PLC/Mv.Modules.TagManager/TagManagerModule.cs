using BatchCoreService;
using DataService;
using Mv.Modules.TagManager.Services;
using Mv.Modules.TagManager.Views;
using Mv.Modules.TagManager.Views.Dialogs;
using Mv.Modules.TagManager.Views.Messages;
using Mv.Modules.TagManager.Views.DashBoard;
using Mv.Ui.Core;
using Mv.Ui.Core.Modularity;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using Unity;

namespace Mv.Modules.TagManager
{

    [Module(ModuleName = "TagManager")]
    public class TagManagerModule : ModuleBase
    {
        private readonly IRegionManager _regionManager;

        public TagManagerModule(IUnityContainer container, IRegionManager regionManager) : base(container)
        {

            _regionManager = regionManager;
        }

        public override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            this.Container.RegisterType<IDriverDataContext, DriverDataContext>();
            this.Container.RegisterSingleton<IDataServer, DAService>();//singleton 单例
            this.Container.RegisterSingleton<IAlarmManager, AlarmManager>();
            containerRegistry.RegisterForNavigation<DriverConfiger>(); //将DriverConfiger注册为Navication（导航）,并且注册到容器中.
            containerRegistry.RegisterForNavigation<GroupMonitor>();
            containerRegistry.RegisterForNavigation<DriverMonitor>();
            containerRegistry.RegisterForNavigation<DriverEditer>();
            containerRegistry.RegisterForNavigation<DashBoardList>();
            containerRegistry.RegisterForNavigation<BoardView>();
            _regionManager.RegisterViewWithRegion(RegionNames.MainTabRegion, typeof(DashBoardCenter));

        }
        public override void OnInitialized(IContainerProvider containerProvider)
        {
            base.OnInitialized(containerProvider);
            containerProvider.Resolve<IAlarmManager>();
        }
    }
}