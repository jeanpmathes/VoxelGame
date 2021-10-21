// <copyright file="ISettingsProvider.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System.Collections.Generic;
using VoxelGame.UI.Settings;

namespace VoxelGame.UI.Providers
{
    public interface ISettingsProvider
    {
        public string Category { get; }

        public IEnumerable<Setting> Settings { get; }
    }
}