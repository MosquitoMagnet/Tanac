using System;
using System.Threading.Tasks;
using Mv.Core.JsonObjects;

namespace Mv.Ui.Core.Upgrade
{
    public interface IUpgradeTask
    {
        Version CurrentVersion { get; }

        string Name { get; }

        Task<bool> UpdateAsync(AppMetadata metadata);
    }
}
