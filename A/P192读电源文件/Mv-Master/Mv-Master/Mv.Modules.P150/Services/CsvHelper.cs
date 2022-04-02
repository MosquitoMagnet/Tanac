using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;


namespace Mv.Modules.P150.Services
{
    public class CsvHelper
    {
        public static bool WriteCsv(string fileName, Dictionary<string, string> hashtable)
        {
                try
                {
                    StreamWriter sw;

                    var dir = Path.GetDirectoryName(fileName);
                    if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                    if (!File.Exists(fileName))
                    {
                        sw = new StreamWriter(fileName, false, Encoding.UTF8);
                        var header = string.Join(',', hashtable.Keys).Trim(',') + Environment.NewLine;
                        var content = string.Join(',', hashtable.Values).Trim(',') + Environment.NewLine;
                        sw.Write(header + content);

                    }
                    else
                    {
                        sw = new StreamWriter(fileName, true, Encoding.UTF8);
                        var content = string.Join(',', hashtable.Values).Trim(',');
                        sw.Write(content + Environment.NewLine);

                    }
                    sw.Close();
                    return true;
                }
                catch (Exception ex)
                {
                    //    AddMsg(ex.Message);
                    return false;
                }           
        }
        public static bool Writelog(string fileName, string msg)
        {
            try
            {
                Dictionary<string, string> dictionary = new Dictionary<string, string>();
                dictionary["Time"] = DateTime.Now.ToString();
                var savemsg = msg.Replace("\r\n", "");
                dictionary["Description"] = savemsg;
                CsvHelper.WriteCsv(fileName, dictionary);

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

    }
}
