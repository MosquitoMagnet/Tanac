using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.IO;
using VM.ScriptUI;
using DAQ.Service;
using System.Collections;
using System.ComponentModel;
using Newtonsoft.Json;

namespace DAQ.Pages
{
    /// <summary>
    /// ScriptEditorView.xaml 的交互逻辑
    /// </summary>
    public partial class ScriptEditorView : Window
    {
		public ScriptBase m_ScriptBase;
        public ScriptEditorView(ScriptBase scriptBase)
        {
            InitializeComponent();
			m_ScriptBase = scriptBase;
		}
		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			m_ScriptBase.Stop();
			textEditorControl.OpenFile(m_ScriptBase.ScriptCodePath);
		}
		private void btnInport_Click(object sender, RoutedEventArgs e)
        {
			Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
			openFileDialog.Filter = "(*.cs) | *.cs";
			if (openFileDialog.ShowDialog() != true)
			{
				return;
			}
			string fileName = openFileDialog.FileName;
			string text2 = fileName.Substring(fileName.LastIndexOf('.') + 1);
			if ("cs" != text2)
			{
				MessageBox.Show("脚本文件格式异常!");
				return;
			}
			try
			{
				FileInfo fileInfo = new FileInfo(fileName);
				if (fileInfo.Length > 20971520)
				{
					MessageBox.Show("文件过大!");
					return;
				}
				textEditorControl.Text = File.ReadAllText(fileName, Encoding.UTF8);
			}
			catch (Exception ex)
			{
				MessageBox.Show("脚本代码导入失败" + "!：" + ex.Message);
				return;
			}
			MessageBox.Show("脚本代码导入成功!");
		}

        private void BtnEditAssembly_Click(object sender, RoutedEventArgs e)
        {
			List<ShellRefrences> list = null;
			if (string.IsNullOrEmpty(m_ScriptBase.Refrences))
			{
				list = new List<ShellRefrences>();
				list.Add(new ShellRefrences
				{
					Name = "mscorlib.dll",
					refrencesType = RefrencesType.System
				});
				list.Add(new ShellRefrences
				{
					Name = "System.dll",
					refrencesType = RefrencesType.System
				});
				list.Add(new ShellRefrences
				{
					Name = "System.Core.dll",
					refrencesType = RefrencesType.System
				});
				list.Add(new ShellRefrences
				{
					Name = "Script.Methods.dll",
					refrencesType = RefrencesType.ModuleScript
				});
			}
			else
			{
				list = JsonConvert.DeserializeObject<List<ShellRefrences>>(m_ScriptBase.Refrences);
			}
			if (list == null)
			{
				return;
			}
			RefrenceEditUI refrenceEditUI = new RefrenceEditUI(list, ScriptType.ModuleScript);
			if (refrenceEditUI.ShowDialog() == true)
			{
				list = refrenceEditUI.RefrencesList.ToList();
				m_ScriptBase.Refrences = JsonConvert.SerializeObject(list);
				string text = textEditorControl.SetListAssembly(list);
				if (!string.IsNullOrEmpty(text))
				{
					MessageBox.Show("未找到程序集，请重新添加" + "\r\n" + text);
				}
			}
		}

        private void btnExport_Click(object sender, RoutedEventArgs e)
        {
			base.Dispatcher.Invoke(() =>
			{
				try
				{
					string text = textEditorControl.Text;
					if (string.IsNullOrWhiteSpace(text))
					{
						MessageBox.Show("请先编辑脚本代码!");
					}
					else
					{
						Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog();
						saveFileDialog.Filter = " Visual C# source files(*.cs)|*.cs";
						saveFileDialog.RestoreDirectory = true;
						if (saveFileDialog.ShowDialog() != true)
						{
							return;
						}
						try
						{
							string text3 = "temp.cs";
							using (TextWriter textWriter = File.CreateText(text3))
							{
								textWriter.Write(text);
								textWriter.Flush();
							}
							string fileName = saveFileDialog.FileName;
							if (File.Exists(fileName))
							{
								FileInfo fileInfo = new FileInfo(fileName);
								if (fileInfo.Attributes.ToString().IndexOf("ReadOnly") != -1)
								{
									fileInfo.Attributes = FileAttributes.Normal;
								}
								File.Delete(fileName);
							}
							string directoryName = System.IO.Path.GetDirectoryName(fileName);
							if (!string.IsNullOrEmpty(directoryName) && !Directory.Exists(directoryName))
							{
								Directory.CreateDirectory(directoryName);
							}
							File.Copy(text3, fileName, true);
							File.Delete(text3);
						}
						catch (Exception ex)
						{
							MessageBox.Show("脚本代码导出失败" + ":" + ex.Message);
							return;
						}
						MessageBox.Show("脚本代码导出成功");
					}
				}
				catch (Exception ex2)
				{

				}
			});
		}

        private void btnCompile_Click(object sender, RoutedEventArgs e)
        {
			m_ScriptBase.InCode = textEditorControl.Text;
			base.Dispatcher.Invoke(() =>
			{
				tbResultInfo.Clear();
			});

			 if(m_ScriptBase.CompileCode(out string msg))
				textEditorControl.SaveFile();
			base.Dispatcher.Invoke(() =>
			{
				tbResultInfo.Text = msg;
			});
		}

        private void btnExcute_Click(object sender, RoutedEventArgs e)
        {
			m_ScriptBase.TestRun();
        }

        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
			m_ScriptBase.Start();
			this.Close();
        }
    }
}
