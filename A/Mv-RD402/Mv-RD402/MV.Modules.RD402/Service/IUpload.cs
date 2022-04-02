using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using System.Xml;
using System.Linq;
using System.Xml.Linq;
using static Mv.Modules.RD402.Service.CE023;
using Mv.Core.Interfaces;
using Prism.Logging;

namespace Mv.Modules.RD402.Service
{
    public interface IUpload
    {
        (bool, string) Upload(string Spindle, string MatrixCode);
    }
    public class XinweiUpload : IUpload
    {
        private readonly ILoggerFacade logger;

        private readonly IConfigureFile configureFile;
        private RD402Config _config;
        public XinweiUpload(ILoggerFacade logger, IConfigureFile configureFile)
        {
            this.logger = logger;
            this.configureFile = configureFile;
            this.configureFile.ValueChanged += _configure_ValueChanged;
            _config= configureFile.GetValue<RD402Config>("RD402Config");

        }
        public (bool, string) Upload(string Spindle, string MatrixCode)
        {
   
            var hashtable = new Hashtable();
            hashtable["SN"] = MatrixCode;
            hashtable["attachSN"] = "";
            hashtable["lineNumber"] = _config.LineNumber;
            hashtable["station"] = _config.Station;
            hashtable["machineNO"] = _config.MachineNumber;
            hashtable["testData"] = "";
            hashtable["testResult"] = "PASS";
            hashtable["softwareVER"] = _config.SoftwareVER;
            hashtable["moName"] = _config.Mo;
            hashtable["coilWinding"] = _config.CoilWinding;
            hashtable["axis"] = Spindle;
            hashtable["fixtureNO"] = "";
            hashtable["cavityNO"] = "";
            hashtable["failItem"] = "";
            hashtable["userData"] = "";          
            return uploadsn(hashtable);
        }
        public (bool, string) uploadsn(Hashtable hashtable)
        {
            try
            {
                if (hashtable == null)
                    throw new ArgumentNullException($"{nameof(uploadsn)}:hashtable cannot be  null");
                string postData = ParsToString(hashtable);
                //string ret = Post("http://172.19.144.140/CE023/CE023.ASMX/TestDataUpload", postData);
                string ret = Post(_config.UploadUrl, postData);
                var document = new XmlDocument();
                document.LoadXml(ret);
                XmlNode root = document.LastChild;
                var nodeList = root.ChildNodes;
                if (nodeList.Count > 1 && int.TryParse(nodeList[0].InnerText, out int result))
                {
                    if (result != 0)
                        return (false, root.InnerText);
                    else
                    {
                        return (true, nodeList[1].InnerText);
                    }
                }
                else
                {
                    return (false, "Parse Error");
                }
            }
            catch (Exception e)
            {
                return (false, e.Message);
            }
        }
        private void _configure_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            if (e.KeyName != nameof(RD402Config)) return;
            var config = configureFile.GetValue<RD402Config>(nameof(RD402Config));
            _config = config;
           
        }
    }
    }
