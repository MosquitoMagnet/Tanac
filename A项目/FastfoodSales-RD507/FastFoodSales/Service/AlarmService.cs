using CsvHelper;
using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Concurrent;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Threading;
using DAQ.Core.Log;


namespace DAQ.Service
{
    public interface IAlarmService
    {
        List<AlarmItem> GetAlarmItems();
        void LoadAlarmInfos(string filePath = "./Alarms/AlarmsLA.csv");
    }
    public class AlarmItem
    {
        public int Id { get; set; }
        public string Address { get; set; }
        public string Type { get; set; }
        public string Severity { get; set; }
        public string Short { get; set; }
        public string Message { get; set; }
        public DateTime Data { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime StopTime { get; set; }
        public TimeSpan TimeSpan { get; set; }
    }
    public class AlarmService : IAlarmService
    {
        internal class AlarmInfoRecord
        {
            [Index(0)]
            public string Address { get; set; }
            [Index(1)]
            public string Type { get; set; }
            [Index(2)]
            public string Severity { get; set; }
            [Index(3)]
            public string Short_Description { get; set; }
            [Index(4)]
            public string Long_Description { get; set; }
        }
        internal class AlarmInfo
        {
            public string Address { get; set; }           
            public string Type { get; set; }
            public string Severity { get; set; }
            public string Short { get; set; }
            public string Message { get; set; }
            public int AddressOffset { get; set; }
            public int BitIndex { get; set; }
        }
        private readonly IDeviceReadWriter device;
        private readonly IConfigureFile configure;
        private  Config config;
        private int addressOffset = 4100;
        private int localOffset = 100;
        private 
        Subject<AlarmItem> subjectAlarmItem = new Subject<AlarmItem>();
        Subject<AlarmInfo> subjectNewAlarm = new Subject<AlarmInfo>();
        private IEnumerable<AlarmInfoRecord> GetAlarmInfos(FileInfo file)
        {
            try
            {
                using (StreamReader reader = new StreamReader(file.FullName, Encoding.Default))
                {
                    using (CsvReader csv = new CsvReader(reader, CultureInfo.CurrentCulture))
                    {
                        return (from x in csv.GetRecords<AlarmInfoRecord>()
                                where !string.IsNullOrEmpty(x.Long_Description)
                                select x into X
                                where regex.IsMatch(X.Address)
                                select X).Distinct().ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.Message);
                return new List<AlarmInfoRecord>();
            }
        }

        Regex regex = new Regex(@"^MB\d{4}[A-F0-9]{1}");
        public AlarmService(IDeviceReadWriter device,IConfigureFile configureFile)
        {
            this.device = device;
            this.configure = configureFile;
            config = configure.Load().GetValue<Config>(nameof(Config)) ?? new Config();
            configure.ValueChanged += configure_ValueChanged;
            LoadAlarmInfos();
            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    ObserveAlarms(10);
                    Thread.Sleep(10);
                }
            }, TaskCreationOptions.LongRunning);
            subjectNewAlarm.Subscribe(m =>
            {
                var dictionary = new Dictionary<string, string>();
                dictionary["Date"] =DateTime.Now.ToString("yyyy-MM-dd");
                dictionary["Time"] = DateTime.Now.ToString("HH:mm:ss.ff");
                dictionary["Type"] = m.Type;
                dictionary["Severity"] = m.Severity;
                dictionary["Short Description"] = m.Short;
                dictionary["Long Description"] = m.Message;
                Helper.SaveFile(Path.Combine(config.FileDir + @"\SFCSLA", DateTime.Now.ToString("yyyyMMdd") + ".csv"), dictionary);
            });
            subjectAlarmItem.Buffer(TimeSpan.FromSeconds(1)).Subscribe((n) =>
            {
                n.ForEach(m =>
                {
                    var dictionary = new Dictionary<string, string>();
                    dictionary["StartTime"] = m.StartTime.ToString("yyyy-MM-ddTHH:mm:ss.ff");
                    dictionary["EndTime"] = m.StopTime.ToString("yyyy-MM-ddTHH:mm:ss.ff");
                    dictionary["Address"] = m.Address;
                    dictionary["Message"] = m.Message;
                    dictionary["TimeInterval"] = m.TimeSpan.ToString();
                    Helper.SaveFile(Path.Combine(config.FileDir + @"\AlarmsLA", DateTime.Now.ToString("yyyyMMdd") + ".csv"), dictionary);
                });
            });
        }
        ConcurrentDictionary<string, AlarmItem> currentAlarmItems = new ConcurrentDictionary<string, AlarmItem>();
        private void ObserveAlarms(long m)
        {
            var inalarms = alarms.Where(x => device.GetBit(x.AddressOffset - addressOffset + localOffset, x.BitIndex));
            var noalarms = alarms.Except(inalarms);
            //添加新的报警信息
            var newalarms = inalarms.Where(x => !currentAlarmItems.ContainsKey(x.Address)).ToList();
            newalarms.ForEach((newAlarm) => currentAlarmItems[newAlarm.Address] = new AlarmItem
            {
                Address = newAlarm.Address,
                Message = newAlarm.Message,
                StartTime = DateTime.Now,
                TimeSpan = TimeSpan.FromSeconds(0)
            });
            newalarms.ForEach(x => subjectNewAlarm.OnNext(x));


            //删除旧的报警信息
            noalarms.Where(x => currentAlarmItems.ContainsKey(x.Address)).ToList()
            .ForEach((solvedAlarm) =>
            {
                AlarmItem value = currentAlarmItems[solvedAlarm.Address];
                value.StopTime = DateTime.Now;
                value.TimeSpan = value.StopTime - value.StartTime;
                subjectAlarmItem.OnNext(value);
                currentAlarmItems.TryRemove(solvedAlarm.Address, out var removed);
            });
            //更新现有报警
            currentAlarmItems.Values.ForEach(x =>
            {
                x.StopTime = DateTime.Now;
                x.TimeSpan = x.StopTime - x.StartTime;
            });

        }

        public List<AlarmItem> GetAlarmItems()
        {
            return currentAlarmItems.Values.ToList();
        }
        List<AlarmInfo> alarms = new List<AlarmInfo>();
        public void LoadAlarmInfos(string filePath = @"./Alarms/AlarmsLA.csv")
        {
            alarms = GetAlarmInfos(new FileInfo(filePath))
                .Select(x => new AlarmInfo
                {
                    Address = x.Address,
                    Type=x.Type,
                    Severity=x.Severity,
                    Message = x.Long_Description,
                    Short = x.Short_Description,
                    AddressOffset = int.Parse(x.Address.Substring(2, 4)),
                    BitIndex = Convert.ToInt32(x.Address.Substring(6, 1), 16)
                }).ToList();
        }
        private void configure_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            if (e.KeyName != nameof(Config)) return;
            var _config = configure.GetValue<Config>(nameof(Config));
            config = _config;
        }
    }
}
