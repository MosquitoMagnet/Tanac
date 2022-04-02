namespace Mv.Modules.P99.Service
{
    public interface IRunTimeService
    {
        double Downtime { get; }
        int GlueCameraNg { get; }
        double Idletime { get; }
        int LoadCameraNg { get; }
        int LoadCount { get; }
        double Looptime { get; }
        double Runtime { get; }
        int ScanCodeNg { get; }
        int UnloadCount { get; }
        double Worktime { get; }
    }
}