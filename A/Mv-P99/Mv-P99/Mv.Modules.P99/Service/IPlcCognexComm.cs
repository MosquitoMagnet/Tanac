namespace Mv.Modules.P99.Service
{
    public interface IPlcCognexComm
    {
        bool IsConnected { get; set; }

        bool GetBit(int id, int index, int bit);
        int GetInt(int id, int index);
        bool GetSetBit(int id, int index, int bit);
        short GetShort(int id, int index);
        string GetString(int id, int index, int len);
        uint GetUInt(int id, int index);
        ushort GetWord(int id, int index);
        void PlcConnect();
        void SetBit(int id, int index, int bit, bool value);
        void SetInt(int id, int index, int value);
        void SetInt(int id, int index, uint value);
        void SetShort(int id, int index, short value);
        void SetString(int id, int index, string value);
    }
}