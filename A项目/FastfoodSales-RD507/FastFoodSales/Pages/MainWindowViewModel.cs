using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stylet;
using StyletIoC;
using DAQ.Pages;
using DAQ.Core.i18n;
using MaterialDesignThemes.Wpf;
using RobotFrmUI;

namespace DAQ
{
    public class MainWindowViewModel : Conductor<object>,IHandle<MsgItem>
    {
        ISnackbarMessageQueue queue = new SnackbarMessageQueue();
        int index = 0;
        IEventAggregator Events { get;  }

        [Inject]
        public HomeViewModel Home { get; set; }
        [Inject]
        public SettingsViewModel Setting { get; set; }
        [Inject]
        public MsgViewModel Msg { get; set; }
        [Inject]
        public PLCViewModel PLC { get; set; }

        private IConfigureFile configure;
        private Config config;
        public MainWindowViewModel( IEventAggregator Events, IConfigureFile configure)
        {
            this.configure = configure;
            config = configure.Load().GetValue<Config>(nameof(Config)) ?? new Config();
            TranslationSource.Instance.Language = config.Language;
            this.Events = Events;
            Events.Subscribe(this);
        }


        public object CurrentPage { get; set; }
        public int Index
        {
            get { return index; }
            set
            {
                index = value;

                switch (index)
                {
                    case 0:
                        ActivateItem(Home);
                        break;

                    case 1:
                        ActivateItem(Msg);
                        break;
                    case 2:
                        ActivateItem(new AboutViewModel());
                        break;
                }

            }
        }

        public ISnackbarMessageQueue Queue { get => queue; set => queue = value; }

        protected override void OnActivate()
        {
            base.OnActivate();
        }
        protected override void OnInitialActivate()
        {
            ActivateItem(Home);
            ActiveSetting();
            base.OnInitialActivate();
        }

        public void ActiveSetting()
        {
            CurrentPage = Setting;
        }
        public void ActiveMessages()
        {
            CurrentPage = Msg;
        }

        public void Handle(MsgItem message)
        {
            queue.Enqueue(message.Value);
        }
    }
}
