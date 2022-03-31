using Mv.Core.I18n;
using Mv.Ui.Core;
using Mv.Ui.Mvvm;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity;

namespace Mv.Shell.ViewModels
{
    public class ShellSettingsTabItemViewModel : ViewModelBase
    {

        private RequestTypeValue requestTypeItem;

        public ShellSettingsTabItemViewModel(IUnityContainer container) : base(container)
        {
        }

        public RequestTypeValue RequestTypeItem
        {
            get { return requestTypeItem; }
            set
            {
                SetProperty(ref requestTypeItem, value);

                TranslationSource.Instance.Language = EnumHelper.GetDescription(requestTypeItem);

            }
        }

        private DelegateCommand<bool?> _setAutoStart;
        public DelegateCommand<bool?> SetAutoStart =>
            _setAutoStart ?? (_setAutoStart = new DelegateCommand<bool?>(ExecuteSetAutoStart));

        void ExecuteSetAutoStart(bool? isAutoStart)
        {
            ProcessController.RunWhenStart(isAutoStart.HasValue?isAutoStart.Value:false);
        }
    }
}

