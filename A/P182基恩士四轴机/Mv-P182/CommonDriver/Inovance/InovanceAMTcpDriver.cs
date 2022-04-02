using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.IO;
using System.Collections;
using DataService;
using Communication.Profinet.Inovance;
using Communication;
using Communication.Core.Address;
using Communication.Core.Net;

namespace CommonDriver
{
    [Description("INOVANCE_A400_800_ModbusTCP")]
    public class InovanceAMTcpDriver : ModbusTcpDriver
    {
        public InovanceAMTcpDriver(IDataServer server, short id, string name, string serverName, int timeOut = 500, IDictionary<string, string> paras = null) : base(server, id, name, serverName, timeOut, paras)
        {
        }

        public InovanceAMTcpDriver()
        {
        }
        public override DeviceAddress GetDeviceAddress(string address)
        {
            DeviceAddress dv = DeviceAddress.Empty;
            if (string.IsNullOrEmpty(address))
                return dv;
            var sindex = address.IndexOf(':');
            if (sindex > 0)
            {
                int slaveId;
                if (int.TryParse(address.Substring(0, sindex), out slaveId))
                    dv.Area = slaveId;
                address = address.Substring(sindex + 1);
            }
            try
            {
                if (address.StartsWith("M"))
                {
                    switch (address[1])
                    {
                        case 'X':
                            {
                                int index = address.IndexOf('.');
                                dv.DBNumber = 3;
                                if (index > 0)
                                {
                                    dv.Start = int.Parse(address.Substring(1, index - 1));
                                    dv.Bit = byte.Parse(address.Substring(index + 1));
                                }
                                else
                                    dv.Start = int.Parse(address.Substring(1));

                            }
                            break;
                        case 'B':
                            {
                                dv.DBNumber = 3;
                                dv.Start = int.Parse(address.Substring(2));
                            }
                            break;
                        case 'W':
                            {
                                dv.DBNumber = 3;
                                dv.Start = int.Parse(address.Substring(1));
                            }
                            break;
                        case 'D':
                            {
                                dv.DBNumber = 3;
                                dv.Start = int.Parse(address.Substring(2));
                            }
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                return dv;
            }
            return dv;
        }
    }
}
