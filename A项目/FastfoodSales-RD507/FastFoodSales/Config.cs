using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAQ
{
    public class Config
    {
        public string PlcIpAddress { get; set; } = "127.0.0.1";
        public int PlcPort { get; set; } = 6000;
        public string NFCIpAddress { get; set; } = "192.168.1.39";
        public int NFCPort { get; set; } = 5000;
        public string PrinterIpAddress { get; set; } = "127.0.0.1";
        public int PrinterPort { get; set; } = 5000;
        public string AirComPort { get; set; } = "COM1";
        public int AirScanInterval { get; set; } = 1800;
        public int AirValueL { get; set; } = 0;
        public int AirValueH { get; set; }=100;
        public string FileDir { get; set; } = @"D:\machine";
        public string CoilModule { get; set; } = "1";
        public string SI { get; set; } = "A";       
        public string LineNumber { get; set; } = "0";
        public string Station { get; set; } = "0";
        public string Spindle { get; set; } = "0";

        public string Mo { get; set; } = "H5109-200400087";
        public string Language { get; set; } = "zh-CN";

    }
}
