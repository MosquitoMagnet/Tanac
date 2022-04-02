using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Mv.Modules.RD402.Hive.Services
{
    public class MachineState
    {
        public class Data
        {
            [JsonProperty("previous_state")]
            public string previous_state { get; set; }

            [JsonProperty("State_change_reason")]
            public string state_change_reason { get; set; }
            public string error_message { get; set; }
            public string code { get; set; }
            public string sw_version { get; set; }//软件版本
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
