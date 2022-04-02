using System.IO;

namespace Mv.Core
{
    public static class MvFiles
    {
        /// <summary>
        /// %AppData%\MV\MV.config
        /// </summary>
        public static readonly string Configure = Path.Combine(MvFolders.AppData, "MV.config");
    }
}