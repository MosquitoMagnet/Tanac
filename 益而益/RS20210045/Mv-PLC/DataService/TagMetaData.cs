using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace DataService
{
    [StructLayout(LayoutKind.Sequential)]
    public class TagMetaData : IComparable<TagMetaData>
    {
        [Browsable(false)]
        public short ID { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public DataType DataType { get; set; }
        public ushort Size { get; set; }
        public string Description { get; set; }
        public int Cycle { get; set; } = 10;
        public bool Archive { get; set; } = true;
        public float Minimum { get; set; }
        public float Maximum { get; set; }
        public ZoomType Zoom { get; set; }
        public string Code { get; set; }
        [Browsable(false)]
        public short GroupID { get; set; }

        public TagMetaData(short id, short grpId, string name, string address, ZoomType zoom, string code,
            DataType type, ushort size, bool archive = false, float max = 0, float min = 0, int cycle = 0)
        {
            ID = id;
            GroupID = grpId;
            Name = name;
            Address = address;
            DataType = type;
            Size = size;
            Archive = archive;
            Maximum = max;
            Minimum = min;
            Zoom = zoom;
            Code = code;
            Cycle = cycle;
        }
        public TagMetaData()
        {

        }

        public int CompareTo(TagMetaData other)
        {
            return this.ID.CompareTo(other.ID);
        }

        public override string ToString()
        {
            return Name;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Scaling : IComparable<Scaling>
    {
        public short ID;

        public ScaleType ScaleType;

        public float EUHi;

        public float EULo;

        public float RawHi;

        public float RawLo;

        public Scaling(short id, ScaleType type, float euHi, float euLo, float rawHi, float rawLo)
        {
            ID = id;
            ScaleType = type;
            EUHi = euHi;
            EULo = euLo;
            RawHi = rawHi;
            RawLo = rawLo;
        }

        public int CompareTo(Scaling other)
        {
            return ID.CompareTo(other.ID);
        }

        public static readonly Scaling Empty = new Scaling { ScaleType = ScaleType.None };
    }

    public struct ItemData<T>
    {
        public T Value;
        public long TimeStamp;
        public QUALITIES Quality;

        public ItemData(T value, long timeStamp, QUALITIES quality)
        {
            Value = value;
            TimeStamp = timeStamp;
            Quality = quality;
        }
    }

    public enum ScaleType : byte
    {
        None = 0,
        Linear = 1,
        SquareRoot = 2
    }
}
