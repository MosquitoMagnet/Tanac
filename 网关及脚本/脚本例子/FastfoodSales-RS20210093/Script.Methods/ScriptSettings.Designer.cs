﻿//------------------------------------------------------------------------------
// <auto-generated>
//     此代码由工具生成。
//     运行时版本:4.0.30319.42000
//
//     对此文件的更改可能会导致不正确的行为，并且如果
//     重新生成代码，这些更改将会丢失。
// </auto-generated>
//------------------------------------------------------------------------------

namespace Script.Methods {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "16.8.1.0")]
    public sealed partial class ScriptSettings : global::System.Configuration.ApplicationSettingsBase {
        
        private static ScriptSettings defaultInstance = ((ScriptSettings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new ScriptSettings())));
        
        public static ScriptSettings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("COM1")]
        public string PORT_A {
            get {
                return ((string)(this["PORT_A"]));
            }
            set {
                this["PORT_A"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("COM4")]
        public string PORT_B {
            get {
                return ((string)(this["PORT_B"]));
            }
            set {
                this["PORT_B"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("COM3")]
        public string PORT_C {
            get {
                return ((string)(this["PORT_C"]));
            }
            set {
                this["PORT_C"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("COM2")]
        public string PORT_D {
            get {
                return ((string)(this["PORT_D"]));
            }
            set {
                this["PORT_D"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("127.0.0.1")]
        public string PLC_IP {
            get {
                return ((string)(this["PLC_IP"]));
            }
            set {
                this["PLC_IP"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("6000")]
        public int PLC_PORT {
            get {
                return ((int)(this["PLC_PORT"]));
            }
            set {
                this["PLC_PORT"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("3600")]
        public ushort PLC_RD {
            get {
                return ((ushort)(this["PLC_RD"]));
            }
            set {
                this["PLC_RD"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("3700")]
        public ushort PLC_WD {
            get {
                return ((ushort)(this["PLC_WD"]));
            }
            set {
                this["PLC_WD"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("100")]
        public ushort PLC_WLen {
            get {
                return ((ushort)(this["PLC_WLen"]));
            }
            set {
                this["PLC_WLen"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("100")]
        public ushort PLC_RLen {
            get {
                return ((ushort)(this["PLC_RLen"]));
            }
            set {
                this["PLC_RLen"] = value;
            }
        }
    }
}
