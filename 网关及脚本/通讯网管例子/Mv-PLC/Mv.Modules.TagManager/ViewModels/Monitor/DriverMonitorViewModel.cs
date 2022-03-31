using BatchCoreService;
using DataService;
using Mv.Modules.TagManager.Views;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using PropertyTools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace Mv.Modules.TagManager.ViewModels
{
    public class DriverVm : BindableBase
    {

        public DriverVm(IDriver driver)
        {
            this.Driver = driver;
            this.Name = driver.Name;
            this.ServerName = driver.ServerName;
            this.IsOpen = !driver.IsClosed;
            System.Reactive.Linq.Observable.Interval(TimeSpan.FromSeconds(1000)).Subscribe(x =>
            {
                IsOpen = !driver.IsClosed;
            });
        }
        private string name;
        public string Name
        {
            get { return name; }
            private set { SetProperty(ref name, value); }
        }
        private string serverName;

        public string ServerName
        {
            get { return serverName; }
            private set { SetProperty(ref serverName, value); }
        }
        private bool isClosed;
        public bool IsOpen
        {
            get { return isClosed; }
            private set { SetProperty(ref isClosed, value); }
        }

        public IDriver Driver { get; }

    }

    public class DriverMonitorViewModel : BindableBase, INavigationAware
    {

        public ObservableCollection<DriverVm> Drivers =>
            new ObservableCollection<DriverVm>(DataServer.Drivers.Select(x => new DriverVm(x)).ToArray());

        public IDataServer DataServer { get; set; }
        private readonly IRegionManager regionManager;
        public DriverMonitorViewModel(IDataServer server, IRegionManager regionManager)
        {
            this.DataServer = server;
            this.regionManager = regionManager;
            RaisePropertyChanged(nameof(Drivers));
        }

        private DelegateCommand _runCommand;
        public DelegateCommand RunCommand =>
            _runCommand ?? (_runCommand = new DelegateCommand(async () => await ExecuteRunCommand()));

        async Task ExecuteRunCommand()
        {
            await Task.Run(() =>
             {
                // DataServer.Run();
             });
            RaisePropertyChanged(nameof(Drivers));
        }


        private DelegateCommand _stopCommand;
        public DelegateCommand StopCommand =>
            _stopCommand ?? (_stopCommand = new DelegateCommand(async () => await ExecuteStopCommandAsync()));

        async Task ExecuteStopCommandAsync()
        {
            await Task.Run(() =>
           {
               DataServer.Stop();
           });
        }


        #region ShowGroupsCommand 显示分组信息
        private DelegateCommand<DriverVm> _showGroupsCommand;
        public DelegateCommand<DriverVm> ShowGroupsCommand =>
            _showGroupsCommand ?? (_showGroupsCommand = new DelegateCommand<DriverVm>(ShowGroup));
        private void ShowGroup(DriverVm driver)
        {
            if (driver == null)
                return;
            regionManager.RequestNavigate("MONITOR_DETAIL", nameof(GroupMonitor), new NavigationParameters
            {
                { nameof(IDriver), driver.Driver }
            });

        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            RunCommand.Execute();
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
            // throw new NotImplementedException();
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
            //  throw new NotImplementedException();
        }
        #endregion

        #region NavigateToEditorCommand 导航到编辑页面
        private DelegateCommand navigateToEditorCommand;
        public DelegateCommand NavigateToEditorCommand =>
        navigateToEditorCommand ?? (navigateToEditorCommand = new DelegateCommand(() =>
        {
            regionManager.RequestNavigate("TAG_CONTENT", nameof(DriverEditer));
        }));
        #endregion






    }
}
