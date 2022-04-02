using System;
using System.Collections.Generic;
using System.Text;

namespace Mv.Modules.P150.Services
{
    public class MachineData
    {
        public class Serials
        {
        }
        public class Data
        {
            #region Fixture CTQ
            public string Spindle { get; set; }
            public string CTQ_FAI1 { get; set; }
            public string CTQ_FAI2 { get; set; }
            public string CTQ_FAI3 { get; set; }
            public string CTQ_FAI4 { get; set; }
            public string CTQ_FAI5 { get; set; }
            public string CTQ_FAI6 { get; set; }
            public string CTQ_FAI7 { get; set; }
            public string CTQ_FAI8 { get; set; }
            public string CTQ_FAI9 { get; set; }
            public string CTQ_FAI10 { get; set; }
            public string CTQ_FAI11 { get; set; }
            public string CTQ_FAI12 { get; set; }
            public string CTQ_FAI13 { get; set; }
            public string CTQ_FAI14 { get; set; }
            public string CTQ_FAI15 { get; set; }
            public string CTQ_FAI16 { get; set; }
            public string CTQ_FAI17 { get; set; }
            public string CTQ_FAI18 { get; set; }
            public string CTQ_FAI19 { get; set; }
            public string CTQ_FAI20 { get; set; }
            public string CTQ_FAI21 { get; set; }
            public string CTQ_FAI22 { get; set; }
            public string CTQ_FAI23 { get; set; }
            public string CTQ_FAI24 { get; set; }
            public string CTQ_FAI25 { get; set; }
            public string Parallelism_between_upper_and_lower_mandrel { get; set; }
            public string Bending_Pin_Position { get; set; }
            public string Tool_cycles { get; set; }
            public string PM_cycles { get; set; }
            public string Wire_OD { get; set; }//线径
            public string Lower_Mandrel_temperature { get; set; }//下模温
            public string Wire_Tension1 { get; set; }
            public string Wire_Tension2 { get; set; }
            public string Wire_Tension3 { get; set; }
            public string Wire_Tension4 { get; set; }
            public string Wire_Tension5 { get; set; }
            public string Wire_Tension6 { get; set; }
            public string Wire_Tension7 { get; set; }
            public string Mandrel_gap1 { get; set; }
            public string Mandrel_gap2 { get; set; }
            public string Mandrel_gap3 { get; set; }
            public string Winding_speed1 { get; set; }//第一段速度
            public string Winding_speed2 { get; set; }//第二段速度
            public string Winding_speed3 { get; set; }//第三段速度
            public string Bending_speed { get; set; }//折弯速度
            public string Iron_speed { get; set; }//烫线速度
            public string sw_version { get; set; }//软件版本
            #endregion
            #region Power
            public string Rstart { get; set; }
            public string Current { get; set; }
            public string Voltage { get; set; }
            public string Power { get; set; }
            public string Rend { get; set; }
            public string Bonding_Temp { get; set; }
            public string Bonding_Time { get; set; }
            public string Tool_Temp { get; set; }
            public string Bonding_Method { get; set; }
            public string RC1 { get; set; }
            public string RC2 { get; set; }
            #endregion
        }
        /// <summary>
        /// 
        /// </summary>
        public string unit_sn { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Serials serials { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string pass { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string input_time { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string output_time { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Data data { get; set; }
        /// <summary>
        /// 
        /// </summary>
    }
}
