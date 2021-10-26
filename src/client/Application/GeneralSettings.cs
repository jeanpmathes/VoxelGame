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

        public string Category => Language.General;
        public string Description => Language.GeneralSettingsDescription;

        public IEnumerable<Setting> Settings => settings;

        public event GeneralSettingChangedHandler<Color>? CrosshairColorChanged;
    }
}