using DataService;
using Communication;
using Communication.Profinet.Delta;
using Communication.BasicFramework;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;

namespace CommonDriver.Delta
{
    [Description("Delta-Dvp-Ascii协议")]
    public class DeltaDvpAsciiDriver : DriverInitBase, IPLCDriver, IMultiReadWrite
    {

        protected Dictionary<int, DeltaDataType> _dictionary = new Dictionary<int, DeltaDataType>()
        {
        };
        protected DeltaDvpSerialAscii mc = new DeltaDvpSerialAscii();
        public void Dispose()
        {
            mc.Close();
            //  throw new System.NotImplementedException();
        }
        public DeltaDvpAsciiDriver()
        {

        }
        public DeltaDvpAsciiDriver(IDataServer server, short id, string name, string serverName, int timeOut = 500, IDictionary<string, string> paras = null) : base(server, id, name, serverName, timeOut, paras)
        {
            ID = id;
            Name = name;
            Parent = server;
            _ip = serverName;
            BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Static;
            var fields = typeof(DeltaDataType).GetFields(bindingFlags)
                .Where(x => x.FieldType == typeof(DeltaDataType))
                .Where(m => m.Name.Contains("Dvp"))
                .Select(x => (DeltaDataType)x.GetValue(null))
                .ToDictionary(x => (int)x.DataCode);
            if (fields != null)
                _dictionary = fields;

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

        string _ip;//服务ip
        public string ServerName
        {
            get { return _ip; }
            set { _ip = value; }
        }
        List<IGroup> _grps = new List<IGroup>(20);
        public short ID { get; }
        public string Name { get; }
        private bool _IsClosed = true;
        public bool IsClosed => _IsClosed;
        public int TimeOut { get; set; }

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

        private RJCP.IO.Ports.Parity _parity = RJCP.IO.Ports.Parity.Even;//奇偶性
        public RJCP.IO.Ports.Parity Parity
        {
            get { return _parity; }
            set { _parity = value; }
        }

        private SerialPortDataBits _databits = SerialPortDataBits.Seven;//停止位
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
        public IEnumerable<IGroup> Groups => _grps;
        public IDataServer Parent { get; }
        public bool Connect()
        {
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
            DeltaShortGroup grp = new DeltaShortGroup(id, name, updateRate, active, this);
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

        public byte[] ReadBytes(DeviceAddress address, ushort size)
        {
            if (address.DBNumber >= 0x0A)
            {
                var a = GetAddress(address);
                var r = mc.Read(GetAddress(address), size);
                _IsClosed = !r.IsSuccess;
                return r.Content;
            }
            else
            {
                address.Bit = 0;
                var a = GetAddress(address);
                var s = mc.ReadBool(GetAddress(address), (ushort)(size * 16));
                _IsClosed = !s.IsSuccess;
                var r = SoftBasic.BoolArrayToByte(s.Content);
                return r;
            }
        }
        public ItemData<uint> ReadUInt32(DeviceAddress address)
        {
            var r = mc.ReadUInt32(GetAddress(address));
            _IsClosed = !r.IsSuccess;
            return r.IsSuccess ? new ItemData<uint>(r.Content, 0, QUALITIES.QUALITY_GOOD)
                : new ItemData<uint>(0, 0, QUALITIES.QUALITY_BAD);
        }

        public ItemData<int> ReadInt32(DeviceAddress address)
        {
            var r = mc.ReadInt32(GetAddress(address));
            _IsClosed = !r.IsSuccess;
            return r.IsSuccess ? new ItemData<int>(r.Content, 0, QUALITIES.QUALITY_GOOD)
                : new ItemData<int>(0, 0, QUALITIES.QUALITY_BAD);
        }

        public ItemData<ushort> ReadUInt16(DeviceAddress address)
        {
            var r = mc.ReadUInt16(GetAddress(address));
            _IsClosed = !r.IsSuccess;
            return r.IsSuccess ? new ItemData<ushort>(r.Content, 0, QUALITIES.QUALITY_GOOD)
                : new ItemData<ushort>(0, 0, QUALITIES.QUALITY_BAD);
        }

        public ItemData<short> ReadInt16(DeviceAddress address)
        {
            var r = mc.ReadInt16(GetAddress(address));
            _IsClosed = !r.IsSuccess;
            return r.IsSuccess ? new ItemData<short>(r.Content, 0, QUALITIES.QUALITY_GOOD)
                : new ItemData<short>(0, 0, QUALITIES.QUALITY_BAD);
        }

        public ItemData<byte> ReadByte(DeviceAddress address)
        {
            var r = mc.Read(GetAddress(address), 1);
            _IsClosed = !r.IsSuccess;
            return !r.IsSuccess ? new ItemData<byte>(0, 0, QUALITIES.QUALITY_BAD)
                : new ItemData<byte>(r.Content[0], 0, QUALITIES.QUALITY_GOOD);
        }

        public ItemData<string> ReadString(DeviceAddress address, ushort size)
        {
            var r = mc.ReadString(GetAddress(address), size);
            return r.IsSuccess ? new ItemData<string>(r.Content, 0, QUALITIES.QUALITY_GOOD)
                : new ItemData<string>("", 0, QUALITIES.QUALITY_BAD);

        }

        public ItemData<float> ReadFloat(DeviceAddress address)
        {
            var r = mc.ReadFloat(GetAddress(address));
            _IsClosed = !r.IsSuccess;
            float a = r.Content;
            return r.IsSuccess ? new ItemData<float>(a, 0, QUALITIES.QUALITY_GOOD)
                : new ItemData<float>(0, 0, QUALITIES.QUALITY_BAD);
        }

        public ItemData<bool> ReadBit(DeviceAddress address)
        {
            var r = mc.ReadBool(GetAddress(address));
            _IsClosed = !r.IsSuccess;
            return r.IsSuccess ? new ItemData<bool>(r.Content, 0, QUALITIES.QUALITY_GOOD)
                : new ItemData<bool>(false, 0, QUALITIES.QUALITY_BAD);
        }

        public ItemData<object> ReadValue(DeviceAddress address)
        {
            return this.ReadValueEx(address);
        }

        public int WriteBytes(DeviceAddress address, byte[] bit)
        {
            return mc.Write(GetAddress(address), bit).IsSuccess ? 0 : -1;
        }

        public int WriteBit(DeviceAddress address, bool bit)
        {
            return mc.Write(GetAddress(address), bit).IsSuccess ? 0 : -1;
        }

        public int WriteBits(DeviceAddress address, byte bits)
        {
            var r = mc.ReadInt16(GetAddress(address));
            _IsClosed = !r.IsSuccess;
            if (r.IsSuccess)
            {
                var m = r.Content & 0xff00 | bits;
                return mc.Write(GetAddress(address), m).IsSuccess ? 0 : -1;
            }

            return -1;
        }

        public int WriteInt16(DeviceAddress address, short value)
        {
            return mc.Write(GetAddress(address), value).IsSuccess ? 0 : -1;
        }

        public int WriteUInt16(DeviceAddress address, ushort value)
        {
            return mc.Write(GetAddress(address), value).IsSuccess ? 0 : -1;
        }

        public int WriteInt32(DeviceAddress address, int value)
        {
            return mc.Write(GetAddress(address), value).IsSuccess ? 0 : -1;
        }

        public int WriteUInt32(DeviceAddress address, uint value)
        {
            return mc.Write(GetAddress(address), value).IsSuccess ? 0 : -1;
        }

        public int WriteFloat(DeviceAddress address, float value)
        {
            return mc.Write(GetAddress(address), value).IsSuccess ? 0 : -1;
        }

        public int WriteString(DeviceAddress address, string str)
        {
            return mc.Write(GetAddress(address), str).IsSuccess ? 0 : -1;
        }

        public int WriteValue(DeviceAddress address, object value)
        {
            return this.WriteValueEx(address, value);
        }

        public int PDU { get; } = 960;
        public virtual DeviceAddress GetDeviceAddress(string address)
        {

            OperateResult<DeltaAddressData> operateResult = DeltaAddressData.ParseDvpFrom(address, 0);
            if (operateResult.IsSuccess)
            {
                if (operateResult.Content.DeltaDataType.DataType == 1)
                {
                    return new DeviceAddress
                    {
                        Area = 0,
                        Start = (operateResult.Content.AddressStart) / 16,
                        DBNumber = operateResult.Content.DeltaDataType.DataCode,
                        DataSize = 0,
                        CacheIndex = 0,
                        Bit = (byte)(((operateResult.Content.AddressStart) % 16))
                    };
                }
                else
                {
                    return new DeviceAddress
                    {
                        Area = 0,
                        Start = operateResult.Content.AddressStart,
                        DBNumber = operateResult.Content.DeltaDataType.DataCode,
                        DataSize = 0,
                        CacheIndex = 0,
                        Bit = 0
                    };
                }
            }
            else
            {
                return DeviceAddress.Empty;
            }
        }

        public string GetAddress(DeviceAddress address)
        {

            if (!_dictionary.ContainsKey(address.DBNumber))
                return null;
            var m = _dictionary[address.DBNumber];
            if (m != null)
            {
                if (m.DataType == 0)
                    return m.AsciiCode.Trim('*') + Convert.ToString(address.Start, m.FromBase);
                else
                    return $"{m.AsciiCode.Trim('*')}{Convert.ToString(address.Start * 16 + address.Bit, m.FromBase)}";
            }
            else
            {
                return null;
            }
        }
        public int Limit { get; } = 960;
        public ItemData<Storage>[] ReadMultiple(DeviceAddress[] addrsArr)
        {
            return this.PLCReadMultiple(new ShortCacheReader(), addrsArr);
        }
        public int WriteMultiple(DeviceAddress[] addrArr, object[] buffer)
        {
            return this.PLCWriteMultiple(new ShortCacheReader(), addrArr, buffer, Limit);
        }
    }
}
