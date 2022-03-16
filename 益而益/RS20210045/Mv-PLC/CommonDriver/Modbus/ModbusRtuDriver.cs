using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.ComponentModel;
using System.IO;
using RJCP.IO.Ports;
using System.Collections;
using DataService;
using Communication.ModBus;
using Communication;
using Communication.Core.Address;
using Communication.Core.Net;

namespace CommonDriver
{
    [Description("ModbusRtu协议")]
    public class ModbusRtuDriver : DriverInitBase, IPLCDriver, IMultiReadWrite
    {
        public string port { get; set; } = "ModbusRtu";      
        public string ServerName
        {
            get { return port; }
            set { port = value; }
        }
        List<IGroup> _grps = new List<IGroup>(20);
        public short ID
        {
            get;
        }
        public string Name { get; }

        IDataServer Server;
        public IDataServer Parent
        {
            get;
        }
        private int _timeOut;
        public int TimeOut
        {
            get { return _timeOut; }
            set { _timeOut = value; }
        }

        string _port = "COM1"; //服务端口
        public string Port
        {
            get { return _port; }
            set { _port = value; }
        }
        int _baudrate = 9600; //服务端口
        public int BaudRate
        {
            get { return _baudrate; }
            set { _baudrate = value; }
        }

        private RJCP.IO.Ports.Parity _parity = RJCP.IO.Ports.Parity.None;//奇偶性
        public RJCP.IO.Ports.Parity Parity
        {
            get { return _parity; }
            set { _parity = value; }
        }

        private SerialPortDataBits _databits = SerialPortDataBits.Eight;//停止位
        public SerialPortDataBits DataBits
        {
            get { return _databits; }
            set { _databits = value; }
        }

        private RJCP.IO.Ports.StopBits _stopbits = RJCP.IO.Ports.StopBits.One;//停止位
        public RJCP.IO.Ports.StopBits StopBits
        {
            get { return _stopbits; }
            set { _stopbits = value; }
        }
        private Communication.Core.DataFormat _dataformat = Communication.Core.DataFormat.CDAB;
        public Communication.Core.DataFormat DataFormat
        {
            get { return _dataformat; }
            set { _dataformat = value; }
        }
        private bool _isStringReverse = true;
        public bool IsStringReverse
        {
            get { return _isStringReverse; }
            set { _isStringReverse = value; }
        }
        private bool _IsClosed = true;
        protected ModbusRtu mc = new ModbusRtu();
        public ModbusRtuDriver(IDataServer server, short id, string name, string serverName, int timeOut = 10000, IDictionary<string, string> paras = null)
            : base(server, id, name, serverName, timeOut, paras)
        {
            ID = id;
            Name = name;
            Parent = server;


        }

        public ModbusRtuDriver()
        {
            ID = 0;
            Name = "modbusrtu";
            Parent = null;


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

        public IEnumerable<IGroup> Groups => _grps;
        /// <summary>
        /// 连接
        /// </summary>
        /// <returns></returns>
        public bool Connect()
        {
            mc.DataFormat = DataFormat;
            mc.IsStringReverse = IsStringReverse;

            mc.SerialPortInni(sp =>
            {
                sp.PortName = _port;//端口号
                sp.BaudRate = _baudrate;//波特率
                sp.DataBits = (int)_databits;//数据位
                sp.StopBits = _stopbits;//数据位
                sp.Parity = _parity;//奇偶
            });

            var MR = mc.Open();
            _IsClosed = !MR.IsSuccess;
            return mc.Open().IsSuccess;

        }
        public IGroup AddGroup(string name, short id, int updateRate, float deadBand = 0, bool active = false)
        {
            ModbusShortGroup grp = new ModbusShortGroup(id, name, updateRate, active, this);
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
        /// 读取字节数组,支持读线圈、保存寄存器、离散输入、输入寄存器
        /// </summary>
        /// <param name="address">标签变量地址结构</param>
        /// <param name="size">长度,</param>
        /// <returns></returns>
        public byte[] ReadBytes(DeviceAddress address, ushort size)
        {
            if (address.DBNumber == 1)
            {
                var addr = GetAddress(address);
                var r = mc.ReadCoil(addr, (ushort)(size * 16));
                _IsClosed = !r.IsSuccess;
                byte[] data = new byte[2 * size];
                var content = r.Content;
                if(r.IsSuccess)
                {
                    BitArray array = new BitArray(content);
                    array.CopyTo(data, 0);
                }               
                return data;

            }
            else if (address.DBNumber == 2)
            {
                var addr = GetAddress(address);
                var r = mc.ReadDiscrete(addr, (ushort)(size * 16));
                _IsClosed = !r.IsSuccess;
                byte[] data = new byte[2 * size];
                var content = r.Content;
                if (r.IsSuccess)
                {
                    BitArray array = new BitArray(content);
                    array.CopyTo(data, 0);
                }
                return data;
            }
            else
            {
                var addr = GetAddress(address);
                var r = mc.Read(addr, size);
                _IsClosed = !r.IsSuccess;
                return r.Content;
            }
        }

        /// <summary>
        /// 读取无符号32位整数,支持读保持寄存器、输入寄存器
        /// </summary>
        /// <param name="address">标签变量地址结构</param>
        /// <returns></returns>
        public ItemData<uint> ReadUInt32(DeviceAddress address)
        {
            if (address.DBNumber == 3 || address.DBNumber == 4)
            {
                var r = mc.ReadUInt32(GetAddress(address));
                _IsClosed = !r.IsSuccess;
                return r.IsSuccess ? new ItemData<uint>(r.Content, 0, QUALITIES.QUALITY_GOOD)
                    : new ItemData<uint>(0, 0, QUALITIES.QUALITY_BAD);
            }
            else
            {
                return new ItemData<uint>(0, 0, QUALITIES.QUALITY_BAD);
            }
        }
        /// <summary>
        /// 读取有符号32位整数，支持读保持寄存器、输入寄存器
        /// </summary>
        /// <param name="address">标签变量地址结构</param>
        /// <returns></returns>
        public ItemData<int> ReadInt32(DeviceAddress address)
        {
            if (address.DBNumber == 3 || address.DBNumber == 4)
            {
                var r = mc.ReadInt32(GetAddress(address));
                _IsClosed = !r.IsSuccess;
                return r.IsSuccess ? new ItemData<int>(r.Content, 0, QUALITIES.QUALITY_GOOD)
                    : new ItemData<int>(0, 0, QUALITIES.QUALITY_BAD);
            }
            else
            {
                return new ItemData<int>(0, 0, QUALITIES.QUALITY_BAD);
            }
        }
        /// <summary>
        /// 读取无符号16位整数，支持读保持寄存器、输入寄存器
        /// </summary>
        /// <param name="address">标签变量地址结构</param>
        /// <returns></returns>
        public ItemData<ushort> ReadUInt16(DeviceAddress address)
        {
            if (address.DBNumber == 3 || address.DBNumber == 4)
            {
                var r = mc.ReadUInt16(GetAddress(address));
                _IsClosed = !r.IsSuccess;
                return r.IsSuccess ? new ItemData<ushort>(r.Content, 0, QUALITIES.QUALITY_GOOD)
                    : new ItemData<ushort>(0, 0, QUALITIES.QUALITY_BAD);
            }
            else
            {
                return new ItemData<ushort>(0, 0, QUALITIES.QUALITY_BAD);
            }
        }
        /// <summary>
        /// 读取有符号16位整数,支持读保持寄存器、输入寄存器
        /// </summary>
        /// <param name="address">标签变量地址结构</param>
        /// <returns></returns>
        public ItemData<short> ReadInt16(DeviceAddress address)
        {
            if (address.DBNumber == 3 || address.DBNumber == 4)
            {
                var r = mc.ReadInt16(GetAddress(address));
                _IsClosed = !r.IsSuccess;
                return r.IsSuccess ? new ItemData<short>(r.Content, 0, QUALITIES.QUALITY_GOOD)
                    : new ItemData<short>(0, 0, QUALITIES.QUALITY_BAD);
            }
            else
            {
                return new ItemData<short>(0, 0, QUALITIES.QUALITY_BAD);
            }
        }
        /// <summary>
        /// 读取单字节,不支持读单个字节
        /// </summary>
        /// <param name="address">标签变量地址结构</param>
        /// <returns></returns>
        public ItemData<byte> ReadByte(DeviceAddress address)
        {
            return new ItemData<byte>(0, 0, QUALITIES.QUALITY_BAD);
        }
        /// <summary>
        /// 读取字符串，支持读保持寄存器、输入寄存器
        /// </summary>
        /// <param name="address">标签变量地址结构</param>
        /// <param name="size">长度</param>
        /// <returns></returns>
        public ItemData<string> ReadString(DeviceAddress address, ushort size)
        {
            if (address.DBNumber == 3 || address.DBNumber == 4)
            {
                var r = mc.ReadString(GetAddress(address), size);
                return r.IsSuccess ? new ItemData<string>(r.Content, 0, QUALITIES.QUALITY_GOOD)
                    : new ItemData<string>("", 0, QUALITIES.QUALITY_BAD);
            }
            else
            {
                return new ItemData<string>("", 0, QUALITIES.QUALITY_BAD);
            }
        }
        /// <summary>
        /// 读取32位浮点数，支持读保持寄存器、输入寄存器
        /// </summary>
        /// <param name="address">标签变量地址结构</param>
        /// <returns></returns>
        public ItemData<float> ReadFloat(DeviceAddress address)
        {
            if (address.DBNumber == 3 || address.DBNumber == 4)
            {
                var r = mc.ReadFloat(GetAddress(address));
                _IsClosed = !r.IsSuccess;
                return r.IsSuccess ? new ItemData<float>(r.Content, 0, QUALITIES.QUALITY_GOOD)
                    : new ItemData<float>(0.0f, 0, QUALITIES.QUALITY_BAD);
            }
            else
            {
                return new ItemData<float>(0.0f, 0, QUALITIES.QUALITY_BAD);
            }
        }
        /// <summary>
        /// 读取单个位,支持读线圈、离散输入
        /// </summary>
        /// <param name="address">标签变量地址结构体</param>
        /// <returns></returns>
        public ItemData<bool> ReadBit(DeviceAddress address)
        {
            if (address.DBNumber == 1)
            {
                var r = mc.ReadCoil(GetAddress(address));
                _IsClosed = !r.IsSuccess;
                return r.IsSuccess ? new ItemData<bool>(r.Content, 0, QUALITIES.QUALITY_GOOD)
                   : new ItemData<bool>(false, 0, QUALITIES.QUALITY_BAD);
            }
            else if (address.DBNumber == 2)
            {
                var r = mc.ReadDiscrete(GetAddress(address));
                _IsClosed = !r.IsSuccess;
                return r.IsSuccess ? new ItemData<bool>(r.Content, 0, QUALITIES.QUALITY_GOOD)
                   : new ItemData<bool>(false, 0, QUALITIES.QUALITY_BAD);
            }
            else
            {
                return new ItemData<bool>(false, 0, QUALITIES.QUALITY_BAD);
            }
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
        /// 写字节数组到设备，支持保持寄存器
        /// </summary>
        /// <param name="address">标签变量地址结构</param>
        /// <param name="bits">需写的字节数组</param>
        /// <returns></returns>
        public int WriteBytes(DeviceAddress address, byte[] bits)
        {
            if (address.DBNumber == 3)
                return mc.Write(GetAddress(address), bits).IsSuccess ? 0 : -1;
            else
                return -1;
        }
        /// <summary>
        /// 写单个位到设备,支持线圈
        /// </summary>
        /// <param name="address">标签变量地址结构</param>
        /// <param name="bit">需写的状态</param>
        /// <returns></returns>
        public int WriteBit(DeviceAddress address, bool bit)
        {
            if (address.DBNumber == 1)
                return mc.Write(GetAddress(address), bit).IsSuccess ? 0 : -1;
            else
                return -1;
        }
        /// <summary>
        /// 写位单个字节到设备,不支持写单个字节
        /// </summary>
        /// <param name="address">标签变量地址结构</param>
        /// <param name="bits">需写的字节</param>
        /// <returns></returns>
        public int WriteByte(DeviceAddress address, byte bits)
        {
            return -1;
        }
        /// <summary>
        /// 写有符号16位整数到设备,支持保持寄存器
        /// </summary>
        /// <param name="address">标签变量地址结构</param>
        /// <param name="value">需写的数据</param>
        /// <returns></returns>
        public int WriteInt16(DeviceAddress address, short value)
        {
            if (address.DBNumber == 3)
                return mc.Write(GetAddress(address), value).IsSuccess ? 0 : -1;
            else
                return -1;
        }
        /// <summary>
        /// 写无符号16位整数到设备，支持保持寄存器
        /// </summary>
        /// <param name="address">标签变量地址结构</param>
        /// <param name="value">需写的数据</param>
        /// <returns></returns>
        public int WriteUInt16(DeviceAddress address, ushort value)
        {
            if (address.DBNumber == 3)
                return mc.Write(GetAddress(address), value).IsSuccess ? 0 : -1;
            else
                return -1;
        }
        /// <summary>
        /// 写有符号32位整数到设备,支持保持寄存器
        /// </summary>
        /// <param name="address">标签变量地址结构</param>
        /// <param name="value">需写的数据</param>
        /// <returns></returns>
        public int WriteInt32(DeviceAddress address, int value)
        {
            if (address.DBNumber == 3)
                return mc.Write(GetAddress(address), value).IsSuccess ? 0 : -1;
            else
                return -1;
        }
        /// <summary>
        /// 写无符号32位整数到设备，支持保持寄存器
        /// </summary>
        /// <param name="address">标签变量地址结构</param>
        /// <param name="value">需写的数据</param>
        /// <returns></returns>
        public int WriteUInt32(DeviceAddress address, uint value)
        {
            if (address.DBNumber == 3)
                return mc.Write(GetAddress(address), value).IsSuccess ? 0 : -1;
            else
                return -1;
        }
        /// <summary>
        /// 写32位浮点数到设备,支持保持寄存器
        /// </summary>
        /// <param name="address">标签变量地址结构</param>
        /// <param name="value">需写的数据</param>
        /// <returns></returns>
        public int WriteFloat(DeviceAddress address, float value)
        {
            if (address.DBNumber == 3)
                return mc.Write(GetAddress(address), value).IsSuccess ? 0 : -1;
            else
                return -1;
        }
        /// <summary>
        /// 写字符串到设备，支持保持寄存器
        /// </summary>
        /// <param name="address">标签变量地址结构</param>
        /// <param name="value">需写的数据</param>
        /// <returns></returns>
        public int WriteString(DeviceAddress address, string str)
        {
            if (address.DBNumber == 3)
                return mc.Write(GetAddress(address), str).IsSuccess ? 0 : -1;
            else
                return -1;
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
                    string[] array = address.Split('.');
                    dv.Start = Convert.ToUInt16(array[0].Substring(2));

                    if (Regex.IsMatch(array[1], @"^[+-]?\d*$"))
                    {
                        int st;
                        int.TryParse(array[1], out st);
                        if (dv.Bit > 7)
                            dv.Bit = (byte)(st - 8);
                        else
                            dv.Bit = (byte)(st + 8);

                    }
                    else
                    {
                        switch (array[1])
                        {
                            case "A": dv.Bit = 2; break;
                            case "B": dv.Bit = 3; break;
                            case "C": dv.Bit = 4; break;
                            case "D": dv.Bit = 5; break;
                            case "E": dv.Bit = 6; break;
                            case "F": dv.Bit = 7; break;
                            default: dv.Bit = 0; break;
                        }
                    }

                }
                else
                {
                    dv.Start = int.Parse(address.Substring(2));
                }
            }
            switch (address[0])
            {
                case '0':
                    {
                        dv.DBNumber = 1;
                        int st;
                        int.TryParse(address, out st);
                        dv.Bit = (byte)(st % 16);
                        st /= 16;
                        dv.Start = st;
                    }
                    break;
                case '1':
                    {
                        dv.DBNumber = 2;
                        int st;
                        int.TryParse(address.Substring(1), out st);
                        dv.Bit = (byte)(st % 16);
                        st /= 16;
                        dv.Start = st;
                    }
                    break;
                case '4':
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
                case '3':
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
        /// <summary>
        /// 解析设备地址
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public virtual string GetAddress(DeviceAddress address)
        {
            if (address.Area == 0)
            {
                if (address.DBNumber == 1)
                    return $"{Convert.ToString(address.Start * 16 + address.Bit, 10)}";
                else if (address.DBNumber == 2)
                    return $"{Convert.ToString(address.Start * 16 + address.Bit, 10)}";
                else if (address.DBNumber == 3)
                    return $"{address.Start}";
                else if (address.DBNumber == 4)
                    return $"x=4;{address.Start}";
                else
                    return null;
            }
            else
            {
                if (address.DBNumber == 1)
                    return $"s={address.Area};{Convert.ToString(address.Start * 16 + address.Bit, 10)}";
                else if (address.DBNumber == 2)
                    return $"s={address.Area};{Convert.ToString(address.Start * 16 + address.Bit, 10)}";
                else if (address.DBNumber == 3)
                    return $"s={address.Area};{address.Start}";
                else if (address.DBNumber == 4)
                    return $"s={address.Area};x=4;{address.Start}";
                else
                    return null;
            }
        }
        public void Dispose()
        {
            mc.Close();
        }
    }
}

