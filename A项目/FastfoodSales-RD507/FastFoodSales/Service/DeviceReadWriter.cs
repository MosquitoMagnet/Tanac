using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Communication.ModBus;
using Communication.Core;
using System.Threading;
using DAQ.Core.Log;
using Stylet;

namespace DAQ.Service
{
    public class ModbusDeviceVM : PropertyChangedBase
    {
        public short IsConnected { get; set; }
    }
    public class ModbusDeviceReadWriter : IDeviceReadWriter
    {
        byte[] rbs = new byte[200 * 2];
        byte[] wbs = new byte[90 * 2];
        byte[] exwbs = new byte[90 * 2];

        public ModbusDeviceVM ModbusDeviceVm { get; set; } = new ModbusDeviceVM();
        ModbusTcpNet modbus;
        private IConfigureFile configure;
        private Config config;
        public ModbusDeviceReadWriter(IConfigureFile configure)
        {
            this.configure = configure;
            config = configure.Load().GetValue<Config>(nameof(Config)) ?? new Config();
            modbus = new ModbusTcpNet(config.PlcIpAddress, config.PlcPort)
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
                    var rr = modbus.Read("0", (ushort)(rbs.Length / 2));
                    if (rr.IsSuccess)
                    {
                        Buffer.BlockCopy(rr.Content, 0, rbs, 0, rr.Content.Length);
                    }
                    else
                    {
                        LogHelper.Info(rr.Message);
                    }
                    var wt = modbus.Write("200", wbs);
                    if (rr.IsSuccess&&wt.IsSuccess)
                        ModbusDeviceVm.IsConnected = 1;
                    else
                        ModbusDeviceVm.IsConnected = -1;
                    var exwt = modbus.Write("290", exwbs);
                    if (exwt.IsSuccess && exwt.IsSuccess)
                        ModbusDeviceVm.IsConnected = 1;
                    else
                        ModbusDeviceVm.IsConnected = -1;
                    Thread.Sleep(1);
                }
            }, TaskCreationOptions.LongRunning);
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

        public int GetInt(int index)
        {
            return modbus.ByteTransform.TransInt32(rbs, index * 2);
        }

        public float GetFloat(int index)
        {
            return modbus.ByteTransform.TransSingle(rbs, index * 2);
        }


        public void SetInt(int index, int value)
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
            var bs = modbus.ByteTransform.TransByte(value, Encoding.ASCII);
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
            return modbus.ByteTransform.TransString(rbs, index*2, len, Encoding.ASCII);

        }

        public bool GetSetBit(int index, int bit)
        {
            return (modbus.ByteTransform.TransUInt16(wbs, index * 2) & (1 << bit)) > 0;
        }
        public void SetBytes(int index, byte[] value)
        {
            Buffer.BlockCopy(value, 0, wbs, index * 2, value.Length);
        }
        public byte[] GetBytes(int index, int len)
        {
            return modbus.ByteTransform.TransByte(rbs, index * 2, len);
        }
        /// <summary>
        /// 设置拓展区域的bytes
        /// </summary>
        /// <param name="index"></param>
        /// <param name="value"></param>
        public void SetBytesEx(int index, byte[] value)
        {
            Buffer.BlockCopy(value, 0, exwbs, index * 2, value.Length);
        }

        public void PlcConnect()
        {
          

        }

    }
}
