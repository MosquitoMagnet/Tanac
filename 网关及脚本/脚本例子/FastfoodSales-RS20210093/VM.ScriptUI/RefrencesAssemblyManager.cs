using System;
using System.Reflection;

namespace VM.ScriptUI
{
	public class RefrencesAssemblyManager
	{
		private static string _systemPath = "";

		public static string SyetemPath
		{
			get
			{
				if (string.IsNullOrEmpty(_systemPath))
				{
					_systemPath = GetSystemPath();
				}
				return _systemPath;
			}
		}

		public static string VisionMasterPath => AppDomain.CurrentDomain.BaseDirectory;

		public static string GlobalScriptPath => VisionMasterPath + "GlobalScript";

		public static string GlobalScriptCustomPath => VisionMasterPath + "GlobalScript\\DLL";

		public static string ModuleScriptPath => VisionMasterPath + "Module(sp)\\x64\\Logic\\ScriptModule";

		public static string ModuleScriptCusotmPath => VisionMasterPath + "Module(sp)\\x64\\Logic\\ScriptModule\\DLL";

		public static string GetSystemPath()
		{
			string imageRuntimeVersion = Assembly.GetAssembly(typeof(object)).ImageRuntimeVersion;
			string text = "";
			text = ((IntPtr.Size == 8) ? ("%SYSTEMROOT%\\Microsoft.NET\\Framework64\\" + imageRuntimeVersion) : ("%SYSTEMROOT%\\Microsoft.NET\\Framework\\" + imageRuntimeVersion));
			return Environment.ExpandEnvironmentVariables(text);
		}

		public static bool CheckAssemly(string assemblyName)
		{
			return true;
		}
	}
}
