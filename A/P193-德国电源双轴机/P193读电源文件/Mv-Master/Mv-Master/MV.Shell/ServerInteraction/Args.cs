namespace MV.Shell.ServerInteraction
{
    /// <summary>
    ///用户注册参数类
    /// </summary>
    public class SignUpArgs
    {
        public string SessionId { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        public string RootCode { get; set; }
    }
    /// <summary>
    ///用户登录参数类
    /// </summary>
    public class LoginArgs
    {
        public string UserName { get; set; }

        public string Password { get; set; }
    }
}
