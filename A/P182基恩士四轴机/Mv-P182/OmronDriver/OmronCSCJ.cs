using System.IO;

namespace OmronDriver
{

    /// <summary>
    /// 欧姆龙Omron CS/CJ系列PLC数据区功能号和错误代码常数定义类
    /// </summary>
    public sealed class OmronCSCJ
    {
        /*****************************************************/
        //OMRON PLC CS/CJ系列数据区功能号
        /****************************************************/
        /// <summary>
        /// CIO 0000 to CIO 6143
        /// </summary>
        public const byte fctCIO = 0xB0;//CIO 0000 to CIO 6143
        /// <summary>
        /// H000 to H511
        /// </summary>
        public const byte fctHR = 0xB2;//H000 to H511
        /// <summary>
        /// A448 to A959
        /// </summary>
        public const byte fctA = 0xB3;//A448 to A959
        /// <summary>
        /// //D00000 to D32767
        /// </summary>
        public const byte fctDM = 0x82;//D00000 to D32767
        /// <summary>
        /// E0_E00000 to E0_E32765
        /// </summary>
        public const byte fctE0 = 0xA0;//E0_E00000 to E0_E32765
        /// <summary>
        /// E1_E00000 to E1_E32765
        /// </summary>
        public const byte fctE1 = 0xA1;//E1_E00000 to E1_E32765
        /// <summary>
        /// E2_E00000 to E2_E32765
        /// </summary>
        public const byte fctE2 = 0xA2;//E2_E00000 to E2_E32765
        /// <summary>
        /// E3_E00000 to E3_E32765
        /// </summary>
        public const byte fctE3 = 0xA3;
        /// <summary>
        /// E4_E00000 to E4_E32765
        /// </summary>
        public const byte fctE4 = 0xA4;//E4_E00000 to E4_E32765
        /// <summary>
        /// E5_E00000 to E5_E32765
        /// </summary>
        public const byte fctE5 = 0xA5;//E5_E00000 to E5_E32765
        /// <summary>
        /// E6_E00000 to E6_E32765
        /// </summary>
        public const byte fctE6 = 0xA6;//E6_E00000 to E6_E32765
        /// <summary>
        /// E7_E00000 to E7_E32765
        /// </summary>
        public const byte fctE7 = 0xA7;//E7_E00000 to E7_E32765
        /// <summary>
        /// E8_E00000 to E8_E32765
        /// </summary>
        public const byte fctE8 = 0xA8;//E8_E00000 to E8_E32765
        /// <summary>
        /// E9_E00000 to E9_E32765
        /// </summary>
        public const byte fctE9 = 0xA9;//E9_E00000 to E9_E32765
        /// <summary>
        /// EA_E00000 to EA_E32765
        /// </summary>
        public const byte fctEA = 0xAA;//EA_E00000 to EA_E32765
        /// <summary>
        /// EB_E00000 to EB_E32765
        /// </summary>
        public const byte fctEB = 0xAB;//EB_E00000 to EB_E32765
        /// <summary>
        /// EC_E00000 to EC_E32765
        /// </summary>
        public const byte fctEC = 0xAC;//EC_E00000 to EC_E32765
        /// <summary>
        /// E00000 to E32765
        /// </summary>
        public const byte fctCurrentbank = 98;//E00000 to E32765


        /// <summary>Constant for exception illegal function.</summary>
        public const byte excIllegalFunction = 1;
        /// <summary>Constant for exception illegal data address.</summary>
        public const byte excIllegalDataAdr = 2;
        /// <summary>Constant for exception illegal data value.</summary>
        public const byte excIllegalDataVal = 3;
        /// <summary>Constant for exception slave device failure.</summary>
        public const byte excSlaveDeviceFailure = 4;
        /// <summary>Constant for exception acknowledge.</summary>
        public const byte excAck = 5;
        /// <summary>Constant for exception slave is busy/booting up.</summary>
        public const byte excSlaveIsBusy = 6;
        /// <summary>Constant for exception gate path unavailable.</summary>
        public const byte excGatePathUnavailable = 10;
        /// <summary>Constant for exception not connected.</summary>
        public const byte excExceptionNotConnected = 253;
        /// <summary>Constant for exception connection lost.</summary>
        public const byte excExceptionConnectionLost = 254;
        /// <summary>Constant for exception response timeout.</summary>
        public const byte excExceptionTimeout = 255;
        /// <summary>Constant for exception wrong offset.</summary>
        public const byte excExceptionOffset = 128;
        /// <summary>Constant for exception send failt.</summary>
        public const byte excSendFailt = 100;
    }
}
