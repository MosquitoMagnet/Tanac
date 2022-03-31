using System;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using MaterialDesignThemes.Wpf;
using Mv.Core.Interfaces;
using Mv.Shell.Constants;
using Mv.Shell.Views;
using Mv.Shell.Views.Authentication;
using Mv.Ui.Core;
using Mv.Ui.Mvvm;
using Refit;
using Unity;
using MvUser = Mv.Authentication.MvUserImpl;

namespace Mv.Shell.ViewModels.Authentication
{
    public class SignInViewModel : ViewModelBase, IViewLoadedAndUnloadedAware<SignInView>
    {
        private readonly INonAuthenticationApi _nonAuthenticationApi;

        private SignUpArgs _signUpArgs;
        private string _userName;
        private bool _isRememberPassword;
        private bool _isAutoSignIn;

        protected IConfigureFile ConfigureFile { get; }

        public string UserName
        {
            get => _userName;
            set => SetProperty(ref _userName, value);
        }

        public bool IsRememberPassword
        {
            get => _isRememberPassword;
            set { if (SetProperty(ref _isRememberPassword, value) && !value) IsAutoSignIn = false; }
        }

        public bool IsAutoSignIn
        {
            get => _isAutoSignIn;
            set { if (SetProperty(ref _isAutoSignIn, value) && value) IsRememberPassword = true; }
        }

        public ICommand SignInCommand { get; set; }
        private ISnackbarMessageQueue _queue;
        public SignInViewModel(IUnityContainer container) : base(container)
        {
          //  _nonAuthenticationApi = Container.Resolve<INonAuthenticationApi>();
            ConfigureFile = Container.Resolve<IConfigureFile>();
            _queue = container.Resolve<ISnackbarMessageQueue>();
            _nonAuthenticationApi = Container.Resolve<INonAuthenticationApi>();
            SignInCommand = new RelayCommand<PasswordBox>(SignInCommandExecute, passwordBox => CanSignIn(UserName, passwordBox.Password));

            EventAggregator.GetEvent<SignUpSuccessEvent>().Subscribe(signUpArgs => _signUpArgs = signUpArgs);
        }

        public void OnLoaded(SignInView view)
        {
            
            var passwordBox = view.PasswordBox;

            // 1. Login info from SignUpView
            if (_signUpArgs != null)
            {
                IsRememberPassword = false;
                IsAutoSignIn = false;
                UserName = _signUpArgs.Username;
                passwordBox.Password = _signUpArgs.Password;

                SignInCommand.Execute(passwordBox);
                _signUpArgs = null;
                return;
            }

            // 2. If there is some residual information on username or password text box, no login information is loaded from elsewhere.
            if (!string.IsNullOrEmpty(UserName) || !string.IsNullOrEmpty(passwordBox.Password)) return;

            // 3. No login info from config file.
            if (!CanSignIn(ConfigureFile.GetValue<string>(ConfigureKeys.Username), ConfigureFile.GetValue<string>(ConfigureKeys.Password))) return;

            // 4. Login info from config file.
            IsRememberPassword = true;
            IsAutoSignIn = ConfigureFile.GetValue<bool>(ConfigureKeys.AutoSignIn);
            UserName = ConfigureFile.GetValue<string>(ConfigureKeys.Username);
            // passwordBox.Password = ConfigureFile.GetValue<string>(ConfigureKeys.Password).DecryptByRijndael();
            passwordBox.Password = ConfigureFile.GetValue<string>(ConfigureKeys.Password); //有些系统获取系统信息会失败

            if (IsAutoSignIn)
            {
                SignInCommand.Execute(passwordBox);
            }
        }

        public void OnUnloaded(SignInView view)
        {
        }

        private async void SignInCommandExecute(PasswordBox password)
        {
            await SignInAsync(UserName, password.Password);
        }

        private async Task SignInAsync(string username, string password)
        {
            EventAggregator.GetEvent<MainWindowLoadingEvent>().Publish(true);

            var r = await AuthenticateAsync(username, password);
            if(r.Item1!=1)
            {
                EventAggregator.GetEvent<MainWindowLoadingEvent>().Publish(false);
                ConfigureFile.SetValue(ConfigureKeys.AutoSignIn, false);
                _queue.Enqueue(r.Item2);
                return;
            }

            Container.RegisterInstance<IMvUser>(new MvUser
            {
                Username = username,
                Role = r.Item3
            });
            
            // Saves data.
            ConfigureFile.SetValue(ConfigureKeys.Username, IsRememberPassword ? username : string.Empty);
            // ConfigureFile.SetValue(ConfigureKeys.Password, IsRememberPassword ? password.EncryptByRijndael() : string.Empty);
            ConfigureFile.SetValue(ConfigureKeys.Password, IsRememberPassword ? password: string.Empty);
            ConfigureFile.SetValue(ConfigureKeys.AutoSignIn, IsAutoSignIn);
            
            // Launches main window and closes itself.
            ShellSwitcher.Switch<AuthenticationWindow, MainWindow>();
        }

        private async Task<(int,string,MvRole)> AuthenticateAsync(string username, string passwordMd5)
        {
            var result = await _nonAuthenticationApi.LoginAsync(new LoginArgs
            {
                UserName = username,
                Password = passwordMd5
            });
            return result;
        }


        private static bool CanSignIn(string username, string password) => !string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password);
    }
}
