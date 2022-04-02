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
    public class HiveDataSettingViewModel : ViewModelBase
    {
        public RD402HiveDataConfig Config { get; }
        private DelegateCommand _cmdSave;

        public DelegateCommand SaveCommand =>
            _cmdSave ??= new DelegateCommand(SaveConfig);

        private IConfigureFile _configure;

        void SaveConfig()
        {
            _configure.SetValue(nameof(RD402HiveDataConfig), Config);

        }

        public HiveDataSettingViewModel(IUnityContainer container, IConfigureFile configure) :
            base(container)
        {
            _configure = configure;
            Config = configure.GetValue<RD402HiveDataConfig>(nameof(RD402HiveDataConfig)) ?? new RD402HiveDataConfig();
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
            set => Config.FA1_A = value;
        }
        public string FA2_A
        {
            get => Config.FA2_A;
            set => Config.FA2_A = value;
        }
        public string FA3_A
        {
            get => Config.FA3_A;
            set => Config.FA3_A = value;
        }
        public string FA4_A
        {
            get => Config.FA4_A;
            set => Config.FA4_A = value;
        }
        public string FA5_A
        {
            get => Config.FA5_A;
            set => Config.FA5_A = value;
        }
        public string FA6_A
        {
            get => Config.FA6_A;
            set => Config.FA6_A = value;
        }
        public string FA7_A
        {
            get => Config.FA7_A;
            set => Config.FA7_A = value;
        }
        public string FA8_A
        {
            get => Config.FA8_A;
            set => Config.FA8_A = value;
        }
        public string FA9_A
        {
            get => Config.FA9_A;
            set => Config.FA9_A = value;
        }
        public string FA10_A
        {
            get => Config.FA10_A;
            set => Config.FA10_A = value;
        }
        public string FA11_A
        {
            get => Config.FA11_A;
            set => Config.FA11_A = value;
        }
        public string FA12_A
        {
            get => Config.FA12_A;
            set => Config.FA12_A = value;
        }
        public string FA13_A
        {
            get => Config.FA13_A;
            set => Config.FA13_A = value;
        }
        public string FA14_A
        {
            get => Config.FA14_A;
            set => Config.FA14_A = value;
        }
        public string FA15_A
        {
            get => Config.FA15_A;
            set => Config.FA15_A = value;
        }
        public string FA16_A
        {
            get => Config.FA16_A;
            set => Config.FA16_A = value;
        }
        public string FA17_A
        {
            get => Config.FA17_A;
            set => Config.FA17_A = value;
        }
        public string FA18_A
        {
            get => Config.FA18_A;
            set => Config.FA18_A = value;
        }
        public string FA19_A
        {
            get => Config.FA19_A;
            set => Config.FA19_A = value;
        }
        public string FA20_A
        {
            get => Config.FA20_A;
            set => Config.FA20_A = value;
        }
        public string FA21_A
        {
            get => Config.FA21_A;
            set => Config.FA21_A = value;
        }
        public string FA22_A
        {
            get => Config.FA22_A;
            set => Config.FA22_A = value;
        }
        public string FA23_A
        {
            get => Config.FA23_A;
            set => Config.FA23_A = value;
        }
        public string FA24_A
        {
            get => Config.FA24_A;
            set => Config.FA24_A = value;
        }
        public string FA25_A
        {
            get => Config.FA25_A;
            set => Config.FA25_A = value;
        }
        public string Parallelism_A
        {
            get => Config.Parallelism_A;
            set => Config.Parallelism_A = value;
        }
        public string BendingPin_A
        {
            get => Config.BendingPin_A;
            set => Config.BendingPin_A = value;
        }
        #endregion
        #region CTQ_B
        public string FA1_B
        {
            get => Config.FA1_B;
            set => Config.FA1_B = value;
        }
        public string FA2_B
        {
            get => Config.FA2_B;
            set => Config.FA2_B = value;
        }
        public string FA3_B
        {
            get => Config.FA3_B;
            set => Config.FA3_B = value;
        }
        public string FA4_B
        {
            get => Config.FA4_B;
            set => Config.FA4_B = value;
        }
        public string FA5_B
        {
            get => Config.FA5_B;
            set => Config.FA5_B = value;
        }
        public string FA6_B
        {
            get => Config.FA6_B;
            set => Config.FA6_B = value;
        }
        public string FA7_B
        {
            get => Config.FA7_B;
            set => Config.FA7_B = value;
        }
        public string FA8_B
        {
            get => Config.FA8_B;
            set => Config.FA8_B = value;
        }
        public string FA9_B
        {
            get => Config.FA9_B;
            set => Config.FA9_B = value;
        }
        public string FA10_B
        {
            get => Config.FA10_B;
            set => Config.FA10_B = value;
        }
        public string FA11_B
        {
            get => Config.FA11_B;
            set => Config.FA11_B = value;
        }
        public string FA12_B
        {
            get => Config.FA12_B;
            set => Config.FA12_B = value;
        }
        public string FA13_B
        {
            get => Config.FA13_B;
            set => Config.FA13_B = value;
        }
        public string FA14_B
        {
            get => Config.FA14_B;
            set => Config.FA14_B = value;
        }
        public string FA15_B
        {
            get => Config.FA15_B;
            set => Config.FA15_B = value;
        }
        public string FA16_B
        {
            get => Config.FA16_B;
            set => Config.FA16_B = value;
        }
        public string FA17_B
        {
            get => Config.FA17_B;
            set => Config.FA17_B = value;
        }
        public string FA18_B
        {
            get => Config.FA18_B;
            set => Config.FA18_B = value;
        }
        public string FA19_B
        {
            get => Config.FA19_B;
            set => Config.FA19_B = value;
        }
        public string FA20_B
        {
            get => Config.FA20_B;
            set => Config.FA20_B = value;
        }
        public string FA21_B
        {
            get => Config.FA21_B;
            set => Config.FA21_B = value;
        }
        public string FA22_B
        {
            get => Config.FA22_B;
            set => Config.FA22_B = value;
        }
        public string FA23_B
        {
            get => Config.FA23_B;
            set => Config.FA23_B = value;
        }
        public string FA24_B
        {
            get => Config.FA24_B;
            set => Config.FA24_B = value;
        }
        public string FA25_B
        {
            get => Config.FA25_B;
            set => Config.FA25_B = value;
        }
        public string Parallelism_B
        {
            get => Config.Parallelism_B;
            set => Config.Parallelism_B = value;
        }
        public string BendingPin_B
        {
            get => Config.BendingPin_B;
            set => Config.BendingPin_B = value;
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
        public string WireTen_A {
            get => Config.WireTen_A;
            set => Config.WireTen_A = value;
        }
        #endregion
        #region KPIV_B    
        public string WireTen_B {
            get => Config.WireTen_B;
            set => Config.WireTen_B = value;
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
    }
}


