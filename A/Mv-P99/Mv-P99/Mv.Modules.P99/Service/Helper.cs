using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Mv.Modules.P99.Service
{
    public static class Helper
    {
        public static long GetTimeStamp()
        {
            DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1, 0, 0, 0, 0));
            DateTime nowTime = DateTime.Now;
            long unixTime = (long)Math.Round((nowTime - startTime).TotalMilliseconds, MidpointRounding.AwayFromZero);
            return unixTime;
        }


        public static bool SaveFile(string fileName, IEnumerable<Dictionary<string, string>> dictionarys)
        {
            try
            {
                var dir = Path.GetDirectoryName(fileName);
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                var hashtable = dictionarys.First();
 
                StringBuilder stringBuilder = new StringBuilder();
                if (!File.Exists(fileName))
                {
                    stringBuilder.Append(string.Join(',', hashtable.Keys).Trim(',') + Environment.NewLine);
                }
                foreach (var dic in dictionarys)
                {
                    stringBuilder.Append(string.Join(',', dic.Values).Trim(',')+Environment.NewLine);
                }
             
                File.AppendAllText(fileName, stringBuilder.ToString() + Environment.NewLine);

                return true;
            }
            catch (Exception ex)
            {
                //    AddMsg(ex.Message);
                return false;
            }
        }

        public static bool SaveFile(string fileName, Dictionary<string, string> hashtable)
        {
            try
            {
                var dir = Path.GetDirectoryName(fileName);
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                if (!File.Exists(fileName))
                {
                    var header = string.Join(',', hashtable.Keys).Trim(',') + Environment.NewLine;
                    var content = string.Join(',', hashtable.Values).Trim(',') + Environment.NewLine;
                    File.AppendAllText(fileName, header + content);
                }
                else
                {
                    var content = string.Join(',', hashtable.Values).Trim(',');
                    File.AppendAllText(fileName, content + Environment.NewLine);
                }
                return true;
            }
            catch (Exception ex)
            {
                //    AddMsg(ex.Message);
                return false;
            }
        }
    }
}