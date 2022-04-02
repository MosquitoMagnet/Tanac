using Mv.Core.Interfaces;
using Mv.Shell.Views.Dialogs;
using Mv.Ui.Mvvm;
using Prism.Regions;
using Unity;

namespace Mv.Shell.ViewModels.Dialogs
{
    public class SettingsDialogViewModel : ViewModelBase,IViewLoadedAndUnloadedAware<SettingsDialog>
    {
        private readonly IConfigureFile configure;

        public SettingsDialogViewModel(IUnityContainer container, IRegionManager regionManager,IConfigureFile configure) : base(container)
        {
            RegionManager = regionManager;
            this.configure = configure;
        }

        public IRegionManager RegionManager { get; }

        public void OnLoaded(SettingsDialog view)
        {
         //   throw new System.NotImplementedException();
        }

        public void OnUnloaded(SettingsDialog view)
        {
          
            //throw new System.NotImplementedException();
        }
    }

}
