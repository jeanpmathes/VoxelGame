// <copyright file="GeneralSettings.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System.Collections.Generic;
using System.Drawing;
using Properties;
using VoxelGame.Core.Resources.Language;
using VoxelGame.UI.Providers;
using VoxelGame.UI.Settings;

namespace VoxelGame.Client.Application
{
    public class GeneralSettings : ISettingsProvider
    {
        public delegate void GeneralSettingChangedHandler<T>(GeneralSettings settings, SettingChangedArgs<T> args);

        private readonly Settings clientSettings;
        private readonly List<Setting> settings = new();

        internal GeneralSettings(Settings clientSettings)
        {
            this.clientSettings = clientSettings;

            settings.Add(
                Setting.CreateColorSetting(
                    Language.CrosshairColor,
                    () => CrosshairColor,
                    color => CrosshairColor = color));

            settings.Add(
                Setting.CreateFloatRangeSetting(
                    Language.CrosshairScale,
                    () => CrosshairScale,
                    f => CrosshairScale = f,
                    min: 0f,
                    max: 0.5f));

            settings.Add(
                Setting.CreateFloatRangeSetting(
                    Language.MouseSensitivity,
                    () => MouseSensitivity,
                    f => MouseSensitivity = f,
                    min: 0f,
                    max: 1f));
        }

        public Color CrosshairColor
        {
            get => clientSettings.CrosshairColor;
            private set
            {
                Color old = CrosshairColor;

                clientSettings.CrosshairColor = value;
                clientSettings.Save();

                CrosshairColorChanged?.Invoke(this, new SettingChangedArgs<Color>(old, value));
            }
        }

        public float CrosshairScale
        {
            get => clientSettings.CrosshairScale;
            private set
            {
                float old = CrosshairScale;

                clientSettings.CrosshairScale = value;
                clientSettings.Save();

                CrosshairScaleChanged?.Invoke(this, new SettingChangedArgs<float>(old, value));
            }
        }

        public float MouseSensitivity
        {
            get => clientSettings.MouseSensitivity;
            private set
            {
                float old = MouseSensitivity;

                clientSettings.MouseSensitivity = value;
                clientSettings.Save();

                MouseSensitivityChanged?.Invoke(this, new SettingChangedArgs<float>(old, value));
            }
        }

        public string Category => Language.General;
        public string Description => Language.GeneralSettingsDescription;

        public IEnumerable<Setting> Settings => settings;

        public event GeneralSettingChangedHandler<Color>? CrosshairColorChanged;
        public event GeneralSettingChangedHandler<float>? CrosshairScaleChanged;
        public event GeneralSettingChangedHandler<float>? MouseSensitivityChanged;
    }
}