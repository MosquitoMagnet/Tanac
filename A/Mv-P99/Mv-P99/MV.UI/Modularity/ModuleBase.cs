using Prism.Ioc;
using Prism.Modularity;
using Unity;


namespace Mv.Ui.Core.Modularity
{
    public abstract class ModuleBase : IModule//抽象Module
    {
        protected IUnityContainer Container { get; }

        protected ModuleBase(IUnityContainer container) => Container = container;

        public virtual void RegisterTypes(IContainerRegistry containerRegistry) { }

        public virtual void OnInitialized(IContainerProvider containerProvider) { }
    }
}
