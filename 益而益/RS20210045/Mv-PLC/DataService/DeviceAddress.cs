using System;
using System.Runtime.InteropServices;

namespace DataService
{
    [StructLayout(LayoutKind.Sequential)]
    public struct DeviceAddress : IComparable<DeviceAddress>
    {
        /// <summary>
        /// PLC的站点
        /// </summary>
        public int Area;
        /// <summary>
        /// 寄存器的起始地址
        /// </summary>
        public int Start;
        /// <summary>
        /// 寄存器的种类
        /// </summary>
        public ushort DBNumber;
        /// <summary>
        /// 寄存器的大小
        /// </summary>
        public ushort DataSize;
        /// <summary>
        /// 缓存索引
        /// </summary>
        public ushort CacheIndex;
        /// <summary>
        /// 寄存器的比特位
        /// </summary>
        public byte Bit;
        /// <summary>
        /// 寄存器的数据类型
        /// </summary>
        public DataType VarType;

        public DeviceAddress(int area, ushort dbnumber, ushort cIndex, int start, ushort size, byte bit, DataType type)
        {
            Area = area;
            DBNumber = dbnumber;
            CacheIndex = cIndex;
            Start = start;
            DataSize = size;
            Bit = bit;
            VarType = type;
        }

        public static readonly DeviceAddress Empty = new DeviceAddress(0, 0, 0, 0, 0, 0, DataType.NONE);

        public int CompareTo(DeviceAddress other)
        {
            return this.Area > other.Area ? 1 :
                this.Area < other.Area ? -1 :
                this.DBNumber > other.DBNumber ? 1 :
                this.DBNumber < other.DBNumber ? -1 :
                this.Start > other.Start ? 1 :
                this.Start < other.Start ? -1 :
                this.Bit > other.Bit ? 1 :
                this.Bit < other.Bit ? -1 : 0;
        }
    }

}
