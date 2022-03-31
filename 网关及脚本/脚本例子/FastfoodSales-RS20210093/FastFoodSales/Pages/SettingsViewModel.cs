using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using Stylet;
using StyletIoC;
using DAQ.Service;
using Script.Methods;

namespace DAQ
{
    public class SettingsViewModel : Screen
    {
        [Inject]
        public IEventAggregator Events { get; set; }
        [Inject]
        public TH2829Port th2829Port { get; set; }
        [Inject]
        public TH9320PortA th9320PortA { get; set; }
        [Inject]
        public TH9320PortB th9320PortB { get; set; }
        [Inject]
        public TH9320PortC th9320PortC { get; set; }

        [Inject]
        public PlcService plc { get; set; }

        public string[] Ports { get { return SerialPort.GetPortNames(); } }

        public string[] PortACMDs { get { return new string[] { "*IDN?", "trig", "TRS:DATA?" }; } }
        public string[] PortBCMDs { get { return new string[] { "*IDN?", "TRIG?", "FETCh?" }; } }
        public string[] PortCCMDs { get { return new string[] { "*IDN?", "TRIG?", "FETCh?" }; } }
        public string[] PortDCMDs { get { return new string[] { "*IDN?", "TRIG?", "FETCh?" }; } }

        public SettingsViewModel()
        {
        }
        protected override void OnInitialActivate()
        {

            base.OnInitialActivate();

        }
        protected override void OnDeactivate()
        {
            base.OnDeactivate();
            ScriptSettings.Default.Save();
        }

        public string PortA
        {
            get { return ScriptSettings.Default.PORT_A; }
            set
            {
                ScriptSettings.Default.PORT_A = value;
                th2829Port.Connect();
            }
        }
        public string PortB
        {
            get { return ScriptSettings.Default.PORT_B; }
            set
            {
                ScriptSettings.Default.PORT_B = value;
                th9320PortA.Connect();
            }
        }
        public string PortC
        {
            get { return ScriptSettings.Default.PORT_C; }
            set
            {
                ScriptSettings.Default.PORT_C = value;
                th9320PortB.Connect();
            }
        }
        public string PortD
        {
            get { return ScriptSettings.Default.PORT_D; }
            set
            {
                ScriptSettings.Default.PORT_D = value;
                th9320PortC.Connect();
            }
        }
        public string PLC_IP
        {
            get { return ScriptSettings.Default.PLC_IP; }
            set { ScriptSettings.Default.PLC_IP = value; }
        }
        public string PortABuffer { get; set; }
        public string PortBBuffer { get; set; }
        public string PortCBuffer { get; set; }
        public string PortDBuffer { get; set; }
        public int PLC_Port
        {
            get { return ScriptSettings.Default.PLC_PORT; }
            set { ScriptSettings.Default.PLC_PORT = value; }
        }

        public ushort PLC_RD
        {
            get { return ScriptSettings.Default.PLC_RD; }
            set { ScriptSettings.Default.PLC_RD = value; }
        }

        public ushort PLC_RLen
        {
            get { return ScriptSettings.Default.PLC_RLen; }
            set { ScriptSettings.Default.PLC_RLen = value; }
        }

        public ushort PLC_WD
        {
            get { return ScriptSettings.Default.PLC_WD; }
            set { ScriptSettings.Default.PLC_WD = value; }
        }

        public ushort PLC_WLen
        {
            get { return ScriptSettings.Default.PLC_WLen; }
            set { ScriptSettings.Default.PLC_WLen = value; }
        }





        public void QueryA(string Cmd)
        {
            PortABuffer = $"Send:\t{Cmd}{Environment.NewLine}";
            bool r = th2829Port.Request(Cmd, out string replay);
            if (r)
            {
                PortABuffer += $"Recieve:\t{replay}{Environment.NewLine}";
            }
            else
            {
                PortABuffer += $"error:\t{replay}{Environment.NewLine}";
            }
        }
        public void QueryB(string Cmd)
        {
            PortBBuffer = $"Send:\t{Cmd}{Environment.NewLine}";
            bool r = th9320PortA.RequestCmd(Cmd, out string replay);
            if (r)
            {
                PortBBuffer += $"Recieve:\t{replay}{Environment.NewLine}";
            }
            else
            {
                PortBBuffer += $"error:\t{replay}{Environment.NewLine}";
            }
        }
        public void QueryC(string Cmd)
        {
            PortCBuffer = $"Send:\t{Cmd}{Environment.NewLine}";
            bool r = th9320PortB.RequestCmd(Cmd, out string replay);
            if (r)
            {
                PortCBuffer += $"Recieve:\t{replay}{Environment.NewLine}";
            }
            else
            {
                PortCBuffer += $"error:\t{replay}{Environment.NewLine}";
            }
        }
        public void QueryD(string Cmd)
        {
            PortDBuffer = $"Send:\t{Cmd}{Environment.NewLine}";
            bool r = th9320PortC.RequestCmd(Cmd, out string replay);
            if (r)
            {
                PortDBuffer += $"Recieve:\t{replay}{Environment.NewLine}";
            }
            else
            {
                PortDBuffer += $"error:\t{replay}{Environment.NewLine}";

            }
        }



        public void TestA()
        {

            bool r = th2829Port.Read();
        }
        public void TestB()
        {

            bool r = th9320PortA.Read();
        }

        public void ceshi()
        {

         plc.PulseRregBit(0);
        }

    }
}
