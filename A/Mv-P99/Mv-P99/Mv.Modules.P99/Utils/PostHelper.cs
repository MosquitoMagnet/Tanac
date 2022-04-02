using System.Collections;
using System.IO;
using System.Net;
using System.Text;
using System.Web;

namespace Mv.Modules.P99.Utils
{
	public static class PostHelper
	{
		private static string Post(string url, string postData)
		{
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
			byte[] data = Encoding.ASCII.GetBytes(postData);
			request.Method = "POST";
			request.ContentType = "application/x-www-form-urlencoded";
			request.ContentLength = data.Length;
			request.Timeout = 2000;
			using (Stream stream = request.GetRequestStream())
			{
				stream.Write(data, 0, data.Length);
			}
			HttpWebResponse response = (HttpWebResponse)request.GetResponse();
			return new StreamReader(response.GetResponseStream()).ReadToEnd();
		}

		public static string ParsToString(Hashtable Pars)
		{
			StringBuilder sb = new StringBuilder();
			foreach (string i in Pars.Keys)
			{
				if (sb.Length > 0)
				{
					sb.Append("&");
				}
				sb.Append(HttpUtility.UrlEncode(i) + "=" + HttpUtility.UrlEncode(Pars[i]!.ToString()));
			}
			return sb.ToString();
		}

		public static string Post(string url, Hashtable data)
		{
			string dataString = ParsToString(data);
			return Post(url, dataString);
		}
	}
}
