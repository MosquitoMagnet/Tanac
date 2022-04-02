using System;
using System.Collections.Generic;
using System.Text;

namespace Mv.Modules.P99.Service
{
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
