using DataService;
using Communication;
using Communication.Core.Address;
using Communication.Profinet.Melsec;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Timers;

namespace CommonDriver
{
    [Description("Melsec-MC-3E协议")]
    public class MelsecMcNetDriver : DriverInitBase, IPLCDriver, IMultiReadWrite
    {
       
        protected Dictionary<int, MelsecMcDataType> _dictionary = new Dictionary<int, MelsecMcDataType>()
        {
        };
        protected MelsecMcNet mc = new MelsecMcNet();        
        public void Dispose()
        {
            mc.ConnectClose();
            //  throw new System.NotImplementedException();
        }
        public MelsecMcNetDriver()
        {

        }
        public MelsecMcNetDriver(IDataServer server, short id, string name, string serverName, int timeOut = 500, IDictionary<string, string> paras = null) : base(server, id, name, serverName, timeOut, paras)
        {
            ID = id;
            Name = name;
            Parent = server;
            _ip = serverName;
            BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Static;
            var fields = typeof(MelsecMcDataType).GetFields(bindingFlags)
                .Where(x => x.FieldType == typeof(MelsecMcDataType))
                .Where(m => !m.Name.Contains("_"))
                .Select(x => (MelsecMcDataType)x.GetValue(null))
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
        int _port = 5000; //服务端口
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
        public bool IsClosed => _IsClosed;
        public int TimeOut { get; set; }
        public IEnumerable<IGroup> Groups => _grps;
        public IDataServer Parent { get; }
        public bool Connect()
        {
            mc.Port = Port;
            mc.IpAddress = ServerName;

            _IsClosed = !mc.ConnectServer().IsSuccess;
            return mc.ConnectServer().IsSuccess;
        }

        public IGroup AddGroup(string name, short id, int updateRate, float deadBand = 0, bool active = false)
        {
            McShortGroup grp = new McShortGroup(id, name, updateRate, active, this);
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
            var a = GetAddress(address);
            var r = mc.Read(GetAddress(address), size);
            _IsClosed = !r.IsSuccess;
            return r.Content;
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
            return r.IsSuccess ? new ItemData<float>(a,0, QUALITIES.QUALITY_GOOD)
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

        public int PDU { get; } = 960;
        public virtual DeviceAddress GetDeviceAddress(string address)
        {

            OperateResult<McAddressData> operateResult = McAddressData.ParseMelsecFrom(address, 0);
            if (operateResult.IsSuccess)
            {
                if (operateResult.Content.McDataType.DataType == 1)
                {
                    var Bits = (byte)(((operateResult.Content.AddressStart) % 16));
                    return new DeviceAddress
                    {
                        Area = 0,
                        Start = (operateResult.Content.AddressStart) / 16,
                        DBNumber = operateResult.Content.McDataType.DataCode,
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
                        DBNumber = operateResult.Content.McDataType.DataCode,
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
    public sealed class McShortGroup : PLCGroup
    {
        public McShortGroup(short id, string name, int updateRate, bool active, IPLCDriver plcReader)
        {
            this._id = id;
            this._name = name;
            this._updateRate = updateRate;
            this._isActive = active;
            this._plcReader = plcReader;
            this._server = _plcReader.Parent;
            this._timer = new Timer();
            this._changedList = new List<int>();
            this._cacheReader = new ShortCacheReader();
        }

        protected override unsafe int Poll()
        {
            try
            {
                if (_plcReader.IsClosed)
                    return -1;
                short[] cache = (short[])_cacheReader.Cache;
                int k = 0; //k为cache中的索引
                foreach (PDUArea area in _rangeList)
                {

                    //    DeviceAddress
                    byte[] rcvBytes = _plcReader.ReadBytes(area.Start, (ushort)area.Len);//从PLC读取数据  
                    if (rcvBytes == null)
                    {
                        k += (area.Len + 1) / 2;
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
                                int iShort1 = iShort - k;
                                if (addr.VarType == DataType.BOOL)
                                {
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
                                    //if (addr.ByteOrder.HasFlag(ByteOrder.BigEndian))
                                    //{
                                    //    for (int i = 0; i < addr.DataSize / 2; i++)
                                    //    {
                                    //        prcv[iShort1 + i] = IPAddress.HostToNetworkOrder(prcv[iShort1 + i]);
                                    //    }
                                    //}
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
                                cache[j + k] = prcv[j];
                            }//将PLC读取的数据写入到CacheReader中
                        }
                        k += len;
                    }

                }
                return 1;
            }
            catch
            {
                return -1;
            }
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
