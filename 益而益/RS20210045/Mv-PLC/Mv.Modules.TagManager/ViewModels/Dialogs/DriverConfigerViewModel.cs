using BatchCoreService;
using DataService;
using MaterialDesignThemes.Wpf;
using Mv.Modules.TagManager.Views.Dialogs;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;

namespace Mv.Modules.TagManager.ViewModels.Dialogs
{
    public class DriverConfigerViewModel : BindableBase, INavigationAware
    {
        public DriverConfigerViewModel()
        {

        }
        private Driver driver;
        public Driver Driver
        {
            get { return driver; }
            set { SetProperty(ref driver, value); }
        }

        public ObservableCollection<Group> Groups=>driver!=null? new ObservableCollection<Group>(driver?.Groups):null ;


        private DelegateCommand _addGroupCommand;
        public DelegateCommand AddGroupCommand =>
            _addGroupCommand ?? (_addGroupCommand = new DelegateCommand(async () => await AddGroupAsync()));

        private DelegateCommand<Group> removeGroupCommand;
        public DelegateCommand<Group> RemoveGroupCommand =>
            removeGroupCommand ?? (removeGroupCommand = new DelegateCommand<Group>(group =>
            {
                Driver.Groups.Remove(group);
                RaisePropertyChanged(nameof(Groups));
            }));

        private async Task AddGroupAsync()
        {

            var dlg = new AddGroupDlg();
            var result = await DialogHost.Show(dlg, "RootDialog");
            if (result.ToString() == "OK")
            {
                var vm = (dlg.DataContext as AddGroupDlgViewModel);
                Driver.Groups.Add(new Group()
                {
                    DriverId = Driver.Id,
                    Name = vm.Name,
                    DeadBand = vm.DeadBand,
                    Active = vm.Active,
                    UpdateRate = vm.UpdateRate
                });

                this.RaisePropertyChanged(nameof(Groups));
            }
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            if (navigationContext.Parameters[nameof(Driver)] is Driver dv)
            {
                if (dv != null)
                {
                    Driver = dv;           
                }
            }
            this.RaisePropertyChanged(nameof(Groups));
            ///hrow new NotImplementedException();
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
  
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
            //throw new NotImplementedException();
        }
    }
}
