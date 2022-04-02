using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Newtonsoft;


namespace Mv.Modules.P99.Hive.Services
{
    public class JsonHelper
    {
        private static object lock_write = new object();
        /// <summary>
        /// 创建json文件
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="json"></param>
        public static bool WriteJsonFile(string fileName, string json)
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
                        sw.Write(json);

                    }
                    else
                    {
                        sw = new StreamWriter(fileName, true, Encoding.UTF8);
                        sw.Write(json);

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
        }

        /// <summary>
        /// 获取json文件
        /// </summary>
        /// <param name="fileName"></param>      
        public static string GetJsonFile(string filepath)
        {
            string json = string.Empty;
            using (FileStream fs = new FileStream(filepath, FileMode.OpenOrCreate, System.IO.FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                using (StreamReader sr = new StreamReader(fs, Encoding.UTF8))
                {
                    json = sr.ReadToEnd().ToString();
                }
            }
            return json;
        }

    }
}
