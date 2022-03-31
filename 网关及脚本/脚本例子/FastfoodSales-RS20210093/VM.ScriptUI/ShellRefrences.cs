using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VM.ScriptUI
{
	[Serializable]
	public class ShellRefrences
	{
		public string Name { get; set; }

		public RefrencesType refrencesType { get; set; }

		public string FullName()
		{
			string text = "";
			switch (refrencesType)
			{
				case RefrencesType.System:
					text = RefrencesAssemblyManager.SyetemPath;
					break;
				case RefrencesType.GlobalScript:
					text = RefrencesAssemblyManager.GlobalScriptPath;
					break;
				case RefrencesType.GlobalScriptCustom:
					text = RefrencesAssemblyManager.GlobalScriptCustomPath;
					break;
				case RefrencesType.ModuleScript:
					text = RefrencesAssemblyManager.ModuleScriptPath;
					break;
				case RefrencesType.ModuleScriptCustom:
					text = RefrencesAssemblyManager.ModuleScriptCusotmPath;
					break;
			}
			return text + "\\" + Name;
		}
	}
}
