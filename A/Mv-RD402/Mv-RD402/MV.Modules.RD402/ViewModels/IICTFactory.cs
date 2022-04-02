using Mv.Modules.RD402.Service;

namespace Mv.Modules.RD402.ViewModels
{
    public interface IFactoryInfo
    {
        string GetBarcode( string MatrixCode, RD402Config config = null, int spindle=0);
        string GetSpindle(int value);
        bool UploadFile(bool result, string Spindle, string MatrixCode);
        (bool, string) GetSn();
    }
}