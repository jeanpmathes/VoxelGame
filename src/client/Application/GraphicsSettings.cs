// <copyright file="GraphicsSettings.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System.Collections.Generic;
using Properties;
using VoxelGame.Core.Resources.Language;
using VoxelGame.UI.Providers;
using VoxelGame.UI.Settings;

namespace VoxelGame.Client.Application
{
    public class GraphicsSettings : ISettingsProvider
    {
        private readonly Settings clientSettings;

        private readonly List<Setting> settings = new();

        internal GraphicsSettings(Settings clientSettings)
        {
            this.clientSettings = clientSettings;

            settings.Add(
                Setting.CreateIntegerSetting(
                    Language.GraphicsSampleCount,
                    () => SampleCount,
                    i => SampleCount = i,
                    min: 1));

            settings.Add(Setting.CreateIntegerSetting(Language.GraphicsMaxFPS, () => MaxFPS, i => MaxFPS = i, min: 0));
        }

        public int SampleCount
        {
            get => clientSettings.SampleCount;

            private set
            {
                clientSettings.SampleCount = value;
                clientSettings.Save();
            }
        }

        public int MaxFPS
        {
            get => clientSettings.MaxFPS;

            private set
            {
                clientSettings.MaxFPS = value;
                clientSettings.Save();
            }
        }

        public string Category => Language.Graphics;
        public IEnumerable<Setting> Settings => settings;
    }
}