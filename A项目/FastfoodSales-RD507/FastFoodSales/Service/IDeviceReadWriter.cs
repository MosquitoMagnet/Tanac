using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAQ.Service
{
    public interface IDeviceReadWriter
    {
        ModbusDeviceVM ModbusDeviceVm { get; set; }
        bool GetBit(int index, int bit);
        bool GetSetBit(int index, int bit);
        short GetShort(int index);
        float GetFloat(int index);
        string GetString(int index, int len);
        byte[] GetBytes(int index, int len);
        ushort GetWord(int index);
        void PlcConnect();
        void SetBit(int index, int bit, bool value);
        void SetShort(int index, short value);
        void SetString(int index, string value);
        int GetInt(int index);
        void SetInt(int index, int value);
        void SetBytes(int index, byte[] value);
        void SetBytesEx(int index, byte[] value);
    }
}
