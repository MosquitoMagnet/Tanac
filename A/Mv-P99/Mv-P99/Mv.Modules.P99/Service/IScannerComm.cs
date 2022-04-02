using System.Threading.Tasks;

namespace Mv.Modules.P99.Service
{
    public interface IScannerComm
    {
        (bool, string) GetCodeAsync(int id, int timeout = 1000);
    }
}