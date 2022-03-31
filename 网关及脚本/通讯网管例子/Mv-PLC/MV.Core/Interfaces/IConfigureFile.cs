using System;
using System.Threading.Tasks;

namespace Mv.Core.Interfaces
{

    public interface INonAuthenticationApi
    {
        Task<(int,string)> SignUpAsync(SignUpArgs args);
        Task<(int,string,MvRole)> LoginAsync(LoginArgs args);
        
    }
    public class MvUser
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public MvRole Role { get; set; }
    }

    public enum MvRole
    {
        User,
        Admin,
        Root
    }

    public class SignUpArgs
    {

        public string Username { get; set; }

        public string Password { get; set; }

        public string VerifyCode { get; set; }

        public MvRole Role { get; set; }
    }

    public class LoginArgs
    {
        public string UserName { get; set; }

        public string Password { get; set; }
    }
    public class ValueChangedEventArgs : EventArgs
    {
        public string KeyName { get; }

        public ValueChangedEventArgs(string keyName) => KeyName = keyName;
    }

    public interface IConfigureFile
    {
        event EventHandler<ValueChangedEventArgs> ValueChanged;

        bool Contains(string key);

        T GetValue<T>(string key);

        IConfigureFile SetValue<T>(string key, T value);

        IConfigureFile Load(string filePath = null);

        IConfigureFile Clear();

        void Delete();
    }

}

