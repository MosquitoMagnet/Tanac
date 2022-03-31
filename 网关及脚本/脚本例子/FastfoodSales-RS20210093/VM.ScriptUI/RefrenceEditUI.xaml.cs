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
using System.Windows.Forms;
using MahApps.Metro.Controls;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Windows.Markup;


namespace VM.ScriptUI
{
    /// <summary>
    /// RefrenceEditUI.xaml 的交互逻辑
    /// </summary>
    public partial class RefrenceEditUI : MetroWindow, INotifyPropertyChanged
	{
        private ObservableCollection<ShellRefrences> refrencesList = new ObservableCollection<ShellRefrences>();

        private ScriptType scriptType;
        public ObservableCollection<ShellRefrences> RefrencesList
        {
            get
            {
                return refrencesList;
            }
            set
            {
                refrencesList = value;
                OnPropertyChanged("RefrencesList");
            }
        }
        public RefrenceEditUI(List<ShellRefrences> shellRefrences, ScriptType type)
        {
            InitializeComponent();
            RefrencesList = new ObservableCollection<ShellRefrences>(shellRefrences);
            scriptType = type;
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            base.DialogResult = true;
            Close();
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            try
            {
                openFileDialog.InitialDirectory = RefrencesAssemblyManager.SyetemPath;
                openFileDialog.RestoreDirectory = false;
                openFileDialog.Multiselect = true;
                openFileDialog.Filter = "(*.dll)|*.dll";
                if (openFileDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK || openFileDialog.FileNames == null || openFileDialog.FileNames.Length == 0)
                {
                    return;
                }
                for (int i = 0; i < openFileDialog.FileNames.Length; i++)
                {
                    Path.GetFileName(openFileDialog.FileNames[i]);
                    if (!CheckRefrence(openFileDialog.FileNames[i]))
                    {
                        System.Windows.MessageBox.Show("添加程序集失败");
                    }
                }
            }
            finally
            {
                openFileDialog?.Dispose();
            }
        }
		private bool CheckRefrence(string refrencePath)
		{
			try
			{
				if (string.IsNullOrEmpty(refrencePath))
				{
					return false;
				}
				if (!IsCsharp(refrencePath))
				{
					return false;
				}
				ShellRefrences shellRefrences = null;
				string directoryName = Path.GetDirectoryName(refrencePath);
				string refrenceName = Path.GetFileName(refrencePath);
				bool flag = scriptType == ScriptType.GlobalScript;
				string text = (flag ? RefrencesAssemblyManager.GlobalScriptCustomPath : RefrencesAssemblyManager.ModuleScriptCusotmPath);
				string text2 = (flag ? RefrencesAssemblyManager.GlobalScriptPath : RefrencesAssemblyManager.ModuleScriptPath);
				if (directoryName == RefrencesAssemblyManager.SyetemPath)
				{
					shellRefrences = new ShellRefrences
					{
						Name = refrenceName,
						refrencesType = RefrencesType.System
					};
				}
				else if (directoryName == text2)
				{
					shellRefrences = new ShellRefrences
					{
						Name = refrenceName,
						refrencesType = (flag ? RefrencesType.GlobalScript : RefrencesType.ModuleScript)
					};
				}
				else if (directoryName == text)
				{
					shellRefrences = new ShellRefrences
					{
						Name = refrenceName,
						refrencesType = (flag ? RefrencesType.GlobalScriptCustom : RefrencesType.ModuleScriptCustom)
					};
				}
				else
				{
					string text3 = text + "\\" + refrenceName;
					if (File.Exists(text3))
					{
						File.Delete(text3);
					}
					File.Copy(refrencePath, text3);
					shellRefrences = new ShellRefrences
					{
						Name = refrenceName,
						refrencesType = (flag ? RefrencesType.GlobalScriptCustom : RefrencesType.ModuleScriptCustom)
					};
				}
				if (shellRefrences != null)
				{
					if (RefrencesList.FirstOrDefault((ShellRefrences x) => x.Name == refrenceName) == null)
					{
						RefrencesList.Add(shellRefrences);
					}
					return true;
				}
			}
			catch (Exception ex)
			{
				//LogHelper.objLog.Error("copy assembly is error," + ex.ToString());
			}
			return false;
		}
		private bool IsCsharp(string fullname)
		{
			try
			{
				AssemblyName.GetAssemblyName(fullname);
				return true;
			}
			catch (Exception ex)
			{
				//LogHelper.objLog.Error("load assembly is error," + ex.ToString());
				return false;
			}
		}
		private void BtnDelete_Click(object sender, RoutedEventArgs e)
		{
			ShellRefrences refrences = (sender as System.Windows.Controls.Button).DataContext as ShellRefrences;
			if (refrences != null)
			{
				ShellRefrences shellRefrences = RefrencesList.FirstOrDefault((ShellRefrences x) => x.Name == refrences.Name);
				if (shellRefrences != null)
				{
					RefrencesList.Remove(shellRefrences);
				}
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propname)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propname));
            }
        }
    }
}
