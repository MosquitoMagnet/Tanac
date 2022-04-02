using Refit;
using System.Threading.Tasks;

namespace MV.Shell.ServerInteraction
{
    /// <summary>
    ///身份验证接口
    /// </summary>
    public interface INonAuthenticationApi
    {
        Task<bool> SignUpAsync(SignUpArgs args);
        Task<bool> LoginAsync(LoginArgs args);
    }

}
