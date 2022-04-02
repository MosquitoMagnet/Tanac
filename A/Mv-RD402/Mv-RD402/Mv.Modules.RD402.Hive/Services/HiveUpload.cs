using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using DataService;
using System.Linq;
using Newtonsoft.Json;
using Prism.Logging;
using Mv.Core.Interfaces;
using Prism.Events;
using System.Threading;
using System.Threading.Tasks;
using MV.Core.Events;

namespace Mv.Modules.RD402.Hive.Services
{
    public interface IHiveUpload
    {

    }
    public class HiveUpload : IHiveUpload
    {
        private readonly ILoggerFacade logger;
        private readonly IDataServer server;
        private readonly IEventAggregator @event;
        private readonly IConfigureFile _configure;
        private RD402HiveConfig _config;
        private bool server1;
        private bool server2;
        private bool server3;
        public HiveUpload(IDataServer server, ILoggerFacade logger, IEventAggregator @event, IConfigureFile configure)
        {
            this.logger = logger;
            this.server = server;
            this.@event = @event;
            _configure = configure;
            _configure.ValueChanged += _configure_ValueChanged;
            _config = configure.GetValue<RD402HiveConfig>(nameof(RD402HiveConfig)) ?? new RD402HiveConfig();
            Task.Factory.StartNew(() => { ErrorUpload(); },TaskCreationOptions.LongRunning);
            Task.Factory.StartNew(() => { StateUpload(); }, TaskCreationOptions.LongRunning);
            Task.Factory.StartNew(() => { DataUpload(); }, TaskCreationOptions.LongRunning);
        }

        private void ErrorUpload()
        {
            while (true)
            {
                if (server1)
                {
                    server["Hive1"]?.Write((short)1);
                    Thread.Sleep(300000);
                    server1 = false;
                }
                else
                {
                    try
                    {
                        var dir = Path.GetDirectoryName($"./Hive/Json/ErrorData/");
                        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                        string[] files = Directory.GetFiles($"./Hive/Json/ErrorData", "*.*", SearchOption.AllDirectories)
                       .Where(s => s.EndsWith(".json")).ToArray();

                        if (files.Length != 0)
                        {
                            var json = JsonHelper.GetJsonFile(files[0]);
                            ErrorData hsm = json.GetErrorDataModel();
                            if (hsm!=null&&hsm.occurrence_time != null && hsm.resolved_time != null)
                            {
                                double total_seconds = 259200;
                                double interval_seconds = 259200;
                                try
                                {
                                    var occurrence_time = DateTime.Parse(hsm.occurrence_time);
                                    var resolved_time = DateTime.Parse(hsm.resolved_time);
                                    total_seconds = (DateTime.Now - occurrence_time).TotalSeconds;
                                    interval_seconds = (resolved_time - occurrence_time).TotalSeconds;
                                }
                                catch
                                {
                                    total_seconds = 259200;
                                    interval_seconds = 259200;
                                }
                                if (total_seconds > 0 && total_seconds < 178100 && interval_seconds > 0 && interval_seconds < 178100)
                                {
                                    if (_config.isUpload)
                                    {
                                        string responseString = "";
                                        var result = false;
                                        result = PostHelper.HivePost("http://10.0.0.2:5008/v5/capture/errordata", json, ref responseString);
                                        HiveServerModel hmodel = responseString.GetHiveServerModel();
                                        this.@event.GetEvent<UserMessageEvent>().Publish(new UserMessage { Content = "error server:" + responseString, Level = 0, Source = "Hive" });
                                        if (result && hmodel != null)
                                        {
                                            server1 = false;
                                            server["Hive1"]?.Write((short)0);
                                            if (hmodel.Status != null && hmodel.Status.ToLower().Contains("success"))
                                            {
                                                CsvHelper.Writelog($"./Hive/LogRun/ErrorData/{DateTime.Now:yyyyMMdd}.txt", "IPC send:" + json);
                                                CsvHelper.Writelog($"./Hive/LogRun/ErrorData/{DateTime.Now:yyyyMMdd}.txt", "error server:" + responseString);
                                                File.Delete(files[0]);
                                            }
                                            else
                                            {
                                                File.Delete(files[0]);
                                                CsvHelper.Writelog($"./Hive/LogRun/ErrorData/{DateTime.Now:yyyyMMdd}_Error.txt", "IPC send:" + json);
                                                CsvHelper.Writelog($"./Hive/LogRun/ErrorData/{DateTime.Now:yyyyMMdd}_Error.txt", "error server:" + responseString + "\r\n");
                                            }
                                        }
                                        else
                                        {
                                            CsvHelper.Writelog($"./Hive/LogRun/ErrorData/{DateTime.Now:yyyyMMdd}_Error.txt", "IPC send:" + json);
                                            CsvHelper.Writelog($"./Hive/LogRun/ErrorData/{DateTime.Now:yyyyMMdd}_Error.txt", "error server:" + responseString + "\r\n");
                                            server1 = true;
                                        }
                                    }
                                    else
                                    {
                                        File.Delete(files[0]);
                                        CsvHelper.Writelog($"./Hive/LogRun/ErrorData/{DateTime.Now:yyyyMMdd}_OffLine.txt", "IPC send:" + json);
                                        CsvHelper.Writelog($"./Hive/LogRun/ErrorData/{DateTime.Now:yyyyMMdd}_OffLine.txt", "error server: offline");
                                    }
                                }
                                else
                                {
                                    File.Delete(files[0]);
                                    logger.Log("day>2" + json.Trim('\r', '\n'), Category.Debug, Priority.None);
                                }
                            }
                            else
                            {
                                File.Delete(files[0]);
                                logger.Log("day>2" + json.Trim('\r', '\n'), Category.Debug, Priority.None);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Log(ex.Message, Category.Debug, Priority.None);
                    }

                }
                Thread.Sleep(200);
            }
        }
        private void StateUpload()
        {
            while (true)
            {
                if (server2)
                {
                    server["Hive2"]?.Write((short)1);
                    Thread.Sleep(300000);
                    server2 = false;
                }
                else
                {
                    try
                    {
                        var dir = Path.GetDirectoryName($"./Hive/Json/StateData/");
                        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                        string[] files = Directory.GetFiles($"./Hive/Json/StateData", "*.*", SearchOption.AllDirectories)
                        .Where(s => s.EndsWith(".json")).ToArray();

                        if (files.Length != 0)
                        {
                            var json = JsonHelper.GetJsonFile(files[0]);
                            MachineState hsm = json.GetMachineStateModel();
                            if (hsm != null && hsm.state_change_time != null)
                            {
                                double total_seconds = 259200;
                                try
                                {
                                    var change_tmie = DateTime.Parse(hsm.state_change_time);
                                    total_seconds = (DateTime.Now - change_tmie).TotalSeconds;
                                }
                                catch
                                {
                                    total_seconds = 259200;
                                }
                                if (total_seconds > 0 && total_seconds < 178100)
                                {
                                    if (_config.isUpload)
                                    {
                                        string responseString = "";
                                        var result = false;
                                        result = PostHelper.HivePost("http://10.0.0.2:5008/v5/capture/machinestate", json, ref responseString);
                                        HiveServerModel hmodel = responseString.GetHiveServerModel();
                                        this.@event.GetEvent<UserMessageEvent>().Publish(new UserMessage { Content = "state server:" + responseString, Level = 0, Source = "Hive" });
                                        if (result && hmodel != null)
                                        {
                                            server2 = false;
                                            server["Hive2"]?.Write((short)0);
                                            if (hmodel.Status != null && hmodel.Status.ToLower().Contains("success"))
                                            {
                                                CsvHelper.Writelog($"./Hive/LogRun/StateData/{DateTime.Now:yyyyMMdd}.txt", "IPC send:" + json);
                                                CsvHelper.Writelog($"./Hive/LogRun/StateData/{DateTime.Now:yyyyMMdd}.txt", "state server:" + responseString);
                                                File.Delete(files[0]);
                                            }
                                            else
                                            {
                                                File.Delete(files[0]);
                                                CsvHelper.Writelog($"./Hive/LogRun/StateData/{DateTime.Now:yyyyMMdd}_Error.txt", "IPC send:" + json);
                                                CsvHelper.Writelog($"./Hive/LogRun/StateData/{DateTime.Now:yyyyMMdd}_Error.txt", "state server:" + responseString);
                                            }
                                        }
                                        else
                                        {
                                            CsvHelper.Writelog($"./Hive/LogRun/StateData/{DateTime.Now:yyyyMMdd}_Error.txt", "IPC send:" + json);
                                            CsvHelper.Writelog($"./Hive/LogRun/StateData/{DateTime.Now:yyyyMMdd}_Error.txt", "state server:Server disconnected" + "\r\n");
                                            server2 = true;
                                        }
                                    }
                                    else
                                    {
                                        File.Delete(files[0]);
                                        CsvHelper.Writelog($"./Hive/LogRun/StateData/{DateTime.Now:yyyyMMdd}_OffLine.txt", "IPC send:" + json);
                                        CsvHelper.Writelog($"./Hive/LogRun/StateData/{DateTime.Now:yyyyMMdd}_OffLine.txt", "state server: offline");
                                    }
                                }
                                else
                                {
                                    File.Delete(files[0]);
                                    logger.Log("day>2" + json.Trim('\r', '\n'), Category.Debug, Priority.None);
                                }
                            }
                            else
                            {
                                File.Delete(files[0]);
                                logger.Log("day>2" + json.Trim('\r', '\n'), Category.Debug, Priority.None);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Log(ex.Message, Category.Debug, Priority.None);
                    }

                }
                Thread.Sleep(200);
            }

        }
        private void DataUpload()
        {
            while (true)
            {
                if (server3)
                {
                    server["Hive3"]?.Write((short)1);
                    Thread.Sleep(300000);
                    server3 = false;
                }
                else
                {
                    try
                    {
                        var dir = Path.GetDirectoryName($"./Hive/Json/MachineData/");
                        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                        string[] files = Directory.GetFiles($"./Hive/Json/MachineData", "*.*", SearchOption.AllDirectories)
                        .Where(s => s.EndsWith(".json")).ToArray();

                        if (files.Length != 0)
                        {
                            var json = JsonHelper.GetJsonFile(files[0]);
                            MachineData hsm = json.GetMachineDataModel();
                            if (hsm != null && hsm.input_time != null && hsm.output_time != null)
                            {
                                double total_seconds = 259200;
                                double interval_seconds = 259200;
                                try
                                {
                                    var input_time = DateTime.Parse(hsm.input_time);
                                    var output_time = DateTime.Parse(hsm.output_time);
                                    total_seconds = (DateTime.Now - input_time).TotalSeconds;
                                    interval_seconds = (output_time - input_time).TotalSeconds;
                                }
                                catch
                                {
                                    total_seconds = 259200;
                                    interval_seconds = 259200;
                                }
                                if (total_seconds > 0 && total_seconds < 178100 && interval_seconds > 0 && interval_seconds < 178100)
                                {
                                    if (_config.isUpload)
                                    {
                                        string responseString = "";
                                        var result = false;
                                        result = PostHelper.HivePost("http://10.0.0.2:5008/v5/capture/machinedata", json, ref responseString);
                                        HiveServerModel hmodel = responseString.GetHiveServerModel();
                                        this.@event.GetEvent<UserMessageEvent>().Publish(new UserMessage { Content = "data server:" + responseString, Level = 0, Source = "Hive" });
                                        if (result && hmodel != null)
                                        {
                                            server3 = false;
                                            server["Hive3"]?.Write((short)0);
                                            if (hmodel.Status != null && hmodel.Status.ToLower().Contains("success"))
                                            {
                                                var write_time = DateTime.Parse(hsm.input_time);
                                                CsvHelper.Writelog($"./Hive/LogRun/MachineData/{write_time:yyyyMMdd}.txt", "IPC send:" + json);
                                                CsvHelper.Writelog($"./Hive/LogRun/MachineData/{write_time:yyyyMMdd}.txt", "data server:" + responseString);
                                                File.Delete(files[0]);

                                            }
                                            else
                                            {
                                                File.Delete(files[0]);
                                                CsvHelper.Writelog($"./Hive/LogRun/MachineData/{DateTime.Now:yyyyMMdd}_Error.txt", "IPC send:" + json);
                                                CsvHelper.Writelog($"./Hive/LogRun/MachineData/{DateTime.Now:yyyyMMdd}_Error.txt", "data server:" + responseString + "\r\n");
                                            }
                                        }
                                        else
                                        {
                                            CsvHelper.Writelog($"./Hive/LogRun/MachineData/{DateTime.Now:yyyyMMdd}_Error.txt", "IPC send:" + json);
                                            CsvHelper.Writelog($"./Hive/LogRun/MachineData/{DateTime.Now:yyyyMMdd}_Error.txt", "data server:Server disconnected" + "\r\n");
                                            server3 = true;
                                        }
                                    }
                                    else
                                    {
                                        File.Delete(files[0]);
                                        CsvHelper.Writelog($"./Hive/LogRun/MachineData/{DateTime.Now:yyyyMMdd}_OffLine.txt", "IPC send:" + json);
                                        CsvHelper.Writelog($"./Hive/LogRun/MachineData/{DateTime.Now:yyyyMMdd}_OffLine.txt", "server send: offline");
                                    }
                                }
                                else
                                {
                                    File.Delete(files[0]);
                                    CsvHelper.Writelog($"./Hive/LogRun/MachineData/{DateTime.Now:yyyyMMdd}_Error.txt", "IPC send:" + json);
                                    CsvHelper.Writelog($"./Hive/LogRun/MachineData/{DateTime.Now:yyyyMMdd}_Error.txt", "software judge:" + "day>2" + "\r\n");
                                    logger.Log("day>2" + json.Trim('\r', '\n'), Category.Debug, Priority.None);
                                }
                            }
                            else
                            {
                                File.Delete(files[0]);
                                CsvHelper.Writelog($"./Hive/LogRun/MachineData/{DateTime.Now:yyyyMMdd}_Error.txt", "IPC send:" + json);
                                CsvHelper.Writelog($"./Hive/LogRun/MachineData/{DateTime.Now:yyyyMMdd}_Error.txt", "software judge:" + "day>2" + "\r\n");
                                logger.Log("day>2" + json.Trim('\r', '\n'), Category.Debug, Priority.None);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Log(ex.Message, Category.Debug, Priority.None);
                    }

                }
                Thread.Sleep(200);
            }
        }
        private void _configure_ValueChanged(object sender, Mv.Core.Interfaces.ValueChangedEventArgs e)
        {
            if (e.KeyName != nameof(RD402HiveConfig)) return;
            var config = _configure.GetValue<RD402HiveConfig>(nameof(RD402HiveConfig));
            _config = config;
        }
    }
    public static class HiveDataEx
    {
        /// <summary>
        /// json字符串转换为HiveServerModel
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static MachineData GetMachineDataModel(this string json)
        {
            try
            {
                return JsonConvert.DeserializeObject<MachineData>(json);
            }
            catch (Exception ex)
            {
                //错误信息需要添加到界面展示
                return null;
            }

        }
        public static MachineState GetMachineStateModel(this string json)
        {
            try
            {
                return JsonConvert.DeserializeObject<MachineState>(json);
            }
            catch (Exception ex)
            {
                //错误信息需要添加到界面展示
                return null;
            }

        }
        public static ErrorData GetErrorDataModel(this string json)
        {
            try
            {
                return JsonConvert.DeserializeObject<ErrorData>(json);
            }
            catch (Exception ex)
            {
                //错误信息需要添加到界面展示
                return null;
            }

        }
        public static HiveServerModel GetHiveServerModel(this string json)
        {
            try
            {
                return JsonConvert.DeserializeObject<HiveServerModel>(json);
            }
            catch (Exception ex)
            {
                //错误信息需要添加到界面展示
                return null;
            }

        }
    }
}
