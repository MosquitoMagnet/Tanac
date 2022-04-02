using System;
using System.Collections.Generic;
using System.Text;

namespace Mv.Modules.P99
{
    public class P99Config
    {
        public string MachineNo { get; set; } = "1";
        public string SaveDir { get; set; } = @"D:\DATA";
        public string UvLightIp { get; set; } = "192.168.1.16";
        public int UvLightPort { get; set; } = 8000;
        public string Station { get; set; } = "T0479";
        public string LineNo{get; set;} = "01";
        public bool CheckCode { get; set; } = true;
        public string Factory { get; set; } = "LinYi";//ICT 信维
        public string SoftwareVER { get; set; } = "1.0.0";//软件版本2021.1.18
    }
}
