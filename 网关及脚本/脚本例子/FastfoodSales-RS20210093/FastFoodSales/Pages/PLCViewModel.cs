using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StyletIoC;
using DAQ.Service;
using MaterialDesignThemes.Wpf;
using Stylet;
using Script.Methods;
using System.Windows;
namespace DAQ.Pages
{
    public class PLCViewModel : Screen
    {
        public PlcService PLC { get; set; }
        [Inject]
        public ScriptMgr Scripts { get; set; }

        protected override void OnViewLoaded()
        {
            base.OnViewLoaded();
        }
        public void SetBit(KV<bool> kv)
        {
             PLC.SetRregBit(kv.Key, !kv.Value);
        }

        public async System.Threading.Tasks.Task SetShortAsync()
       {
            if (ShortItem == null)
                return;
            var vm = new TagWriterViewModel(ShortItem.Key,ShortItem.Value.ToString());
            var dlg = new TagWriter() { DataContext=vm };
            var result = await DialogHost.Show(dlg);
            if (result.ToString() == "OK")
            {

                PLC.SetRregShort(ShortItem.Key, short.Parse(vm.Value));

            }
        }
        public async Task SetIntAsync()
        {
            if (IntItem == null)
                return;
            var vm = new TagWriterViewModel(IntItem.Key, IntItem.Value.ToString());
            var dlg = new TagWriter() { DataContext = vm };
            var result = await DialogHost.Show(dlg);
            if (result.ToString() == "OK")
            {

                PLC.SetRregInt(IntItem.Key, int.Parse(vm.Value));

            }
        }
        public async Task SetFloatAsync()
        {
            if (FloatItem == null)
                return;
            var vm = new TagWriterViewModel(FloatItem.Key, FloatItem.Value.ToString());
            var dlg = new TagWriter() { DataContext = vm };
            var result = await DialogHost.Show(dlg);
            if (result.ToString() == "OK")
            {

                PLC.SetRregFloat(FloatItem.Key, float.Parse(vm.Value));

            }
        }
        public async Task SetStringAsync()
        {
            if (StringItem == null)
                return;
            var vm = new TagWriterViewModel(StringItem.Key, StringItem.Value.ToString());
            var dlg = new TagWriter() { DataContext = vm };
            var result = await DialogHost.Show(dlg);
            if (result.ToString() == "OK")
            {

                PLC.SetRregString(StringItem.Key, vm.Value);

            }
        }

        public void EditScript()
        {
            if(!MainWindowViewModel.authbool)
            {
                MessageBox.Show("无操作权限");
                return;
            }

            if (ScriptItem == null)
                return;
            ScriptEditorView scriptEditorView = new ScriptEditorView(ScriptItem);
            scriptEditorView.ShowDialog();
        }

        public ScriptBase ScriptItem
        {
            get;
            set;
        }


        public KV<short> ShortItem
        { 
          get; 
          set; 
        }
        public KV<int> IntItem
        {
            get;
            set;
        }
        public KV<float> FloatItem
        {
            get;
            set;
        }
        public KV<string> StringItem
        {
            get;
            set;
        }
        public PLCViewModel(PlcService PLC)
        {
            this.PLC = PLC;
        }
        public PLCViewModel()
        {

        }
    }
}



