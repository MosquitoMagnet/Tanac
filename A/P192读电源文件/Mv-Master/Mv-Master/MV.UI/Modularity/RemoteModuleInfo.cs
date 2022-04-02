using System.Collections.Generic;
using Prism.Modularity;

namespace Mv.Ui.Core.Modularity
{
    public class RemoteModuleInfo : ModuleInfo
    {
        public List<RemoteRef> RemoteRefs { get; set; }
    }
}
