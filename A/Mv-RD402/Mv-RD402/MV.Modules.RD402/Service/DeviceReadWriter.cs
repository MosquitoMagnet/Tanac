using System;
using System.Collections.Generic;
using System.IO.Packaging;
using System.Text;
using HslCommunication.Profinet.Melsec;
using Mv.Core;
using Mv.Core.Interfaces;
using System.Reactive;
using System.Reactive.Linq;
using MaterialDesignThemes.Wpf;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Sockets;
using HslCommunication;
using System.Collections;
using Newtonsoft.Json;
using System.IO;
using System.Collections.Concurrent;

namespace Mv.Modules.RD402.Service
{
    public class InkPrinter : IInkPrinter
    {
        private readonly IConfigureFile configureFile;
        private readonly RD402Config _config;
        private readonly ISnackbarMessageQueue _messageQueue;

        public InkPrinter(IConfigureFile configureFile, ISnackbarMessageQueue messageQueue)
        {
            this.configureFile = configureFile;
            _config = configureFile.Load().GetValue<RD402Config>(nameof(RD402Config));
            this._messageQueue = messageQueue;
        }

        public async Task<bool> WritePrinterTextAsync(string text)
        {
            try
            {
                using (TcpClient client = new TcpClient())
                {
                    string cmd = $"STM:0:2::1{text}:";
                    byte[] bs = new byte[cmd.Length + 2];
                    byte[] br = new byte[10];
                    await client.ConnectAsync(_config.PrinterIpAddress, _config.PrinterPort).ConfigureAwait(false);
                    var sour = Encoding.ASCII.GetBytes(cmd);
                    Buffer.BlockCopy(sour, 0, bs, 1, sour.Length);
                    bs[0] = 0x02;
                    bs[bs.Length - 1] = 0x03;
                    client.Client.Send(bs);
                    client.Client.Receive(br);
                    if (br[0] == 0x06)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                _messageQueue.Enqueue(ex.Message);
                return false;
            }
        }

        public async Task<bool> WritePrinterCodeAsync(string text)
        {
            try
            {
                using (TcpClient client = new TcpClient
                {
                    ReceiveTimeout = 1000,

                    SendTimeout = 1000
                })
                {
                    await client.ConnectAsync(_config.PrinterIpAddress, _config.PrinterPort).ConfigureAwait(false);
                    string cmd = $"S2M:2:1:::::1:0:1{text}:";
                    byte[] bs = new byte[cmd.Length + 2];
                    byte[] br = new byte[10];
                    var sour = Encoding.ASCII.GetBytes(cmd);
                    Buffer.BlockCopy(sour, 0, bs, 1, sour.Length);
                    bs[0] = 0x02;
                    bs[bs.Length - 1] = 0x03;
                    client.Client.Send(bs);
                    client.Client.Receive(br);
                    if (br[0] == 0x06)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                _messageQueue.Enqueue(ex.Message);
                return false;
                // throw;
            }
        }
    }

    public class DeviceReadWriter : IDisposable, IDeviceReadWriter
    {
        public DeviceReadWriter(IConfigureFile configureFile, ISnackbarMessageQueue messageQueue)
        {

            Guards.ThrowIfNull(configureFile);
            _messageQueue = messageQueue;
            _configureFile = configureFile.Load();
            if (_configureFile.Contains(nameof(RD402Config)))
            {
                _config = _configureFile.GetValue<RD402Config>(nameof(RD402Config));
            }
            else
            {
                _config = new RD402Config();
                _configureFile.SetValue(nameof(RD402Config), _config);
            }
            PlcConnect();
        }
        public bool IsConnected { get; set; }
        public void PlcConnect()
        {
            _config = _configureFile.Load().GetValue<RD402Config>(nameof(RD402Config));
            _melsec = new MelsecMcNet { IpAddress = _config.PLCIpAddress, Port = _config.PLCPort };
            if (_rbs == null || _rbs.Length != _config.ReadLens * 2)
                _rbs = new byte[_config.ReadLens * 2];
            if (_wbs == null || _wbs.Length != _config.WriteLens * 2)
                _wbs = new byte[_config.WriteLens * 2];
            _cts?.Cancel();

            _cts = new CancellationTokenSource();
            try
            {
                Task.Factory.StartNew(() =>
                {
                    OperateResult con = _melsec.ConnectServer();
                    if (!con.IsSuccess)
                        _messageQueue.Enqueue(con.Message);
                    if (_cts.IsCancellationRequested)
                        return;
                    while (!_cts.IsCancellationRequested)
                    {
                        int istart = Environment.TickCount;
                        var rbs = PlcRead();
                        if (rbs != null)
                        {
                            Buffer.BlockCopy(rbs, 0, _rbs, 0, rbs.Length);
                        }
                        PlcWrite(_wbs);
                        Thread.Sleep(1);
                    }
                }, _cts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
            }
            catch (AggregateException exs)
            {
                foreach (var e in exs.InnerExceptions)
                {
                    _messageQueue.Enqueue(e.Message);
                }
            }

        }

        byte[] PlcRead()
        {
            var rs = _melsec.Read(_config.ReadAddrStart, _config.ReadLens);
            IsConnected = rs.IsSuccess;
            if (rs.IsSuccess)
            {
                return rs.Content;
            }
            else
            {
                return null;
            }
        }

        bool PlcWrite(byte[] bs)
        {
            var con = _melsec.Write(_config.WriteAddrStart, bs);

            return con.IsSuccess;
        }

        public ushort GetWord(int index)
        {
            return BitConverter.ToUInt16(_rbs, index * 2);
        }
        public bool GetBit(int index, int bit)
        {
            var m = GetWord(index);
            var r = (m & (1 << bit)) > 0;
            return r;
        }

        public short GetShort(int index)
        {
            return BitConverter.ToInt16(_rbs, index * 2);
        }

        public void SetShort(int index, short value)
        {
            Buffer.BlockCopy(BitConverter.GetBytes(value), 0, _wbs, index * 2, 2);

        }

        public void SetBit(int index, int bit, bool value)
        {
            if (value)
            {
                var mInt16 = (ushort)(BitConverter.ToUInt16(_wbs, index * 2) | (1 << bit));
                SetShort(index, (short)mInt16);
            }
            else
            {
                var mInt16 = (ushort)(BitConverter.ToUInt16(_wbs, index * 2) & (~(1 << bit)));
                SetShort(index, (short)mInt16);
            }
        }

        public void SetString(int index, string value)
        {
            var bs = Encoding.ASCII.GetBytes(value);
            Buffer.BlockCopy(bs, 0, _wbs, index * 2, bs.Length);
        }

        /// <summary>
        /// 从读取缓冲区读取
        /// </summary>
        /// <param name="index">字的索引</param>
        /// <param name="len">字符串长度</param>
        /// <returns></returns>
        public string GetString(int index, int len)
        {
            return Encoding.ASCII.GetString(_rbs, index, len);
        }

        public bool GetSetBit(int index, int bit)
        {
            return (BitConverter.ToUInt16(_wbs, index * 2) & (1 << bit)) > 0;
        }

        MelsecMcNet _melsec;
        IConfigureFile _configureFile;
        RD402Config _config;
        ISnackbarMessageQueue _messageQueue;
        CancellationTokenSource _cts;
        byte[] _rbs;
        byte[] _wbs;


        bool disposed = false;
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;
            if (disposing)
            {
                _cts.Dispose();
            }
            disposed = true;
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }

}
