using DataService;
using System.Collections.Generic;
using System.ComponentModel;

namespace BatchCoreService
{
    public class Group
    {
        [Browsable(false)]
        public short Id { get; set; }
        [ReadOnly(true)]
        public string Name { get; set; }
        public int UpdateRate { get; set; } = 100;
        public int DeadBand { get; set; } = 2000;
        public bool Active { get; set; } = true;
        [Browsable(false)]
        public int DriverId { get; set; }
        public ICollection<TagMetaData> TagMetas { get; set; } = new List<TagMetaData>();
    }
}
