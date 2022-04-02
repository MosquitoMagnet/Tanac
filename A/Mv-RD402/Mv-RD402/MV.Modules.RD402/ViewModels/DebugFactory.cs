using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Mv.Core.Interfaces;
using Mv.Modules.RD402.Service;
using Unity;

namespace Mv.Modules.RD402.ViewModels
{
    public class DebugFactory : IFactoryInfo
    {
        private readonly IConfigureFile configure;
        private readonly IDeviceReadWriter device;
        private readonly IGetSn snGetter;
        private readonly RD402Config _config;

        public DebugFactory(IConfigureFile configure, IDeviceReadWriter device, IUnityContainer container)
        {
            this.configure = configure;
            this.device = device;
            this.snGetter = container.Resolve<IGetSn>("调试");
            _config = configure.Load().GetValue<RD402Config>(nameof(RD402Config));
        }
        public string GetSpindle(int value)
        {
            if (value >= 0 && value < 10)
                return value.ToString();
            return Chr(Encoding.ASCII.GetBytes("A")[0] + (value - 10));
        }
        string Chr(int asciiCode)
        {
            if (asciiCode >= 0 && asciiCode <= 255)
            {
                var asciiEncoding = new ASCIIEncoding();
                byte[] byteArray = { (byte)asciiCode };
                var strCharacter = asciiEncoding.GetString(byteArray);
                return strCharacter;
            }

            throw new Exception("ASCII code is not valid.");
        }

        public bool UploadFile(bool result, string Spindle, string MatrixCode)
        {
            var hashtable = new Dictionary<string, string>();
            hashtable["MI"] = "Sunway";
            hashtable["Station"] = _config.Station;
            hashtable["Project"] = _config.Project;
            hashtable["Stage"] = _config.Stage;
            hashtable["Model"] = _config.Model;
            hashtable["Config"] = _config.Config;
            hashtable["Test Time"] = DateTime.Now.ToString();
            hashtable["Fail Item"] = result ? "" : "Barcode NG";
            hashtable["Fixture"] = "";
            hashtable["Cavity"] = "";
            hashtable["Mandrel number"] = _config.MachineNumber;
            hashtable["Winding spindle"] = Spindle;
            hashtable["STC SN"] = MatrixCode.Split('+')[0];
            hashtable["Coil SN"] = "";
            hashtable["FG SN"] = "";
            return RD402Helper.SaveFile(Path.Combine(_config.FileDir, DateTime.Today.ToString("yyyy-MM-dd") + ".csv"), hashtable);
        }

        public (bool, string) GetSn()
        {
            var hashtable = new Hashtable();
            hashtable["lineNumber"] = _config.LineNumber;
            hashtable["station"] = _config.Station;
            hashtable["machineNO"] = _config.MachineNumber;
            hashtable["softwareVER"] = _config.SoftwareVER;
            hashtable["moName"] = _config.Mo;
            hashtable["coilWinding"] = _config.CoilWinding;
            hashtable["axis"] = GetSpindle(device.GetWord(1));
            return snGetter.getsn(hashtable);
        }

        public string GetBarcode(string MatrixCode, RD402Config config = null, int spindle = 0)
        {
            if (MatrixCode?.Length > 4)
                return MatrixCode.Substring(MatrixCode.Length - 4);
            else
                return "";
        }
    }
}