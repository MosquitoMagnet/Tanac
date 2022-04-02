using System;
using System.Collections.Generic;
using System.Text;

namespace Mv.Modules.P99.Service
{
    public class MachineState
    {
        public class Data
        {
            public string previous_state { get; set; }
            public string state_change_reason { get; set; }
            public string error_message { get; set; }
            public string code { get; set; }
        }

        public string machine_state { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string state_change_time { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Data data { get; set; }
    }
}
