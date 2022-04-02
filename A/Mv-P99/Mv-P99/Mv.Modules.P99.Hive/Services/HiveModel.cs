using System;
using System.Collections.Generic;
using System.Text;

namespace Mv.Modules.P99.Hive.Services
{
    /// <summary>
    /// hive 服务器返回信息的model
    /// </summary>
    [Serializable]
    public class HiveServerModel
    {
        //  ErrorCode
        //      ErrorText
        //      ErrorValidation
        //pu      Status
        /// <summary>
        /// 错误编码
        /// </summary>
        public Object ErrorCode { get; set; }
        /// <summary>
        /// 错误信息
        /// </summary>
        public string ErrorText { get; set; }
        /// <summary>
        /// 错误校验明细
        /// </summary>
        public object ErrorValidation { get; set; }
        /// <summary>
        /// 状态
        /// </summary>
        public string Status { get; set; }
    }
    public class MachineData
    {
        public class Serials
        {
        }
        public class Data
        {
            #region Fixture CTQ
            public string Spindle_NO { get; set; }
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
    public class ErrorData
    {
        public class Data
        {

        }
        /// <summary>
        ///正在记录错误的描述性消息 
        /// </summary>
        public string message { get; set; }
        /// <summary>
        /// 发生的错误代码
        /// </summary>
        public string code { get; set; }
        /// <summary>
        ///错误的严重性 
        /// </summary>
        public string severity { get; set; }
        /// <summary>
        /// 错误发生时间
        /// </summary>
        public string occurrence_time { get; set; }
        /// <summary>
        /// 错误结束时间
        /// </summary>
        public string resolved_time { get; set; }
        /// <summary>
        ///除事件外要记录的任何机器数据 
        /// </summary>
        public Data data { get; set; }
    }
}
