using DataService;
using System.Collections.Generic;
using System.Net;
using System.Timers;

namespace CommonDriver
{
    public sealed class NetShortGroup : PLCGroup
    {
        public NetShortGroup(short id, string name, int updateRate, bool active, IPLCDriver plcReader)
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
    }
    public sealed class ShortGroup : PLCGroup
    {
        public ShortGroup(short id, string name, int updateRate, bool active, IPLCDriver plcReader)
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
         

    }
    public sealed class NetByteGroup : PLCGroup
    {
        public NetByteGroup(short id, string name, int updateRate, bool active, IPLCDriver plcReader)
        {
            this._id = id;
            this._name = name;
            this._updateRate = updateRate;
            this._isActive = active;
            this._plcReader = plcReader;
            this._server = _plcReader.Parent;
            this._timer = new Timer();
            this._changedList = new List<int>();
            this._cacheReader = new NetByteCacheReader();
        }      
        protected override unsafe int Poll()
        {
            if (_plcReader.IsClosed)
                return -1;
            byte[] cache = (byte[])_cacheReader.Cache;
            int offset = 0;
            foreach (PDUArea area in _rangeList)
            {
                byte[] rcvBytes = _plcReader.ReadBytes(area.Start, (ushort)area.Len);//从PLC读取数据  
                if (rcvBytes == null || rcvBytes.Length == 0)
                {
                    //_plcReader.Connect();  
                    continue;
                }
                else
                {
                    int index = area.StartIndex;//index指向_items中的Tag元数据
                    int count = index + area.Count;
                    while (index < count)
                        {
                            DeviceAddress addr = _items[index].Address;
                            int iByte = addr.CacheIndex;
                            int iByte1 = iByte - offset;
                            if (addr.VarType == DataType.BOOL)
                            {
                            int tmp = rcvBytes[iByte1] ^ cache[iByte];
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
                            ushort size = addr.DataSize;
                            for (int i = 0; i < size; i++)
                            {
                                if (iByte1 + i < rcvBytes.Length && rcvBytes[iByte1 + i] != cache[iByte + i])
                                {
                                    _changedList.Add(index);
                                    break;
                                }
                            }
                            index++;
                        }
                    }
                    for (int j = 0; j < rcvBytes.Length; j++)
                        cache[j + offset] = rcvBytes[j];//将PLC读取的数据写入到CacheReader中
                }
                offset += rcvBytes.Length;
            }
            return 0;
        }
          
    }
}

