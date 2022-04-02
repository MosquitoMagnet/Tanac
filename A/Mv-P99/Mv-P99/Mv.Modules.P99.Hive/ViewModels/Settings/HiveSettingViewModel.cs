using System;
using System.Collections.Generic;
using System.Text;
using Mv.Core.Interfaces;
using Mv.Modules.P99.Hive.Services;
using Mv.Ui.Mvvm;
using Prism.Commands;
using MaterialDesignThemes.Wpf;
using Prism.Mvvm;
using Unity;

namespace Mv.Modules.P99.Hive.ViewModels
{
    public class HiveSettingViewModel : ViewModelBase
    {
        public P99HiveConfig Config { get; }
        private DelegateCommand _cmdSave;

        public DelegateCommand SaveCommand =>
            _cmdSave ??= new DelegateCommand(SaveConfig);

        private IConfigureFile _configure;

        void SaveConfig()
        {
            _configure.SetValue(nameof(P99HiveConfig), Config);

        }

        public HiveSettingViewModel(IUnityContainer container, IConfigureFile configure) :
            base(container)
        {
            _configure = configure;
            Config = configure.GetValue<P99HiveConfig>(nameof(P99HiveConfig)) ?? new P99HiveConfig();
        }
       
        #region Upload  
        public bool isUpload
        {
            get => Config.isUpload;
            set => Config.isUpload = value;
        }
        #endregion

    }
}


