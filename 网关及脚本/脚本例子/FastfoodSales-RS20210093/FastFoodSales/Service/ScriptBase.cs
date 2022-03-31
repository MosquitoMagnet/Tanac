using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Stylet;
using Script.Support;
using System.ComponentModel;
using System.Collections;
using StyletIoC;
using Script.Methods;

namespace DAQ.Service
{
    /// <summary>
    /// 脚本运行状态
    /// </summary>
    public enum ScriptState
    {
        Stop,
        Runing,
        Error,
        TestSuccessful,
        Testing,
        TestFail
    }
    public class ScriptBase : PropertyChangedBase
    {


        public TH2829Port TH2829  { get; set; }

        public TH9320PortA TH9320A { get; set; }

        public TH9320PortB TH9320B { get; set; }

        public TH9320PortC TH9320C { get; set; }

        public PlcService PLC { get; set; }

        public string Remarks { get; set; }
        public ScriptState State { get; set; } = ScriptState.Stop;
        public ScriptSupport objScriptSupport;
        public String ScriptCodePath => AppDomain.CurrentDomain.BaseDirectory+ $"ScriptModule\\ScriptCode\\UserScript_{Name}.cs";
        private bool m_ThreadStatus = false;
        private string strCompileError;
        /// <summary>
        /// 脚本代码内容
        /// </summary>
        public string InCode
        {
            get;
            set;

        } = "";
        public class RefrenceInfo
        {
            public string Name { get; set; }

            public string Type { get; set; }
        }
        public string Refrences
        {
            get;
            set;
        }
        private ArrayList arrayRefrences = new ArrayList();
        private List<RefrenceInfo> listRefrences = new List<RefrenceInfo>();
        public AutoResetEvent m_AutoResetEvent = new AutoResetEvent(false);
        public Thread m_Thread;
        public string Name { get; set; }
        public ScriptBase()
        {
            m_Thread = new Thread(Process);
            m_Thread.IsBackground = true;
            m_Thread.Start();
            objScriptSupport = new ScriptSupport();
        }
        ~ScriptBase()
        {
            try
            {
                m_Thread.Abort();
                m_AutoResetEvent.Dispose();

            }
            catch (Exception ex)
            {

            }
        }
        public void Start()
        {
            m_ThreadStatus = true;
            m_AutoResetEvent.Set();
            State = ScriptState.Runing;
        }
        public void Stop()
        {

            m_ThreadStatus = false;
            State = ScriptState.Stop;
        }
        public void Process()
        {
            while(true)
            {
                if(m_ThreadStatus==false)
                {
                    m_AutoResetEvent.WaitOne();//阻塞等待
                }
                else
                {
                    if (CodeRun())
                        State = ScriptState.Runing;
                    else
                        State = ScriptState.Error;
                    Thread.Sleep(1);
                }
            }
        }
        /// <summary>
        /// 编译脚本
        /// </summary>
        /// <returns></returns>
        public bool CompileCode(out string compileError)
        {
            try
            {
                Stop();//先把脚本停止运行
                ArrayList myResut;
                objScriptSupport.Compile(InCode, GetRefrences(), out myResut, false, Name);
                int num = 0;
                int num2 = 0;
                string text = "";
                string text2 = "";
                foreach (object item in myResut)
                {
                    CompileMessage hikCompileMessage = (CompileMessage)item;
                    if (hikCompileMessage.IsError)
                    {
                        num2++;
                        text2 += $"Line:{hikCompileMessage.Line} -- Error:{hikCompileMessage.Text}\r\n";
                    }
                    else
                    {
                        num++;
                        text2 += $"Line:{hikCompileMessage.Line} -- Warnings:{hikCompileMessage.Text}\r\n";
                    }
                }
                text = string.Format("{0} : Compile complete -- {1} Errors, {2} Warnings \r\n", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff"), num2, num);
                strCompileError = text + text2;
                compileError = strCompileError;
                if (num2 > 0)
                    return false;
                else
                    return true;
            }
            catch(Exception ex)
            {
                compileError= "编译失败:" + ex.Message;
                return false;
            }
        }
        public void LoadExternAssembly()
        {
           var myResut = new ArrayList();
            objScriptSupport.LoadExternAssembly(Name,out myResut);
        }
        /// <summary>
        /// 脚本运行
        /// </summary>
        /// <returns></returns>
        public bool CodeRun()
        {
            objScriptSupport.PLC = PLC;
            objScriptSupport.TH2829 = TH2829;
            objScriptSupport.TH9320A = TH9320A;
            objScriptSupport.TH9320B = TH9320B;
            objScriptSupport.TH9320C = TH9320C;
            return objScriptSupport.CodeRun();
        }
        /// <summary>
        /// 脚本运行一次
        /// </summary>
        /// <returns></returns>
        public bool TestRun()
        {
            try
            {
                Stop();
                State = ScriptState.Testing;
                if (CodeRun())
                    State = ScriptState.TestSuccessful;
                else
                    State = ScriptState.TestFail;
                return true;
            }
            catch(Exception ex)
            {
                return false;
            }
        }
        private ArrayList GetRefrences()
        {
            arrayRefrences.Clear();
            if (listRefrences.Count <= 0)
            {
                listRefrences.Add(new RefrenceInfo
                {
                    Name = "mscorlib.dll",
                    Type = "0"
                });
                listRefrences.Add(new RefrenceInfo
                {
                    Name = "System.dll",
                    Type = "0"
                });
                listRefrences.Add(new RefrenceInfo
                {
                    Name = "System.Core.dll",
                    Type = "0"
                });
                listRefrences.Add(new RefrenceInfo
                {
                    Name = "System.Windows.Forms.dll",
                    Type = "0"
                });
                listRefrences.Add(new RefrenceInfo
                {
                    Name = "Script.Methods.dll",
                    Type = "3"
                });
                listRefrences.Add(new RefrenceInfo
                {
                    Name = "Stylet.dll",
                    Type = "3"
                });
            }
            listRefrences.ForEach(delegate (RefrenceInfo x)
            {
                arrayRefrences.Add(x.Name);
            });
            return arrayRefrences;
        }
    }
}
