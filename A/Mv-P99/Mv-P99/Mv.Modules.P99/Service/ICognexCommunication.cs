using System.Threading.Tasks;

namespace Mv.Modules.P99.Service
{
    public interface ICognexCommunication
    {
        Task<bool> CalibrationAsync(int id, int step, (double, double, double) position);
        Task<(bool, string, int, int, int, int,int,int,int,int)> TakePhotoAsync(int id,double x,double y);
        Task<(bool, string, double, double)> TrainCameraAsync(int id, (double, double, double, double) pos);
    }
}