using System;
using Mv.Ui.Core.Modularity;
using Prism.Ioc;
using Prism.Regions;
using Unity;

namespace Mv.Modules.TagService
{
    public class TagService : ModuleBase
    {
        private readonly IRegionManager _regionManager;

        public TagService(IUnityContainer container, IRegionManager regionManager) : base(container)
        {
            _regionManager = regionManager;
        }

        public override void RegisterTypes(IContainerRegistry containerRegistry)
        {
        }
    }
}