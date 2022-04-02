using Prism.Events;
using Prism.Logging;
using System;

namespace MV.Core.Events
{
    public class UserMessage
    {
        public DateTime Time { get; set; } = DateTime.Now;//时间
        public string Source { get; set; }
        public Category Level { get; set; }
        public string Content { get; set; }//内容
    }


    public class UserMessageEvent : PubSubEvent<UserMessage>
    {

    }
}
