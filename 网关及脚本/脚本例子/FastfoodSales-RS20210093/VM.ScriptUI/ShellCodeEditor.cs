using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.IO;
using System.Windows.Controls;
using System.Reflection;
using VM.CodeCompletion;
using ICSharpCode.AvalonEdit.Indentation.CSharp;

namespace VM.ScriptUI
{
   public class ShellCodeEditor:CodeTextEditor
   {
        private List<Assembly> objListAssembly;

		public static string VisionMasterPath => AppDomain.CurrentDomain.BaseDirectory;

		private List<string> objListAssmblyName;

        public ShellCodeEditor()
        {
			base.Completion = new CSharpCompletion(GetAssemblies());
			base.TextArea.IndentationStrategy = new CSharpIndentationStrategy(base.Options);
			ContextMenu val = new ContextMenu();
            val.Items.Add(new MenuItem { Command=ApplicationCommands.Copy });
            val.Items.Add(new MenuItem { Command = ApplicationCommands.Cut });
            val.Items.Add(new MenuItem { Command = ApplicationCommands.Paste });
            val.Items.Add(new MenuItem { Command = ApplicationCommands.Undo });
            val.Items.Add(new MenuItem { Command = ApplicationCommands.Redo });
            val.Items.Add(new MenuItem { Command = cmdCtrlSpace,Header="智能感知" });
			base.TextArea.ContextMenu = val;
		}
		public List<Assembly> GetAssemblies()
		{
			if (objListAssembly != null && objListAssembly.Count > 0)
			{
				return objListAssembly;
			}
			if (objListAssembly == null)
			{
				objListAssembly = new List<Assembly>();
			}
			objListAssembly.Clear();
			objListAssembly = new List<Assembly>
		    {
			typeof(object).Assembly,//引用mscorlib.dll
			typeof(Uri).Assembly,//引用system.dll
			typeof(Enumerable).Assembly,//引用system.core.dll
			typeof(System.Windows.MessageBox).Assembly,
			};
			Assembly item = Assembly.LoadFrom(AppDomain.CurrentDomain.BaseDirectory + "Script.Methods.dll");
			objListAssembly.Add(item);
			if (objListAssmblyName == null)
			{
				objListAssmblyName = new List<string>();
			}
			objListAssmblyName.Clear();
			objListAssembly.ForEach((x)=>
			{
				objListAssmblyName.Add(x.FullName.Substring(0, x.FullName.IndexOf(",")) + ".dll");
			});
			return objListAssembly;
		}
		public string SetListAssembly(List<ShellRefrences> assembilesName)
		{
			string text = string.Empty;
			if (assembilesName == null || assembilesName.Count < 1)
			{
				return text;
			}
			if (objListAssembly == null)
			{
				objListAssembly = new List<Assembly>();
			}
			if (objListAssmblyName == null)
			{
				objListAssmblyName = new List<string>();
			}
			objListAssembly.Clear();
			objListAssmblyName.Clear();
			List<string> list = new List<string>();
			for (int i = 0; i < assembilesName.Count; i++)
			{
				if (string.IsNullOrEmpty(assembilesName[i].Name))
				{
					continue;
				}
				bool flag = false;
				string text2 = assembilesName[i].FullName();
				if (assembilesName[i].refrencesType == RefrencesType.GlobalScriptCustom)
				{
					if (File.Exists(text2))
					{
						list.Add(text2);
					}
					else
					{
						text = text + assembilesName[i].Name + "\r\n";
					}
				}
				else
				{
					flag = true;
				}
				if (flag)
				{
					Assembly item = Assembly.LoadFrom(text2);
					objListAssembly.Add(item);
				}
				objListAssmblyName.Add(assembilesName[i].Name);
			}
			base.Completion = new CSharpCompletion(objListAssembly);
			list.ForEach(delegate (string x)
			{
				base.Completion.AddAssembly(x);
			});
			return text;
		}
		public bool UpdateAssembile(ShellRefrences[] assembilesName)
		{
			try
			{
				if (assembilesName == null || assembilesName.Length < 1)
				{
					return false;
				}
				List<Assembly> list = new List<Assembly>();
				List<string> list2 = new List<string>();
				for (int i = 0; i < assembilesName.Length; i++)
				{
					if(string.IsNullOrEmpty(assembilesName[i].Name))
                    {
						continue;
                    }
					bool flag = false;
					string text = assembilesName[i].FullName();
					if (assembilesName[i].refrencesType == RefrencesType.ModuleScriptCustom)
					{
						if (File.Exists(text))
						{
							list2.Add(text);
						}
					}
					else
					{
						flag = true;
					}
					if (flag)
					{
						Assembly item = Assembly.LoadFrom(text);
						list.Add(item);
					}

				}
				base.Completion = new CSharpCompletion(list);
				list2.ForEach((x) => { base.Completion.AddAssembly(x); });
			}
			catch (Exception)
			{
			}
			return true;
		}
	}
}
