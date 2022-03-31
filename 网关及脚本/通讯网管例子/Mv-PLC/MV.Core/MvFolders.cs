using System;
using System.IO;

namespace Mv.Core
{
    public  static partial class  MvFolders
    {
        static MvFolders()
        {
            Directory.CreateDirectory(Apps);
            Directory.CreateDirectory(Logs);
            Directory.CreateDirectory(Users);
            Directory.CreateDirectory(Drivers);
            Directory.CreateDirectory(Modules);
           
        }

        /// <summary>
        /// It represents the path where the "MV.exe" is located.
        /// </summary>
        public static readonly string MainProgram = AppDomain.CurrentDomain.BaseDirectory;

        /// <summary>
        /// %AppData%\MV
        /// </summary>
        public static readonly string AppData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MV");

        /// <summary>
        /// %AppData%\MV\Apps
        /// </summary>
        public static readonly string Apps = Path.Combine(AppData, nameof(Apps));

        /// <summary>
        /// %AppData%\MV\Logs
        /// </summary>
        public static readonly string Logs = Path.Combine(AppData, nameof(Logs));
        /// <summary>
        /// %AppData%\MV\Users
        /// </summary>
        public static readonly string Users = Path.Combine(AppData, nameof(Users));
        public static readonly string Drivers = Path.Combine(MainProgram, nameof(Drivers));
        public static readonly string Modules = Path.Combine(MainProgram, nameof(Modules));

 
    }
}