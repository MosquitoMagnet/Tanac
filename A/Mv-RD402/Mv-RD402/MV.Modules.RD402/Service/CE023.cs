using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using System.Xml;
using System.Linq;
using System.Xml.Linq;
using Mv.Core.Interfaces;
using Prism.Logging;

namespace Mv.Modules.RD402.Service
{
    public class CE023 : IGetSn
    {
        private readonly ILoggerFacade logger;

        private readonly IConfigureFile configureFile;
        private RD402Config _config;
        public CE023(ILoggerFacade logger, IConfigureFile configureFile)
        {
            this.logger = logger;
            this.configureFile = configureFile;
            this.configureFile.ValueChanged += _configure_ValueChanged;
            _config = configureFile.GetValue<RD402Config>("RD402Config");

        }
        public static string Post(string url, string postData)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);

            var data = Encoding.UTF8.GetBytes(postData);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            System.Net.ServicePointManager.DefaultConnectionLimit = 200;
            request.KeepAlive = false;
            request.ContentLength = data.Length;
            request.Timeout = 2000;

            using (var stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
                var response = (HttpWebResponse)request.GetResponse();
                var sr = response.GetResponseStream();
                var responseString = new StreamReader(sr).ReadToEnd();
                return responseString;
            }

        }
        public static string ParsToString(Hashtable Pars)
        {
            StringBuilder sb = new StringBuilder();
            foreach (string k in Pars.Keys)
            {
                if (sb.Length > 0)
                {
                    sb.Append("&");
                }
                sb.Append(HttpUtility.UrlEncode(k) + "=" + HttpUtility.UrlEncode(Pars[k].ToString()));
            }
            return sb.ToString();
        }

        public (bool, string) getsn(Hashtable hashtable)
        {
            try
            {
                if (hashtable == null)
                    throw new ArgumentNullException($"{nameof(getsn)}:hashtable cannot be  null");
                string postData = ParsToString(hashtable);
                string ret = Post(_config.GetsnUrl, postData);
                //string ret = Post("http://172.19.144.140/CE023/CE023.ASMX/GetCoilSN", postData);               
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
