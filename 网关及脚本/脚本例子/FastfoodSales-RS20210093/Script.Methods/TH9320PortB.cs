using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using Stylet;
using System.Collections.ObjectModel;
using HslCommunication;
using System.Threading;

namespace Script.Methods
{

    public class TH9320PortB
    {
        protected SerialPort port = new SerialPort();
        public string PortName => ScriptSettings.Default.PORT_C;
        public ObservableCollection<HiPotTestViewModel> TestSpecs { get; set; } = new ObservableCollection<HiPotTestViewModel>();
        public bool IsConnected { get; set; }
        private int connectErrorCount = 0;                        // 连接错误次数
        private int sleepTime { get; set; } = 20;                // 睡眠的时间
        private int receiveTimeout { get; set; } = 5000;         // 接收数据的超时时间
        protected IEventAggregator Events { get; set; }
        protected string InstName { get; set; }
        public TH9320PortB(IEventAggregator events)
        {
            Events = events;
            InstName = "TH9320";
            for (int i = 0; i < 10; i++)
            {
                TestSpecs.Add(new HiPotTestViewModel());
            }
        }
        public bool Connect()
        {
            try
            {
                if (port.IsOpen)
                    port.Close();

                port.PortName = PortName;
                port.BaudRate = 9600;
                port.Open();
                port.WriteLine("*IDN?");
                port.ReadTimeout = 1000;
                string v = port.ReadLine();
                if (v.Length > 0)
                {
                    if (!string.IsNullOrEmpty(InstName))
                    {
                        IsConnected = v.Contains(InstName);
                        if (IsConnected)
                        port.WriteLine("FETCh:AUTO 0");
                    }
                    else
                        IsConnected = false;
                    return IsConnected;
                }
                else
                {
                    port.Close();
                    return false;
                }
            }
            catch (Exception EX)
            {
                Events.Publish(new MsgItem() { Level = "E", Time = DateTime.Now, Value = PortName + ":" + EX.Message });
                LogHelper.Error(EX.Message);
                port.Close();
                return false;
            }
        }
        public void DisConnect()
        {
            IsConnected = false;

            port.Close();
        }
        public bool Read()
        {

            if (!IsConnected)
            {
                Connect();
            }
            try
            {
                if (Request("FETCh?", out string reply))
                {
                    var group = reply.Split(';');
                    if (group.Length > 0)
                    {
                        int len = 0;
                        if (group.Length > TestSpecs.Count)
                            len = TestSpecs.Count;
                        else
                            len = group.Length;

                        for (int i = 0; i < len; i++)
                        {
                            var select = group[i].Split(',');
                            if (select.Length == 4)
                            {
                                TestSpecs[i].TestProject = select[0];
                                TestSpecs[i].TestVoltage = float.Parse(select[1]);
                                TestSpecs[i].TestCurrent = float.Parse(select[2]);
                                TestSpecs[i].Results = select[3];
                                if (select[3].Contains("PASS"))
                                    TestSpecs[i].TestResults = 1;
                                else
                                    TestSpecs[i].TestResults = -1;
                            }
                            else if (select.Length == 3)
                            {
                                TestSpecs[i].TestProject = select[0];
                                TestSpecs[i].TestVoltage = float.Parse(select[1]);
                                TestSpecs[i].TestCurrent = float.Parse(select[2]);
                                TestSpecs[i].Results = "N/A";
                                TestSpecs[i].TestResults = -1;
                            }
                            else
                            {
                                Events.Publish(new MsgItem() { Time = DateTime.Now, Level = "E", Value = $"{PortName}\tError reply" });
                            }
                        }
                        return true;
                    }
                    else
                    {
                        Events.Publish(new MsgItem() { Time = DateTime.Now, Level = "E", Value = $"{PortName}\tError reply" });
                        return false;
                    }
                }
                else
                {
                    Events.Publish(new MsgItem
                    {
                        Level = "D",
                        Time = DateTime.Now,
                        Value = $"{PortName}\t{"Error reply"}{Environment.NewLine}"
                    });
                    return false;
                }
            }
            catch (Exception ex)
            {
                Events.Publish(new MsgItem() { Time = DateTime.Now, Level = "E", Value = $"{PortName}\t{ex.Message}{Environment.NewLine}" });
                LogHelper.Error(ex.Message);
                return false;
            }
        }
        public bool Request(string cmd, out string reply)
        {
            Events.Publish(new MsgItem
            {
                Level = "D",
                Time = DateTime.Now,
                Value = $"{PortName}\t{cmd}{Environment.NewLine}"
            });
            if (IsConnected)
            {
                port.DiscardInBuffer();
                port.WriteLine(cmd);
                try
                {
                    port.ReadTimeout = 1000;
                    reply = port.ReadLine();
                    Events.Publish(new MsgItem
                    {
                        Level = "D",
                        Time = DateTime.Now,
                        Value = $"{PortName}\t{reply}{Environment.NewLine}"
                    });
                    return true;
                }
                catch (Exception ex)
                {
                    reply = ex.Message;
                    Events.Publish(new MsgItem
                    {
                        Level = "E",
                        Time = DateTime.Now,
                        Value = $"{PortName}\t{reply}{Environment.NewLine}"
                    });
                    return false;
                }
            }
            else
            {
                Events.Publish(new MsgItem
                {
                    Level = "E",
                    Time = DateTime.Now,
                    Value = $"{PortName}\t{" port is not connected"}{Environment.NewLine}"
                });
                reply = "port is not connected";
                return false;
            }
        }
        public bool RequestCmd(string cmd, out string reply)
        {
            Events.Publish(new MsgItem
            {
                Level = "D",
                Time = DateTime.Now,
                Value = $"{PortName}\t{cmd}{Environment.NewLine}"
            });
            if (IsConnected)
            {
                port.DiscardInBuffer();
                port.WriteLine(cmd);
                try
                {
                    port.ReadTimeout = 1000;
                    reply = port.ReadLine();
                    Events.Publish(new MsgItem
                    {
                        Level = "D",
                        Time = DateTime.Now,
                        Value = $"{PortName}\t{reply}{Environment.NewLine}"
                    });
                    return true;
                }
                catch (Exception ex)
                {
                    reply = ex.Message;
                    Events.Publish(new MsgItem
                    {
                        Level = "E",
                        Time = DateTime.Now,
                        Value = $"{PortName}\t{reply}{Environment.NewLine}"
                    });
                    return false;
                }
            }
            else
            {
                Events.Publish(new MsgItem
                {
                    Level = "E",
                    Time = DateTime.Now,
                    Value = $"{PortName}\t{" port is not connected"}{Environment.NewLine}"
                });
                reply = "port is not connected";
                Connect();
                return false;
            }
        }
    }
}
