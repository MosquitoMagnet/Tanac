using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.IO;
using System.Diagnostics;
using System.Threading.Tasks;
namespace Script.Methods
{
    public class ScriptMethods
    {
        public TH2829Port TH2829;
        public TH9320PortA TH9320A;
        public TH9320PortB TH9320B;
        public TH9320PortC TH9320C;

        public PlcService PLC;

        public struct BitEdge
        {
            public bool BitChanged { get; private set; }
            private bool _currentValue;
            public bool CurrentValue
            {
                get => _currentValue;
                set
                {
                    BitChanged = ((_currentValue) != (value));
                    _currentValue = value;
                }
            }
        }

        /// <summary>
        /// 将当前线程挂起指定的毫秒数
        /// </summary>
        /// <param name="millisecondsTimeout"></param>
        public static void Sleep(int millisecondsTimeout)
        {
            Thread.Sleep(millisecondsTimeout);
        }
        /// <summary>
        /// 弹窗显示
        /// </summary>
        /// <param name="msg"></param>S
        public static void ShowMessageBox(string msg)
        {
            MessageBox.Show(msg);
        }
        /// <summary>
        /// 保存文件
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="hashtable"></param>
        /// <returns></returns>
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
                    string header = "";
                    string content = "";
                    foreach (var key in hashtable.Keys)
                    {
                        if (key.Contains(","))
                        {
                            header+=","+ "\"" + key + "\"";
                        }
                        else
                        {
                            header += "," + key;
                        }
                    }
                    foreach (var value in hashtable.Values)
                    {
                        if (value.Contains(","))
                        {
                            content += "," + "\"" + value + "\"";                          
                        }
                        else
                        {
                            content += "," + value;
                        }
                    }
                     header = header.Trim(',') + Environment.NewLine;
                     content = content.Trim(',') + Environment.NewLine;
                    sw.Write(header + content);

                }
                else
                {
                    sw = new StreamWriter(fileName, true, Encoding.UTF8);
                    string content = "";
                    foreach (var value in hashtable.Values)
                    {
                        if (value.Contains(","))
                        {
                            content += "," + "\"" + value + "\"";
                        }
                        else
                        {
                            content += "," + value;
                        }
                    }
                    content = content.Trim(',');
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

        public static void LogError(string message)
        {
            LogHelper.Error(message);
        }
        public static void LogInfo(string message)
        {
            LogHelper.Info(message);
        }
    }
}
