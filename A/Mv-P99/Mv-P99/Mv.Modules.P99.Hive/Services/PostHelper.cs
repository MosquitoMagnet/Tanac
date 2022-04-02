using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net;
using RestSharp;

namespace Mv.Modules.P99.Hive.Services
{
    public class PostHelper
    {
        public static (bool, string) Post(string Url, string Data)
        {
            try
            {

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);
                request.Proxy = null;
                request.Method = "POST";
                request.Timeout = 20000;
                //request.Referer = Referer;
                byte[] bytes = Encoding.UTF8.GetBytes(Data);
                request.ContentType = "application/json";
                request.ContentLength = bytes.Length;
                Stream myResponseStream = request.GetRequestStream();
                myResponseStream.Write(bytes, 0, bytes.Length);
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                StreamReader myStreamReader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                string retString = myStreamReader.ReadToEnd();
                myStreamReader.Close();
                myResponseStream.Close();
                if (response != null)
                {
                    response.Close();
                }
                if (request != null)
                {
                    request.Abort();
                }
                return (true, retString);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
        /// <summary>
        /// Upload to HIVE server by http request
        /// </summary>
        /// <param name="url"></param>
        /// <param name="postData"></param>
        /// <param name="responseString"></param>
        /// <returns></returns>  
        public static bool HivePost(string url, string postData, ref string responseString)
        {

            #region restsharp
            try
            {
                var client = new RestClient(url);
                client.Timeout = 1000 * 5;
                var request = new RestRequest(Method.POST);
                request.AddHeader("Content-Type", "application/json");
                request.AddParameter("application/json", postData, ParameterType.RequestBody);
                IRestResponse response = client.Execute(request);
                responseString = response.Content;
            }
            catch (Exception ex)
            {
                responseString = ex.ToString();
                return false;
            }

            #endregion
            return true;
        }
    }
}
