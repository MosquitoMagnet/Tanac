using System;
using System.Collections.Generic;
using System.Text;
using Mv.Core.Interfaces;
using Mv.Modules.P150.Services;
using Mv.Ui.Mvvm;
using Prism.Commands;
using System.Windows.Forms;
using MaterialDesignThemes.Wpf;
using Prism.Mvvm;
using Unity;
using CsvHelper;
using CsvHelper.Configuration.Attributes;
using System.IO;
using System.Linq;
using System.Globalization;

namespace Mv.Modules.P150.ViewModels
{
    public class FARecord
    {
        [Index(0)]
        public string FA1 { get; set; }
        [Index(1)]
        public string FA2 { get; set; }
        [Index(2)]
        public string FA3 { get; set; }
        [Index(3)]
        public string FA4 { get; set; }
        [Index(4)]
        public string FA5 { get; set; }
        [Index(5)]
        public string FA6 { get; set; }
        [Index(6)]
        public string FA7 { get; set; }
        [Index(7)]
        public string FA8 { get; set; }
        [Index(8)]
        public string FA9 { get; set; }
        [Index(9)]
        public string FA10 { get; set; }
        [Index(10)]
        public string FA11 { get; set; }
        [Index(11)]
        public string FA12 { get; set; }
        [Index(12)]
        public string FA13 { get; set; }
        [Index(13)]
        public string FA14 { get; set; }
        [Index(14)]
        public string FA15 { get; set; }
        [Index(15)]
        public string FA16 { get; set; }
        [Index(16)]
        public string FA17 { get; set; }
        [Index(17)]
        public string FA18 { get; set; }
        [Index(18)]
        public string FA19 { get; set; }
        [Index(19)]
        public string FA20 { get; set; }
        [Index(20)]
        public string FA21 { get; set; }
        [Index(21)]
        public string FA22 { get; set; }
        [Index(22)]
        public string FA23 { get; set; }
        [Index(23)]
        public string FA24 { get; set; }
        [Index(24)]
        public string FA25 { get; set; }
        [Index(25)]
        public string Parallelism { get; set; }
        [Index(26)]
        public string BendingPin { get; set; }
    }
    public class SettingViewModel : ViewModelBase
    {
        public TraceConfig Config { get; }
        private DelegateCommand _cmdSave;
        private DelegateCommand _importA;
        private DelegateCommand _importB;

        public DelegateCommand SaveCommand =>
            _cmdSave ??= new DelegateCommand(SaveConfig);

        public DelegateCommand ImportACommand=>
            _importA ??= new DelegateCommand(ImportAConfig);
        public DelegateCommand ImportBCommand =>
            _importB ??= new DelegateCommand(ImportBConfig);



        private IConfigureFile _configure;

        void SaveConfig()
        {
            _configure.SetValue(nameof(TraceConfig), Config);           
        }
        void ImportAConfig()
        {
            try
            {
                string path = OpenImportDialog("Csv", "|*.csv");
                var farecords = GetFARecord(path);
                FA1_A = farecords[0].FA1;
                FA2_A = farecords[0].FA2;
                FA3_A = farecords[0].FA3;
                FA4_A = farecords[0].FA4;
                FA5_A = farecords[0].FA5;
                FA6_A = farecords[0].FA6;
                FA7_A = farecords[0].FA7;
                FA8_A = farecords[0].FA8;
                FA9_A = farecords[0].FA9;
                FA10_A = farecords[0].FA10;
                FA11_A = farecords[0].FA11;
                FA12_A = farecords[0].FA12;
                FA13_A = farecords[0].FA13;
                FA14_A = farecords[0].FA14;
                FA15_A = farecords[0].FA15;
                FA16_A = farecords[0].FA16;
                FA17_A = farecords[0].FA17;
                FA18_A = farecords[0].FA18;
                FA19_A = farecords[0].FA19;
                FA20_A = farecords[0].FA20;
                FA21_A = farecords[0].FA21;
                FA22_A = farecords[0].FA22;
                FA23_A = farecords[0].FA23;
                FA24_A = farecords[0].FA24;
                FA25_A = farecords[0].FA25;
                Parallelism_A = farecords[0].Parallelism;
                BendingPin_A = farecords[0].BendingPin;
                SaveConfig();
                MessageBox.Show("导入成功");
            }
            catch(Exception ex)
            {
                MessageBox.Show("导入失败,请检测导入文件是否正确");
            }

        }
        void ImportBConfig()
        {
            try
            {
                string path = OpenImportDialog("Csv", "|*.csv");
                var farecords = GetFARecord(path);
                FA1_B = farecords[0].FA1;
                FA2_B = farecords[0].FA2;
                FA3_B = farecords[0].FA3;
                FA4_B = farecords[0].FA4;
                FA5_B = farecords[0].FA5;
                FA6_B = farecords[0].FA6;
                FA7_B = farecords[0].FA7;
                FA8_B = farecords[0].FA8;
                FA9_B = farecords[0].FA9;
                FA10_B = farecords[0].FA10;
                FA11_B = farecords[0].FA11;
                FA12_B = farecords[0].FA12;
                FA13_B = farecords[0].FA13;
                FA14_B = farecords[0].FA14;
                FA15_B = farecords[0].FA15;
                FA16_B = farecords[0].FA16;
                FA17_B = farecords[0].FA17;
                FA18_B = farecords[0].FA18;
                FA19_B = farecords[0].FA19;
                FA20_B = farecords[0].FA20;
                FA21_B = farecords[0].FA21;
                FA22_B = farecords[0].FA22;
                FA23_B = farecords[0].FA23;
                FA24_B = farecords[0].FA24;
                FA25_B = farecords[0].FA25;
                Parallelism_B = farecords[0].Parallelism;
                BendingPin_B = farecords[0].BendingPin;
                SaveConfig();
                MessageBox.Show("导入成功");
            }
            catch (Exception ex)
            {
                MessageBox.Show("导入失败,请检测导入文件是否正确");
            }
        }
        public string OpenImportDialog(string fileDes, string filter, string initPath = null)
        {
            string text = initPath;
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.CheckFileExists = true;
            openFileDialog.CheckPathExists = true;
            openFileDialog.Multiselect = false;
            if (text != null)
            {
                openFileDialog.InitialDirectory = text;
            }
            openFileDialog.Title = "导入" + fileDes;
            openFileDialog.Filter = fileDes+ filter;
            if (openFileDialog.ShowDialog() != DialogResult.OK)
            {
                return null;
            }
            try
            {
                text = openFileDialog.FileName;
            }
            catch (Exception ex)
            {
                return null;
            }
            return text;
        }
        private List<FARecord> GetFARecord(string path)
        {
            try
            {
                FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using var reader = new StreamReader(fs, encoding: Encoding.UTF8);
                using var csv = new CsvReader(reader, CultureInfo.CurrentCulture);
                var records = csv.GetRecords<FARecord>()
                .Where(a => !string.IsNullOrEmpty(a.FA1))
                .Where(b => !string.IsNullOrEmpty(b.FA2))
                .Where(b => !string.IsNullOrEmpty(b.FA3))
                .Where(b => !string.IsNullOrEmpty(b.FA4))
                .Where(b => !string.IsNullOrEmpty(b.FA5))
                .Where(b => !string.IsNullOrEmpty(b.FA6))
                .Where(b => !string.IsNullOrEmpty(b.FA7))
                .Where(b => !string.IsNullOrEmpty(b.FA8))
                .Where(b => !string.IsNullOrEmpty(b.FA9))
                .Where(b => !string.IsNullOrEmpty(b.FA10))
                .Where(b => !string.IsNullOrEmpty(b.FA11))
                .Where(b => !string.IsNullOrEmpty(b.FA12))
                .Where(b => !string.IsNullOrEmpty(b.FA13))
                .Where(b => !string.IsNullOrEmpty(b.FA14))
                .Where(b => !string.IsNullOrEmpty(b.FA15))
                .Where(b => !string.IsNullOrEmpty(b.FA16))
                .Where(b => !string.IsNullOrEmpty(b.FA17))
                .Where(b => !string.IsNullOrEmpty(b.FA18))
                .Where(b => !string.IsNullOrEmpty(b.FA19))
                .Where(b => !string.IsNullOrEmpty(b.FA20))
                .Where(b => !string.IsNullOrEmpty(b.FA21))
                .Where(b => !string.IsNullOrEmpty(b.FA22))
                .Where(b => !string.IsNullOrEmpty(b.FA23))
                .Where(b => !string.IsNullOrEmpty(b.FA24))
                .Where(b => !string.IsNullOrEmpty(b.FA25))
                .Where(b => !string.IsNullOrEmpty(b.Parallelism))
                .Where(b => !string.IsNullOrEmpty(b.BendingPin))
                .Distinct().ToList();
                return records;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public SettingViewModel(IUnityContainer container, IConfigureFile configure) :
            base(container)
        {
            _configure = configure;
            Config = configure.GetValue<TraceConfig>(nameof(TraceConfig)) ?? new TraceConfig();
        }
        public static string[] Factories => new[] { "AxisA", "AxisB", "Upload" };
        public string Factory
        {
            get => Config.Factory;
            set
            {
                Config.Factory = value;
                RaisePropertyChanged(nameof(Factory));
            }
        }
        #region CTQ_A
        public string FA1_A
        {
            get => Config.FA1_A;
            set
            {
                Config.FA1_A = value;
                RaisePropertyChanged(nameof(FA1_A));
            }
        }
        public string FA2_A
        {
            get => Config.FA2_A;
            set
            {
                Config.FA2_A = value;
                RaisePropertyChanged(nameof(FA2_A));
            }
        }
        public string FA3_A
        {
            get => Config.FA3_A;
            set
            {
                Config.FA3_A = value;
                RaisePropertyChanged(nameof(FA3_A));
            }
        }
        public string FA4_A
        {
            get => Config.FA4_A;
            set
            {
                Config.FA4_A = value;
                RaisePropertyChanged(nameof(FA4_A));
            }
        }
        public string FA5_A
        {
            get => Config.FA5_A;
            set
            {
                Config.FA5_A = value;
                RaisePropertyChanged(nameof(FA5_A));
            }
        }
        public string FA6_A
        {
            get => Config.FA6_A;
            set
            {
                Config.FA6_A = value;
                RaisePropertyChanged(nameof(FA6_A));
            }
        }
        public string FA7_A
        {
            get => Config.FA7_A;
            set
            {
                Config.FA7_A = value;
                RaisePropertyChanged(nameof(FA7_A));
            }
        }
        public string FA8_A
        {
            get => Config.FA8_A;
            set
            {
                Config.FA8_A = value;
                RaisePropertyChanged(nameof(FA8_A));
            }
        }
        public string FA9_A
        {
            get => Config.FA9_A;
            set
            {
                Config.FA9_A = value;
                RaisePropertyChanged(nameof(FA9_A));
            }
        }
        public string FA10_A
        {
            get => Config.FA10_A;
            set
            {
                Config.FA10_A = value;
                RaisePropertyChanged(nameof(FA10_A));
            }
        }
        public string FA11_A
        {
            get => Config.FA11_A;
            set
            {
                Config.FA11_A = value;
                RaisePropertyChanged(nameof(FA11_A));
            }
        }
        public string FA12_A
        {
            get => Config.FA12_A;
            set
            {
                Config.FA12_A = value;
                RaisePropertyChanged(nameof(FA12_A));
            }
        }
        public string FA13_A
        {
            get => Config.FA13_A;
            set
            {
                Config.FA13_A = value;
                RaisePropertyChanged(nameof(FA13_A));
            }
        }
        public string FA14_A
        {
            get => Config.FA14_A;
            set
            {
                Config.FA14_A = value;
                RaisePropertyChanged(nameof(FA14_A));
            }
        }
        public string FA15_A
        {
            get => Config.FA15_A;
            set
            {
                Config.FA15_A = value;
                RaisePropertyChanged(nameof(FA15_A));
            }
        }
        public string FA16_A
        {
            get => Config.FA16_A;
            set
            {
                Config.FA16_A = value;
                RaisePropertyChanged(nameof(FA16_A));
            }
        }
        public string FA17_A
        {
            get => Config.FA17_A;
            set
            {
                Config.FA17_A = value;
                RaisePropertyChanged(nameof(FA17_A));
            }
        }
        public string FA18_A
        {
            get => Config.FA18_A;
            set
            {
                Config.FA18_A = value;
                RaisePropertyChanged(nameof(FA18_A));
            }
        }
        public string FA19_A
        {
            get => Config.FA19_A;
            set
            {
                Config.FA19_A = value;
                RaisePropertyChanged(nameof(FA19_A));
            }
        }
        public string FA20_A
        {
            get => Config.FA20_A;
            set
            {
                Config.FA20_A = value;
                RaisePropertyChanged(nameof(FA20_A));
            }
        }
        public string FA21_A
        {
            get => Config.FA21_A;
            set
            {
                Config.FA21_A = value;
                RaisePropertyChanged(nameof(FA21_A));
            }
        }
        public string FA22_A
        {
            get => Config.FA22_A;
            set
            {
                Config.FA22_A = value;
                RaisePropertyChanged(nameof(FA22_A));
            }
        }
        public string FA23_A
        {
            get => Config.FA23_A;
            set
            {
                Config.FA23_A = value;
                RaisePropertyChanged(nameof(FA23_A));
            }
        }
        public string FA24_A
        {
            get => Config.FA24_A;
            set
            {
                Config.FA24_A = value;
                RaisePropertyChanged(nameof(FA24_A));
            }
        }
        public string FA25_A
        {
            get => Config.FA25_A;
            set
            {
                Config.FA25_A = value;
                RaisePropertyChanged(nameof(FA25_A));
            }
        }
        public string Parallelism_A
        {
            get => Config.Parallelism_A;
            set
            {
                Config.Parallelism_A = value;
                RaisePropertyChanged(nameof(Parallelism_A));
            }
        }
        public string BendingPin_A
        {
            get => Config.BendingPin_A;
            set
            {
                Config.BendingPin_A = value;
                RaisePropertyChanged(nameof(BendingPin_A));
            }
        }
        #endregion
        #region CTQ_B
        public string FA1_B
        {
            get => Config.FA1_B;
            set
            {
                Config.FA1_B = value;
                RaisePropertyChanged(nameof(FA1_B));
            }
        }
        public string FA2_B
        {
            get => Config.FA2_B;
            set
            {
                Config.FA2_B = value;
                RaisePropertyChanged(nameof(FA2_B));
            }
        }
        public string FA3_B
        {
            get => Config.FA3_B;
            set
            {
                Config.FA3_B = value;
                RaisePropertyChanged(nameof(FA3_B));
            }
        }
        public string FA4_B
        {
            get => Config.FA4_B;
            set
            {
                Config.FA4_B = value;
                RaisePropertyChanged(nameof(FA4_B));
            }
        }
        public string FA5_B
        {
            get => Config.FA5_B;
            set
            {
                Config.FA5_B = value;
                RaisePropertyChanged(nameof(FA5_B));
            }
        }
        public string FA6_B
        {
            get => Config.FA6_B;
            set
            {
                Config.FA6_B = value;
                RaisePropertyChanged(nameof(FA6_B));
            }
        }
        public string FA7_B
        {
            get => Config.FA7_B;
            set
            {
                Config.FA7_B = value;
                RaisePropertyChanged(nameof(FA7_B));
            }
        }
        public string FA8_B
        {
            get => Config.FA8_B;
            set
            {
                Config.FA8_B = value;
                RaisePropertyChanged(nameof(FA8_B));
            }
        }
        public string FA9_B
        {
            get => Config.FA9_B;
            set
            {
                Config.FA9_B = value;
                RaisePropertyChanged(nameof(FA9_B));
            }
        }
        public string FA10_B
        {
            get => Config.FA10_B;
            set
            {
                Config.FA10_B = value;
                RaisePropertyChanged(nameof(FA10_B));
            }
        }
        public string FA11_B
        {
            get => Config.FA11_B;
            set
            {
                Config.FA11_B = value;
                RaisePropertyChanged(nameof(FA11_B));
            }
        }
        public string FA12_B
        {
            get => Config.FA12_B;
            set
            {
                Config.FA12_B = value;
                RaisePropertyChanged(nameof(FA12_B));
            }
        }
        public string FA13_B
        {
            get => Config.FA13_B;
            set
            {
                Config.FA13_B = value;
                RaisePropertyChanged(nameof(FA13_B));
            }
        }
        public string FA14_B
        {
            get => Config.FA14_B;
            set
            {
                Config.FA14_B = value;
                RaisePropertyChanged(nameof(FA14_B));
            }
        }
        public string FA15_B
        {
            get => Config.FA15_B;
            set
            {
                Config.FA15_B = value;
                RaisePropertyChanged(nameof(FA15_B));
            }
        }
        public string FA16_B
        {
            get => Config.FA16_B;
            set
            {
                Config.FA16_B = value;
                RaisePropertyChanged(nameof(FA16_B));
            }
        }
        public string FA17_B
        {
            get => Config.FA17_B;
            set
            {
                Config.FA17_B = value;
                RaisePropertyChanged(nameof(FA17_B));
            }
        }
        public string FA18_B
        {
            get => Config.FA18_B;
            set
            {
                Config.FA18_B = value;
                RaisePropertyChanged(nameof(FA18_B));
            }
        }
        public string FA19_B
        {
            get => Config.FA19_B;
            set
            {
                Config.FA19_B = value;
                RaisePropertyChanged(nameof(FA19_B));
            }
        }
        public string FA20_B
        {
            get => Config.FA20_B;
            set
            {
                Config.FA20_B = value;
                RaisePropertyChanged(nameof(FA20_B));
            }
        }
        public string FA21_B
        {
            get => Config.FA21_B;
            set
            {
                Config.FA21_B = value;
                RaisePropertyChanged(nameof(FA21_B));
            }
        }
        public string FA22_B
        {
            get => Config.FA22_B;
            set
            {
                Config.FA22_B = value;
                RaisePropertyChanged(nameof(FA22_B));
            }
        }
        public string FA23_B
        {
            get => Config.FA23_B;
            set
            {
                Config.FA23_B = value;
                RaisePropertyChanged(nameof(FA23_B));
            }
        }
        public string FA24_B
        {
            get => Config.FA24_B;
            set
            {
                Config.FA24_B = value;
                RaisePropertyChanged(nameof(FA24_B));
            }
        }
        public string FA25_B
        {
            get => Config.FA25_B;
            set
            {
                Config.FA25_B = value;
                RaisePropertyChanged(nameof(FA25_B));
            }
        }
        public string Parallelism_B
        {
            get => Config.Parallelism_B;
            set
            {
                Config.Parallelism_B = value;
                RaisePropertyChanged(nameof(Parallelism_B));
            }
        }
        public string BendingPin_B
        {
            get => Config.BendingPin_B;
            set
            {
                Config.BendingPin_B = value;
                RaisePropertyChanged(nameof(BendingPin_B));
            }
        }
        #endregion
        #region Material_A
        public string OD_A {
            get => Config.OD_A;
            set => Config.OD_A = value;
        }
        #endregion
        #region Material_B
        public string OD_B {
            get => Config.OD_B;
            set => Config.OD_B = value;
        }

        #endregion
        #region KPIV_A    
        public string WireTen_A1 {
            get => Config.WireTen_A1;
            set => Config.WireTen_A1 = value;
        }
        public string WireTen_A2
        {
            get => Config.WireTen_A2;
            set => Config.WireTen_A2 = value;
        }
        public string WireTen_A3
        {
            get => Config.WireTen_A3;
            set => Config.WireTen_A3 = value;
        }
        public string WireTen_A4
        {
            get => Config.WireTen_A4;
            set => Config.WireTen_A4 = value;
        }
        public string WireTen_A5
        {
            get => Config.WireTen_A5;
            set => Config.WireTen_A5 = value;
        }
        public string WireTen_A6
        {
            get => Config.WireTen_A6;
            set => Config.WireTen_A6 = value;
        }
        public string WireTen_A7
        {
            get => Config.WireTen_A7;
            set => Config.WireTen_A7 = value;
        }
        #endregion
        #region KPIV_B    
        public string WireTen_B1 {
            get => Config.WireTen_B1;
            set => Config.WireTen_B1 = value;
        }
        public string WireTen_B2
        {
            get => Config.WireTen_B2;
            set => Config.WireTen_B2 = value;
        }
        public string WireTen_B3
        {
            get => Config.WireTen_B3;
            set => Config.WireTen_B3 = value;
        }
        public string WireTen_B4
        {
            get => Config.WireTen_B4;
            set => Config.WireTen_B4 = value;
        }
        public string WireTen_B5
        {
            get => Config.WireTen_B5;
            set => Config.WireTen_B5 = value;
        }
        public string WireTen_B6
        {
            get => Config.WireTen_B6;
            set => Config.WireTen_B6 = value;
        }
        public string WireTen_B7
        {
            get => Config.WireTen_B7;
            set => Config.WireTen_B7 = value;
        }
        #endregion
        #region Upload  
        public bool isFA1 {
            get => Config.isFA1;
            set => Config.isFA1 = value;
        }
        public bool isFA2 {
            get => Config.isFA2;
            set => Config.isFA2 = value;
        }
        public bool isFA3 {
            get => Config.isFA3;
            set => Config.isFA3 = value;
        }
        public bool isFA4 {
            get => Config.isFA4;
            set => Config.isFA4 = value;
        }
        public bool isFA5 {
            get => Config.isFA5;
            set => Config.isFA5 = value;
        }
        public bool isFA6 {
            get => Config.isFA6;
            set => Config.isFA6 = value;
        }
        public bool isFA7 {
            get => Config.isFA7;
            set => Config.isFA7 = value;
        }
        public bool isFA8 {
            get => Config.isFA8;
            set => Config.isFA8 = value;
        }
        public bool isFA9 {
            get => Config.isFA9;
            set => Config.isFA9 = value;
        }
        public bool isFA10 {
            get => Config.isFA10;
            set => Config.isFA10 = value;
        }
        public bool isFA11 {
            get => Config.isFA11;
            set => Config.isFA11 = value;
        }
        public bool isFA12 {
            get => Config.isFA12;
            set => Config.isFA12 = value;
        }
        public bool isFA13 {
            get => Config.isFA13;
            set => Config.isFA13 = value;
        }
        public bool isFA14
        {
            get => Config.isFA14;
            set => Config.isFA14 = value;
        }
        public bool isFA15
        {
            get => Config.isFA15;
            set => Config.isFA15 = value;
        }
        public bool isFA16
        {
            get => Config.isFA16;
            set => Config.isFA16 = value;
        }
        public bool isFA17
        {
            get => Config.isFA17;
            set => Config.isFA17 = value;
        }
        public bool isFA18
        {
            get => Config.isFA18;
            set => Config.isFA18 = value;
        }
        public bool isFA19
        {
            get => Config.isFA19;
            set => Config.isFA19 = value;
        }
        public bool isFA20
        {
            get => Config.isFA20;
            set => Config.isFA20 = value;
        }
        public bool isFA21
        {
            get => Config.isFA21;
            set => Config.isFA21 = value;
        }
        public bool isFA22
        {
            get => Config.isFA22;
            set => Config.isFA22 = value;
        }
        public bool isFA23
        {
            get => Config.isFA23;
            set => Config.isFA23 = value;
        }
        public bool isFA24
        {
            get => Config.isFA24;
            set => Config.isFA24 = value;
        }
        public bool isFA25
        {
            get => Config.isFA25;
            set => Config.isFA25 = value;
        }
        public bool isParallelism {
            get => Config.isParallelism;
            set => Config.isParallelism = value;
        }
        public bool isBendingPin {
            get => Config.isBendingPin;
            set => Config.isBendingPin = value;
        }
        public bool isUsageC {
            get => Config.isUsageC;
            set => Config.isUsageC = value;
        }
        public bool isUsageM {
            get => Config.isUsageM;
            set => Config.isUsageM = value;
        }
        public bool isOD {
            get => Config.isOD;
            set => Config.isOD = value;
        }
        public bool isLower {
            get => Config.isLower;
            set => Config.isLower = value;
        }
        public bool isWireTen {
            get => Config.isWireTen;
            set => Config.isWireTen = value;
        }
        public bool isGrap1 {
            get => Config.isGrap1;
            set => Config.isGrap1 = value;
        }
        public bool isGrap2 {
            get => Config.isGrap2;
            set => Config.isGrap2 = value;
        }
        public bool isGrap3 {
            get => Config.isGrap3;
            set => Config.isGrap3 = value;
        }
        public bool isWspeed1 {
            get => Config.isWspeed1;
            set => Config.isWspeed1 = value;
        }
        public bool isWspeed2 {
            get => Config.isWspeed2;
            set => Config.isWspeed2 = value;
        }
        public bool isWspeed3 {
            get => Config.isWspeed3;
            set => Config.isWspeed3 = value;
        }
        public bool isWspeedB {
            get => Config.isWspeedB;
            set => Config.isWspeedB = value;
        }
        public bool isIspeed {
            get => Config.isIspeed;
            set => Config.isIspeed = value;
        }
        #endregion
        #region Upload Power
        public bool isRstart
        {
            get => Config.isRstart;
            set => Config.isRstart = value;
        }
        public bool isCurrent
        {
            get => Config.isCurrent;
            set => Config.isCurrent = value;
        }
        public bool isVoltage
        {
            get => Config.isVoltage;
            set => Config.isVoltage = value;
        }
        public bool isPower
        {
            get => Config.isPower;
            set => Config.isPower = value;
        }
        public bool isRend
        {
            get => Config.isRend;
            set => Config.isRend = value;
        }
        public bool isBonding_Temp
        {
            get => Config.isBonding_Temp;
            set => Config.isBonding_Temp = value;
        }
        public bool isBonding_Time
        {
            get => Config.isBonding_Time;
            set => Config.isBonding_Time = value;
        }
        public bool isTool_Temp
        {
            get => Config.isTool_Temp;
            set => Config.isTool_Temp = value;
        }
        public bool isBonding_Method
        {
            get => Config.isBonding_Method;
            set => Config.isBonding_Method = value;
        }
        public bool isRC1
        {
            get => Config.isRC1;
            set => Config.isRC1 = value;
        }
        public bool isRC2
        {
            get => Config.isRC2;
            set => Config.isRC2 = value;
        }
        #endregion
        public bool isUpload
        {
            get => Config.isUpload;
            set => Config.isUpload = value;
        }
    }
}


