using System;
using System.Collections.Generic;
using System.Text;

namespace Mv.Modules.TagManager
{
    public class Config
    {
        public string Line { get; set; } = "二楼G16";
        public string Shift { get; set; } = "白班";
        public string MachineType { get; set; } = "0100085";
        public int PlanYield = 6000;
        public string PersonNumber = "6";
        public string Title { get; set; } = "G15/G19磁环自动线";
    }
    public class TagManagerConfig
    {
        public string Version { get; set; } = "0.0.1";
        public string PlcIpAddress { get; set; } = "127.0.0.1";
        public int PlcPort { get; set; } = 502;
        public ushort WriteAddrStart { get; set; } = 6000;
        public ushort ReadAddrStart { get; set; } = 6000;
        public ushort WriteLens { get; set; } = 50;
        public ushort ReadLens { get; set; } = 50;


    }
}
