using DataService;
using Communication.Profinet.Omron;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Xml.Xsl;

namespace CommonDriver
{
    [Description("FINS TCP协议")]
    public class FinsTcpDriver : DriverInitBase, IPLCDriver, IMultiReadWrite
    {
        string _ip;//服务ip
        int _port = 9600; //服务端口
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
        public short ID { get; }
        public string Name { get; }
        private bool _IsClosed = true;
        protected OmronFinsNet mc = new OmronFinsNet();
        public FinsTcpDriver(IDataServer server, short id, string name, string serverName, int timeOut = 500, IDictionary<string, string> paras = null) : base(server, id, name, serverName, timeOut, paras)
        {
            ID = id;
            Name = name;
            Parent = server;
            _ip = serverName;
       
        }

        public FinsTcpDriver() : base(null, 0, "fins", "127.0.0.1", 100, null)
        {
            ID = 0;
            Name = "Fins";
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
        public bool Connect()
        {
            mc.Port = Port;
            mc.IpAddress = ServerName??"127.0.0.1";
            mc.SA1 = 222;
            mc.DA1 = 1;
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

        public byte[] ReadBytes(DeviceAddress address, ushort size)
        {
            var addr = GetAddress(address);
        //    addr = addr.Substring(0, addr.IndexOf('.'));
            var r = mc.Read(addr, size);
            _IsClosed = !r.IsSuccess;
            return r.Content;
        }
        public ItemData<uint> ReadUInt32(DeviceAddress address)
        {
            var addr = GetAddress(address);
            addr = addr.Substring(0, addr.IndexOf('.'));
            var r = mc.ReadUInt32(addr);
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
            return r.IsSuccess ? new ItemData<float>(r.Content, 0, QUALITIES.QUALITY_GOOD)
                : new ItemData<float>(0, 0, QUALITIES.QUALITY_BAD);
        }

        public ItemData<bool> ReadBit(DeviceAddress address)
        {
            var r = mc.ReadBool($"{GetAddress(address)}.{address.Bit}");
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
            return mc.Write($"{GetAddress(address)}.{address.Bit}", bit).IsSuccess ? 0 : -1;
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

        public DeviceAddress GetDeviceAddress(string address)
        {
            Communication.OperateResult<OmronFinsDataType, byte[]> result;
            if (address.Contains('.'))
            {
                if (char.IsDigit(address[0]))
                {
                    address = "C" + address;
                }
                result = OmronFinsNetHelper.AnalysisAddress(address, true);
            }
            else
            {
                result = OmronFinsNetHelper.AnalysisAddress(address, false);
            }          
            if(result.IsSuccess)
            {
                if(address.Contains('.'))
                {
                    return new DeviceAddress
                    {
                        Area = 0,
                        Start =int.Parse(address.Substring(1,address.IndexOf('.')-1)),
                        DBNumber =result.Content1.WordCode,
                        DataSize = 0,
                        CacheIndex = 0,
                        Bit = (byte)(byte.Parse(address.Substring(address.IndexOf('.')+1)))
                    };
                }
                else
                {
                    return new DeviceAddress
                    {
                        Area = 0,
                        Start = int.Parse(address.Substring(1)),
                        DBNumber = result.Content1.WordCode,
                        DataSize = 0,
                        CacheIndex = 0,
                        Bit = 0
                    };
                }
            }
            else
            { }
            return DeviceAddress.Empty;
        }

        public string GetAddress(DeviceAddress address)
        {
            if(address.DBNumber==OmronFinsDataType.CIO.WordCode)
            {
                return $"C{address.Start}";
            }
            if (address.DBNumber == OmronFinsDataType.AR.WordCode)
            {
                return $"A{address.Start}";
            }
            if (address.DBNumber == OmronFinsDataType.DM.WordCode)
            {
                return $"D{address.Start}";
            }
            if (address.DBNumber == OmronFinsDataType.HR.WordCode)
            {
                return $"H{address.Start}";
            }
            if (address.DBNumber == OmronFinsDataType.WR.WordCode)
            {
                return $"W{address.Start}";
            }
            return String.Empty;
        }

        public void Dispose()
        {
            mc.ConnectClose();
        }
    }
}
