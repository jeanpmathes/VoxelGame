﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System.CodeDom.Compiler;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.CompilerServices;
using VoxelGame.Core.Visuals;

namespace Properties {
    
    
    [CompilerGenerated()]
    [GeneratedCode("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "15.9.0.0")]
    internal sealed partial class Settings : ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [UserScopedSetting()]
        [DebuggerNonUserCode()]
        [DefaultSettingValue("0.3")]
        public double MouseSensitivity {
            get {
                return ((double)(this["MouseSensitivity"]));
            }
            set {
                this["MouseSensitivity"] = value;
            }
        }
        
        [UserScopedSetting()]
        [DebuggerNonUserCode()]
        [DefaultSettingValue("0.0225")]
        public double CrosshairScale {
            get {
                return ((double)(this["CrosshairScale"]));
            }
            set {
                this["CrosshairScale"] = value;
            }
        }
        
        [UserScopedSetting()]
        [DebuggerNonUserCode()]
        [DefaultSettingValue("White")]
        public Color CrosshairColor {
            get {
                return ((Color)(this["CrosshairColor"]));
            }
            set {
                this["CrosshairColor"] = value;
            }
        }
        
        [UserScopedSetting()]
        [DebuggerNonUserCode()]
        [DefaultSettingValue("480, 270")]
        public Size WindowSize {
            get {
                return ((Size)(this["WindowSize"]));
            }
            set {
                this["WindowSize"] = value;
            }
        }
        
        [UserScopedSetting()]
        [DebuggerNonUserCode()]
        [DefaultSettingValue("Medium")]
        public Quality FoliageQuality {
            get {
                return ((Quality)(this["FoliageQuality"]));
            }
            set {
                this["FoliageQuality"] = value;
            }
        }
        
        [UserScopedSetting()]
        [DebuggerNonUserCode()]
        [DefaultSettingValue("1")]
        public double RenderResolutionScale {
            get {
                return ((double)(this["RenderResolutionScale"]));
            }
            set {
                this["RenderResolutionScale"] = value;
            }
        }
        
        [UserScopedSetting()]
        [DebuggerNonUserCode()]
        [DefaultSettingValue("25, 25, 25")]
        public Color DarkSelectionColor {
            get {
                return ((Color)(this["DarkSelectionColor"]));
            }
            set {
                this["DarkSelectionColor"] = value;
            }
        }
        
        [UserScopedSetting()]
        [DebuggerNonUserCode()]
        [DefaultSettingValue("150, 150, 150")]
        public Color BrightSelectionColor {
            get {
                return ((Color)(this["BrightSelectionColor"]));
            }
            set {
                this["BrightSelectionColor"] = value;
            }
        }
        
        [UserScopedSetting()]
        [DebuggerNonUserCode()]
        [DefaultSettingValue("1")]
        public double ScaleOfUI {
            get {
                return ((double)(this["ScaleOfUI"]));
            }
            set {
                this["ScaleOfUI"] = value;
            }
        }
    }
}
