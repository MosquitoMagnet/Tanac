using System;
using System.Threading.Tasks;

namespace Mv.Ui.Core.Upgrade
{
    public class AppUpgradeTask : UpgradeTaskBase
    {
        public AppUpgradeTask(string name) : base(name) { }

        protected override Task<bool> UpgradeCoreAsync(UpgradeInfo metadata)
        {
            throw new NotImplementedException();
        }
    }
}
