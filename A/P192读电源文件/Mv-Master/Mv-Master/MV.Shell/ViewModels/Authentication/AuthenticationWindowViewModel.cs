﻿using System.Windows.Controls;
using MaterialDesignThemes.Wpf;
using Mv.Shell.Views.Authentication;
using Mv.Ui.Mvvm;
using Unity;

namespace Mv.Shell.ViewModels.Authentication
{
    public class AuthenticationWindowViewModel : ViewModelBase, IViewLoadedAndUnloadedAware<AuthenticationWindow>, INotificable
    {
        private bool _isLoading;
        private TabItem _signInTabItem;


        public AuthenticationWindowViewModel(IUnityContainer container) : base(container)
        {
            this.GlobalMessageQueue = container.Resolve<ISnackbarMessageQueue>();
            EventAggregator.GetEvent<MainWindowLoadingEvent>().Subscribe(e => IsLoading = e);
            EventAggregator.GetEvent<SignUpSuccessEvent>().Subscribe(signUpInfo => _signInTabItem.IsSelected = true); // It cannot be done by binding the IsSelected property, it will cause an animation error.
            /*
             * There are some puzzle here:
             * 1. If SignInTabItem.IsSelected is not directly set to "True" (e.g. Set value by Binding), a style error will occur in AuthenticationWindow UI;
             * 2. If SignInTabItem.IsSelected is first set to "True" in Xaml, then binding to a property, (e.g. SetBinding(TabItem.IsSelectedProperty, XXX) in OnLoaded or code behind)
             *    which can avoid the error in 1. But an animation error will occur when loading SignInView every time.
             */
        }


        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public void OnLoaded(AuthenticationWindow view)
        {
            _signInTabItem = view.SignInTabItem;
        }

        public void OnUnloaded(AuthenticationWindow view) { }
        public ISnackbarMessageQueue GlobalMessageQueue { get; set; }
    }
}
