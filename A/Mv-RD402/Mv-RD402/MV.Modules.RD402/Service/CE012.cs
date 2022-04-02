using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using System.Xml;
using System.Linq;
using System.Xml.Linq;

namespace Mv.Modules.RD402.Service
{
    public class CE012 : IGetSn
    {

        public static string Post(string url, string postData)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);

            var data = Encoding.ASCII.GetBytes(postData);         
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            System.Net.ServicePointManager.DefaultConnectionLimit = 200;
            request.KeepAlive = false;
            request.ContentLength = data.Length;
            request.Timeout = 2000;
            request.UseDefaultCredentials = true;

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
                string ret = Post("http://172.19.144.106:8011/CE023.asmx/GetSTCCoilSN", postData);             
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
    }
}