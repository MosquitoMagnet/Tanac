namespace Mv.Modules.RD402.Service
{
    public class RD402Config
    {
        public string PrinterIpAddress { get; set; } = "127.0.0.1";
        //192.168.1.240
        public int PrinterPort { get; set; } = 5000;
        public string PLCIpAddress { get; set; } = "127.0.0.1";
        //192.168.1.10
        public int PLCPort { get; set; } = 3600;
        public string WriteAddrStart { get; set; } = "D3700";
        public string ReadAddrStart { get; set; } = "D3600";
        public ushort WriteLens { get; set; } = 50;
        public ushort ReadLens { get; set; } = 50;
        public string LineNo { get; set; } = "01";
        public string MachineCode { get; set; } = "1";
        public string WireVendor { get; set; } = "T";
        public string WireConfig { get; set; } = "E";

        public string SoftwareVER { get; set; } = "1.0.1";

        public string CoilWinding { get; set; } = "1";

        public string Station { get; set; } = "1";

        public string LineNumber { get; set; } = "BU21-B009";

        public string Mo { get; set; } = "H5109-200400087";

        public string FileDir { get; set; } = @"D:\machine";

        public string MachineNumber { get; set; } = "TZ-1";
       
        public string Factory { get; set; } = "ICT";//ICT 信维

        public string Project { get; set; } = "CE012";
        public string Stage { get; set; } = "EVT";
        public string Model { get; set; } = "D53";
        public string Config { get; set; } = "SW1";
        #region ICT二维码比对数据
        public string QRCheck { get; set; } = "0Y8V";//二维码比对
        public string VendorCode { get; set; } = "DLC";//工厂代码
        public string EECode { get; set; } = "0Y8V";//EECODE
        public string Revision { get; set; } = "1";//Apple版次代码
        public bool isQRCheck { get; set; } = false;//二维码比对
        #endregion
        public string WireVendorNameA { get; set; } = "东尼";
        public string WireVendorNameB { get; set; } = "赛特";
        public string WireVendorCodeA { get; set; } = "11";
        public string WireVendorCodeB { get; set; } = "22";
        public bool isWireVendorA { get; set; } = false;
        public bool isWireVendorB { get; set; } = false;

        #region 信维CE023接口

        public string GetsnUrl { get; set; }= "http://172.19.144.140/CE023/CE023.ASMX/GetCoilSN";
        public string UploadUrl { get; set; } = "http://172.19.144.140/CE023/CE023.ASMX/TestDataUpload";

        #endregion
    }
}
