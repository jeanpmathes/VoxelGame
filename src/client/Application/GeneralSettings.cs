// <copyright file="GeneralSettings.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System.Collections.Generic;
using VoxelGame.Core.Resources.Language;
using VoxelGame.UI.Providers;
using VoxelGame.UI.Settings;

namespace VoxelGame.Client.Application
{
    public class GeneralSettings : ISettingsProvider
    {
        private readonly List<Setting> settings = new();

        public string Category => Language.General;
        public string Description => Language.GeneralSettingsDescription;

        public IEnumerable<Setting> Settings => settings;
    }
}
