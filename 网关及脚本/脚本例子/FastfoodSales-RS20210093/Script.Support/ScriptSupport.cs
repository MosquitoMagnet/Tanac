using System;
using System.Windows;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;
using Microsoft.CSharp;
using Script.Methods;
using System.Threading;

namespace Script.Support
{
    public class ScriptSupport
    {

        public PlcService PLC { get; set; }
        public TH2829Port TH2829 { get; set; }
        public TH9320PortA TH9320A { get; set; }
        public TH9320PortB TH9320B { get; set; }
        public TH9320PortC TH9320C { get; set; }

        private Type objType = null;
        private Assembly objAssembly = null;
        private IProcessMethods objiProcessClass;
        private object objectClass = null;

        /// <summary>
        /// 从编译器返回的编译结果
        /// </summary>
        private CompilerResults compilerResults = null;
        private string currentAssemblyName = "";
        private string currentAssemblyPdbName = "";
        private DateTime lastDateTime;
        public ArrayList ResultInfo;
        private CSharpCodeProvider objCSharpCodePrivoder = new CSharpCodeProvider();

        /// <summary>
        /// 脚本里需要用到那个类 就在这里注册 则该类同一个命名空间下的所有类都能使用 
        /// </summary>
        public ScriptSupport()
        {
            
        }
        /// <summary>
        /// 编译程序集
        /// </summary>
        /// <param name="source"></param>
        /// <param name="references"></param>
        /// <param name="myResut"></param>
        /// <param name="compileDebug"></param>
        /// <param name="nModuleid"></param>
        public void Compile(string source, ArrayList references, out ArrayList myResut, bool compileDebug, string name)
        {
            currentAssemblyName = $"ScriptModule\\UserScript_{name}.dll";
            currentAssemblyPdbName = $"ScriptModule\\UserScript_{name}.pdb";
            myResut = new ArrayList();
            if (null == references)
            {
                myResut.Add(new CompileMessage("程序编译参数异常", 0, 0, false));
                return;
            }
            try
            {
                objectDispose();
                compilerResults = null;
                if (File.Exists(currentAssemblyName))
                {
                    File.Delete(currentAssemblyName);
                }
                CompilerParameters compilerParameters = new CompilerParameters();
                compilerParameters.GenerateExecutable = false;
                compilerParameters.GenerateInMemory = false;
                compilerParameters.IncludeDebugInformation = false;
                compilerParameters.OutputAssembly = currentAssemblyName;
                compilerParameters.WarningLevel = 4;
                string text = AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "ScriptModule\\TEMP\\";
                if (Directory.Exists(text))
                {
                    compilerParameters.GenerateInMemory = false;
                    compilerParameters.TempFiles = new TempFileCollection(text, true);
                    compilerParameters.IncludeDebugInformation = true;
                    compilerParameters.TempFiles.KeepFiles = true;
                }
                foreach (object reference in references)
                {
                    string value = (string)reference;
                    compilerParameters.ReferencedAssemblies.Add(value);
                }
                string text2 = "/lib:";
                string text3 = AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "ScriptModule\\DLL";
                string compilerOptions = compilerParameters.CompilerOptions;
                compilerParameters.CompilerOptions = compilerOptions + " \"" + text2 + text3 + "\"";
                compilerParameters.CompilerOptions += " /unsafe";
                try
                {
                    compilerResults = objCSharpCodePrivoder.CompileAssemblyFromSource(compilerParameters, source);
                    if (File.Exists(currentAssemblyName))
                    {
                        FileInfo fileInfo = new FileInfo(AppDomain.CurrentDomain.SetupInformation.ApplicationBase + currentAssemblyName);
                        lastDateTime = fileInfo.LastWriteTime;
                    }
                    else
                    {
                        lastDateTime = DateTime.Now;
                    }
                }
                catch (Exception)
                {
                    myResut.Add(new CompileMessage("编译错误：可能未正确添加引用集", 0, 0, false));
                    return;
                }
                bool flag = false;
                for (int i = 0; i < compilerResults.Errors.Count; i++)
                {
                    if (!compilerResults.Errors[i].IsWarning)
                    {
                        flag = true;
                    }
                    CompileMessage value2 = new CompileMessage(compilerResults.Errors[i].ErrorText, compilerResults.Errors[i].Line, compilerResults.Errors[i].Column, compilerResults.Errors[i].IsWarning);
                    myResut.Add(value2);
                }
                if (!flag )
                {
                    LoadExternAssembly(name, out myResut);

                }
            }
            catch (Exception ex)
            {
                myResut.Add(new CompileMessage("编译错误：发生异常，" + ex.Message, 0, 0, false));
            }
        }
        public void objectDispose()
        {
            objiProcessClass = null;
            objectClass = null;
            objAssembly = null;
            GC.Collect();
        }
        private bool GetIsReLoadAssembly()
        {
            if (!File.Exists(currentAssemblyName))
            {
                return false;
            }
            return true;
        }
        /// <summary>
        /// 加载程序集
        /// </summary>
        /// <param name="updateCode"></param>
        /// <returns></returns>
        public bool LoadExternAssembly(string name, out ArrayList myResut)
        {
            currentAssemblyName = $"ScriptModule\\UserScript_{name}.dll";
            myResut = new ArrayList();
            try
            {
                if (!GetIsReLoadAssembly())
                {
                    myResut.Add(new CompileMessage("LanguageCompileCreateObjectError UserScript", 0, 0, false));
                    return false;
                }
                objectDispose();
                objAssembly = Assembly.Load(File.ReadAllBytes(currentAssemblyName));
                objType = objAssembly.GetType("UserScript");
                if (!(objType == null))
                {
                        objectClass = Activator.CreateInstance(objType);
                }

                if (!CheckClass(true, out myResut))
                {
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                myResut.Add(new CompileMessage("LanguageCompileCreateObjectError UserScript"+ex.Message, 0, 0, false));
                return false;
            }
        }

        public bool CheckClass(bool bold, out ArrayList myResut)
        {
            myResut = new ArrayList();
            if (objectClass == null)
            {
                myResut.Add(new CompileMessage("LanguageCompileCreateObjectError UserScript", 0, 0, false));
                objAssembly = null;
                objectClass = null;
                return false;
            }
            if (null == objectClass.GetType().GetMethod("Init"))
            {
                myResut.Add(new CompileMessage("LanguageCompileMissFunctionError Init()", 0, 0, false));
                objAssembly = null;
                objectClass = null;
                return false;
            }
            if (null == objectClass.GetType().GetMethod("Process"))
            {
                myResut.Add(new CompileMessage("LanguageCompileMissFunctionError Process()", 0, 0, false));
                objAssembly = null;
                objectClass = null;
                return false;
            }
            if (!bold)
            {
                objiProcessClass = (IProcessMethods)objectClass;
            }
            else
            {
                objiProcessClass = null;
            }
            try
            {
                if (objiProcessClass != null)
                {
                    objiProcessClass.Init();
                }
                else
                {
                    objectClass.GetType().InvokeMember("Init", BindingFlags.InvokeMethod, null, objectClass, null);
                }
            }
            catch (Exception ex)
            {
                myResut.Add(new CompileMessage("LanguageCompileExecuteError Init()，" + ex.Message, 0, 0, false));
                objAssembly = null;
                objectClass = null;
                return false;
            }
            return true;
        }

        /// <summary>
        /// 运行脚本
        /// </summary>
        /// <returns></returns>
        public bool CodeRun()
        {
            if(null==objectClass)
            {
                return false;
            }
           try
           {
                ((ScriptMethods)objectClass).PLC = PLC;
                ((ScriptMethods)objectClass).TH2829 = TH2829;
                ((ScriptMethods)objectClass).TH9320A = TH9320A;
                ((ScriptMethods)objectClass).TH9320B = TH9320B;
                ((ScriptMethods)objectClass).TH9320C = TH9320C;
                bool flag = (bool)this.objectClass.GetType().InvokeMember("Process", BindingFlags.InvokeMethod, null, this.objectClass, null);
                return flag;

            }
            catch(Exception ex)
            {
                return false;
            }
        }
    }
}
