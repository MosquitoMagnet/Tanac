using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Threading;
using Mv.Ui.Mvvm;
using System.Text.RegularExpressions;
using MV.Core.Events;
using Prism.Commands;
using Prism.Regions;
using Prism.Events;
using Prism.Logging;
using Prism.Mvvm;
using Unity;
using Mv.Modules.TagManager.Models;
using System.Windows;
using Mv.Modules.TagManager.Views;
using Mv.Modules.TagManager.Views.DashBoard;
using System.Timers;
using Mv.Modules.TagManager.Services;
using Mv.Core.Interfaces;

namespace Mv.Modules.TagManager.ViewModels.DashBoard
{
    public class BoardViewModel : ViewModelBase,INavigationAware
    {
        private readonly IRegionManager regionManager;
        private readonly IAlarmManager alarmManager;
        private readonly IConfigureFile _configure;
        private System.Timers.Timer timer;
        private System.Timers.Timer timer2;
        private Config _config;
        string[] week = new string[] {"日", "一", "二","三","四","五","六" };
        private readonly Dispatcher dispatcher = Dispatcher.CurrentDispatcher;
        public ObservableCollection<AlarmItem> AlarmItems { get; set; } = new ObservableCollection<AlarmItem>();
        public BoardViewModel(IUnityContainer container, IRegionManager regionManager, IConfigureFile configureFile, IAlarmManager alarmManager) : base(container)
        {
            this.regionManager = regionManager;
            this.alarmManager = alarmManager;
            this._configure = configureFile;
            _configure.ValueChanged += _configure_ValueChanged;
            _config = configureFile.GetValue<Config>(nameof(Config)) ?? new Config();
            EventAggregator.GetEvent<AlarmItemEvent>().Subscribe(x =>
            {
                Invoke(() =>
                {
                  AlarmItems.Clear();
                  foreach(var a in x.Alarms)
                  {
                        AlarmItems.Add(a);
                  }
                });
            });
            CurrentTime = DateTime.Now;
            CurrentWeek = week[(int)DateTime.Now.DayOfWeek];
            timer = new System.Timers.Timer();
            timer.Interval = 1000;
            timer.Elapsed += Timer_Elapsed;
            timer.Start();

            timer2 = new System.Timers.Timer();
            timer2.Interval = 60000;
            timer2.Elapsed += Timer2_Elapsed;
        }

        private void Timer2_Elapsed(object sender, ElapsedEventArgs e)
        {
            dispatcher.BeginInvoke(() =>
            {
                regionManager.RequestNavigate("Dash_CONTENT", nameof(DashBoardList));
            });
        }

        private void _configure_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            if (e.KeyName != nameof(Config)) return;
            var config = _configure.GetValue<Config>(nameof(Config));
            _config = config;
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            CurrentTime = DateTime.Now;
            CurrentWeek=week[(int)DateTime.Now.DayOfWeek];
            CurrentYield = this.alarmManager.DeviceDatas[8].Yield;
            Prod_N = this.alarmManager.DeviceDatas[8].Prod_N;
            Prod_Y = CurrentYield - Prod_N / 1000;
            if (PlanYield == 0)
            {
                Rate = 0;
            }
            else
            {
                double a = (double)CurrentYield / (double)PlanYield;
                Rate = Math.Round(a * 100, 1);
                if (CurrentYield == 0)
                {
                    Rate1 = 0;
                }
                else
                {
                    double b = (double)Prod_Y / (double)CurrentYield;
                    Rate1 = Math.Round(b * 100, 1);
                }
            }
        }
        #region 属性
        private DateTime currentTime;
        public DateTime CurrentTime
        {
            get { return currentTime; }
            set { SetProperty(ref currentTime, value); }
        }
        private string currentWeek;
        public string CurrentWeek
        {
            get { return currentWeek; }
            set { SetProperty(ref currentWeek, value); }
        }
        public ObservableCollection<DeviceItem> DeviceItems
        {
            get => this.alarmManager.DeviceDatas;
        }

        private int currentYield;
        public int CurrentYield
        {
            get { return currentYield; }
            set { SetProperty(ref currentYield, value); }
        }
        private int prod_N;
        public int Prod_N
        {
            get { return prod_N; }
            set { SetProperty(ref prod_N, value); }
        }
        
        private int prod_Y;
        public int Prod_Y
        {
            get { return prod_Y; }
            set { SetProperty(ref prod_Y, value); }
        }

        public int PlanYield
        {
            get { return _config.PlanYield; }
            set
            {
               _config.PlanYield=value;
                SaveConfig();
             }
        }
        public string Shift
        {
            get { return _config.Shift; }
            set
            {
                _config.Shift = value;
                SaveConfig();
            }
        }
        public string Line
        {
            get { return _config.Line; }
            set
            {
                _config.Line = value;
                SaveConfig();
            }
        }
        public string PersonNumber
        {
            get { return _config.PersonNumber; }
            set
            {
                _config.PersonNumber= value;
                SaveConfig();
            }
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
        public string MachineType
        {
            get { return _config.MachineType; }
            set
            {
                _config.MachineType = value;
                SaveConfig();
            }
        }
        private double rate;
        public double Rate
        {
            get { return rate; }
            set { SetProperty(ref rate, value); }
        }
        private double rate1;
        public double Rate1
        {
            get { return rate1; }
            set { SetProperty(ref rate1, value); }
        }
        void SaveConfig()
        {
            _configure.SetValue(nameof(Config), _config);
        }
        #endregion

        public void OnNavigatedTo(NavigationContext navigationContext)//导航后目的页面触发，一般用于初始化或者接受上页面的传递参数
        {
            timer2.Enabled = true;
            timer2.Start();
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)//True则重用该View实例，Flase则每一次导航到该页面都会实例化一次
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)//导航到其他页面发生,导航之前触发,一般用于保存该页面的数据
        {
            timer2.Enabled = false;
            timer2.Stop();
        }

        #region NavigateToEditorCommand 导航到机台详细页面
        private DelegateCommand navigateToEditorCommand;
        public DelegateCommand NavigateToEditorCommand =>
        navigateToEditorCommand ?? (navigateToEditorCommand = new DelegateCommand(() =>
        {
            regionManager.RequestNavigate("Dash_CONTENT", nameof(DashBoardList));
        }));
        #endregion

    }    
}
