using System.Collections.Generic;

namespace BatchCoreService
{
    public interface IDriverDataContext
    {
        IEnumerable<Driver> GetDrivers();
        void SetDrivers(IEnumerable<Driver> drivers);
    }
}