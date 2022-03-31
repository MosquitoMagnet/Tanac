using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HslCommunication;
using HslCommunication.Core;
using HslCommunication.Profinet.Omron;
using HslCommunication.Profinet.Siemens;
using HslCommunication.Profinet.Melsec;
using System.IO;
using System.Text.RegularExpressions;
using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration.Attributes;
using Stylet;
using StyletIoC;

namespace Script.Methods
{
    public class KV<T> : PropertyChangedBase
    {
    public int Index { get; set; }
    public DateTime Time { get; set; }
    public string Key { get; set; }
    public string Address { get; set; }
    public int BitIndex { get; set; }
    public int Size { get; set; }
    public string Description { get; set; }
    public T Value { get; set; }
    }
    public class PlcService : PropertyChangedBase,IDisposable
    {

        public ushort ReadAddrStart=>ScriptSettings.Default.PLC_RD;
        public ushort ReadLens { get; set; } = ScriptSettings.Default.PLC_RLen;
        public ushort WriteAddrStart => ScriptSettings.Default.PLC_WD;
        public ushort WriteLens { get; set; } = ScriptSettings.Default.PLC_WLen;

        byte[] _rbs;
        byte[] _wbs;

        private MelsecMcNet _rw;
        CancellationTokenSource _cts;
        public bool IsConnected { get; set; }


        public BindableCollection<KV<bool>> KVBits { get; set; } = new BindableCollection<KV<bool>>();
        public BindableCollection<KV<short>> KvShorts { get; } = new BindableCollection<KV<short>>();
        public BindableCollection<KV<int>> KvInts { get; } = new BindableCollection<KV<int>>();
        public BindableCollection<KV<float>> KvFloats { get; } = new BindableCollection<KV<float>>();
        public BindableCollection<KV<string>> KvStrings { get; } = new BindableCollection<KV<string>>();


        public BindableCollection<KV<bool>> KvBitsW { get; set; } = new BindableCollection<KV<bool>>();
        public BindableCollection<KV<short>> KvShortsW { get; } = new BindableCollection<KV<short>>();
        public BindableCollection<KV<int>> KvIntsW { get; } = new BindableCollection<KV<int>>();
        public BindableCollection<KV<float>> KvFloatsW { get; } = new BindableCollection<KV<float>>();
        public BindableCollection<KV<string>> KvStringsW { get; } = new BindableCollection<KV<string>>();


        public IEventAggregator Events { get; set; }
        public PlcService(IEventAggregator eventAggregator)
        {
            LoadRegTagInfos();
            LoadWegTagInfos();
            this.Events = eventAggregator;
        }
        public void Connect()
        {
            _rw = new MelsecMcNet();
            _rw.IpAddress = ScriptSettings.Default.PLC_IP;
            _rw.Port= ScriptSettings.Default.PLC_PORT;
            if (_rbs == null || _rbs.Length != ReadLens * 2)
                _rbs = new byte[ReadLens * 2];
            if (_wbs == null || _wbs.Length != WriteLens * 2)
                _wbs = new byte[WriteLens * 2];
            _cts?.Cancel();
            _cts = new CancellationTokenSource();
            try
            {
                Task.Factory.StartNew(() =>
                {
                    OperateResult con = _rw.ConnectServer();
                    if(!con.IsSuccess)
                        Events.Publish(new MsgItem() { Level = "E", Time = DateTime.Now, Value = "PLC:" + con.Message });
                    if (_cts.IsCancellationRequested)
                        return;
                    while (!_cts.IsCancellationRequested)
                    {
                        var rs = _rw.Read("D" + ReadAddrStart, ReadLens);
                        IsConnected = rs.IsSuccess;
                        if (rs.IsSuccess)
                        {
                            Buffer.BlockCopy(rs.Content, 0, _rbs, 0, _rbs.Length);
                            foreach (var x in KVBits)
                            {
                                x.Value = GetBit(x.Index, x.BitIndex);
                            }
                            foreach (var x in KvShorts)
                            {
                                x.Value = GetShort(x.Index);
                            }
                            foreach (var x in KvInts)
                            {
                                x.Value = GetInt(x.Index);
                            }
                            foreach (var x in KvFloats)
                            {
                                x.Value = GetFloat(x.Index);
                            }
                            foreach (var x in KvStrings)
                            {
                                x.Value = GetString(x.Index, x.Size);
                            }

                        }
                        var ws = _rw.Write("D" + WriteAddrStart, _wbs);
                        Thread.Sleep(1);
                    }
                }, _cts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
            }
            catch(AggregateException exs)
            {
                foreach(var e in exs.InnerExceptions)
                {
                    Events.Publish(new MsgItem() { Level = "E", Time = DateTime.Now, Value = "PLC:" + e.Message });
                }
            }
        }
        private bool WriteBit(KV<bool> kV,bool value)
        {
            var opr = _rw.ReadUInt16(kV.Address);
            if (opr.IsSuccess)
            {
                ushort m;
                if (value)
                {
                    m = (UInt16)(opr.Content | ((ushort)(1 << kV.BitIndex)));
                }
                else
                {
                    m = (UInt16)(opr.Content & (~(1 << kV.BitIndex)));
                }
                return _rw.Write(kV.Address, m).IsSuccess;
            }
            return false;
        }
        private void PulseBit(KV<bool> kV, int Delayms = 100)
        {
            Task.Factory.StartNew(() =>
            {
                WriteBit(kV, true);
                Thread.Sleep(Delayms);
                WriteBit(kV, false);
            });
        }
        private bool WriteShort(KV<short> kV, short value)
        {
           var a=_rw.Write(kV.Address, value).IsSuccess;
            return a;
        }
        private bool WriteInt(KV<int> kV, int value)
        {
            return _rw.Write(kV.Address, value).IsSuccess;
        }
        private bool WriteFloat(KV<float> kV, float value)
        {
            return _rw.Write(kV.Address, value).IsSuccess;
        }
        private bool WriteString(KV<string> kV, string value)
        {
            if (value.Length > kV.Size)
                value = value.Substring(0, kV.Size);
            return _rw.Write(kV.Address, value).IsSuccess;
        }

        private ushort GetWord(int index)
        {
            return BitConverter.ToUInt16(_rbs, index*2);
        }
        private bool GetBit(int index, int bit)
        {
            var m = GetWord(index);
            var r = (m & (1 << bit)) > 0;
            return r;
        }
        private short GetShort(int index)
        {
            return BitConverter.ToInt16(_rbs, index*2);
        }
        private int GetInt(int index)
        {
            return BitConverter.ToInt32(_rbs, index*2);
        }
        private float GetFloat(int index)
        {
            return BitConverter.ToSingle(_rbs, index*2);
        }
        private string GetString(int index, int len)
        {
            return Encoding.ASCII.GetString(_rbs, index*2, len).Trim('\0');
        }


        private bool SetShort(int index, short value)
        {
            try
            {
                Buffer.BlockCopy(BitConverter.GetBytes(value), 0, _wbs, index * 2, 2);
                return true;
            }
            catch
            {
                return false;
            }

        }
        private bool SetInt(int index, int value)
        {
            try
            {
                Buffer.BlockCopy(BitConverter.GetBytes(value), 0, _wbs, index * 2, 4);
                return true;
            }
            catch
            {
                return false;
            }

        }
        private bool SetFloat(int index, float value)
        {
            try
            {
                Buffer.BlockCopy(BitConverter.GetBytes(value), 0, _wbs, index * 2, 4);
                return true;
            }
            catch
            {
                return false;
            }

        }
        private bool SetBit(int index, int bit, bool value)
        {
            try
            {
                if (value)
                {
                    var mInt16 = (ushort)(BitConverter.ToUInt16(_wbs, index * 2) | (1 << bit));
                    SetShort(index, (short)mInt16);
                }
                else
                {
                    var mInt16 = (ushort)(BitConverter.ToUInt16(_wbs, index * 2) & (~(1 << bit)));
                    SetShort(index, (short)mInt16);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
        private bool SetString(int index, string value)
        {
            try
            {
                var bs = Encoding.ASCII.GetBytes(value);
                Buffer.BlockCopy(bs, 0, _wbs, index * 2, bs.Length);
                return true;
            }
            catch
            {
                return false;
            }
        }


        /// <summary>
        /// 获取指定的位寄存器的状态
        /// </summary>
        /// <returns></returns>
        public bool GetRregBit(string name)
        {
            try
            {
                return KVBits.FirstOrDefault(c => c.Key.ToUpper() == name.ToUpper()).Value;
            }
            catch
            {

                return false;
            }

        }
        /// <summary>
        /// 获取指定的位寄存器的状态
        /// </summary>
        /// <returns></returns>
        public bool GetRregBit(int index)
        {
            try
            {
                
                return KVBits[index].Value;
            }
            catch
            {
                return false;
            }

        }
        /// <summary>
        /// 获取指定的16位有符号整型寄存器的状态
        /// </summary>
        /// <returns></returns>
        public short GetRregShort(string name)
        {
            try
            {
                return KvShorts.FirstOrDefault(c => c.Key.ToUpper() == name.ToUpper()).Value;
            }
            catch
            {
                return 0;
            }
        }
        /// <summary>
        /// 获取指定的16位有符号整型寄存器的状态
        /// </summary>
        /// <returns></returns>
        public short GetRregShort(int index)
        {
            try
            {
                return KvShorts[index].Value; ;
            }
            catch
            {
                return 0;
            }
        }
        /// <summary>
        /// 获取指定的32位有符合整型寄存器的状态
        /// </summary>
        /// <returns></returns>
        public int GetRregInt(string name)
        {
            try
            {
                return KvInts.FirstOrDefault(c => c.Key.ToUpper() == name.ToUpper()).Value;
            }
            catch
            {
                return 0;
            }
        }
        /// <summary>
        /// 获取指定的32位有符合整型寄存器的状态
        /// </summary>
        /// <returns></returns>
        public int GetRregInt(int index)
        {
            try
            {
                return KvInts[index].Value;
            }
            catch
            {
                return 0;
            }
        }
        /// <summary>
        /// 获取指定的浮点型寄存器的状态
        /// </summary>
        /// <returns></returns>
        public float GetRregFloat(string name)
        {
            try
            {
                return KvFloats.FirstOrDefault(c => c.Key.ToUpper() == name.ToUpper()).Value;
            }
            catch
            {
                return 0f;
            }
        }
        /// <summary>
        /// 获取指定的浮点型寄存器的状态
        /// </summary>
        /// <returns></returns>
        public float GetRregFloat(int index)
        {
            try
            {
                return KvFloats[index].Value;
            }
            catch
            {
                return 0f;
            }
        }
        /// <summary>
        /// 获取指定的字符串型寄存器的状态
        /// </summary>
        /// <returns></returns>
        public string GetRregString(string name)
        {
            try
            {
                return KvStrings.FirstOrDefault(c => c.Key.ToUpper() == name.ToUpper()).Value;;
            }
            catch
            {
                return "";
            }
        }
        /// <summary>
        /// 获取指定的字符串型寄存器的状态
        /// </summary>
        /// <returns></returns>
        public string GetRregString(int index)
        {
            try
            {
                return KvStrings[index].Value;
            }
            catch
            {
                return "";
            }
        }

        /// <summary>
        /// 设置指定的位寄存器的状态
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool SetRregBit(string name, bool value)
        {
            try
            {
                var tag = KVBits.FirstOrDefault(c => c.Key.ToUpper() == name.ToUpper());

                return WriteBit(tag, value);
            }
            catch
            {
                return false;
            }

        }
        /// <summary>
        /// 设置指定的位寄存器的状态
        /// </summary>
        /// <param name="index"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool SetRregBit(int index, bool value)
        {
            try
            {
               
                return WriteBit(KVBits[index], value);
            }
            catch
            {
                return false;
            }

        }
        /// <summary>
        /// 设置指定的位寄存器的脉冲信号
        /// </summary>
        /// <param name="name"></param>
        /// <param name="Delayms"></param>
        /// <returns></returns>
        public void PulseRregBit(string name, int Delayms = 100)
        {
            try
            {
                var tag= KVBits.FirstOrDefault(c => c.Key.ToUpper() == name.ToUpper());

                PulseBit(tag,Delayms);
            }
            catch
            {

            }

        }
        /// <summary>
        /// 设置指定的位寄存器的脉冲信号
        /// </summary>
        /// <param name="index"></param>
        /// <param name="Delayms"></param>
        /// <returns></returns>
        public void PulseRregBit(int index, int Delayms = 100)
        {
            try
            {
                
                PulseBit(KVBits[index], Delayms);
            }
            catch
            {

            }

        }
        /// <summary>
        /// 设置指定的16位有符号整型寄存器的状态
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool SetRregShort(string name, short value)
        {
            try
            {
                var tag = KvShorts.FirstOrDefault(c => c.Key.ToUpper() == name.ToUpper());

                return WriteShort(tag, value);
            }
            catch
            {
                return false;
            }

        }
        /// <summary>
        /// 设置指定的16位有符号整型寄存器的状态
        /// </summary>
        /// <param name="index"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool SetRregShort(int index, short value)
        {
            try
            {
            
                return WriteShort(KvShorts[index], value);
            }
            catch
            {
                return false;
            }

        }
        /// <summary>
        /// 设置指定的32位有符号整型寄存器的状态
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool SetRregInt(string name, int value)
        {
            try
            {
                var tag = KvInts.FirstOrDefault(c => c.Key.ToUpper() == name.ToUpper());

                return WriteInt(tag, value);
            }
            catch
            {
                return false;
            }

        }
        /// <summary>
        /// 设置指定的32位有符号整型寄存器的状态
        /// </summary>
        /// <param name="index"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool SetRregInt(int index, int value)
        {
            try
            {

                return WriteInt(KvInts[index], value);
            }
            catch
            {
                return false;
            }

        }
        /// <summary>
        /// 设置指定的浮点型寄存器的状态
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool SetRregFloat(string name, float value)
        {
            try
            {
                var tag = KvFloats.FirstOrDefault(c => c.Key.ToUpper() == name.ToUpper());

                return WriteFloat(tag, value);
            }
            catch
            {
                return false;
            }

        }
        /// <summary>
        /// 设置指定的浮点型寄存器的状态
        /// </summary>
        /// <param name="index"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool SetRregFloat(int index, float value)
        {
            try
            {
                return WriteFloat(KvFloats[index], value);
            }
            catch
            {
                return false;
            }

        }
        /// <summary>
        /// 设置指定的字符串型寄存器的状态
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool SetRregString(string name, string value)
        {
            try
            {
                var tag = KvStrings.FirstOrDefault(c => c.Key.ToUpper() == name.ToUpper());

                return WriteString(tag, value);
            }
            catch
            {
                return false;
            }

        }
        /// <summary>
        /// 设置指定的字符串型寄存器的状态
        /// </summary>
        /// <param name="index"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool SetRregString(int index, string value)
        {
            try
            {
                return WriteString(KvStrings[index], value);
            }
            catch
            {
                return false;
            }

        }




        /// <summary>
        /// 设置指定的位寄存器的状态
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool SetWregBit(string name, bool value)
        {
            try
            {
                var tag = KvBitsW.FirstOrDefault(c => c.Key.ToUpper() == name.ToUpper());

                return SetBit(tag.Index,tag.BitIndex ,value);
            }
            catch
            {
                return false;
            }

        }
        /// <summary>
        /// 设置指定的位寄存器的状态
        /// </summary>
        /// <param name="index"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool SetWregBit(int index, bool value)
        {
            try
            {
                var tag = KvBitsW[index];
                return SetBit(tag.Index, tag.BitIndex, value);
            }
            catch
            {
                return false;
            }

        }
        /// <summary>
        /// 设置指定的16位有符号整型寄存器的状态
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool SetWregShort(string name, short value)
        {
            try
            {
                var tag = KvShortsW.FirstOrDefault(c => c.Key.ToUpper() == name.ToUpper());

                return SetShort(tag.Index, value);
            }
            catch
            {
                return false;
            }

        }
        /// <summary>
        /// 设置指定的16位有符号整型寄存器的状态
        /// </summary>
        /// <param name="index"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool SetWregShort(int index, short value)
        {
            try
            {

                return SetShort(KvShortsW[index].Index, value);
            }
            catch
            {
                return false;
            }

        }
        /// <summary>
        /// 设置指定的32位有符号整型寄存器的状态
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool SetWregInt(string name, int value)
        {
            try
            {
                var tag = KvIntsW.FirstOrDefault(c => c.Key.ToUpper() == name.ToUpper());

                return SetInt(tag.Index, value);
            }
            catch
            {
                return false;
            }

        }
        /// <summary>
        /// 设置指定的32位有符号整型寄存器的状态
        /// </summary>
        /// <param name="index"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool SetWregInt(int index, int value)
        {
            try
            {

                return SetInt(KvIntsW[index].Index, value);
            }
            catch
            {
                return false;
            }

        }
        /// <summary>
        /// 设置指定的浮点型寄存器的状态
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool SetWregFloat(string name, float value)
        {
            try
            {
                var tag = KvFloatsW.FirstOrDefault(c => c.Key.ToUpper() == name.ToUpper());

                return SetFloat(tag.Index, value);
            }
            catch
            {
                return false;
            }

        }
        /// <summary>
        /// 设置指定的浮点型寄存器的状态
        /// </summary>
        /// <param name="index"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool SetWregFloat(int index, float value)
        {
            try
            {
                return SetFloat(KvFloatsW[index].Index, value);
            }
            catch
            {
                return false;
            }

        }
        /// <summary>
        /// 设置指定的字符串型寄存器的状态
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool SetWregString(string name, string value)
        {
            try
            {
                var tag = KvStringsW.FirstOrDefault(c => c.Key.ToUpper() == name.ToUpper());
                if (value.Length > tag.Size)
                    value=value.Substring(0, tag.Size);
                return SetString(tag.Index, value);
            }
            catch
            {
                return false;
            }

        }
        /// <summary>
        /// 设置指定的字符串型寄存器的状态
        /// </summary>
        /// <param name="index"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool SetWregString(int index, string value)
        {
            try
            {
                var tag = KvStringsW[index];
                if (value.Length > tag.Size)
                    value = value.Substring(0, tag.Size);
                return SetString(tag.Index, value);
            }
            catch
            {
                return false;
            }

        }












        private IEnumerable<RegTagInfo> GetTagInfos(string filePath)
        {
            try
            {
                FileStream data = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using (StreamReader reader = new StreamReader(data, Encoding.Default))
                {
                    using (CsvReader csv = new CsvReader(reader, CultureInfo.CurrentCulture))
                    {
                        return (from x in csv.GetRecords<RegTagInfo>()
                                where !string.IsNullOrEmpty(x.Address)
                                select x
                                ).Distinct().ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                return new List<RegTagInfo>();
            }
        }
        internal class RegTagInfo
        {
            [Index(0)]
            public string Name { get; set; }
            [Index(1)]
            public string Address { get; set; }
            [Index(2)]
            public string Size { get; set; }
            [Index(3)]
            public string Description { get; set; }
        }
        Regex regexbit = new Regex(@"^DB\d{4}.[A-F0-9]{1}");
        Regex regexshort = new Regex(@"^DW\d{4}");
        Regex regexint = new Regex(@"^DD\d{4}");
        Regex regexfloat = new Regex(@"^DF\d{4}");
        Regex regexstring = new Regex(@"^DS\d{4}");

        Regex regexbit1 = new Regex(@"^DB\d{5}.[A-F0-9]{1}");
        Regex regexshort1 = new Regex(@"^DW\d{5}");
        Regex regexint1 = new Regex(@"^DD\d{5}");
        Regex regexfloat1 = new Regex(@"^DF\d{5}");
        Regex regexstring1 = new Regex(@"^DS\d{5}");

        private void LoadRegTagInfos(string filePath = @"./ScriptModule/RregAdress.csv")
        {
            var tagList = GetTagInfos(filePath);
            foreach(var x in tagList)
            {
                if(x.Address.Length==6)
                {
                if (regexshort.IsMatch(x.Address))
                {
                    int address = int.Parse(x.Address.Substring(2, 4));
                    int offer = address - ReadAddrStart;
                    if (offer >= 0 && offer <= ReadLens)
                        KvShorts.Add(new KV<short>
                        {
                            Index = address - ReadAddrStart,
                            Key = x.Name,
                            Address =$"D{address}",
                            Description = x.Description
                        });
                }
                if (regexint.IsMatch(x.Address))
                {
                    int address = int.Parse(x.Address.Substring(2, 4));
                    int offer = address - ReadAddrStart+1;
                    if (offer >= 0 && offer <= ReadLens)
                        KvInts.Add(new KV<int>
                        {
                            Index = address - ReadAddrStart,
                            Key = x.Name,
                            Address = $"D{address}",
                            Description = x.Description
                        });
                }
                if (regexfloat.IsMatch(x.Address))
                {
                    int address = int.Parse(x.Address.Substring(2, 4));
                    int offer = address - ReadAddrStart + 1;
                    if (offer >= 0 && offer <= ReadLens)
                        KvFloats.Add(new KV<float>
                        {
                            Index = address - ReadAddrStart,
                            Key = x.Name,
                            Address = $"D{address}",
                            Description = x.Description
                        });
                }
                if (regexstring.IsMatch(x.Address))
                {
                    int address = int.Parse(x.Address.Substring(2, 4));
                    int size = int.Parse(x.Size);
                    int offer = address - ReadAddrStart+ size % 2 + size / 2 - 1;
                    if (offer >= 0 && offer <= ReadLens)
                        KvStrings.Add(new KV<string>
                        {
                            Index=address-ReadAddrStart,
                            Key = x.Name,
                            Address = $"D{address}",
                            Size=size,
                            Description = x.Description
                        });;
                }
                }
                else if(x.Address.Length == 7)
                {
                    if (regexshort1.IsMatch(x.Address))
                    {
                        int address = int.Parse(x.Address.Substring(2, 5));
                        int offer = address - ReadAddrStart;
                        if (offer >= 0 && offer <= ReadLens)
                            KvShorts.Add(new KV<short>
                            {
                                Index = address - ReadAddrStart,
                                Key = x.Name,
                                Address = $"D{address}",
                                Description = x.Description
                            });
                    }
                    if (regexint1.IsMatch(x.Address))
                    {
                        int address = int.Parse(x.Address.Substring(2, 5));
                        int offer = address - ReadAddrStart + 1;
                        if (offer >= 0 && offer <= ReadLens)
                            KvInts.Add(new KV<int>
                            {
                                Index = address - ReadAddrStart,
                                Key = x.Name,
                                Address = $"D{address}",
                                Description = x.Description
                            });
                    }
                    if (regexfloat1.IsMatch(x.Address))
                    {
                        int address = int.Parse(x.Address.Substring(2, 5));
                        int offer = address - ReadAddrStart + 1;
                        if (offer >= 0 && offer <= ReadLens)
                            KvFloats.Add(new KV<float>
                            {
                                Index = address - ReadAddrStart,
                                Key = x.Name,
                                Address = $"D{address}",
                                Description = x.Description
                            });
                    }
                    if (regexstring1.IsMatch(x.Address))
                    {
                        int address = int.Parse(x.Address.Substring(2, 5));
                        int size = int.Parse(x.Size);
                        int offer = address - ReadAddrStart + size % 2 + size / 2 - 1;
                        if (offer >= 0 && offer <= ReadLens)
                            KvStrings.Add(new KV<string>
                            {
                                Index = address - ReadAddrStart,
                                Key = x.Name,
                                Address = $"D{address}",
                                Size = size,
                                Description = x.Description
                            }); ;
                    }
                }
                else if(x.Address.Length==8)
                {
                    if (regexbit.IsMatch(x.Address))
                    {
                        int address = int.Parse(x.Address.Substring(2, 4));
                        int offer = address - ReadAddrStart;
                        if (offer >= 0 && offer <= ReadLens)
                            KVBits.Add(new KV<bool>
                            {
                                Index = address - ReadAddrStart,
                                Key = x.Name,
                                Address = $"D{address}",
                                BitIndex = Convert.ToInt32(x.Address.Substring(7, 1), 16),
                                Description = x.Description
                            });
                    }
                }
                else if (x.Address.Length == 9)
                {
                    if (regexbit1.IsMatch(x.Address))
                    {
                        int address = int.Parse(x.Address.Substring(2, 5));
                        int offer = address - ReadAddrStart;
                        if (offer >= 0 && offer <= ReadLens)
                            KVBits.Add(new KV<bool>
                            {
                                Index = address - ReadAddrStart,
                                Key = x.Name,
                                Address = $"D{address}",
                                BitIndex = Convert.ToInt32(x.Address.Substring(8, 1), 16),
                                Description = x.Description
                            });
                    }
                }
            }
        }
        private void LoadWegTagInfos(string filePath = @"./ScriptModule/WregAdress.csv")
        {
            var tagList = GetTagInfos(filePath);
            foreach (var x in tagList)
            {              
                if (x.Address.Length == 6)
                {
                    if (regexshort.IsMatch(x.Address))
                    {
                        int address = int.Parse(x.Address.Substring(2, 4));
                        int offer = address - WriteAddrStart;
                        if (offer >= 0 && offer <= WriteLens)
                            KvShortsW.Add(new KV<short>
                            {
                                Index = address - WriteAddrStart,
                                Key = x.Name,
                                Address = $"D{address}",
                                Description = x.Description
                            });
                    }
                    if (regexint.IsMatch(x.Address))
                    {
                        int address = int.Parse(x.Address.Substring(2, 4));
                        int offer = address - WriteAddrStart + 1;
                        if (offer >= 0 && offer <= WriteLens)
                            KvIntsW.Add(new KV<int>
                            {
                                Index = address - WriteAddrStart,
                                Key = x.Name,
                                Address = $"D{address}",
                                Description = x.Description
                            });
                    }
                    if (regexfloat.IsMatch(x.Address))
                    {
                        int address = int.Parse(x.Address.Substring(2, 4));
                        int offer = address - WriteAddrStart + 1;
                        if (offer >= 0 && offer <= WriteLens)
                            KvFloatsW.Add(new KV<float>
                            {
                                Index = address - WriteAddrStart,
                                Key = x.Name,
                                Address = $"D{address}",
                                Description = x.Description
                            });
                    }
                    if (regexstring.IsMatch(x.Address))
                    {
                        int address = int.Parse(x.Address.Substring(2, 4));
                        int size = int.Parse(x.Size);
                        int offer = address - WriteAddrStart + size % 2 + size / 2 - 1;
                        if (offer >= 0 && offer <= WriteLens)
                            KvStringsW.Add(new KV<string>
                            {
                                Index = address - WriteAddrStart,
                                Key = x.Name,
                                Address = $"D{address}",
                                Size = size,
                                Description = x.Description
                            }); ;
                    }
                }
                else if(x.Address.Length == 7)
                {
                    if (regexshort1.IsMatch(x.Address))
                    {
                        int address = int.Parse(x.Address.Substring(2, 5));
                        int offer = address - WriteAddrStart;
                        if (offer >= 0 && offer <= WriteLens)
                            KvShortsW.Add(new KV<short>
                            {
                                Index = address - WriteAddrStart,
                                Key = x.Name,
                                Address = $"D{address}",
                                Description = x.Description
                            });
                    }
                    if (regexint1.IsMatch(x.Address))
                    {
                        int address = int.Parse(x.Address.Substring(2, 5));
                        int offer = address - WriteAddrStart + 1;
                        if (offer >= 0 && offer <= WriteLens)
                            KvIntsW.Add(new KV<int>
                            {
                                Index = address - WriteAddrStart,
                                Key = x.Name,
                                Address = $"D{address}",
                                Description = x.Description
                            });
                    }
                    if (regexfloat1.IsMatch(x.Address))
                    {
                        int address = int.Parse(x.Address.Substring(2, 5));
                        int offer = address - WriteAddrStart + 1;
                        if (offer >= 0 && offer <= WriteLens)
                            KvFloatsW.Add(new KV<float>
                            {
                                Index = address - WriteAddrStart,
                                Key = x.Name,
                                Address = $"D{address}",
                                Description = x.Description
                            });
                    }
                    if (regexstring1.IsMatch(x.Address))
                    {
                        int address = int.Parse(x.Address.Substring(2, 5));
                        int size = int.Parse(x.Size);
                        int offer = address - WriteAddrStart + size % 2 + size / 2 - 1;
                        if (offer >= 0 && offer <= WriteLens)
                            KvStringsW.Add(new KV<string>
                            {
                                Index = address - WriteAddrStart,
                                Key = x.Name,
                                Address = $"D{address}",
                                Size = size,
                                Description = x.Description
                            }); ;
                    }
                }
                else if(x.Address.Length==8)
                {
                    if (regexbit.IsMatch(x.Address))
                    {
                        int address = int.Parse(x.Address.Substring(2, 4));
                        int offer = address - WriteAddrStart;
                        if (offer >= 0 && offer <= WriteLens)
                            KvBitsW.Add(new KV<bool>
                            {
                                Index = address - WriteAddrStart,
                                Key = x.Name,
                                Address = $"D{address}",
                                BitIndex = Convert.ToInt32(x.Address.Substring(7, 1), 16),
                                Description = x.Description
                            });
                    }
                }
                else if (x.Address.Length == 9)
                {
                    if (regexbit1.IsMatch(x.Address))
                    {
                        int address = int.Parse(x.Address.Substring(2, 5));
                        int offer = address - WriteAddrStart;
                        if (offer >= 0 && offer <= WriteLens)
                            KvBitsW.Add(new KV<bool>
                            {
                                Index = address - WriteAddrStart,
                                Key = x.Name,
                                Address = $"D{address}",
                                BitIndex = Convert.ToInt32(x.Address.Substring(8, 1), 16),
                                Description = x.Description
                            });
                    }
                }
            }
        }

        bool disposed = false;
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;
            if (disposing)
            {
                _cts.Dispose();
            }
            disposed = true;
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
