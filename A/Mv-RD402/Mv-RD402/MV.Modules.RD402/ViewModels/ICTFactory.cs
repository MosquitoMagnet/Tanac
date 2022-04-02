using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Mv.Core.Interfaces;
using Mv.Modules.RD402.Service;
using Unity;

namespace Mv.Modules.RD402.ViewModels
{
    public class ICTFactory : IFactoryInfo
    {
        private readonly IConfigureFile configure;
        private readonly IGetSn snGetter;
        private IDeviceReadWriter _device;
        private RD402Config _config;

        public ICTFactory(IConfigureFile configure, IUnityContainer container)
        {
            this.configure = configure;
            this.snGetter = container.Resolve<IGetSn>("ICT");
            this._device = container.Resolve<IDeviceReadWriter>();
            _config = configure?.GetValue<RD402Config>(nameof(RD402Config));
        }

        public string GetSpindle(int value)
        {
            string content1 = "ABCD";
            if (value > 16 || value < 1)
                return value.ToString();
            return content1.Substring((value - 1) / 4, 1) + ((value - 1) % 4 + 1).ToString();
        }

        public bool UploadFile(bool result, string Spindle, string MatrixCode)
        {
            var hashtable = new Dictionary<string, string>();
            hashtable["SN"] = MatrixCode;
            hashtable["Time"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            hashtable["Machine_number"] = _config.MachineNumber;
            hashtable["Mandrel_number"] = Spindle;
            hashtable["Result"] = result ? "PASS" : "FAIL"; ;
            return RD402Helper.SaveFile(Path.Combine(_config.FileDir, MatrixCode+"_" +DateTime.Now.ToString("yyyyMMddHHmmss") + ".csv"), hashtable);
        }

        public string GetBarcode(string MatrixCode, RD402Config config = null, int spindle = 0)
        {
            string LineCode = "", Vendor = "", DayOfWeek = "", WireConfig = "";
            if (MatrixCode != null && MatrixCode.Length > 21)
            {
                LineCode = "0" + MatrixCode.Substring(18, 1);
                Vendor = MatrixCode.Substring(19, 1);
                DayOfWeek = MatrixCode.Substring(6, 1);
                WireConfig = MatrixCode.Substring(21, 1);
            }
            return $"{LineCode}{config?.MachineCode}{GetSpindle(spindle)}{DayOfWeek}{Vendor}{WireConfig}";
        }

        public (bool, string) GetSn()
        {
            var hashtable = new Hashtable();
            hashtable["p"] = "P106_PrintSN";
            hashtable["c"] = "QUERY_RECORD";
            hashtable["sn"] = _config.Mo;
            hashtable["tsid"] = _config.LineNumber;
            return snGetter.getsn(hashtable);
        }
    }
}