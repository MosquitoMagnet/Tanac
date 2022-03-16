using DataService;
using Communication;
using Communication.Core.Address;
using Communication.Profinet.Keyence;
using Communication.Profinet.Melsec;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;

namespace CommonDriver
{
    [Description("Keyence-MC-3E协议")]
    public sealed class KeyenceMcNetDriver : MelsecMcNetDriver
    {

        public KeyenceMcNetDriver(IDataServer server, short id, string name, string serverName, int timeOut = 500, IDictionary<string, string> paras = null) : base(server, id, name, serverName, timeOut, paras)
        {

            BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Static;
            var fields = typeof(MelsecMcDataType).GetFields(bindingFlags)
                .Where(x => x.FieldType == typeof(MelsecMcDataType))
                .Where(m => m.Name.Contains("Keyence"))
                .Select(x => (MelsecMcDataType)x.GetValue(null))
                .ToDictionary(x => (int)x.DataCode);
            if (fields != null)
                _dictionary = fields;
            mc = new KeyenceMcNet(serverName, Port);
        }

        public KeyenceMcNetDriver()
        {

        }
        public override DeviceAddress GetDeviceAddress(string address)
        {
            if (string.IsNullOrEmpty(address))
            {
                return DeviceAddress.Empty;
            }
            if (address.ToUpper().StartsWith("MR"))
            {
                var bit = int.TryParse(address.Substring(address.Length - 2), out var v0);
                var r = int.TryParse(address.Substring(2, address.Length - 4), out var v1);
                if (bit && r)
                {
                    address = $"M{v1 * 16 + v0 }";
                }
                else
                {
                    return DeviceAddress.Empty;
                }
            }
            if (address.ToUpper().StartsWith("EM"))
            {
                var r = int.TryParse(address.Substring(2), out var v1);
                if (r)
                {
                    address = $"D{100000 + v1 }";
                }
                else
                {
                    return DeviceAddress.Empty;
                }
            }
            if (address.ToUpper().StartsWith("DM"))
            {
                var r = int.TryParse(address.Substring(2), out var v1);
                if (r)
                {
                    address = $"D{ v1 }";
                }
                else
                {
                    return DeviceAddress.Empty;
                }
            }
            var m = McAddressData.ParseKeyenceFrom(address, 0);
            OperateResult<McAddressData> operateResult = McAddressData.ParseMelsecFrom(address, 0);
            if (operateResult.IsSuccess)
            {
                if (operateResult.Content.McDataType.DataType == 1)
                {
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
    }
}
