using Mv.Modules.P99.Service;
using Mv.Ui.Mvvm;
using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using Unity;

namespace Mv.Modules.P99.ViewModels
{
    public class AlarmItemVm : BindableBase
    {
        private string address;

        public string Address
        {
            get { return address; }
            set { SetProperty(ref address, value); }
        }

        private string message;

        public string Message
        {
            get { return message; }
            set { SetProperty(ref message, value); }
        }

        private string startTime;

        public string StartTime
        {
            get { return startTime; }
            set { SetProperty(ref startTime, value); }
        }

        private int timeStamp;

        public int TimeStamp
        {
            get { return timeStamp; }
            set { SetProperty(ref timeStamp, value); }
        }
    }

    public class AlarmsViewModel : ViewModelBase
    {
        private readonly IAlarmService alarmService;
        private readonly IRunTimeService runTimeService;
        private int unloadCount;
        public int UnloadCount
        {
            get { return unloadCount; }
            set { SetProperty(ref unloadCount, value); }
        }

        private double workTime;
        public double WorkTime
        {
            get { return workTime; }
            set { SetProperty(ref workTime, value); }
        }

        private int scanCodeNg;
        public int ScanCodeNg
        {
            get { return scanCodeNg; }
            set { SetProperty(ref scanCodeNg, value); }
        }



        private int loadCount;
        public int LoadCount
        {
            get { return loadCount; }
            set { SetProperty(ref loadCount, value); }
        }
        private double loopTime;
        public double Looptime
        {
            get { return loopTime; }
            set { SetProperty(ref loopTime, value); }
        }
        private double runtime;
        public double Runtime
        {
            get { return runtime; }
            set { SetProperty(ref runtime, value); }
        }




        private double downtime;
        public double Downtime
        {
            get { return downtime; }
            set { SetProperty(ref downtime, value); }
        }

        private int glueCameraNg;
        public int GlueCameraNg
        {
            get { return glueCameraNg; }
            set { SetProperty(ref glueCameraNg, value); }
        }

        private double idleTime;
        public double Idletime
        {
            get { return idleTime; }
            set { SetProperty(ref idleTime, value); }
        }

        private int loadcammeraNg;
        public int LoadCameraNg
        {
            get { return loadcammeraNg; }
            set { SetProperty(ref loadcammeraNg, value); }
        }



        public AlarmsViewModel(IUnityContainer container, IAlarmService alarmService, IRunTimeService runTimeService) : base(container)
        {
            this.alarmService = alarmService;
            this.runTimeService = runTimeService;
            Observable.Interval(TimeSpan.FromSeconds(0.5)).Subscribe(x =>
            {
                var alarmItems = alarmService.GetAlarmItems();
                Invoke(() =>
                {
                    AlarmItems.Clear();
                    alarmItems.ForEach(x => AlarmItems.Add(new AlarmItemVm
                    {
                        Address = x.Address,
                        Message = x.Message,
                        StartTime = x.StartTime.ToString(),
                        TimeStamp = (int)x.TimeSpan.TotalSeconds
                    }));
                    Downtime = runTimeService.Downtime;
                    GlueCameraNg = runTimeService.GlueCameraNg;
                    Idletime = runTimeService.Idletime;
                    LoadCameraNg = runTimeService.LoadCameraNg;
                    LoadCount = runTimeService.LoadCount;
                    Looptime = runTimeService.Looptime;
                    Runtime = runTimeService.Runtime;
                    ScanCodeNg = runTimeService.ScanCodeNg;
                    UnloadCount = runTimeService.UnloadCount;
                    WorkTime = runTimeService.Worktime;
                });

            });
        }

        public ObservableCollection<AlarmItemVm> AlarmItems { get; set; } = new ObservableCollection<AlarmItemVm>();
    }
}