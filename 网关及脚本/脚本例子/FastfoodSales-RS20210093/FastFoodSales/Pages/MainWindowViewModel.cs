using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stylet;
using StyletIoC;
using DAQ.Pages;
using DAQ.Service;
using MaterialDesignThemes.Wpf;
using System.Windows;

namespace DAQ
{
    public class MainWindowViewModel : Conductor<object>, IHandle<AlarmItem>
    {

        private string password="";
        public static bool authbool { get; set; } = false;
        int index = 0;
        [Inject]
        public IEventAggregator Events { get; set; }

        [Inject]
        public HomeViewModel Home { get; set; }
        [Inject]
        public SettingsViewModel Setting { get; set; }
        [Inject]
        public MsgViewModel Msg { get; set; }
        [Inject]
        public PLCViewModel PLC { get; set; }

        public bool IsDialogOpen { get { return AlarmList.Count > 0; } }

        public BindableCollection<AlarmItem> AlarmList { get; set; } = new BindableCollection<AlarmItem>();

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

        protected override void OnActivate()
        {
            Events.Subscribe(this);
            base.OnActivate();
        }
        protected override void OnInitialActivate()
        {
            ActivateItem(Home);
            ActiveValues();
            base.OnInitialActivate();
        }

        public void ShowSetting()
        {
            if (authbool)
                ActivateItem(Setting);
            else
                MessageBox.Show("请先登录权限");
        }

        public async Task LoginUser()
        {
            var vm = new LoginUserViewModel(password);
            var dlg = new LoginUser() { DataContext = vm };
            var result = await DialogHost.Show(dlg);
            if (result.ToString() == "OK")
            {

                if(vm.Value == "tanac123456")
                {
                    MessageBox.Show("权限登录成功");
                    authbool = true;
                    password = "######";

                }
                else
                {
                    MessageBox.Show("密码错误,权限登录失败");
                    authbool = false;
                    password = "";
                }

            }
            else
            {
                authbool = false;
                password = "";
            }


        }

        public void ActiveValues()
        {
            CurrentPage = PLC;
        }
        public void ActiveMessages()
        {
            CurrentPage = Msg;
        }

        public void Handle(AlarmItem message)
        {
            if (!message.Value)
            {
                if(AlarmList.Any(x=>x.Address==message.Address))
                {
                    var a = AlarmList.Where(x => x.Address == message.Address);
                    foreach(var v in a)
                    {
                        AlarmList.Remove(v);
                    }
                }               
            }
            else
            {
                AlarmList.Add(message);
            }
        }
    }
}
