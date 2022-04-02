using Communication.Core;
using Communication.ModBus;
using Prism.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Mv.Modules.P99.Service
{
    public class PlcCognexComm : IPlcCognexComm
    {

        List<(byte[], byte[])> localbs = new List<(byte[], byte[])>(2) {
             (new byte[10*2],new byte[30*2]),
            (new byte[10*2],new byte[30*2])
        };
        public bool IsConnected { get; set; }
        ModbusTcpNet modbus;
        private readonly ILoggerFacade logger;

        public PlcCognexComm(ILoggerFacade logger)
        {
            modbus = new ModbusTcpNet("192.168.1.1", 5000)
            {
                DataFormat = DataFormat.CDAB,
                IsStringReverse = true
            };
        
            modbus.AddressStartWithZero = true;

            Task.Factory.StartNew(() =>
            {
                modbus.ConnectServer();
                while (true)
                {

                    for (int i = 0; i < 2; i++)
                    {
                        var rr = modbus.Read((0 + 40 * i).ToString(), 10);
                        if (rr.IsSuccess)
                        {
                            Buffer.BlockCopy(rr.Content, 0, localbs[i].Item1, 0, rr.Content.Length);
                        }
                        else
                        {
                            logger.Log(rr.Message, Category.Warn, Priority.None);
                        }
                        IsConnected = rr.IsSuccess;
                        var wt = modbus.Write((10 + 40 * i).ToString(), localbs[i].Item2);
                    }
                }
            }, TaskCreationOptions.LongRunning);
            this.logger = logger;
        }
        public ushort GetWord(int id, int index)
        {
            return modbus.ByteTransform.TransUInt16(localbs[id].Item1, index * 2);
        }
        public bool GetBit(int id, int index, int bit)
        {
            var m = GetWord(id, index);
            var r = (m & (1 << bit)) > 0;
            return r;
        }

        public short GetShort(int id, int index)
        {
            return modbus.ByteTransform.TransInt16(localbs[id].Item1, index * 2);
        }

        public uint GetUInt(int id, int index)
        {
            return modbus.ByteTransform.TransUInt32(localbs[id].Item1, index * 2);
        }

        public int GetInt(int id, int index)
        {
            return modbus.ByteTransform.TransInt32(localbs[id].Item1, index * 2);
        }

        public void SetInt(int id, int index, int value)
        {
            Buffer.BlockCopy(modbus.ByteTransform.TransByte(value), 0, localbs[id].Item2, index * 2, 4);
        }

        public void SetInt(int id, int index, uint value)
        {
            Buffer.BlockCopy(modbus.ByteTransform.TransByte(value), 0, localbs[id].Item2, index * 2, 4);
        }
        public void SetShort(int id, int index, short value)
        {
            Buffer.BlockCopy(modbus.ByteTransform.TransByte(value), 0, localbs[id].Item2, index * 2, 2);
        }

        public void SetBit(int id, int index, int bit, bool value)
        {
            if (value)
            {
                var mInt16 = (ushort)(modbus.ByteTransform.TransUInt16(localbs[id].Item2, index * 2) | (1 << bit));
                SetShort(id, index, (short)mInt16);
            }
            else
            {
                var mInt16 = (ushort)(modbus.ByteTransform.TransUInt16(localbs[id].Item2, index * 2) & (~(1 << bit)));
                SetShort(id, index, (short)mInt16);
            }
        }

        public void SetString(int id, int index, string value)
        {
            var bs = modbus.ByteTransform.TransByte(value, Encoding.ASCII);
            Buffer.BlockCopy(bs, 0, localbs[id].Item2, index * 2, bs.Length);
        }

        /// <summary>
        /// 从读取缓冲区读取
        /// </summary>
        /// <param name="index">字的索引</param>
        /// <param name="len">字符串长度</param>
        /// <returns></returns>
        public string GetString(int id, int index, int len)
        {
            return modbus.ByteTransform.TransString(localbs[id].Item1, index, len, Encoding.ASCII);

        }

        public bool GetSetBit(int id, int index, int bit)
        {
            return (modbus.ByteTransform.TransUInt16(localbs[id].Item2, index * 2) & (1 << bit)) > 0;
        }

        public void PlcConnect()
        {
            //  throw new NotImplementedException();
        }

    }
    public interface IPlcScannerComm : IPlcCognexComm { }
}
