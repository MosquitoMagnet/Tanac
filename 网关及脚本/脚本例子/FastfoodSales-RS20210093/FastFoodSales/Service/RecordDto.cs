using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAQ.Service
{
    public class RecordDto : ISource
    {
        public string Source { get; set; } = "TestData";
        public DateTime DateTime { get; set; }
        public string QR { get; set; }
        public double HiPot1_0 { get; set; }
        public double HiPot1_1 { get; set; }
        public double HiPot1_2 { get; set; }
        public double HiPot1_3 { get; set; }
        public double HiPot1_4 { get; set; }
        public double HiPot1_5 { get; set; }
        public double HiPot1_6 { get; set; }
        public double HiPot1_7 { get; set; }
        public double HiPot2_0 { get; set; }
        public double HiPot2_1 { get; set; }
        public double HiPot2_2 { get; set; }
        public double HiPot2_3 { get; set; }
        public double HiPot2_4 { get; set; }
        public double HiPot3_0 { get; set; }
        public double HiPot3_1 { get; set; }
        public double Lcr1 { get; set; }
        public double Lcr2 { get; set; }
        public double Lcr3 { get; set; }
        public double Lcr4 { get; set; }
        public double Lcr5 { get; set; }
        public double Lcr6 { get; set; }
        public double Lcr7 { get; set; }
        public double MEA1 { get; set; }
        public double MEA2 { get; set; }
        public double MEA3 { get; set; }
        public double MEA4 { get; set; }
        public double MEA5 { get; set; }
        public double MEA6 { get; set; }
        public double MEA7 { get; set; }
        public double MEA8 { get; set; }
        public double MEA9 { get; set; }
        public double MEA10 { get; set; }
        public double MEA11 { get; set; }
        public double MEA12 { get; set; }
        public double MEA13 { get; set; }
        public double MEA14 { get; set; }
        public double MEA15 { get; set; }
        public double MEA16 { get; set; }
        public double MEA17 { get; set; }
        public double MEA18 { get; set; }
        public double MEA19 { get; set; }
        public double MEA20 { get; set; }
    }
}
