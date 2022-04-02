using System;
using System.Collections.Generic;
using System.Text;

namespace Mv.Modules.P99.Service
{
    public class MachineData
    {
        public class Serials
        {
        }
        public class Data
        {
            #region Fixture CTQ
            public string Spindle_NO{ get; set; }
            public string Mandrel_NO { get; set; }
            public string sw_version { get; set; }//软件版本
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
