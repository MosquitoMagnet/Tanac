using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stylet;
using StyletIoC;
using Script.Methods;

namespace DAQ.Service
{
   public class ScriptMgr : PropertyChangedBase
    {


        public TH2829Port TH2829 { get; set; }

        public TH9320PortA TH9320A { get; set; }

        public TH9320PortB TH9320B { get; set; }

        public TH9320PortC TH9320C { get; set; }

        public PlcService PLC { get; set; }


        protected IEventAggregator Events { get; set; }

        public BindableCollection<ScriptBase> ScriptBaseList { get; set; } = new BindableCollection<ScriptBase>();

        public ScriptMgr(IEventAggregator events,PlcService plc,TH2829Port tH2829Port, TH9320PortA tH9320PortA, TH9320PortB tH9320PortB, TH9320PortC tH9320PortC)
        {
            Events = events;
            PLC = plc;
            TH2829 = tH2829Port;
            TH9320A = tH9320PortA;
            TH9320B = tH9320PortB;
            TH9320C = tH9320PortC;
            ScriptBaseList.Add(new ScriptBase { Name="脚本1",Remarks="高压测试1脚本",PLC=this.PLC,TH2829=this.TH2829,TH9320A=this.TH9320A, TH9320B = this.TH9320B, TH9320C= this.TH9320C });
            ScriptBaseList.Add(new ScriptBase { Name = "脚本2", Remarks = "高压测试2脚本", PLC = this.PLC, TH2829 = this.TH2829, TH9320A = this.TH9320A, TH9320B = this.TH9320B, TH9320C = this.TH9320C });
            ScriptBaseList.Add(new ScriptBase { Name = "脚本3", Remarks = "高压测试3脚本", PLC = this.PLC, TH2829 = this.TH2829, TH9320A = this.TH9320A, TH9320B = this.TH9320B, TH9320C = this.TH9320C });
            ScriptBaseList.Add(new ScriptBase { Name = "脚本4", Remarks = "电性能测试脚本", PLC = this.PLC, TH2829 = this.TH2829, TH9320A = this.TH9320A, TH9320B = this.TH9320B, TH9320C = this.TH9320C });
            ScriptBaseList.Add(new ScriptBase { Name = "脚本5", Remarks = "CCD尺寸检测1脚本", PLC = this.PLC, TH2829 = this.TH2829, TH9320A = this.TH9320A, TH9320B = this.TH9320B, TH9320C = this.TH9320C });
            ScriptBaseList.Add(new ScriptBase { Name = "脚本6", Remarks = "CCD尺寸检测2脚本", PLC = this.PLC, TH2829 = this.TH2829, TH9320A = this.TH9320A, TH9320B = this.TH9320B, TH9320C = this.TH9320C });
            ScriptBaseList.Add(new ScriptBase { Name = "脚本7", Remarks = "数据保存脚本", PLC = this.PLC, TH2829 = this.TH2829, TH9320A = this.TH9320A, TH9320B = this.TH9320B, TH9320C = this.TH9320C });
            ScriptBaseList.Add(new ScriptBase { Name = "脚本8", Remarks = "备用脚本", PLC = this.PLC, TH2829 = this.TH2829, TH9320A = this.TH9320A, TH9320B = this.TH9320B, TH9320C = this.TH9320C });
            foreach (var a in ScriptBaseList)
            {
                a.LoadExternAssembly();
            }
        }

        /// <summary>
        /// 所有脚本连续运行
        /// </summary>
        /// <returns></returns>
        public bool StartRun()
        {
          foreach(var a in ScriptBaseList)
          {
                a.Start();  
          }
          return true;
        }

   }
}
