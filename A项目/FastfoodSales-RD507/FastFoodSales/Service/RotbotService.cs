using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AxMELFARXMLib;
using System.Timers;
using Stylet;
using SimpleTCP;
using DAQ.Core.Log;
using System.Net.Sockets;



namespace DAQ.Service
{
    public interface IRotbotService
    {
        bool IsConnected { get; set; }
        RotbotVM rotbotVm { get; set; }
        event EventHandler<string> CodeDataReceived;
        bool BroadcastRobot(string str);

    }
    public class RotbotVM : PropertyChangedBase
    {
        public short RunStatus { get; set; } = -1; //-1故障 0 1运行
        public int ErrorNo { get; set; } = 0;

        public string ErrorMeeage { get; set; }
    }

    public class RotbotService:IRotbotService
    {
        private SimpleTcpServer serverRotbot;
        SimpleTcpClient tcpClient = new SimpleTcpClient();
        private AxMelfaRxM axMelfaRxM1;
        System.Windows.Forms.Integration.WindowsFormsHost host = new System.Windows.Forms.Integration.WindowsFormsHost();
        private readonly IDeviceReadWriter device;
        private int robot_errorNo;
        private Timer timer = new Timer();
        public event EventHandler<string> CodeDataReceived;
        public RotbotService(IDeviceReadWriter deviceReadWriter)
        {
            this.device = deviceReadWriter;
            // 创建 host 对象
            axMelfaRxM1 = new AxMelfaRxM();
            serverRotbot = new SimpleTcpServer();
            ((System.ComponentModel.ISupportInitialize)axMelfaRxM1).BeginInit();//开始初始化ocx对象
            host.Child = axMelfaRxM1;
            ((System.ComponentModel.ISupportInitialize)axMelfaRxM1).EndInit();
            StartComServer();
            axMelfaRxM1.MsgRecvEvent += axMelfaRxM1_MsgRecvEvent;
            serverRotbot.DataReceived += DataReceived;
            timer.Interval = 1500;
            timer.Enabled = true;
            timer.Elapsed += Timer_Elapsed;
            timer.Start();
            serverRotbot.Start(1024);
        }
        private bool StartComServer()
        {
            bool ret = true;
            if (axMelfaRxM1.ServerLive() == false)
            {
                ret = axMelfaRxM1.ServerStart();
            }
            return ret;
        }
        private void axMelfaRxM1_MsgRecvEvent(object sender, EventArgs e)
        {
            int ret = 0;
            int RobotID = 0;
            int msgID = 0;
            int Status = 0;
            int Error = 0;

            string data = "";
            string tmp = "";
            int cnt = 0;

            try
            {
                ret = axMelfaRxM1.GetRecvDataM(ref RobotID, ref msgID, ref data, ref Status, ref Error);
                if (ret == 1)
                {
                    int msgcnt;
                    msgcnt = axMelfaRxM1.GetDataCnt(data);
                    switch (msgID)
                    {
                        // Get the Error No
                        case 203:
                            axMelfaRxM1.GetOneDataCPP(0, data, ref tmp);
                            cnt = int.Parse(tmp);
                            if (cnt == 0)
                            {
                                robot_errorNo = 0;
                                rotbotVm.RunStatus = 1;
                                rotbotVm.ErrorNo = 0;
                                rotbotVm.ErrorMeeage = "";
                            }
                            for (int i = 0; i < cnt; ++i)
                            {
                                tmp = "";
                                rotbotVm.RunStatus = -1;
                                axMelfaRxM1.GetOneDataCPP(i * 8 + 1, data, ref tmp);    // Error No.
                                robot_errorNo = int.Parse(tmp);
                                rotbotVm.ErrorNo = int.Parse(tmp);
                                axMelfaRxM1.GetOneDataCPP(i * 8 + 2, data, ref tmp);    // Error Message
                                rotbotVm.ErrorMeeage = tmp;
                            }
                            device.SetInt(51, robot_errorNo);
                            break;
                        default:
                            break;
                    }
                }
                else
                {

                }
            }
            catch (Exception ex)
            {

            }
        }

        private void Timer_Elapsed(object sender,EventArgs e)
        {
            try
            {
                timer.Enabled = false;
                int status;
                if (axMelfaRxM1.CheckConnectingM(1))
                {
                    IsConnected = true;
                    device.SetShort(50, 1);
                }
                else
                {
                    rotbotVm.RunStatus = -1;
                    rotbotVm.ErrorNo = 99999999;
                    rotbotVm.ErrorMeeage = "Robot connected unsuccessfully";
                    IsConnected = false;
                    device.SetShort(50, -1);
                }
                status = axMelfaRxM1.RequestServiceM(1, 203, 0, null, 0, 0, 0);
            }
            catch
            {

            }
            timer.Enabled = true;
        }
        private void DataReceived(object sender, Message e)
        {
            if (CodeDataReceived != null)
            {
                string m = e.MessageString;
                CodeDataReceived(this, m);
            }
        }
        public bool BroadcastRobot(string str)
        {
            try
            {
                serverRotbot.BroadcastLine(str);
                return true;
            }
            catch(Exception ex)
            {
                LogHelper.Error(ex.Message);
                return false;
            }
        }
        public bool IsConnected { get; set; }
        public RotbotVM rotbotVm { get; set; } = new RotbotVM();
    }
}
