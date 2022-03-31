using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StyletIoC;
using Stylet;
using DAQ.Service;
using System.Windows.Data;
using System.Globalization;
using DAQ.Pages;
using Script.Methods;

namespace DAQ
{
    public class HomeViewModel
    {
        [Inject]
        IEventAggregator EventAggregator { get; set; }
        [Inject]
        public MsgViewModel Msg { get; set; }
        [Inject]
        public TH2829Port tH2829Port { get; set; }
        [Inject]
        public TH9320PortA tH9320PortA { get; set; }
        [Inject]
        public TH9320PortB tH9320PortB { get; set; }
        [Inject]
        public TH9320PortC tH9320PortC { get; set; }
    }

}
