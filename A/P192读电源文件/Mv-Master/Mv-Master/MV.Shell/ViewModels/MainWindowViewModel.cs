using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;
using MaterialDesignThemes.Wpf;
using Mv.Ui.Core;
using Mv.Ui.Mvvm;
using Prism.Regions;
using Unity;

namespace Mv.Shell.ViewModels
{
    public class MainWindowViewModel : ViewModelBase, IViewLoadedAndUnloadedAware, INotificable
    {
        private ICommand _feedbackCommand;
        private bool _appStoreIsDisplayed;

        public ISnackbarMessageQueue GlobalMessageQueue { get; set; }

        public MainWindowViewModel(IUnityContainer container, IRegionManager regionManager) : base(container)
        {
            RegionManager = regionManager;
            FeedbackCommand = new RelayCommand(() => Process.Start("http://www.baidu.com"));
        }

        public IRegionManager RegionManager { get; }

        public ICommand FeedbackCommand
        {
            get => _feedbackCommand;
            set => SetProperty(ref _feedbackCommand, value);
        }

        public bool AppStoreIsDisplayed
        {
            get => _appStoreIsDisplayed;
            set
            {
                if (_appStoreIsDisplayed) return;
                if (!SetProperty(ref _appStoreIsDisplayed, value)) return;

                var region = RegionManager.Regions[RegionNames.MainTabRegion];
                foreach (var activeView in region.ActiveViews)
                {
                    region.Deactivate(activeView);
                }
            }
        }

        public void OnLoaded()
        {
            var region = RegionManager.Regions[RegionNames.MainTabRegion];
            region.ActiveViews.CollectionChanged += OnActiveViewsChanged;
            if (!region.Views.Any()) AppStoreIsDisplayed = true;

            GlobalMessageQueue.Enqueue("Welcome!");
        }

        public void OnUnloaded()
        {
        }

        private void OnActiveViewsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action != NotifyCollectionChangedAction.Add) return;

            _appStoreIsDisplayed = false;
            RaisePropertyChanged(nameof(AppStoreIsDisplayed));
        }
    }
}
