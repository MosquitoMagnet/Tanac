using System.Collections.Generic;
using System.IO;
using Mv.Core;
using Newtonsoft.Json;
using Prism.Modularity;

namespace Mv.Ui.Core.Modularity
{
    public static class ModuleCatalogHelper
    {
        //private static readonly ILog Logger = LogManager.GetLogger(typeof(ModuleCatalogHelper));

        public const string MvModulesPlaceholder = "{Modules}";
        public const string MvCommonDllsPlaceholder = "{CommonDlls}";

        public static readonly string MvCommonDlls = Path.Combine(MvFolders.Apps, "Common");

        public static ModuleCatalog CreateFromJson(string jsonText)
        {
            var moduleInfos = JsonConvert.DeserializeObject<List<RemoteModuleInfo>>(jsonText);

            moduleInfos.ForEach(item =>
            {
                if (item.RemoteRefs == null) item.RemoteRefs = new List<RemoteRef>();
            });

            moduleInfos.ForEach(item =>
            {
                item.Ref = ReplacePlaceholder(item.Ref);

                foreach (var remoteRef in item.RemoteRefs)
                {
                    remoteRef.RemotePath = ReplacePlaceholder(remoteRef.RemotePath);
                    remoteRef.LocalPath = ReplacePlaceholder(remoteRef.LocalPath);
                }
            });

            return new ModuleCatalog(moduleInfos);
        }

        private static string ReplacePlaceholder(string filePath) => filePath
            .Replace(MvModulesPlaceholder, MvFolders.Apps)
            .Replace(MvCommonDllsPlaceholder, MvCommonDlls);
    }
}
