using System;
using Stylet;
using System.Text.RegularExpressions;
using System.IO.Ports;
using System.Threading;
using HslCommunication;
using System.Collections.ObjectModel;


namespace Script.Methods
{
    public class TurnsTestViewModel:PropertyChangedBase
    {
        public float TurnsResult { get; set; }
        public float LxsResult { get; set; }
        public float LxsUhResult { get; set; }
        public float LksResult { get; set; }
        public float CxsResult { get; set; }
        public float DcrsResult { get; set; }
        public float LxsqResult { get; set; }
        public float AcrsResult { get; set; }
        public float ZxsResult { get; set; }
        public string PhsResult { get; set; }
        public string TurnsJudge { get; set; }
        public string LxsJudge { get; set; }
        public string LksJudge { get; set; }
        public string CxsJudge { get; set; }
        public string DcrsJudge { get; set; }
        public string LxsqJudge { get; set; }
        public string AcrsJudge { get; set; }
        public string ZxsJudge { get; set; }
        public string BlsJudge { get; set; }
        public string PssJudge { get; set; }
        public string TrsWholeJudge { get; set; }
    }

   public class TH2829Port
    {
        protected SerialPort port = new SerialPort();
        public string PortName =>ScriptSettings.Default.PORT_A;
        public ObservableCollection<TurnsTestViewModel> TestSpecs { get; set; } = new ObservableCollection<TurnsTestViewModel>();
        private int connectErrorCount = 0;                        // 连接错误次数
        private int sleepTime { get; set; } = 20;                // 睡眠的时间
        private int receiveTimeout { get; set; } = 5000;         // 接收数据的超时时间
        public bool IsConnected { get; set; }
        protected IEventAggregator Events { get; set; }
        protected string InstName { get; set; }
        public TH2829Port(IEventAggregator events)
        {
            Events = events;
            InstName = "TH2829";
            for (int i = 0; i < 10; i++)
            {
                TestSpecs.Add(new TurnsTestViewModel());
            }         
        }
        public bool Connect()
        {
            try
            {
                if (port.IsOpen)
                    port.Close();

                port.PortName = PortName;
                port.BaudRate = 115200;
                port.Open();
                port.WriteLine("*IDN?");
                port.ReadTimeout = 1000;
                string v = port.ReadLine();
                if (v.Length > 0)
                {
                    if (!string.IsNullOrEmpty(InstName))
                    {
                        IsConnected = v.Contains(InstName);                       
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
                if (RequestCmd("TRS:DATA?", out byte[] reply))
                {
                  //长度检查
                  if(reply.Length!=1891)
                  {

                        Events.Publish(new MsgItem
                        {
                            Level = "D",
                            Time = DateTime.Now,
                            Value = $"{PortName}\t{"Error reply len"}{Environment.NewLine}"
                        });
                        return false;
                  }
                  else
                  {
                        for (int i = 0; i < 10; i++)
                        {
                            TestSpecs[i].TurnsResult = BitConverter.ToSingle(reply, 10 + i * 4);
                            TestSpecs[i].LxsResult = BitConverter.ToSingle(reply, 170 + i * 4);
                            TestSpecs[i].LxsUhResult = BitConverter.ToSingle(reply, 170 + i * 4) * 1000000;
                            TestSpecs[i].LksResult= BitConverter.ToSingle(reply, 330 + i * 4);
                            TestSpecs[i].CxsResult = BitConverter.ToSingle(reply, 490 + i * 4);
                            TestSpecs[i].DcrsResult = BitConverter.ToSingle(reply, 650 + i * 4);
                            TestSpecs[i].LxsqResult = BitConverter.ToSingle(reply, 810 + i * 4);
                            TestSpecs[i].AcrsResult= BitConverter.ToSingle(reply, 970 + i * 4);
                            TestSpecs[i].ZxsResult = BitConverter.ToSingle(reply, 1130 + i * 4);
                            TestSpecs[i].PhsResult = ((Char)reply[1290 + i ]).ToString();
                            TestSpecs[i].TurnsJudge = ((Char)reply[1330 + i]).ToString();
                            TestSpecs[i].LxsJudge = ((Char)reply[1370 + i]).ToString();
                            TestSpecs[i].LksJudge = ((Char)reply[1410 + i]).ToString();
                            TestSpecs[i].CxsJudge = ((Char)reply[1450 + i]).ToString();
                            TestSpecs[i].DcrsJudge = ((Char)reply[1490 + i]).ToString();
                            TestSpecs[i].LxsqJudge = ((Char)reply[1530 + i]).ToString();
                            TestSpecs[i].AcrsJudge = ((Char)reply[1570 + i]).ToString();
                            TestSpecs[i].ZxsJudge = ((Char)reply[1610 + i]).ToString();
                            TestSpecs[i].BlsJudge = ((Char)reply[1650 + i]).ToString();
                            TestSpecs[i].PssJudge = ((Char)reply[1690 + i]).ToString();
                            TestSpecs[i].TrsWholeJudge =  ((Char)reply[1730]).ToString();                          
                        }

                        return true;
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
            catch(Exception ex)
            {

                Events.Publish(new MsgItem
                {
                    Level = "D",
                    Time = DateTime.Now,
                    Value = $"{PortName}\t{ex.Message}{Environment.NewLine}"
                });
                LogHelper.Error(ex.Message);
                return false;
            }
        }
        private bool RequestCmd(string cmd, out byte[] reply)
        {
            Events.Publish(new MsgItem
            {
                Level = "D",
                Time = DateTime.Now,
                Value = $"{PortName}\t{cmd}{Environment.NewLine}"
            });
            if (IsConnected)
            {

                port.WriteLine(cmd);
                try
                {

                    OperateResult<byte[]> result = SPReceived(true);
                    if (!result.IsSuccess)
                    {
                        reply = null;
                        return false;
                    }
                    reply = result.Content;
                    return true;
                }
                catch (Exception ex)
                {
                    reply = null;
                    Events.Publish(new MsgItem
                    {
                        Level = "E",
                        Time = DateTime.Now,
                        Value = $"{PortName}\t{ex.Message}{Environment.NewLine}"
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
                reply = null;
                return false;
            }
        }
        protected  OperateResult<byte[]> SPReceived(bool awaitData)
        {
            byte[] buffer = new byte[2000];
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            DateTime start = DateTime.Now;                                  // 开始时间，用于确认是否超时的信息
            while (true)
            {
                Thread.Sleep(sleepTime);
                try
                {
                    if (port.BytesToRead < 1)
                    {
                        if ((DateTime.Now - start).TotalMilliseconds > receiveTimeout)
                        {
                            ms.Dispose();
                            if (connectErrorCount < 1_0000_0000) connectErrorCount++;
                            return new OperateResult<byte[]>(-connectErrorCount, $"Time out: {receiveTimeout}");
                        }
                        else if (ms.Length > 0)
                        {
                            break;
                        }
                        else if (awaitData)
                        {
                            continue;
                        }
                        else
                        {
                            break;
                        }
                    }

                    // 继续接收数据
                    int sp_receive = port.Read(buffer, 0, buffer.Length);
                    ms.Write(buffer, 0, sp_receive);
                }
                catch (Exception ex)
                {
                    ms.Dispose();
                    if (connectErrorCount < 1_0000_0000) connectErrorCount++;
                    return new OperateResult<byte[]>(-connectErrorCount, ex.Message);
                }
            }
            byte[] result = ms.ToArray();
            ms.Dispose();
            connectErrorCount = 0;
            return OperateResult.CreateSuccessResult(result);
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
                port.WriteLine(cmd);
                try
                {

                    OperateResult<byte[]> result = SPReceived(true);
                    if (!result.IsSuccess)
                    {
                        reply = null;
                        return false;
                    }
                    reply = BytetoHex(result.Content);
                    return true;
                }
                catch (Exception ex)
                {
                    reply =ex.Message;
                    Events.Publish(new MsgItem
                    {
                        Level = "E",
                        Time = DateTime.Now,
                        Value = $"{PortName}\t{ex.Message}{Environment.NewLine}"
                    });
                    return false;
                }
            }
            else
            {
                Connect();
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
        private string BytetoHex(byte[] byteArray)
        {
            var str = new System.Text.StringBuilder();
            for (int i = 0; i < byteArray.Length; i++)
            {
                str.Append(String.Format("{0:X} ", byteArray[i]));
            }
            string s = str.ToString();
            return s;
        }
    }
}
