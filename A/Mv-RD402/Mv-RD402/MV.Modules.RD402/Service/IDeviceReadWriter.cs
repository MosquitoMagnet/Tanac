namespace Mv.Modules.RD402.Service
{
    public interface IDeviceReadWriter
    {
        bool IsConnected { get; set; }

        bool GetBit(int index, int bit);
        bool GetSetBit(int index, int bit);
        short GetShort(int index);
        string GetString(int index, int len);
        ushort GetWord(int index);
        void PlcConnect();
        void SetBit(int index, int bit, bool value);
        void SetShort(int index, short value);
        void SetString(int index, string value);
    }
}