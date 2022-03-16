using DataService;
using MaterialDesignThemes.Wpf;
using Mv.Modules.TagManager.ViewModels.Monitor;
using Mv.Modules.TagManager.Views;
using Mv.Modules.TagManager.Views.Monitor;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Mv.Modules.TagManager.ViewModels
{
    public class GroupMonitorViewModel : BindableBase, INavigationAware
    {

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
        }


        public ObservableCollection<TagItem> TagItems { get; set; } = new ObservableCollection<TagItem>();
        public IDriver Driver { get; set; }

        public ObservableCollection<IGroup> Groups => Driver != null ? new ObservableCollection<IGroup>(Driver.Groups) : null;

        private DelegateCommand<IGroup> _selectedChanged;
        public DelegateCommand<IGroup> SelectedChanged =>
            _selectedChanged ?? (_selectedChanged = new DelegateCommand<IGroup>(ExecuteSelectedChanged));

        void ExecuteSelectedChanged(IGroup group)
        {
            TagItems.Clear();
            if (group==null||group.Items == null)
                return;
            foreach (var item in group.Items)
            {
                TagItems.Add(new TagItem(item, item.GetTagName(), item.GetMetaData().Address));
            }
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {

            if (navigationContext.Parameters[nameof(IDriver)] is IDriver dv)
            {
                if (dv != null)
                {
                    Driver = dv;
                    this.RaisePropertyChanged(nameof(Groups));
                }
            }
        }
        public TagItem SelectedItem { get; set; }

        private DelegateCommand execEditTag;
        public DelegateCommand EditTagCommand =>
            execEditTag ?? (execEditTag = new DelegateCommand(async()=>await EditTagAsync()));

        async System.Threading.Tasks.Task EditTagAsync()
        {
            if (SelectedItem == null)
                return;
            var vm = new TagWriterViewModel(SelectedItem.Tag);
            var dlg = new TagWriter() { DataContext=vm};

            var result = await DialogHost.Show(dlg, "RootDialog");
            if (result.ToString() == "OK")
            {
               // if (realWrite)
                    SelectedItem.Write(vm.Value);
                //else
                //    SelectedItem.SimWrite(vm.Value);
            }
        }
       
    }
}
