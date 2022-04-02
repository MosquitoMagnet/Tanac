using System.Threading.Tasks;

namespace MV.Shell.ServerInteraction
{
    /// <summary>
    ///身份验证类
    /// </summary>
    public class NonAuthenticationApi : INonAuthenticationApi
    {
        /// <summary>
        ///用户注册方法
        /// </summary>
        Task<bool> INonAuthenticationApi.SignUpAsync(SignUpArgs args)
        {
            return Task.FromResult(true);
        }
        /// <summary>
        ///用户登录方法
        /// </summary>
        Task<bool> INonAuthenticationApi.LoginAsync(LoginArgs args)
        {
            return Task.FromResult(true);
        }
    }

}
