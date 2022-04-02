namespace Mv.Modules.P99.Service
{
    public interface IEpson2Cognex
    {
        bool ClientConnected { get; }
        bool ServerConnected { get; }
    }
}