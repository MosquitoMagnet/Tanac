using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.IO;
using System.Collections;
using DataService;
using Communication.ModBus;
using Communication;
using Communication.Core.Address;
using Communication.Core.Net;

namespace CommonDriver
{
    [Description("ModbusTcp协议")]
    public class ModbusTcpDriver : DriverInitBase, IPLCDriver, IMultiReadWrite
    {
        string _ip;//服务ip
        int _port = 502; //服务端口



        public int Port
        {
            get { return _port; }
            set { _port = value; }
        }

        public string ServerName
        {
            get { return _ip; }
            set { _ip = value; }
        }
        List<IGroup> _grps = new List<IGroup>(20);
        public short ID
        {
            get;
        }
        public string Name { get; }
        private bool _IsClosed = true;
        protected ModbusTcpNet mc = new ModbusTcpNet();
        public ModbusTcpDriver(IDataServer server, short id, string name, string serverName, int timeOut = 500, IDictionary<string, string> paras = null) : base(server, id, name, serverName, timeOut, paras)
        {
            ID = id;
            Name = name;
            Parent = server;
            _ip = serverName;

        }

        public ModbusTcpDriver() : base(null, 0, "modbustcp", "127.0.0.1", 100, null)
        {
            ID = 0;
            Name = "modbustcp";
            Parent = null;
            _ip = "127.0.0.1";
        }

        event EventHandler<Exception> IDriver.OnError
        {
            add
            {
                throw new NotImplementedException();
            }

            remove
            {
                throw new NotImplementedException();
            }
        }

        public bool IsClosed => _IsClosed;
        public int TimeOut { get; set; }
        public IEnumerable<IGroup> Groups => _grps;
        public IDataServer Parent { get; }
        /// <summary>
        /// 连接
        /// </summary>
        /// <returns></returns>
        public bool Connect()
        {
            mc.Port = Port;
            mc.DataFormat = Communication.Core.DataFormat.CDAB;
            mc.IpAddress = ServerName ?? "127.0.0.1";
            var MR = mc.ConnectServer();
            _IsClosed = !MR.IsSuccess;

            return mc.ConnectServer().IsSuccess;
        }
        public IGroup AddGroup(string name, short id, int updateRate, float deadBand = 0, bool active = false)
        {
            NetShortGroup grp = new NetShortGroup(id, name, updateRate, active, this);
            _grps.Add(grp);
            return grp;
        }

        public bool RemoveGroup(IGroup grp)
        {
            grp.IsActive = false;
            return _grps.Remove(grp);
        }

        public event ErrorEventHandler OnError;
        public event ShutdownRequestEventHandler OnClose;

        /// <summary>
        /// 读取字节数组
        /// </summary>
        /// <param name="address">标签变量地址结构</param>
        /// <param name="size">长度,</param>
        /// <returns></returns>
        public byte[] ReadBytes(DeviceAddress address, ushort size)
        {
            if (address.DBNumber != 1)
            {
                var addr = GetAddress(address);
                //    addr = addr.Substring(0, addr.IndexOf('.'));          
                var r = mc.Read(addr, size);
                _IsClosed = !r.IsSuccess;
                return r.Content;
            }
            else
            {
                var addr = GetAddress(address);
                var r = address.DBNumber == 2 ? mc.ReadDiscrete(addr, (ushort)(size * 16)) :
                    mc.ReadCoil(addr, (ushort)(size * 16));
                _IsClosed = !r.IsSuccess;
                var content = r.Content;
                byte[] data = new byte[2 * size];
                BitArray array = new BitArray(content);
                array.CopyTo(data, 0);
                return data;
            }
        }

        /// <summary>
        /// 读取无符号32位整数
        /// </summary>
        /// <param name="address">标签变量地址结构</param>
        /// <returns></returns>
        public ItemData<uint> ReadUInt32(DeviceAddress address)
        {
            var r = mc.ReadUInt32(GetAddress(address));
            _IsClosed = !r.IsSuccess;
            return r.IsSuccess ? new ItemData<uint>(r.Content, 0, QUALITIES.QUALITY_GOOD)
                : new ItemData<uint>(0, 0, QUALITIES.QUALITY_BAD);
        }
        /// <summary>
        /// 读取有符号32位整数
        /// </summary>
        /// <param name="address">标签变量地址结构</param>
        /// <returns></returns>
        public ItemData<int> ReadInt32(DeviceAddress address)
        {
            var r = mc.ReadInt32(GetAddress(address));
            _IsClosed = !r.IsSuccess;
            return r.IsSuccess ? new ItemData<int>(r.Content, 0, QUALITIES.QUALITY_GOOD)
                : new ItemData<int>(0, 0, QUALITIES.QUALITY_BAD);
        }
        /// <summary>
        /// 读取无符号16位整数
        /// </summary>
        /// <param name="address">标签变量地址结构</param>
        /// <returns></returns>
        public ItemData<ushort> ReadUInt16(DeviceAddress address)
        {
            var r = mc.ReadUInt16(GetAddress(address));
            _IsClosed = !r.IsSuccess;
            return r.IsSuccess ? new ItemData<ushort>(r.Content, 0, QUALITIES.QUALITY_GOOD)
                : new ItemData<ushort>(0, 0, QUALITIES.QUALITY_BAD);
        }
        /// <summary>
        /// 读取有符号16位整数
        /// </summary>
        /// <param name="address">标签变量地址结构</param>
        /// <returns></returns>
        public ItemData<short> ReadInt16(DeviceAddress address)
        {
            var r = mc.ReadInt16(GetAddress(address));
            _IsClosed = !r.IsSuccess;
            return r.IsSuccess ? new ItemData<short>(r.Content, 0, QUALITIES.QUALITY_GOOD)
                : new ItemData<short>(0, 0, QUALITIES.QUALITY_BAD);
        }
        /// <summary>
        /// 读取单字节
        /// </summary>
        /// <param name="address">标签变量地址结构</param>
        /// <returns></returns>
        public ItemData<byte> ReadByte(DeviceAddress address)
        {
            var r = mc.Read(GetAddress(address), 1);
            _IsClosed = !r.IsSuccess;
            return !r.IsSuccess ? new ItemData<byte>(0, 0, QUALITIES.QUALITY_BAD)
                : new ItemData<byte>(r.Content[0], 0, QUALITIES.QUALITY_GOOD);
        }
        /// <summary>
        /// 读取字符串
        /// </summary>
        /// <param name="address">标签变量地址结构</param>
        /// <param name="size">长度</param>
        /// <returns></returns>
        public ItemData<string> ReadString(DeviceAddress address, ushort size)
        {
            var r = mc.ReadString(GetAddress(address), size);
            return r.IsSuccess ? new ItemData<string>(r.Content, 0, QUALITIES.QUALITY_GOOD)
                : new ItemData<string>("", 0, QUALITIES.QUALITY_BAD);
        }
        /// <summary>
        /// 读取32位浮点数 
        /// </summary>
        /// <param name="address">标签变量地址结构</param>
        /// <returns></returns>
        public ItemData<float> ReadFloat(DeviceAddress address)
        {
            var r = mc.ReadFloat(GetAddress(address));
            _IsClosed = !r.IsSuccess;
            return r.IsSuccess ? new ItemData<float>(r.Content, 0, QUALITIES.QUALITY_GOOD)
                : new ItemData<float>(0.0f, 0, QUALITIES.QUALITY_BAD);
        }
        /// <summary>
        /// 读取单个位
        /// </summary>
        /// <param name="address">标签变量地址结构体</param>
        /// <returns></returns>
        public ItemData<bool> ReadBit(DeviceAddress address)
        {
            var r = mc.ReadCoil(GetAddressBit(address));
            _IsClosed = !r.IsSuccess;
            return r.IsSuccess ? new ItemData<bool>(r.Content, 0, QUALITIES.QUALITY_GOOD)
                : new ItemData<bool>(false, 0, QUALITIES.QUALITY_BAD);
        }
        /// <summary>
        /// 读object类型
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public ItemData<object> ReadValue(DeviceAddress address)
        {
            return this.ReadValueEx(address);
        }

        /// <summary>
        /// 写字节数组到设备
        /// </summary>
        /// <param name="address">标签变量地址结构</param>
        /// <param name="bits">需写的字节数组</param>
        /// <returns></returns>
        public int WriteBytes(DeviceAddress address, byte[] bits)
        {
            return mc.Write(GetAddress(address), bits).IsSuccess ? 0 : -1;
        }
        /// <summary>
        /// 写单个位到设备
        /// </summary>
        /// <param name="address">标签变量地址结构</param>
        /// <param name="bit">需写的状态</param>
        /// <returns></returns>
        public int WriteBit(DeviceAddress address, bool bit)
        {
            return mc.Write(GetAddressBit(address), bit).IsSuccess ? 0 : -1;
        }
        /// <summary>
        /// 写位数组到设备
        /// </summary>
        /// <param name="address">标签变量地址结构</param>
        /// <param name="bits">需写的位数组</param>
        /// <returns></returns>
        public int WriteBits(DeviceAddress address, byte bits)
        {
            return mc.Write(GetAddress(address), bits).IsSuccess ? 0 : -1;
        }
        /// <summary>
        /// 写有符号16位整数到设备
        /// </summary>
        /// <param name="address">标签变量地址结构</param>
        /// <param name="value">需写的数据</param>
        /// <returns></returns>
        public int WriteInt16(DeviceAddress address, short value)
        {
            return mc.Write(GetAddress(address), value).IsSuccess ? 0 : -1;
        }
        /// <summary>
        /// 写无符号16位整数到设备
        /// </summary>
        /// <param name="address">标签变量地址结构</param>
        /// <param name="value">需写的数据</param>
        /// <returns></returns>
        public int WriteUInt16(DeviceAddress address, ushort value)
        {
            return mc.Write(GetAddress(address), value).IsSuccess ? 0 : -1;
        }
        /// <summary>
        /// 写有符号32位整数到设备
        /// </summary>
        /// <param name="address">标签变量地址结构</param>
        /// <param name="value">需写的数据</param>
        /// <returns></returns>
        public int WriteInt32(DeviceAddress address, int value)
        {
            return mc.Write(GetAddress(address), value).IsSuccess ? 0 : -1;
        }
        /// <summary>
        /// 写无符号32位整数到设备
        /// </summary>
        /// <param name="address">标签变量地址结构</param>
        /// <param name="value">需写的数据</param>
        /// <returns></returns>
        public int WriteUInt32(DeviceAddress address, uint value)
        {
            return mc.Write(GetAddress(address), value).IsSuccess ? 0 : -1;
        }
        /// <summary>
        /// 写32位浮点数到设备
        /// </summary>
        /// <param name="address">标签变量地址结构</param>
        /// <param name="value">需写的数据</param>
        /// <returns></returns>
        public int WriteFloat(DeviceAddress address, float value)
        {
            return mc.Write(GetAddress(address), value).IsSuccess ? 0 : -1;
        }
        /// <summary>
        /// 写字符串到设备
        /// </summary>
        /// <param name="address">标签变量地址结构</param>
        /// <param name="value">需写的数据</param>
        /// <returns></returns>
        public int WriteString(DeviceAddress address, string str)
        {
            return mc.Write(GetAddress(address), str).IsSuccess ? 0 : -1;
        }
        /// <summary>
        /// 写object类型
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public int WriteValue(DeviceAddress address, object value)
        {
            return this.WriteValueEx(address, value);
        }

        public int Limit { get; } = 960;

        public int PDU => 960;

        public ItemData<Storage>[] ReadMultiple(DeviceAddress[] addrsArr)
        {
            return this.PLCReadMultiple(new ShortCacheReader(), addrsArr);
        }
        public int WriteMultiple(DeviceAddress[] addrArr, object[] buffer)
        {
            return this.PLCWriteMultiple(new ShortCacheReader(), addrArr, buffer, Limit);
        }
        /// <summary>
        /// 获取设备地址
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public virtual DeviceAddress GetDeviceAddress(string address)
        {

            DeviceAddress dv = DeviceAddress.Empty;
            if (string.IsNullOrEmpty(address))
                return dv;
            var sindex = address.IndexOf(':');
            if (sindex > 0)
            {
                int slaveId;
                if (int.TryParse(address.Substring(0, sindex), out slaveId))
                    dv.Area = slaveId;
                address = address.Substring(sindex + 1);
            }
            if (address.StartsWith("MW"))
            {
                int index = address.IndexOf('.');
                dv.DBNumber = 3;
                if (index > 0)
                {
                    dv.Start = int.Parse(address.Substring(2, index - 1));
                    dv.Bit = byte.Parse(address.Substring(index + 1));
                }
                else
                    dv.Start = int.Parse(address.Substring(2));
                dv.Start += 1;
                //dv.ByteOrder = ByteOrder.Network;
            }
            switch (address[0])
            {
                case '1':
                    {
                        dv.DBNumber = 1;
                        int st;
                        int.TryParse(address, out st);
                        dv.Bit = (byte)(st % 16);
                        st /= 16;
                        dv.Start = st;
                    }
                    break;
                case '2':
                    {
                        dv.DBNumber = 2;
                        int st;
                        int.TryParse(address.Substring(1), out st);
                        dv.Bit = (byte)(st % 16);
                        st /= 16;
                        dv.Start = st;
                    }
                    break;
                case '3':
                    {
                        int index = address.IndexOf('.');
                        dv.DBNumber = 3;
                        if (index > 0)
                        {
                            dv.Start = int.Parse(address.Substring(1, index - 1));
                            dv.Bit = byte.Parse(address.Substring(index + 1));
                        }
                        else
                            dv.Start = int.Parse(address.Substring(1));
                        //dv.ByteOrder = ByteOrder.Network;
                    }
                    break;
                case '4':
                    {
                        int index = address.IndexOf('.');
                        dv.DBNumber = 4;
                        if (index > 0)
                        {
                            dv.Start = int.Parse(address.Substring(1, index - 1));
                            dv.Bit = byte.Parse(address.Substring(index + 1));
                        }
                        else
                            dv.Start = int.Parse(address.Substring(1));
                        //dv.ByteOrder = ByteOrder.Network;
                    }
                    break;
            }
            return dv;
        }
        public virtual string GetAddress(DeviceAddress address)
        {
            if (address.Area == 0) return address.DBNumber == 4 ? string.Format("x={0};{1}", address.DBNumber, address.Start)
                : address.Start.ToString();
            else return address.DBNumber == 4 ? string.Format("s={0};x={0};{1}", address.Area, address.DBNumber, address.Start)
               : string.Format("s={0};{1}", address.Area, address.Start);
        }
        public string GetAddressBit(DeviceAddress address)
        {
            if (address.Area == 0) return address.DBNumber == 4 ? string.Format("s={0};x={0};{1}.{2}", address.DBNumber, address.Start, address.Bit)
              : address.DBNumber == 3 ? address.Start.ToString() : (address.Start + address.Bit).ToString();
            else return address.DBNumber == 4 ? string.Format("s={0};x={0};{1}.{2}", address.Area, address.DBNumber, address.Start, address.Bit)
              : address.DBNumber == 3 ? string.Format("s={0};{1}", address.Area, address.Start) : string.Format("s={0};{1}", address.Area, address.Start + address.Bit);
        }
        public void Dispose()
        {
            mc.ConnectClose();
        }
    }
}


