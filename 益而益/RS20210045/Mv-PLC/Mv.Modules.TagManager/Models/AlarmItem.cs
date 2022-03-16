using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Prism.Mvvm;
using Prism.Events;

namespace Mv.Modules.TagManager.Models
{
    public class AlarmItem:BindableBase
    {
        /// <summary>
        /// 报警发送工位
        /// </summary>
        public string Source { get; set; }
        /// <summary>
        /// 报警地址
        /// </summary>
        public string Address { get; set; }
        /// <summary>
        /// 报警内容
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// 报警日期
        /// </summary>
        public DateTime Date { get; set; }
        /// <summary>
        /// 报警触发时间
        /// </summary>
        public DateTime StartTime { get; set; }
        /// <summary>
        /// 报警结束时间
        /// </summary>
        public DateTime EndTime { get; set; }
    }
    public class AlarmList : BindableBase
    {
        public List<AlarmItem> Alarms { get; set; }
    }
    public class AlarmItemEvent : PubSubEvent<AlarmList>
    {

    }
}
