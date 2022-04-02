using System.Collections.Generic;
using System.ComponentModel;

namespace BatchCoreService
{
    public class Driver
    {
        [Browsable(false)]
        public short Id { get; set; }
        [ReadOnly(true)]
        public string Name { get; set; } = "";
        [ReadOnly(true)]
        public string Assembly { get; set; }
        [ReadOnly(true)]
        public string ClassName { get; set; }
        public string Server { get; set; }
        [ReadOnly(true)]
        public string Description { get; set; }
        public int Timeout { get; set; } = 500;
        public ICollection<Group> Groups { get; set; } = new List<Group>();
        public ICollection<DriverArgument> Arguments { get; set; } = new List<DriverArgument>();
        public bool Active { get; set; }
    }
}
