using System.Collections.Generic;

namespace Mv.Modules.P99.Service
{
    public interface IAlarmService
    {
        List<AlarmItem> GetAlarmItems();
        void LoadAlarmInfos(string filePath = "./Alarms/Alarms.csv");
    }
}