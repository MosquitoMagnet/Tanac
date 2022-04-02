using Mv.Shell.Views.Dialogs;
using Mv.Ui.Mvvm;
using Prism.Commands;
using System.ComponentModel.DataAnnotations;
using System.Windows.Controls;
using Unity;
namespace Mv.Shell.ViewModels.Dialogs
{
    public class ProfileDialogViewModel : ViewModelValidateBase, IViewLoadedAndUnloadedAware<ProfileDialog>
    {


        public ProfileDialogViewModel(IUnityContainer container) : base(container) { }

        private string oldPassword;
        [StringLength(10, MinimumLength = 3, ErrorMessage = "password must have at least 3 characters")]
        public string OldPassword
        {
            get { return oldPassword; }
            set { SetProperty(ref oldPassword, value); }
        }
        private string newPassword;
        [StringLength(10, MinimumLength = 3, ErrorMessage = "password must have at least 3 characters")]
        public string NewPassword
        {
            get { return newPassword; }
            set { SetProperty(ref newPassword, value); }
        }
        private string newPasswordConform;
        [StringLength(10, MinimumLength = 3, ErrorMessage = "password must have at least 3 characters")]
        public string NewPasswordConfirm
        {
            get { return newPasswordConform ; }
            set { SetProperty(ref newPasswordConform, value); }
        }
        private DelegateCommand changePassword;
        public DelegateCommand ChangePassword =>
            changePassword ?? (changePassword = new DelegateCommand(async()=>await ExecuteCommandNameAsync(), CanExecuteCommandName));

        async System.Threading.Tasks.Task ExecuteCommandNameAsync()
        {
            (int, string) m = await this.MvUser.ChangePassword(oldPassword, newPassword);
            SnackbarMessageQueue.Enqueue(m.Item2);
        }

        bool CanExecuteCommandName()
        {
            if (string.IsNullOrEmpty(OldPassword) || string.IsNullOrEmpty(NewPassword))
                return false;
            if (OldPassword == NewPassword)
                return false;
            if(NewPassword != NewPasswordConfirm)
            {
                return false;
            }
            return true;
        }
        PasswordBox oldPasswordbox;
        PasswordBox newPasswordbox;
        PasswordBox newPasswordBoxConfirm;
        public void OnLoaded(ProfileDialog view)
        {
            oldPasswordbox = view.oldpassword;
            newPasswordbox = view.newpassword;
            newPasswordBoxConfirm = view.newpasswordconfirm;

            oldPasswordbox.PasswordChanged += Passwordbox_PasswordChanged;
            newPasswordbox.PasswordChanged += Passwordbox_PasswordChanged;
            newPasswordBoxConfirm.PasswordChanged += Passwordbox_PasswordChanged;
        }

        private void Passwordbox_PasswordChanged(object sender, System.Windows.RoutedEventArgs e)
        {
            var passwordBox = (PasswordBox)sender;
            if (passwordBox.Name == "oldpassword")
            {
                OldPassword = passwordBox.Password;
            }
            else if (passwordBox.Name == "newpassword")
            {
                NewPassword = passwordBox.Password;
            }
            else
            {
                NewPasswordConfirm = passwordBox.Password;
            }
            changePassword.RaiseCanExecuteChanged();
        }

        public void OnUnloaded(ProfileDialog view)
        {
         //   throw new System.NotImplementedException();
        }
    }
}
