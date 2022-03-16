using DataService;
using Prism.Events;
using Prism.Logging;
using Prism.Regions;
using PropertyTools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Reactive.Linq;
using MV.Core.Events;
using Mv.Core.Interfaces;
using System.Windows.Threading;
using System.IO;
using System.Globalization;
using System.Threading;
using CsvHelper;
using CsvHelper.Configuration.Attributes;
using System.Text.RegularExpressions;
using Mv.Modules.TagManager.Models;
using Mv.Modules.TagManager.Views.DashBoard;
using System.Windows;

namespace Mv.Modules.TagManager.Services
{
    public struct Edge
    {
        public bool ValueChanged { get; private set; }
        public int OldValue { get; private set; }
        private int _currentValue;
        public int CurrentValue
        {
            get => _currentValue;
            set
            {
                ValueChanged = ((_currentValue) != (value));
                OldValue = _currentValue;
                _currentValue = value;
            }
        }
    }
    public interface IAlarmManager
    {
        public ObservableCollection<DeviceItem> DeviceDatas { get; set; }

    }
    public class AlarmManager : IAlarmManager
    {

        public ObservableCollection<DeviceItem> DeviceDatas { get; set; } = new ObservableCollection<DeviceItem>();
        public List<Models.AlarmItem> AlarmItems2 { get; set; } = new List<Models.AlarmItem>();
        internal class AlarmInfoRecord
        {
            [Index(0)] //第0列
            public string Type{ get; set; }
            [Index(1)]//第1列
            public string Index{ get; set; }
            [Index(2)]
            public string Description { get; set; }
            [Index(3)]
            public string Address { get; set; }
        }
        internal class AlarmInfo
        {
            public string Type { get; set; }
            public int Index { get; set; }
            public string Description { get; set; }
            public string Address { get; set; }
        }
        private readonly IDataServer server;
        private readonly IEventAggregator @event;
        private readonly ILoggerFacade logger;
        private readonly IRegionManager regionManager;
        private Edge[] edge = new Edge[9] { new Edge(), new Edge(), new Edge(), new Edge(), new Edge(), new Edge(), new Edge(), new Edge(), new Edge() };
        private Edge[] edges = new Edge[9] { new Edge(), new Edge(), new Edge(), new Edge(), new Edge(), new Edge(), new Edge(), new Edge(), new Edge() };
        private string[] name = new string[] { "点胶1", "点胶2", "卷线1", "卷线2", "卷线3", "卷线4", "卷线5", "卷线6", "焊锡站" };
        private string[] adress = new string[] { "401000", "401016", "401100", "401116", "401200", "401216", "401300", "401316", "401400", "401416", "401500", "401516", "401600", "401616", "401700", "401716", "401800", "401816", };
        private readonly Dispatcher dispatcher = Dispatcher.CurrentDispatcher;
        public AlarmManager(IDataServer server, IEventAggregator @event, ILoggerFacade logger,IRegionManager regionManager)
        {
            this.server = server;
            this.@event = @event;
            this.logger = logger;
            this.regionManager = regionManager;
            System.Timers.Timer timer = new System.Timers.Timer();
            timer.Enabled = true; //是否引发Elapsed 事件
            timer.Interval = 1800000;
            timer.Start();
            timer.Elapsed += new System.Timers.ElapsedEventHandler(write);
            void write(object source, ElapsedEventArgs e)
            {
                Dictionary<string, string> dictionary = new Dictionary<string, string>();
                dictionary["点胶1产量"] = server["点胶1产量"].ToString();
                dictionary["点胶2产量"] = server["点胶2产量"].ToString();
                dictionary["卷线1产量"] = server["卷线1产量"].ToString();
                dictionary["卷线2产量"] = server["卷线2产量"].ToString();
                dictionary["卷线3产量"] = server["卷线3产量"].ToString();
                dictionary["卷线4产量"] = server["卷线4产量"].ToString();
                dictionary["卷线5产量"] = server["卷线5产量"].ToString();
                dictionary["卷线6产量"] = server["卷线6产量"].ToString();
                dictionary["焊锡站产量"] = server["焊锡站产量"].ToString();
                dictionary["时间"] = DateTime.Now.ToString("T");
                CsvHelper.WriteCsv($"./RunLog/Yield/{DateTime.Now:yyyyMM}/{DateTime.Now:yyyyMMdd}.csv", dictionary);
            }
            LoadAlarmInfos();
            DeviceDatas.Add(new DeviceItem { Name = "点胶1" });
            DeviceDatas.Add(new DeviceItem { Name = "点胶2" });
            DeviceDatas.Add(new DeviceItem { Name = "卷线1" });
            DeviceDatas.Add(new DeviceItem { Name = "卷线2" });
            DeviceDatas.Add(new DeviceItem { Name = "卷线3" });
            DeviceDatas.Add(new DeviceItem { Name = "卷线4" });
            DeviceDatas.Add(new DeviceItem { Name = "卷线5" });
            DeviceDatas.Add(new DeviceItem { Name = "卷线6" });
            DeviceDatas.Add(new DeviceItem { Name = "焊锡站" });
            DeviceDatas.Add(new DeviceItem { Name = "备用1" });
            DeviceDatas.Add(new DeviceItem { Name = "备用2" });
            DeviceDatas.Add(new DeviceItem { Name = "备用3" });
            DeviceDatas.Add(new DeviceItem { Name = "备用4" });
            DeviceDatas.Add(new DeviceItem { Name = "备用5" });
            DeviceDatas.Add(new DeviceItem { Name = "备用6" });
            DeviceDatas.Add(new DeviceItem { Name = "备用7" });
            DeviceDatas.Add(new DeviceItem { Name = "备用8" });
            DeviceDatas.Add(new DeviceItem { Name = "备用9" });       
            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    try
                    {
                        for (int i = 0; i < 9; i++)
                        {
                            DeviceDatas[i].State = server[Enum.GetName(typeof(Station), i * 5)].Value.Int32;
                            DeviceDatas[i].Yield = server[Enum.GetName(typeof(Station), i * 5 + 1)].Value.Int32;
                            DeviceDatas[i].Prod_N = server[Enum.GetName(typeof(Station), i * 5 + 2)].Value.Int32;
                            DeviceDatas[i].Cycle = server[Enum.GetName(typeof(Station), i * 5 + 3)].Value.Int32;
                            DeviceDatas[i].Errcode = server[Enum.GetName(typeof(Station), i * 5 + 4)].Value.Int32;
                            if (DeviceDatas[i].State == 1000 || DeviceDatas[i].State == 2000 || DeviceDatas[i].State == 4000 || DeviceDatas[i].State == 0)
                            {
                                DeviceDatas[i].Summary = 1;
                                updata(i, name[i], false);
                            }
                            else if (DeviceDatas[i].State == 3000)
                            {
                                DeviceDatas[i].Summary = 2;
                                updata(i, name[i], false);
                            }
                            else
                            {
                                DeviceDatas[i].Summary = 0;
                                updata(i, name[i], true);
                            }
                        }
                        for (int j = 0; j < 9;j++)
                        {
                            edge[j].CurrentValue = server[Enum.GetName(typeof(Station), j * 5)].Value.Int32;
                            edges[j].CurrentValue = server[Enum.GetName(typeof(Station), j * 5 + 4)].Value.Int32;
                        }

                        Thread.Sleep(500);
                    }

                    catch (Exception ex)
                    {

                    }
                }           
            }, TaskCreationOptions.LongRunning); //产量显示
        }
        #region 写入相关报警
        private void updata(int index, string name,bool isError)
        {
            if (isError)
            {
                var alarmTypes = alarms.Where(x => x.Type == name).ToList();//拿到对应工位所有报警信息
                AlarmInfo alarm;
                for (int j=0;j<2;j++)
                {
                    if(j==0)
                       alarm = alarmTypes.Where(x =>  x.Index == edge[index].CurrentValue && x.Address == adress[index*2]).FirstOrDefault();
                    else
                       alarm = alarmTypes.Where(x => x.Index == edges[index].CurrentValue && x.Address == adress[index*2 + 1]).FirstOrDefault();
                    if (alarm != null)
                    {
                        var a = AlarmItems2.Where(x => x.Message == $"{alarm.Type} {alarm.Description}" && x.Address == alarm.Address).FirstOrDefault(); //&& x.Message == $"{alarm.Type} {alarm.Description}"
                        if (a == null)
                        {
                            Dictionary<string, string> dictionary = new Dictionary<string, string>();
                            dictionary["Time"] = DateTime.Now.ToString("yyyy/MM/ddTHH:mm:ss:ff");
                            dictionary["工位"] = alarm.Type;
                            dictionary["Message"] = alarm.Description;
                            CsvHelper.WriteCsv($"./RunLog/Alarm/{DateTime.Now:yyyyMM}/{DateTime.Now:yyyyMMdd}.csv", dictionary);
                            AlarmItems2.Add(new Models.AlarmItem { Source = alarm.Type, Message = $"{alarm.Type} {alarm.Description}", Date = DateTime.Now,Address=alarm.Address });
                            this.@event.GetEvent<AlarmItemEvent>().Publish(new Models.AlarmList { Alarms = AlarmItems2 });
                        }
                    }
                }            
            }
            else
            {
                var number = AlarmItems2.Where(x => x.Source == name).Count();
                if (number>0)
                {
                    AlarmItems2.RemoveAll(x => x.Source == name);
                    this.@event.GetEvent<AlarmItemEvent>().Publish(new Models.AlarmList { Alarms = AlarmItems2 });
                }
            }
        }
        List<AlarmInfo> alarms = new List<AlarmInfo>();
        #endregion
        #region 读取报警文件
        public void LoadAlarmInfos(string filePath = @"./Configs/Alarms.csv")
        {
            alarms = GetAlarmInfos(filePath)
                .Select(x => new AlarmInfo
                {
                    Type = x.Type,
                    Description = x.Description,
                    Index = int.Parse(x.Index),
                    Address = x.Address,
                }).ToList();
        }
        Regex regex = new Regex(@"^[0-9]*[1-9][0-9]*$");
        private IEnumerable<AlarmInfoRecord> GetAlarmInfos(string filePath)
        {
            try
            {
                FileStream data = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using (StreamReader reader = new StreamReader(data, Encoding.UTF8))
                {
                    using (CsvReader csv = new CsvReader(reader, CultureInfo.CurrentCulture))
                    {
                        return (from x in csv.GetRecords<AlarmInfoRecord>()
                                where !string.IsNullOrEmpty(x.Description)
                                where !string.IsNullOrEmpty(x.Address)
                                select x into X
                                where regex.IsMatch(X.Index)
                                select X).Distinct().ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Log(ex.Message, Category.Exception,Priority.High);
                MessageBox.Show("载入报警描述文件失败:" + ex.Message);
                return new List<AlarmInfoRecord>();
            }
        }
        #endregion
        public enum Station
        {
            点胶1状态,
            点胶1产量,
            点胶1不良,
            点胶1周期,
            点胶1ERR,
            点胶2状态,
            点胶2产量,
            点胶2不良,
            点胶2周期,
            点胶2ERR,
            卷线1状态,
            卷线1产量,
            卷线1不良,
            卷线1周期,
            卷线1ERR,
            卷线2状态,
            卷线2产量,
            卷线2不良,
            卷线2周期,
            卷线2ERR,
            卷线3状态,
            卷线3产量,
            卷线3不良,
            卷线3周期,
            卷线3ERR,
            卷线4状态,
            卷线4产量,
            卷线4不良,
            卷线4周期,
            卷线4ERR,
            卷线5状态,
            卷线5产量,
            卷线5不良,
            卷线5周期,
            卷线5ERR,
            卷线6状态,
            卷线6产量,
            卷线6不良,
            卷线6周期,
            卷线6ERR,
            焊锡站状态,
            焊锡站产量,
            焊锡站不良,
            焊锡站周期,
            焊锡站ERR,
        }
        //***********************切换界面函数*************************//
        //dispatcher.BeginInvoke(()=>
        //{
        //    regionManager.RequestNavigate("Dash_CONTENT", nameof(DashBoardList));
        //});
        //******************************************************//

    }

}
