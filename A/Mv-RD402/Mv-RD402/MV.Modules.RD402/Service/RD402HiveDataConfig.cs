using System;
using System.Collections.Generic;
using System.Text;

namespace Mv.Modules.RD402
{
    public class RD402HiveDataConfig
    {
        #region CTQ_A
        public string FA1_A { get; set; } ="1";
        public string FA2_A { get; set; } = "1";
        public string FA3_A { get; set; } = "1";
        public string FA4_A { get; set; } = "1";
        public string FA5_A { get; set; } = "1";
        public string FA6_A { get; set; } = "1";
        public string FA7_A { get; set; } = "1";
        public string FA8_A { get; set; } = "1";
        public string FA9_A { get; set; } = "1";
        public string FA10_A { get; set; } = "1";
        public string FA11_A { get; set; } = "1";
        public string FA12_A { get; set; } = "1";
        public string FA13_A { get; set; } = "1";
        public string FA14_A { get; set; } = "1";
        public string FA15_A { get; set; } = "1";
        public string FA16_A { get; set; } = "1";
        public string FA17_A { get; set; } = "1";
        public string FA18_A { get; set; } = "1";
        public string FA19_A { get; set; } = "1";
        public string FA20_A { get; set; } = "1";
        public string FA21_A { get; set; } = "1";
        public string FA22_A { get; set; } = "1";
        public string FA23_A { get; set; } = "1";
        public string FA24_A { get; set; } = "1";
        public string FA25_A { get; set; } = "1";
        public string Parallelism_A { get; set; } = "1";
        public string BendingPin_A { get; set; } = "1";
        #endregion
        #region CTQ_B
        public string FA1_B { get; set; } = "1";
        public string FA2_B { get; set; } = "1";
        public string FA3_B { get; set; } = "1";
        public string FA4_B { get; set; } = "1";
        public string FA5_B { get; set; } = "1";
        public string FA6_B { get; set; } = "1";
        public string FA7_B { get; set; } = "1";
        public string FA8_B { get; set; } = "1";
        public string FA9_B { get; set; } = "1";
        public string FA10_B { get; set; } = "1";
        public string FA11_B { get; set; } = "1";
        public string FA12_B { get; set; } = "1";
        public string FA13_B { get; set; } = "1";
        public string FA14_B { get; set; } = "1";
        public string FA15_B { get; set; } = "1";
        public string FA16_B { get; set; } = "1";
        public string FA17_B { get; set; } = "1";
        public string FA18_B { get; set; } = "1";
        public string FA19_B { get; set; } = "1";
        public string FA20_B { get; set; } = "1";
        public string FA21_B { get; set; } = "1";
        public string FA22_B { get; set; } = "1";
        public string FA23_B { get; set; } = "1";
        public string FA24_B { get; set; } = "1";
        public string FA25_B { get; set; } = "1";
        public string Parallelism_B { get; set; } = "1";
        public string BendingPin_B { get; set; } = "1";
        #endregion
        #region Material_A
        public string OD_A { get; set; } = "1";
        #endregion
        #region Material_B
        public string OD_B { get; set; } = "1";
       
        #endregion
        #region KPIV_A    
        public string WireTen_A { get; set; } = "1";
        #endregion
        #region KPIV_B    
        public string WireTen_B { get; set; } = "1";
        #endregion
        #region Upload  
        public bool isFA1 { get; set; }
        public bool isFA2 { get; set; }
        public bool isFA3 { get; set; }
        public bool isFA4 { get; set; }
        public bool isFA5 { get; set; }
        public bool isFA6 { get; set; }
        public bool isFA7 { get; set; }
        public bool isFA8 { get; set; }
        public bool isFA9 { get; set; }
        public bool isFA10 { get; set; }
        public bool isFA11 { get; set; }
        public bool isFA12 { get; set; }
        public bool isFA13 { get; set; }
        public bool isFA14 { get; set; }
        public bool isFA15 { get; set; }
        public bool isFA16 { get; set; }
        public bool isFA17 { get; set; }
        public bool isFA18 { get; set; }
        public bool isFA19 { get; set; }
        public bool isFA20 { get; set; }
        public bool isFA21 { get; set; }
        public bool isFA22 { get; set; }
        public bool isFA23 { get; set; }
        public bool isFA24 { get; set; }
        public bool isFA25 { get; set; }
        public bool isParallelism { get; set; }
        public bool isBendingPin { get; set; }
        public bool isUsageC { get; set; }
        public bool isUsageM { get; set; }
        public bool isOD { get; set; }
        public bool isLower { get; set; }
        public bool isWireTen { get; set; }
        public bool isGrap1 { get; set; }
        public bool isGrap2 { get; set; }
        public bool isGrap3 { get; set; }
        public bool isWspeed1 { get; set; }
        public bool isWspeed2 { get; set; }
        public bool isWspeed3 { get; set; }
        public bool isWspeedB { get; set; }
        public bool isIspeed { get; set; }
        #endregion
        public string Factory { get; set; } = "AxisA";
        public bool isUpload { get; set; } = true;
    }
}
