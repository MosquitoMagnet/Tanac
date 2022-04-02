using System;
using System.Collections.Generic;
using System.Text;
using Mv.Core.Interfaces;
using Mv.Modules.RD402.Service;
using Mv.Ui.Mvvm;
using Prism.Commands;
using MaterialDesignThemes.Wpf;
using Prism.Mvvm;
using Unity;

namespace Mv.Modules.RD402.ViewModels
{


    public class Rd402SettingViewModel : ViewModelBase
    {
        public RD402Config Config { get; }
        private DelegateCommand _cmdSave;
        private DeviceReadWriter _device;

        public DelegateCommand SaveCommand =>
            _cmdSave ??= new DelegateCommand(SaveConfig);

        private IConfigureFile _configure;

        void SaveConfig()
        {
            _configure.SetValue(nameof(RD402Config), Config);
            _device.PlcConnect();

        }

        public Rd402SettingViewModel(IUnityContainer container, IConfigureFile configure, DeviceReadWriter device) :
            base(container)
        {         
            _configure = configure;
            Config = configure.GetValue<RD402Config>(nameof(RD402Config)) ?? new RD402Config();
            _device = device;
            //  DialogHost.CloseDialogCommand.Execute()
        }

        public string PlcIpAddress
        {
            get => Config.PLCIpAddress;
            set => Config.PLCIpAddress = value;
        }


        public int PlcPort
        {
            get => Config.PLCPort;
            set => Config.PLCPort = value;
        }

        public string PrinterIpAddress
        {
            get => Config.PrinterIpAddress;
            set => Config.PrinterIpAddress = value;
        }

        public int PrinterPort
        {
            get => Config.PrinterPort;
            set => Config.PrinterPort = value;
        }

        public ushort ReadLens
        {
            get => Config.ReadLens;
            set => Config.ReadLens = value;
        }

        public string ReadAddrStart
        {
            get => Config.ReadAddrStart;
            set => Config.ReadAddrStart = value;
        }

        public ushort WirteLens
        {
            get => Config.WriteLens;
            set => Config.WriteLens = value;
        }

        public string WriteAddrStart
        {
            get => Config.WriteAddrStart;
            set => Config.WriteAddrStart = value;
        }


        public string Mo
        {
            get => Config.Mo;
            set => Config.Mo = value;
        }

        public string LineNumber
        {
            get => Config.LineNumber;
            set => Config.LineNumber = value;
        }

        public string MachineCode
        {
            get => Config.MachineCode;
            set => Config.MachineCode = value;
        }

        public string UploadData
        {
            get => Config.FileDir;
            set => Config.FileDir = value;
        }
        public string MachineNumberFile
        {
            get => Config.MachineNumber;
            set => Config.MachineNumber = value;
        }

        public string Factory
        {
            get => Config.Factory;
            set
            {
                Config.Factory = value;
                RaisePropertyChanged(nameof(Factory));
            }
        }

        public static string[] Factories => new[] { "ICT", "信维", "调试" };
        public string coilWinding
        {
            get => Config.CoilWinding;
            set => Config.CoilWinding = value;
        }
        public string Station
        {
            get => Config.Station;
            set => Config.Station = value;
        }
        #region ICT二维码比对
        public bool isQRCheck
        {
            get => Config.isQRCheck;
            set => Config.isQRCheck = value;
        }
        public string QRCheck
        {
            get => Config.QRCheck;
            set => Config.QRCheck = value;
        }
        /// <summary>
        /// Apple版次代码
        /// </summary>
        /// <param name="message"></param>
        public string Revision
        {
            get => Config.Revision;
            set => Config.Revision = value;
        }
        /// <summary>
        /// Engineering Reference Code
        /// </summary>
        /// <param name="message"></param>
        public string EECode
        {
            get => Config.EECode;
            set => Config.EECode = value;
        }
        /// <summary>
        /// 工厂代码
        /// </summary>
        public string VendorCode
        {
            get => Config.VendorCode;
            set => Config.VendorCode = value;
        }




        #endregion
        public string SoftwareVER
        {
            get => Config.SoftwareVER;
            set => Config.SoftwareVER = value;
        }

        public string GetsnUrl
        {
            get => Config.GetsnUrl;
            set => Config.GetsnUrl = value;
        }
        public string UploadUrl
        {
            get => Config.UploadUrl;
            set => Config.UploadUrl = value;
        }
        public string WireVendorNameA
        {
            get => Config.WireVendorNameA;
            set
            {
                Config.WireVendorNameA = value;
                RaisePropertyChanged(nameof(WireVendorNameA));
            }
        }
        public string WireVendorNameB
        {
            get => Config.WireVendorNameB;
            set
            {
                Config.WireVendorNameB = value;
                RaisePropertyChanged(nameof(WireVendorNameB));
            }
        }
        public string WireVendorCodeA
        {
            get => Config.WireVendorCodeA;
            set => Config.WireVendorCodeA = value;
        }
        public string WireVendorCodeB
        {
            get => Config.WireVendorCodeB;
            set => Config.WireVendorCodeB = value;
        }
        public bool isWireVendorA
        {
            get => Config.isWireVendorA;
            set => Config.isWireVendorA = value;
        }
        public bool isWireVendorB
        {
            get => Config.isWireVendorB;
            set => Config.isWireVendorB = value;
        }
    }
}
