using System.Collections.Generic;
using System.ComponentModel;

namespace VM.ScriptUI
{
    public class ShellRun : INotifyPropertyChanged
	{
		public string CodeFileName { get; set; }
		public List<ShellRefrences> RefrencesList { get; set; }

		public event PropertyChangedEventHandler PropertyChanged;

		public void OnPropertyChanged(string name)
		{
			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
		}
		public void Clear()
		{
			CodeFileName = "";
		}
     }
}
