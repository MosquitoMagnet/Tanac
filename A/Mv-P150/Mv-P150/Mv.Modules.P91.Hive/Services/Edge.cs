using System;
using System.Collections.Generic;
using System.Text;

namespace Mv.Modules.P91.Hive.Services
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
    public enum Feedback
    {
        数据上传OK = 11,
        数据上传NG = 12
    }
    public enum TagNames
    {
        Save1,    //	A轴二维码保存
        Save2,    //	B轴二维码保存
        Save3,    //	C轴二维码保存
        Save4,    //	D轴二维码保存
        Save5,    //	D轴二维码保存
        State,//设备状态
        StateDT,
        DTRes,
        FMA,//A轴换模后使用次数
        FMB,//B轴换模后使用次数
        FMC,//C轴换模后使用次数
        FMD,//D轴换模后使用次数
        PMA,//A轴维修后使用次数
        PMB,//B轴维修后使用次数
        PMC,//C轴维修后使用次数
        PMD,//D轴维修后使用次数
        YRTA,//A轴预热温度
        YRTB,//B轴预热温度
        YRTC,//A轴预热温度
        YRTD,//B轴预热温度
        SXTA1,//A轴送线温度1
        SXTB1,//B轴预热温度1
        SXTC1,//C轴预热温度1
        SXTD1,//D轴预热温度1
        SXTA2,//A轴送线温度2
        SXTB2,//B轴预热温度2
        SXTC2,//C轴预热温度2
        SXTD2,//D轴预热温度2
        LMTA,//A轴下模温度
        LMTB,//B轴下模温度
        LMTC,//C轴下模温度
        LMTD,//D轴下模温度
        MG1A, //A轴Grap1速度
        MG2A,//A轴Grap2速度
        MG3A, //A轴Grap3速度
        MG1B,//B轴Grap1速度
        MG2B,//B轴Grap2速度
        MG3B,//B轴Grap3速度
        MG1C,//C轴Grap1速度
        MG2C,//C轴Grap2速度
        MG3C,//C轴Grap3速度
        MG1D,//D轴Grap1速度
        MG2D,//D轴Grap2速度
        MG3D,//D轴Grap3速度
        WS1A,//A轴第1段速度
        WS2A,//A轴第2段速度 
        WS3A,//A轴第3段速度
        WS1B,//B轴第1段速度
        WS2B,//B轴第2段速度
        WS3B,//B轴第3段速度
        WS1C,//C轴第1段速度
        WS2C,//C轴第2段速度
        WS3C,//C轴第3段速度
        WS1D,//D轴第1段速度
        WS2D,//D轴第2段速度
        WS3D,//D轴第3段速度
        WSBA,//A轴折弯速度
        WSBB,//B轴折弯速度 
        WSBC,//C轴折弯速度 
        WSBD,//D轴折弯速度 
        ISA,//A轴烫线速度
        ISB,//B轴烫线速度
        ISC,//C轴烫线速度
        ISD,//D轴烫线速度
        ZLA,//A轴张力值
        ZLB,//B轴张力值
        ZLC,//C轴张力值
        ZLD,//D轴张力值
        INTIME1,//产品1输入时间
        OUTTIME1,//产品1输出时间
        INTIME2,//产品2入时间
        OUTTIME2,//产品2输出时间
        INTIME3,//产品3入时间
        OUTTIME3,//产品3输出时间
        INTIME4,//产品4入时间
        OUTTIME4,//产品4输出时间
        Gettime//获取时间命令
    }
}
