using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace DataService
{
    public interface IReaderWriter
    {
        byte[] ReadBytes(DeviceAddress address, ushort size);
        ItemData<int> ReadInt32(DeviceAddress address);
        ItemData<short> ReadInt16(DeviceAddress address);
        ItemData<byte> ReadByte(DeviceAddress address);
        ItemData<string> ReadString(DeviceAddress address, ushort size);
        ItemData<float> ReadFloat(DeviceAddress address);
        ItemData<bool> ReadBit(DeviceAddress address);
        ItemData<object> ReadValue(DeviceAddress address);

        int WriteBytes(DeviceAddress address, byte[] bit);
        int WriteBit(DeviceAddress address, bool bit);
        int WriteByte(DeviceAddress address, byte bits);
        int WriteInt16(DeviceAddress address, short value);
        int WriteInt32(DeviceAddress address, int value);
        int WriteFloat(DeviceAddress address, float value);
        int WriteString(DeviceAddress address, string str);
        int WriteValue(DeviceAddress address, object value);
    }


    public interface ICache : IReaderWriter
    {
        int Size { get; set; }
        int ByteCount { get; }
        Array Cache { get; }
        int GetOffset(DeviceAddress start, DeviceAddress end);
    }

    public interface IMultiReadWrite
    {
        int Limit { get; }
        ItemData<Storage>[] ReadMultiple(DeviceAddress[] addrsArr);
        int WriteMultiple(DeviceAddress[] addrArr, object[] buffer);
    }

    public interface IDriver : IDisposable
    {
        short ID { get; }
        string Name { get; }
        string ServerName { get; set; }//可以考虑增加一个附加参数，Sever只定义本机名
        bool IsClosed { get; }
        int TimeOut { get; set; }
        IEnumerable<IGroup> Groups { get;  }
        IDataServer Parent { get;  }
        bool Connect();
        IGroup AddGroup(string name, short id, int updateRate, float deadBand = 0f, bool active = false);
        bool RemoveGroup(IGroup group);
        event ShutdownRequestEventHandler OnClose;
        event EventHandler<Exception> OnError;
    }

    public interface IPLCDriver : IDriver, IReaderWriter
    {
        int PDU { get; }
        DeviceAddress GetDeviceAddress(string address);
        string GetAddress(DeviceAddress address);
    }


    public abstract class DriverInitBase
    {
        public DriverInitBase()
        {

        }
        public DriverInitBase(IDataServer server, short id, string name, string serverName, int timeOut = 500, IDictionary<string, string> paras = null)
        {
            if (paras != null)
            {
                var properties = GetType().GetProperties().Where(x => x.CanWrite).Where(x => paras.Keys.Contains(x.Name));
                foreach (var para in paras)
                {
                    try
                    {
                        var prop = properties.FirstOrDefault(x => x.Name == para.Key);
                        if (prop != null)
                        {
                            if (prop.PropertyType.IsEnum)
                                prop.SetValue(this, Enum.Parse(prop.PropertyType, para.Value), null);
                            else
                                prop.SetValue(this, Convert.ChangeType(para.Value, prop.PropertyType, CultureInfo.CreateSpecificCulture("en-US")), null);
                        }
                    }
                    catch (Exception ex) 
                    {
                       
                      //  throw;
                    }
                }
            }
        }
    }

    public interface IFileDriver : IDriver, IReaderWriter
    {
        string FileName { get; set; }
        FileData[] ReadAll(short groupId);
        //bool RecieveData(string data);
    }

    public class ShutdownRequestEventArgs : EventArgs
    {
        public ShutdownRequestEventArgs(string reson)
        {
            shutdownReason = reson;
        }
        public string shutdownReason;
    }

    public delegate void ShutdownRequestEventHandler(object sender, ShutdownRequestEventArgs e);
}
