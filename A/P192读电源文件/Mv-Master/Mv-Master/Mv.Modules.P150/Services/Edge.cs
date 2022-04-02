using System;
using System.Collections.Generic;
using System.Text;

namespace Mv.Modules.P150.Services
{
    public struct Edge
    {
        public bool ValueChanged { get; private set; }
        public string OldValue { get; private set; }
        private string _currentValue;
        public string CurrentValue
        {
            get => _currentValue;
            set
            {
                ValueChanged = ((_currentValue) != (value));
                OldValue = _currentValue;
                _currentValue = value;
            }
        }
    }
    public enum TagNames
    {
        Save1,    //	A轴二维码保存
        Save2,    //	B轴二维码保存
        Save3,    //	C轴二维码保存
        Save4,    //	D轴二维码保存
        SaveOK1,    //	A轴二维码保存OK
        SaveOK2,    //	B轴二维码保存OK
        SaveOK3,    //	C轴二维码保存OK
        SaveOK4,    //	D轴二维码保存OK
        State,//设备状态
        StateA,
        StateB,
        FMA,//A轴换模后使用次数D1021
        FMB,//B轴换模后使用次数D1025
        PMA,//A轴维修后使用次数D1023
        PMB,//B轴维修后使用次数D1027
        LMTA,//A轴下模温度
        LMTB,//B轴下模温度
        MG1A, //A轴Grap1速度D0010544 4
        MG2A,//A轴Grap2速度D0010554  4
        MG3A, //A轴Grap3速度D0010519 4
        MG1B,//B轴Grap1速度D0011144  4
        MG2B,//B轴Grap2速度D0011154  4
        MG3B,//B轴Grap3速度D0011119  4
        WS1A,//A轴第1段速度D0010424  2
        WS2A,//A轴第2段速度D0010434  2  
        WS3A,//A轴第3段速度D0010454  2
        WS1B,//B轴第1段速度D11024    2
        WS2B,//B轴第2段速度D11034    2
        WS3B,//B轴第3段速度D11054    2
        WSBA,//A轴折弯速度D0010444   2
        WSBB,//B轴折弯速度D0011044   2
        ISA,//A轴烫线速度D0010624    4
        ISB,//B轴烫线速度     4
        INTIMEA,//A轴输入时间     4
        OUTTIMEA,//A轴输出时间     4
        INTIMEB,//B轴输入时间     4
        OUTTIMEB,//B轴输出时间     4
        INDATEA,//B轴输入时间     4
        OUTDATEA,//B轴输出时间     4
        INDATEB,//B轴输入时间     4
        OUTDATEB,//B轴输出时间     4
        RstartA,
        CurrentA,//德国电源A轴电流
        VoltageA,//德国电源A轴电压
        PowerA,//德国电源A轴功率
        RendA,//德国电源A轴Rend
        BdTempA,//德国电源A轴Bonding_temp
        BdTimeA,//德国电源A轴Bonding_time
        ToolTempA,//德国电源A轴ToolTemp
        RC1A,//德国电源A轴RC1
        RC2A,//德国电源A轴RC2
        RstartB,
        CurrentB,
        VoltageB,
        PowerB,
        RendB,
        BdTempB,
        BdTimeB,
        ToolTempB,
        RC1B,
        RC2B,
        Hour,//写入的小时
        Min,//写入的分钟
        Sec,//写入的秒
        Gettime,//获取时间
        GetCT1,//获取A轴CT时间
        GetCT2,//获取B轴CT时间
    }
}
