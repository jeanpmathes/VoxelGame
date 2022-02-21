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
    /// <summary>
    ///     General game settings that are not part of any other settings category.
    ///     Changed settings in this class will be saved.
    /// </summary>
    public class GeneralSettings : ISettingsProvider
    {
        /// <summary>
        ///     A handler for when settings have been changed.
        /// </summary>
        /// <typeparam name="T">The type of the settings value.</typeparam>
        public delegate void GeneralSettingChangedHandler<T>(GeneralSettings settings, SettingChangedArgs<T> args);

        private readonly Settings clientSettings;
        private readonly List<Setting> settings = new();

        internal GeneralSettings(Settings clientSettings)
        {
            this.clientSettings = clientSettings;

            settings.Add(
                Setting.CreateColorSetting(
                    this,
                    Language.CrosshairColor,
                    () => CrosshairColor,
                    color => CrosshairColor = color));

            settings.Add(
                Setting.CreateFloatRangeSetting(
                    this,
                    Language.CrosshairScale,
                    () => CrosshairScale,
                    f => CrosshairScale = f,
                    min: 0f,
                    max: 0.5f));

            settings.Add(
                Setting.CreateFloatRangeSetting(
                    this,
                    Language.MouseSensitivity,
                    () => MouseSensitivity,
                    f => MouseSensitivity = f,
                    min: 0f,
                    max: 1f));
        }

        /// <summary>
        ///     Get or set the crosshair color setting.
        /// </summary>
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

        /// <summary>
        ///     Get or set the crosshair scale setting.
        /// </summary>
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

        /// <summary>
        ///     Get or set the mouse sensitivity setting.
        /// </summary>
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

        /// <inheritdoc />
        public string Category => Language.General;

        /// <inheritdoc />
        public string Description => Language.GeneralSettingsDescription;

        /// <inheritdoc />
        public IEnumerable<Setting> Settings => settings;

        /// <summary>
        ///     Is invoked when the crosshair color setting has been changed.
        /// </summary>
        public event GeneralSettingChangedHandler<Color>? CrosshairColorChanged;

        /// <summary>
        ///     Is invoked when the crosshair scale setting has been changed.
        /// </summary>
        public event GeneralSettingChangedHandler<float>? CrosshairScaleChanged;

        /// <summary>
        ///     Is invoked when the mouse sensitivity setting has been changed.
        /// </summary>
        public event GeneralSettingChangedHandler<float>? MouseSensitivityChanged;
    }
}
