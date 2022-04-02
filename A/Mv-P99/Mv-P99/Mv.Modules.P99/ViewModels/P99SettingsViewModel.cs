using Mv.Core.Interfaces;
using Mv.Modules.P99.Views;
using Mv.Ui.Mvvm;
using Prism.Commands;
using Unity;

namespace Mv.Modules.P99.ViewModels
{
    public class P99SettingsViewModel : ViewModelBase,IViewLoadedAndUnloadedAware<P99Settings>
    {
        private readonly IConfigureFile configureFile;

        private DelegateCommand _saveConfig;

        public P99SettingsViewModel(IUnityContainer container, IConfigureFile configureFile) : base(container)
        {
            this.configureFile = configureFile;
            Config = configureFile.GetValue<P99Config>(nameof(P99Config)) ?? new P99Config();
        }

        public P99Config Config { get; set; }

        public void OnLoaded(P99Settings view)
        {
       //     throw new System.NotImplementedException();
        }

        public void OnUnloaded(P99Settings view)
        {
            Save();
    //        throw new System.NotImplementedException();
        }

        private void Save()
        {
            configureFile.SetValue(nameof(P99Config), Config);
        }
        public string Factory
        {
            get => Config.Factory;
            set
            {
                Config.Factory = value;
                RaisePropertyChanged(nameof(Factory));
            }
        }

        public static string[] Factories => new[] {"ICT", "Sunway", "LinYi"};
    }
}