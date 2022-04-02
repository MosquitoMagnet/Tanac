using MaterialDesignThemes.Wpf;
using Mv.Core.Interfaces;
using Mv.Ui.Core;
using Mv.Ui.Mvvm;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using Unity;

namespace Mv.Shell.ViewModels.Authentication
{
    public class SignUpViewModel:BindableBase, INotificable
    {
        private SignUpArgs _signUpArgs=new SignUpArgs();

        public string Username
        {
            get => _signUpArgs.Username??"";
            set
            {
                var temp = _signUpArgs.Username;
                if (SetProperty(ref temp, value))
                {
                    _signUpArgs.Username = temp;
                }
            }
        }
        public string VerifyCode
        {
            get => _signUpArgs.VerifyCode ?? "";
            set
            {
                var temp = _signUpArgs.VerifyCode;
                if (SetProperty(ref temp, value))
                {
                    _signUpArgs.VerifyCode = temp;
                }
            }
        }
        IEventAggregator EventAggregator;
        IUnityContainer Container;
        public SignUpViewModel(IUnityContainer container)
        {
            SignUpCommand = new RelayCommand<PasswordBox>((m) => SignUpCommandExecute(m), (m) => SignUpCommandCanExecute(m));
            Container = container;
            EventAggregator = container.Resolve<IEventAggregator>();
        }


        public ICommand SignUpCommand { get; }

        public ISnackbarMessageQueue GlobalMessageQueue { get; set; }
        

        private bool SignUpCommandCanExecute(PasswordBox passwordBox) => new[]
        {
            Username,
            passwordBox.Password,
        }.All(field => !string.IsNullOrEmpty(field));

        private async void SignUpCommandExecute(PasswordBox passwordBox)
        {
            EventAggregator.GetEvent<MainWindowLoadingEvent>().Publish(true);
            var nonAuthApi = Container.Resolve<INonAuthenticationApi>();

            ConfigureSignUpArgs(configure: args => args.Password = passwordBox.Password);
             var  m=await nonAuthApi.SignUpAsync(_signUpArgs);
             if (m.Item1 == 1)
             {
                 passwordBox.Password = string.Empty;
                 GlobalMessageQueue.Enqueue("Registered successfully!");
                 EventAggregator.GetEvent<SignUpSuccessEvent>().Publish(_signUpArgs);
             }
             else
             {
                 GlobalMessageQueue.Enqueue(m.Item2);
             }
             EventAggregator.GetEvent<MainWindowLoadingEvent>().Publish(false);
        }

        private void ConfigureSignUpArgs(Action<SignUpArgs> configure = null, bool force = false)
        {
            if (_signUpArgs == null || force) _signUpArgs = new SignUpArgs();

            configure?.Invoke(_signUpArgs);
        }
    }
}
