using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Threading;
using Mv.Ui.Mvvm;
using MV.Core.Events;
using Prism.Commands;
using Prism.Regions;
using Prism.Events;
using Prism.Logging;
using Prism.Mvvm;
using Unity;
using Mv.Modules.TagManager.Models;
using DataService;
using System.Windows;
using Mv.Modules.TagManager.Views;
using Mv.Modules.TagManager.Views.DashBoard;
using Mv.Modules.TagManager.Services;
using Mv.Core.Interfaces;
using System.Timers;

namespace Mv.Modules.TagManager.ViewModels.DashBoard
{
    public class DashBoardListViewModel : ViewModelBase,INavigationAware
    {
        private readonly IAlarmManager alarmManager;
        private readonly IRegionManager regionManager;
        private readonly IConfigureFile _configure;
        private Config _config;
        private System.Timers.Timer timer;
        private readonly Dispatcher dispatcher = Dispatcher.CurrentDispatcher;
        public DashBoardListViewModel(IDataServer server, IAlarmManager alarmManager, IUnityContainer container, IRegionManager regionManager, IConfigureFile configureFile) : base(container)
        {
            this.alarmManager = alarmManager;
            this.regionManager = regionManager;
            this._configure = configureFile;
            _configure.ValueChanged += _configure_ValueChanged; ;
            _config = configureFile.GetValue<Config>(nameof(Config)) ?? new Config();
            timer = new System.Timers.Timer();
            timer.Interval = 30000;
            timer.Elapsed += Timer_Elapsed;
        }
        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            dispatcher.BeginInvoke(() =>
            {
                regionManager.RequestNavigate("Dash_CONTENT", nameof(BoardView));
            });
        }
        private void _configure_ValueChanged(object sender, Core.Interfaces.ValueChangedEventArgs e)
        {
            if (e.KeyName != nameof(Config)) return;
            var config = _configure.GetValue<Config>(nameof(Config));
            _config = config;
        }
        public string Title
        {
            get { return _config.Title; }
            set
            {
                _config.Title = value;
                SaveConfig();
            }
        }
        void SaveConfig()
        {
            _configure.SetValue(nameof(Config), _config);
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            timer.Enabled = true;
            timer.Start();
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
            timer.Enabled = false;
            timer.Stop();
        }

        public ObservableCollection<DeviceItem> DeviceItems
        {
            get => this.alarmManager.DeviceDatas;
        }
        #region NavigateToBackCommand 导航到主界面页面
        private DelegateCommand navigateToBackCommand;
        public DelegateCommand NavigateToBackCommand =>
        navigateToBackCommand ?? (navigateToBackCommand = new DelegateCommand(() =>
        {
            regionManager.RequestNavigate("Dash_CONTENT", nameof(BoardView));
        }));
        #endregion
        #region NavigateToEditorCommand 导航到标签服务器页面
        private DelegateCommand navigateToEditorCommand;
        public DelegateCommand NavigateToEditorCommand =>
        navigateToEditorCommand ?? (navigateToEditorCommand = new DelegateCommand(() =>
        {
            regionManager.RequestNavigate("Dash_CONTENT", nameof(DriverMonitor));
        }));
        #endregion
    }
}
