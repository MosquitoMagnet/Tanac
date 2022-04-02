using System;
using System.Collections.Generic;
using System.Text;

namespace Mv.Modules.P150.Services
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
}
