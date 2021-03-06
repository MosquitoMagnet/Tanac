using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Mv.Modules.TagManager.Services
{
    public class CsvHelper
    {
        private static object lock_write = new object();
        public static bool WriteCsv(string fileName, Dictionary<string, string> hashtable)
        {
            lock (lock_write)
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
                    return false;
                }
            }
        }
    }
}
