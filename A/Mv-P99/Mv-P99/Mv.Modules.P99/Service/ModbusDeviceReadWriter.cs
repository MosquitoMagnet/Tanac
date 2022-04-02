using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Communication.Core;
using Communication.ModBus;
using Prism.Logging;

namespace Mv.Modules.P99.Service
{
    public class ModbusDeviceReadWriter : IDeviceReadWriter
    {
        byte[] rbs = new byte[400 * 2];
        byte[] wbs = new byte[40*2];
        public bool IsConnected { get ; set; }
        ModbusTcpNet modbus;
        private readonly ILoggerFacade logger;

        public ModbusDeviceReadWriter(ILoggerFacade logger)
        {
            modbus = new ModbusTcpNet("127.0.0.1", 60000)
            {
                IsStringReverse = true,
                DataFormat = DataFormat.CDAB
            };
      
            modbus.AddressStartWithZero = true;
            Task.Factory.StartNew(() =>
            {
                modbus.ConnectServer();
                while (true)
                {
                    var rr = modbus.Read("0", (ushort)(rbs.Length/2));
                    if (rr.IsSuccess)
                    {                    
                        Buffer.BlockCopy(rr.Content,0,rbs, 0, rr.Content.Length);
                    }
                    else
                    {
                        logger.Log(rr.Message, Category.Warn, Priority.None);
                    }
                    IsConnected = rr.IsSuccess;
                    var wt=modbus.Write("400",wbs);
                    Thread.Sleep(1);
                }
            }, TaskCreationOptions.LongRunning);
            this.logger = logger;
        }
        public ushort GetWord(int index)
        {         
            return modbus.ByteTransform.TransUInt16(rbs, index * 2);
        }
        public bool GetBit(int index, int bit)
        {
            var m = GetWord(index);
            var r = (m & (1 << bit)) > 0;
            return r;
        }

        public short GetShort(int index)
        {
            return modbus.ByteTransform.TransInt16(rbs, index * 2);
        }

        public int GetInt( int index)
        {
            return modbus.ByteTransform.TransInt32(rbs, index * 2);
        }

        public void SetInt( int index, int value)
        {
            Buffer.BlockCopy(modbus.ByteTransform.TransByte(value), 0, wbs, index * 2, 4);
        }

        public void SetShort(int index, short value)
        {
            Buffer.BlockCopy(modbus.ByteTransform.TransByte(value), 0, wbs, index * 2, 2);   
        }

        public void SetBit(int index, int bit, bool value)
        {
            if (value)
            {
                var mInt16 = (ushort)(modbus.ByteTransform.TransUInt16(wbs, index * 2) | (1 << bit));
                SetShort(index, (short)mInt16);
            }
            else
            {
                var mInt16 = (ushort)(modbus.ByteTransform.TransUInt16(wbs, index * 2) & (~(1 << bit)));
                SetShort(index, (short)mInt16);
            }
        }

        public void SetString(int index, string value)
        {
            var bs = modbus.ByteTransform.TransByte(value,Encoding.ASCII);
            Buffer.BlockCopy(bs, 0, wbs, index * 2, bs.Length);
        }

        /// <summary>
        /// 从读取缓冲区读取
        /// </summary>
        /// <param name="index">字的索引</param>
        /// <param name="len">字符串长度</param>
        /// <returns></returns>
        public string GetString(int index, int len)
        {
           return modbus.ByteTransform.TransString(rbs, index, len, Encoding.ASCII);
          
        }

        public bool GetSetBit(int index, int bit)
        {
            return (modbus.ByteTransform.TransUInt16(wbs, index * 2) & (1 << bit)) > 0;
        }

        public void PlcConnect()
        {
          //  throw new NotImplementedException();
        }

    
    }
}
