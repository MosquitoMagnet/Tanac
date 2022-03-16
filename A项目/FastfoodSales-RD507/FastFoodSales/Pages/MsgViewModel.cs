using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stylet;
using StyletIoC;
using DAQ.Service;
using System;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Windows.Threading;


namespace DAQ.Pages
{

    public class AlarmItemVm
    {

        public string Address { get; set; }

        public string Message { get; set; }

        public string MessageE { get; set; }

        public string StartTime { get; set; }
        public int TimeStamp { get; set; }
    }



    public class MsgViewModel:IHandle<MsgItem>
    {
        IEventAggregator @events;
        private readonly IAlarmService alarms;
        private readonly Dispatcher dispatcher = Dispatcher.CurrentDispatcher;
        public MsgViewModel(IEventAggregator @event, IAlarmService alarmService)
        {
            @events = @event;
            events.Subscribe(this);
            this.alarms = alarmService;
            Observable.Interval(TimeSpan.FromSeconds(0.5)).Subscribe(x =>
            {
                var alarmItems = alarms.GetAlarmItems();
                dispatcher.BeginInvoke((Action)delegate ()
                {
                    AlarmItems.Clear();
                    alarmItems.ForEach(xm => AlarmItems.Add(new AlarmItemVm
                    {
                        Address = xm.Address,
                        Message = xm.Message,
                        StartTime = xm.StartTime.ToString(),
                        TimeStamp = (int)xm.TimeSpan.TotalSeconds
                    }));
                });




            });
        }
        public BindableCollection<MsgItem> MsgItems { get; set; } = new BindableCollection<MsgItem>();

        public BindableCollection<AlarmItemVm> AlarmItems { get; set; } = new BindableCollection<AlarmItemVm>();

        public BindableCollection<string> Messages { get; set; } = new BindableCollection<string>();

        public void Handle(MsgItem message)
        {
            MsgItems.Insert(0,message);
            if(MsgItems.Count>20)
            {
                MsgItems.RemoveAt(MsgItems.Count-1);
            }
        }
        protected virtual void Invoke(Action action) => OnUIThread(action);
        private void OnUIThread(Action action)
        {
            try
            {
                Dispatcher.CurrentDispatcher.BeginInvoke(action);
            }
            catch (Exception ex)
            {
                ;
                //        throw;
            }
        }
    }

}

