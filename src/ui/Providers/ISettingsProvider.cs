// <copyright file="ISettingsProvider.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System.Collections.Generic;
using VoxelGame.UI.Settings;

namespace VoxelGame.UI.Providers;

/// <summary>
///     Provide the user interface with settings for a given category.
/// </summary>
public interface ISettingsProvider
{
    /// <summary>
    ///     The name of the settings category.
    /// </summary>
    public string Category { get; }

    /// <summary>
    ///     A description for the settings category.
    /// </summary>
    public string Description { get; }

    /// <summary>
    ///     Get all settings for this category.
    /// </summary>
    public IEnumerable<Setting> Settings { get; }

    internal void Validate()
    {
        foreach (Setting setting in Settings) setting.Validate();
    }
}

