using DataService;
using Prism.Events;
using Prism.Logging;
using PropertyTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Reactive.Linq;
using MV.Core.Events;
using Mv.Core.Interfaces;
using System.IO;
using System.Globalization;
using System.Threading;

namespace Mv.Modules.TagManager.Services
{
    public struct Edge
    {
        public bool ValueChanged { get; private set; }
        public string OldValue { get; private set; }
        private string _currentValue;
        public string CurrentValue
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
    public enum Feedback
    {
        数据上传OK = 11,
        数据上传NG = 12
    }
    public interface ISaveManager
    { }
    public class SaveManager : ISaveManager
    {
        private readonly IDataServer server;
        private readonly IEventAggregator @event;
        private readonly ILoggerFacade logger;
        private Edge[] edges = new Edge[7] { new Edge(), new Edge(), new Edge(), new Edge(), new Edge(), new Edge(), new Edge() };
        public SaveManager(IDataServer server, IEventAggregator @event, ILoggerFacade logger)
        {
            this.server = server;
            this.@event = @event;
            this.logger = logger;
            var save1Group = server.GetGroupByName("SAVE1");
            var save2Group = server.GetGroupByName("SAVE2");
            var save3Group = server.GetGroupByName("SAVE3");
            var save4Group = server.GetGroupByName("SAVE4");
            var save5Group = server.GetGroupByName("SAVE5");
            var alarmGroup = server.GetGroupByName("alarms");//PLC报警
            if (alarmGroup != null)
            {
                foreach (var item in alarmGroup.Items)
                {
                    item.ValueChanged += Item_ValueChanged;
                }
            }
            var PARASGroup = server.GetGroupByName("PARAS");//PLC数据
            if (PARASGroup != null)
            {
                foreach (var item in PARASGroup.Items)
                {
                    item.ValueChanged += PARA_ValueChanged;
                }
            }
            var SFCGroup = server.GetGroupByName("SFC");//PLC数据
            if (SFCGroup != null)
            {
                foreach (var item in SFCGroup.Items)
                {
                    item.ValueChanged += SFC_ValueChanged;
                }
            }
            Task.Factory.StartNew(() => {
                while (true)
                { 
                var TAG = server["HANDSHAKE"];
                TAG?.Write((short)1);//加？表示可为空值
                Thread.Sleep(1000);
                TAG?.Write((short)0);//加？表示可为空值
                Thread.Sleep(1000);
                }
            }, TaskCreationOptions.LongRunning);
            Task.Factory.StartNew(() => {
                var TAG = server["SAVE1"];
                while (true)
                {
                    if (TAG != null)
                    {
                        edges[0].CurrentValue = TAG.ToString();
                        if (edges[0].ValueChanged && edges[0].CurrentValue == "1")
                        {
                            try
                            {
                            Dictionary<string, string> dictionary = new Dictionary<string, string>();
                            dictionary["Time"] = DateTime.Now.ToString("yyyy/MM/ddTHH:mm:ss:ff");
                            foreach (var item in save1Group.Items)
                            {
                               if(item.GetTagName()!="SAVE1")
                               dictionary[item.GetTagName()] =item.ToString();
                            }
                               var result=CsvHelper.WriteCsv($"./RunLog/Data1/{DateTime.Now:yyyyMM}/{DateTime.Now:yyyyMMdd}.csv", dictionary);
                                if(result)
                                TAG?.Write((short)Feedback.数据上传OK);
                                else
                                TAG?.Write((short)Feedback.数据上传NG);

                            }
                            catch(Exception ex)
                            {
                                TAG?.Write((short)Feedback.数据上传NG);
                                logger.Log(ex.Message, Category.Debug, Priority.None);
                            }
                        }
                    }
                    Thread.Sleep(100);
                }
            }, TaskCreationOptions.LongRunning);
            Task.Factory.StartNew(() => {
                var TAG = server["SAVE2"];
                while (true)
                {
                    if (TAG != null)
                    {
                        edges[1].CurrentValue = TAG.ToString();
                        if (edges[1].ValueChanged && edges[1].CurrentValue == "1")
                        {
                            try
                            {
                                Dictionary<string, string> dictionary = new Dictionary<string, string>();
                                dictionary["Time"] = DateTime.Now.ToString("yyyy/MM/ddTHH:mm:ss:ff");
                                foreach (var item in save2Group.Items)
                                {
                                    if (item.GetTagName() != "SAVE2")
                                        dictionary[item.GetTagName()] = item.ToString();
                                }
                                var result = CsvHelper.WriteCsv($"./RunLog/Data2/{DateTime.Now:yyyyMM}/{DateTime.Now:yyyyMMdd}.csv", dictionary);
                                if (result)
                                    TAG?.Write((short)Feedback.数据上传OK);
                                else
                                    TAG?.Write((short)Feedback.数据上传NG);

                            }
                            catch (Exception ex)
                            {
                                TAG?.Write((short)Feedback.数据上传NG);
                                logger.Log(ex.Message, Category.Debug, Priority.None);
                            }
                        }
                    }
                    Thread.Sleep(100);
                }
            }, TaskCreationOptions.LongRunning);
            Task.Factory.StartNew(() => {
                var TAG = server["SAVE3"];
                while (true)
                {
                    if (TAG != null)
                    {
                        edges[2].CurrentValue = TAG.ToString();
                        if (edges[2].ValueChanged && edges[2].CurrentValue == "1")
                        {
                            try
                            {
                                Dictionary<string, string> dictionary = new Dictionary<string, string>();
                                dictionary["Time"] = DateTime.Now.ToString("yyyy/MM/ddTHH:mm:ss:ff");
                                foreach (var item in save3Group.Items)
                                {
                                    if (item.GetTagName() != "SAVE3")
                                        dictionary[item.GetTagName()] = item.ToString();
                                }
                                var result = CsvHelper.WriteCsv($"./RunLog/Data3/{DateTime.Now:yyyyMM}/{DateTime.Now:yyyyMMdd}.csv", dictionary);
                                if (result)
                                    TAG?.Write((short)Feedback.数据上传OK);
                                else
                                    TAG?.Write((short)Feedback.数据上传NG);

                            }
                            catch (Exception ex)
                            {
                                TAG?.Write((short)Feedback.数据上传NG);
                                logger.Log(ex.Message, Category.Debug, Priority.None);
                            }
                        }
                    }
                    Thread.Sleep(100);
                }
            }, TaskCreationOptions.LongRunning);
            Task.Factory.StartNew(() => {
                var TAG = server["SAVE4"];
                while (true)
                {
                    if (TAG != null)
                    {
                        edges[3].CurrentValue = TAG.ToString();
                        if (edges[3].ValueChanged && edges[3].CurrentValue == "1")
                        {
                            try
                            {
                                Dictionary<string, string> dictionary = new Dictionary<string, string>();
                                dictionary["Time"] = DateTime.Now.ToString("yyyy/MM/ddTHH:mm:ss:ff");
                                foreach (var item in save4Group.Items)
                                {
                                    if (item.GetTagName() != "SAVE4")
                                        dictionary[item.GetTagName()] = item.ToString();
                                }
                                var result = CsvHelper.WriteCsv($"./RunLog/Data4/{DateTime.Now:yyyyMM}/{DateTime.Now:yyyyMMdd}.csv", dictionary);
                                if (result)
                                    TAG?.Write((short)Feedback.数据上传OK);
                                else
                                    TAG?.Write((short)Feedback.数据上传NG);

                            }
                            catch (Exception ex)
                            {
                                TAG?.Write((short)Feedback.数据上传NG);
                                logger.Log(ex.Message, Category.Debug, Priority.None);
                            }
                        }
                    }
                    Thread.Sleep(100);
                }
            }, TaskCreationOptions.LongRunning);
            Task.Factory.StartNew(() => {
                var TAG = server["SAVE5"];
                while (true)
                {
                    if (TAG != null)
                    {
                        edges[4].CurrentValue = TAG.ToString();
                        if (edges[4].ValueChanged && edges[4].CurrentValue == "1")
                        {
                            try
                            {
                                Dictionary<string, string> dictionary = new Dictionary<string, string>();
                                dictionary["Time"] = DateTime.Now.ToString("yyyy/MM/ddTHH:mm:ss:ff");
                                foreach (var item in save5Group.Items)
                                {
                                    if (item.GetTagName() != "SAVE5")
                                        dictionary[item.GetTagName()] = item.ToString();
                                }
                                var result = CsvHelper.WriteCsv($"./RunLog/Data5/{DateTime.Now:yyyyMM}/{DateTime.Now:yyyyMMdd}.csv", dictionary);
                                if (result)
                                    TAG?.Write((short)Feedback.数据上传OK);
                                else
                                    TAG?.Write((short)Feedback.数据上传NG);

                            }
                            catch (Exception ex)
                            {
                                TAG?.Write((short)Feedback.数据上传NG);
                                logger.Log(ex.Message, Category.Debug, Priority.None);
                            }
                        }
                    }
                    Thread.Sleep(100);
                }
            }, TaskCreationOptions.LongRunning);
        }
        private void PARA_ValueChanged(object sender, DataService.ValueChangedEventArgs e)
        {
            if (sender is ITag tag)
            {
                var value = tag.GetValue(e.Value).ToString();
                var oldvalue = tag.GetValue(e.OldValue).ToString();
                var meta = tag.GetMetaData();
                var msg = $"{meta.Name}\t{meta.Address}\t{meta.Description}\t{"From_" + oldvalue + "_to_" + value}";
                this.@event.GetEvent<UserMessageEvent>().Publish(new UserMessage { Content = msg, Level = 0, Source = "PARAS" });
                Dictionary<string, string> dictionary = new Dictionary<string, string>();
                dictionary["Time"] = DateTime.Now.ToString("yyyy/MM/ddTHH:mm:ss:ff");
                dictionary["msg"] = msg;
                CsvHelper.WriteCsv($"./RunLog/PARAS/{DateTime.Now:yyyyMM}/{DateTime.Now:yyyyMMdd}.csv", dictionary);
            }
        }
        private void Item_ValueChanged(object sender, DataService.ValueChangedEventArgs e)
        {
            if (sender is BoolTag tag)
            {

                var value = (bool)tag.GetValue(e.Value) ? "From_OFF_to_ON" : "From_ON_to_OFF";
                var meta = tag.GetMetaData();
                var msg = $"{meta.Name}\t{meta.Address}\t{meta.Description}\t{value}";
                this.@event.GetEvent<UserMessageEvent>().Publish(new UserMessage { Content = msg, Level = 0, Source = "alarms" });
                Dictionary<string, string> dictionary = new Dictionary<string, string>();
                dictionary["Time"] = DateTime.Now.ToString("yyyy/MM/ddTHH:mm:ss:ff");
                dictionary["msg"] = msg;
                CsvHelper.WriteCsv($"./RunLog/alarms/{DateTime.Now:yyyyMM}/{DateTime.Now:yyyyMMdd}.csv", dictionary);
            }

        }
        private void SFC_ValueChanged(object sender, DataService.ValueChangedEventArgs e)
        {
            if (sender is BoolTag tag)
            {

                var value = (bool)tag.GetValue(e.Value) ? "From_OFF_to_ON" : "From_ON_to_OFF";
                var meta = tag.GetMetaData();
                var msg = $"{meta.Name}\t{meta.Address}\t{meta.Description}\t{value}";
                this.@event.GetEvent<UserMessageEvent>().Publish(new UserMessage { Content = msg, Level = 0, Source = "alarms" });
                if ((bool)tag.GetValue(e.Value))
                {
                var sfcname = "";
                if (tag.GetMetaData().Code != null)
                     sfcname = tag.GetMetaData().Code;
                Dictionary<string, string> dictionary = new Dictionary<string, string>();
                dictionary["Time"] = DateTime.Now.ToString("yyyy/MM/ddTHH:mm:ss:ff");
                dictionary["msg"] = meta.Description;
                CsvHelper.WriteCsv($"./RunLog/SFC/{DateTime.Now:yyyyMM}/{sfcname}_{DateTime.Now:yyyyMMdd}.csv", dictionary);
                }
            }
            }
        #region IDisposable Support
        private bool disposedValue = false; // 要检测冗余调用

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    var alarmGroup = server?.GetGroupByName("alarms");
                    if (alarmGroup != null)
                    {
                        foreach (var item in alarmGroup.Items)
                        {
                            item.ValueChanged -= Item_ValueChanged;
                        }
                    }
                    var PARASGroup = server.GetGroupByName("PARAS");
                    if (PARASGroup != null)
                    {
                        foreach (var item in PARASGroup.Items)
                        {
                            item.ValueChanged -= PARA_ValueChanged;
                        }
                    }
                    var SFCGroup = server.GetGroupByName("SFC");
                    if (SFCGroup != null)
                    {
                        foreach (var item in SFCGroup.Items)
                        {
                            item.ValueChanged -= SFC_ValueChanged;
                        }
                    }
                    // TODO: 释放托管状态(托管对象)。
                }

                // TODO: 释放未托管的资源(未托管的对象)并在以下内容中替代终结器。
                // TODO: 将大型字段设置为 null。

                disposedValue = true;
            }
        }

        // TODO: 仅当以上 Dispose(bool disposing) 拥有用于释放未托管资源的代码时才替代终结器。
        // ~AlarmManager()
        // {
        //   // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
        //   Dispose(false);
        // }

        // 添加此代码以正确实现可处置模式。
        public void Dispose()
        {
            // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
            Dispose(true);
            // TODO: 如果在以上内容中替代了终结器，则取消注释以下行。
            // GC.SuppressFinalize(this);
        }
        #endregion
    }

}
