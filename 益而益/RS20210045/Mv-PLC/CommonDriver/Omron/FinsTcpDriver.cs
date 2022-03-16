using DataService;
using Communication.Profinet.Omron;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Xml.Xsl;
using System.Timers;
using System.Net;

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
            FinsShortGroup grp = new FinsShortGroup(id, name, updateRate, active, this);
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

        public int WriteByte(DeviceAddress address, byte bits)
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
    public sealed class FinsShortGroup : PLCGroup
    {
        public FinsShortGroup(short id, string name, int updateRate, bool active, IPLCDriver plcReader)
        {
            this._id = id;
            this._name = name;
            this._updateRate = updateRate;
            this._isActive = active;
            this._plcReader = plcReader;
            this._server = _plcReader.Parent;
            this._timer = new Timer();
            this._changedList = new List<int>();
            this._cacheReader = new NetShortCacheReader();
        }

        protected override unsafe int Poll()
        {
            short[] cache = (short[])_cacheReader.Cache;
            int offset = 0;
            foreach (PDUArea area in _rangeList)
            {
                byte[] rcvBytes = _plcReader.ReadBytes(area.Start, (ushort)area.Len);//从PLC读取数据  
                if (rcvBytes == null || rcvBytes.Length == 0)
                {
                    offset += (area.Len + 1) / 2;
                    //_plcReader.Connect();
                    continue;
                }
                else
                {
                    int len = rcvBytes.Length / 2;
                    fixed (byte* p1 = rcvBytes)
                    {
                        short* prcv = (short*)p1;
                        int index = area.StartIndex;//index指向_items中的Tag元数据
                        int count = index + area.Count;
                        while (index < count)
                        {
                            DeviceAddress addr = _items[index].Address;
                            int iShort = addr.CacheIndex;
                            int iShort1 = iShort - offset;
                            if (addr.VarType == DataType.BOOL)
                            {
                                prcv[iShort1] = IPAddress.HostToNetworkOrder(prcv[iShort1]);
                                int tmp = prcv[iShort1] ^ cache[iShort];
                                DeviceAddress next = addr;
                                if (tmp != 0)
                                {
                                    while (addr.Start == next.Start)
                                    {
                                        if ((tmp & (1 << next.Bit)) > 0) _changedList.Add(index);
                                        if (++index < count)
                                            next = _items[index].Address;
                                        else
                                            break;
                                    }
                                }
                                else
                                {
                                    while (addr.Start == next.Start && ++index < count)
                                    {
                                        next = _items[index].Address;
                                    }
                                }
                            }
                            else
                            {
                                if (addr.DataSize <= 2)
                                {
                                    if (prcv[iShort1] != cache[iShort]) _changedList.Add(index);
                                }
                                else
                                {
                                    int size = addr.DataSize / 2;
                                    for (int i = 0; i < size; i++)
                                    {
                                        if (prcv[iShort1 + i] != cache[iShort + i])
                                        {
                                            _changedList.Add(index);
                                            break;
                                        }
                                    }
                                }
                                index++;
                            }
                        }
                        for (int j = 0; j < len; j++)
                        {
                            cache[j + offset] = prcv[j];
                        }//将PLC读取的数据写入到CacheReader中
                    }
                    offset += len;
                }
            }
            return 0;
        }
        protected override void UpdatePDUArea()
        {
            int count = _items.Count;
            if (count > 0)
            {
                DeviceAddress _start = _items[0].Address;
                _start.Bit = 0;
                int bitCount = _cacheReader.ByteCount;
                if (count > 1)
                {
                    int cacheLength = 0;//缓冲区的大小
                    int cacheIndexStart = 0;
                    int startIndex = 0;
                    DeviceAddress segmentEnd = DeviceAddress.Empty;
                    DeviceAddress tagAddress = DeviceAddress.Empty;
                    DeviceAddress segmentStart = _start;
                    for (int j = 1, i = 1; i < count; i++, j++)
                    {
                        tagAddress = _items[i].Address;//当前变量地址 
                        int offset1 = _cacheReader.GetOffset(tagAddress, segmentStart);
                        if (offset1 > (_plcReader.PDU - tagAddress.DataSize) / bitCount)
                        {
                            segmentEnd = _items[i - 1].Address;
                            int len = _cacheReader.GetOffset(segmentEnd, segmentStart);
                            len += segmentEnd.DataSize <= bitCount ? 1 : segmentEnd.DataSize / bitCount;
                            tagAddress.CacheIndex = (ushort)(cacheIndexStart + len);
                            _items[i].Address = tagAddress;
                            _rangeList.Add(new PDUArea(segmentStart, len, startIndex, j));
                            startIndex += j; j = 0;
                            cacheLength += len;//更新缓存长度
                            cacheIndexStart = cacheLength;
                            segmentStart = tagAddress;//更新数据片段的起始地址
                            segmentStart.Bit = 0;
                        }
                        else
                        {
                            tagAddress.CacheIndex = (ushort)(cacheIndexStart + offset1);
                            _items[i].Address = tagAddress;
                        }
                        if (i == count - 1)
                        {
                            segmentEnd = _items[i].Address;
                            int segmentLength = _cacheReader.GetOffset(segmentEnd, segmentStart);
                            if (segmentLength > (_plcReader.PDU - segmentEnd.DataSize) / bitCount)
                            {
                                segmentEnd = _items[i - 1].Address;
                                segmentLength = segmentEnd.DataSize <= bitCount ? 1 : segmentEnd.DataSize / bitCount;
                            }
                            tagAddress.CacheIndex = (ushort)(cacheIndexStart + segmentLength);
                            _items[i].Address = tagAddress;
                            segmentLength += segmentEnd.DataSize <= bitCount ? 1 : segmentEnd.DataSize / bitCount;
                            _rangeList.Add(new PDUArea(segmentStart, segmentLength, startIndex, j + 1));
                            cacheLength += segmentLength;
                        }
                    }
                    _cacheReader.Size = cacheLength;
                }
                else//组别内只有一个变量的情况
                {
                    _cacheReader.Size = _start.DataSize <= bitCount ? 1 : _start.DataSize / bitCount;//改变Cache的Size属性值将创建Cache的内存区域
                    int length = _start.DataSize <= bitCount ? 1 : _start.DataSize / bitCount;
                    _rangeList.Add(new PDUArea(_start, length, 0, 1));//读取的起始地址及长度
                }
            }
        }
    }
}
