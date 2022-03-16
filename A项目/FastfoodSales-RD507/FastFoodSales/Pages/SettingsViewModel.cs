using Stylet;
using StyletIoC;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows.Markup;
using DAQ.Core.i18n;
using DAQ.Core;
using DAQ.Service;
using System.IO.Ports;
using System.Text.RegularExpressions;
using System.Windows;

namespace DAQ
{
    public class SettingsViewModel : Screen
    {
        [Inject]
        public IEventAggregator Events { get; set; }
        private readonly IConfigureFile configureFile;
        private readonly IDeviceReadWriter device;
        public Config _config { get; set; }
        public SettingsViewModel(IConfigureFile configure, IDeviceReadWriter device)
        {
            this.configureFile = configure;
            _config = configure.Load().GetValue<Config>(nameof(Config)) ?? new Config();

        }
        protected override void OnInitialActivate()
        {

            base.OnInitialActivate();

        }
        protected override void OnDeactivate()
        {
            base.OnDeactivate();
        }
        public static string[] Langs => new[] { "English", "简体中文", "日本語", "한국어" };
        public string[] Ports { get { return SerialPort.GetPortNames(); } }

        public string Language
        {
            get
            {
                string language = "简体中文";
                if (_config.Language == "en-US")
                    language = "English";
                else if (_config.Language == "ja-JP")
                    language = "日本語";
                else if (_config.Language == "ko-KR")
                    language = "한국어";
                else
                    language = "简体中文";
                return language;
            }
            set
            {
                if (value == "English")
                    _config.Language = "en-US";
                else if (value == "日本語")
                    _config.Language = "ja-JP";
                else if (value == "한국어")
                    _config.Language = "ko-KR";
                else
                    _config.Language = "zh-CN";
                TranslationSource.Instance.Language = _config.Language;
                SaveConfig();
            }
        }
        void SaveConfig()
        {
            configureFile.SetValue(nameof(Config), _config);
        }
        Regex regex1_3 = new Regex(@"^[1-3]{1}");
        Regex regex0_Z = new Regex(@"^[0-9A-HJ-NP-Z]{1}");
        Regex regexA_Z = new Regex(@"^[A-Z]{1}");
        public void SaveCommand()
        {
            if(!regex1_3.IsMatch(CoilModuleCode)||CoilModuleCode.Length!=1)
            {
                MessageBox.Show("Coil module Config code:Nos.1-3", "Error");
                return;
            }
            if (!regexA_Z.IsMatch(SICode)||SICode.Length != 1)
            {
                MessageBox.Show("A-Z(First letter of SI's name)", "SICode Error");
                return;
            }
            SaveConfig();
        }

        public string CoilModuleCode
        {
            get => _config.CoilModule;
            set => _config.CoilModule=value;
        }
        public string SICode
        {
            get => _config.SI;
            set => _config.SI = value;
        }
        public string LineCode
        {
            get => _config.LineNumber;
            set => _config.LineNumber = value;
        }
        public string StationCode
        {
            get => _config.Station;
            set => _config.Station = value;
        }
        public string SpindleCode
        {
            get => _config.Spindle;
            set => _config.Spindle = value;
        }

    }
}
