using BatchCoreService;
using Mv.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mv.Modules.TagManager.Services
{

    public class DriverDataContext : IDriverDataContext
    {

        public const string DRIVERS = "DRIVERS";
        private readonly IConfigureFile configure;
        public DriverDataContext(IConfigureFile configure)
        {
            this.configure = configure;
        }

        public IEnumerable<Driver> GetDrivers()
        {
            var drivers = configure.GetValue<IEnumerable<Driver>>(DRIVERS) ?? new List<Driver>();
            return drivers;
        }

        public void SetDrivers(IEnumerable<Driver> drivers)
        {
            var ms = Enumerable.Range(0, drivers.Count()).Zip(drivers);
            foreach (var m in ms)
            {
                m.Second.Id = (short)m.First;
            }
            var gs = Enumerable.Range(0, drivers.SelectMany(x=>x.Groups).Count()).Zip(drivers.SelectMany(x => x.Groups));
            foreach (var g in gs)
            {
                g.Second.Id = (short)g.First;
                g.Second.TagMetas.ForEach(x => x.GroupID = g.Second.Id);
            }
            var ts = Enumerable.Range(0, drivers.SelectMany(x => x.Groups).SelectMany(m=>m.TagMetas).Count()).Zip(drivers.SelectMany(x => x.Groups).SelectMany(m => m.TagMetas));
            foreach (var t in ts)
            {
                t.Second.ID = (short)t.First;
            }
            configure.SetValue(DRIVERS, drivers);
        }
    }
}
