using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.ComponentModel;
using System.IO;
using System.Collections;
using DataService;
using Communication.ModBus;
using Communication;
using Communication.Core.Address;
using Communication.Core.Net;
using System.Windows.Forms;


namespace CommonDriver.Modbus
{
    [Description("ModbusTcpServer协议")]
    public class ModbusTcpServerDriver : DriverInitBase, IPLCDriver, IMultiReadWrite
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
        /// <summary>
        /// RTU是否开启
        /// </summary>
        /// <returns></returns>
        public bool RtuEnable { get; set; } = false;

        /// <summary>
        /// RTU的站点号
        /// </summary>
        /// <returns></returns>
        public int RtuStation { get; set; } = 1;
        /// <summary>
        /// RTU的串口号
        /// </summary>
        /// <returns></returns>
        public string SerialPort { get; set; } = "COM10";

        /// <summary>
        /// RTU的串口波特率
        /// </summary>
        /// <returns></returns>
        public int BaudRate { get; set; } = 9600;

        List<IGroup> _grps = new List<IGroup>(20);
        public short ID
        {
            get;
        }
        public string Name { get; }
        private bool _IsClosed = true;
        protected ModbusTcpServer mc = new ModbusTcpServer();
        public ModbusTcpServerDriver(IDataServer server, short id, string name, string serverName, int timeOut = 500, IDictionary<string, string> paras = null) : base(server, id, name, serverName, timeOut, paras)
        {
            ID = id;
            Name = name;
            Parent = server;
            _ip = serverName;

        }

        public ModbusTcpServerDriver() : base(null, 0, "ModbusTcpServer", "127.0.0.1", 100, null)
        {
            ID = 0;
            Name = "ModbusTcpServer";
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
            mc.Station = RtuStation;
            mc.ServerStart();
            if(RtuEnable)
            {
                try
                {
                    mc.StartModbusRtu(SerialPort, BaudRate);
                }
                catch(Exception ex)
                {
                    MessageBox.Show($"ModbusTcpServerDriver_{Name}:" + ex.Message);
                }
            }
            _IsClosed = !mc.IsStarted;
            return mc.IsStarted;
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
        /// 读取字节数组
        /// </summary>
        /// <param name="address">标签变量地址结构</param>
        /// <param name="size">长度,</param>
        /// <returns></returns>
        public byte[] ReadBytes(DeviceAddress address, ushort size)
        {
            if (address.DBNumber == 1)
            {
                var addr = GetAddress(address);
                var r =mc.ReadCoil(addr, (ushort)(size * 16));
                byte[] data = new byte[2 * size];
                BitArray array = new BitArray(r);
                array.CopyTo(data, 0);
                return data;

            }
            else if(address.DBNumber == 2)
            {
                var addr = GetAddress(address);
                var r = mc.ReadDiscrete(addr, (ushort)(size * 16));
                byte[] data = new byte[2 * size];
                BitArray array = new BitArray(r);
                array.CopyTo(data, 0);
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
            var r = mc.ReadCoil(GetAddress(address));
            return mc.IsStarted ? new ItemData<bool>(r, 0, QUALITIES.QUALITY_GOOD)
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
            try
            {
                if (address.DBNumber == 1)
                {
                    mc.WriteCoil(GetAddress(address), bit);
                }
                else if(address.DBNumber ==2)
                {
                    var a = GetAddress(address);
                    mc.WriteDiscrete(GetAddress(address), bit);
                }
                return 0;
            }
            catch(Exception ex)
            {
                return -1;
            }
        }
        /// <summary>
        /// 写位单个字节到设备
        /// </summary>
        /// <param name="address">标签变量地址结构</param>
        /// <param name="bits">需写的字节</param>
        /// <returns></returns>
        public int WriteByte(DeviceAddress address, byte bits)
        {
            if (address.DBNumber == 1)
                return mc.Write(GetAddress(address), bits).IsSuccess ? 0 : -1;
            else
                return -1;
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
        public virtual string GetAddress(DeviceAddress address)
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
        public void Dispose()
        {
            try
            {
                mc.CloseModbusRtu();
                mc.ServerClose();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"ModbusTcpServerDriver_{Name}:" + ex.Message);
            }
        }
    }
}
