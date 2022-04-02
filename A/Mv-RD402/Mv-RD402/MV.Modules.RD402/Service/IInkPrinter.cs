using System.Threading.Tasks;

namespace Mv.Modules.RD402.Service
{
    public interface IInkPrinter
    {
        Task<bool> WritePrinterCodeAsync(string text);
        Task<bool> WritePrinterTextAsync(string text);
    }
}