// <copyright file="GraphicsSettings.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Properties;
using VoxelGame.Core.Resources.Language;
using VoxelGame.Core.Visuals;
using VoxelGame.UI.Providers;
using VoxelGame.UI.Settings;

namespace VoxelGame.Client.Application;

/// <summary>
///     Game settings concerning the game graphics and visuals.
/// </summary>
public class GraphicsSettings : ISettingsProvider
{
    private readonly Settings clientSettings;

    private readonly List<Setting> settings = new();

    internal GraphicsSettings(Settings clientSettings)
    {
        this.clientSettings = clientSettings;

        settings.Add(
            Setting.CreateIntegerSetting(
                this,
                Language.GraphicsSampleCount,
                () => SampleCount,
                i => SampleCount = i,
                min: 1)); // todo: use this, maybe in ray gen (and adapt description accordingly)

        settings.Add(
            Setting.CreateQualitySetting(
                this,
                Language.GraphicsFoliageQuality,
                () => FoliageQuality,
                quality => FoliageQuality = quality)); // todo: use this, maybe in the intersect shader
    }

    /// <summary>
    ///     Get or set the sample count setting. This is the number of samples used for anti-aliasing.
    /// </summary>
    public int SampleCount
    {
        get => clientSettings.SampleCount;

        private set
        {
            clientSettings.SampleCount = value;
            clientSettings.Save();
        }
    }

    /// <summary>
    ///     Get or set the foliage quality setting.
    /// </summary>
    public Quality FoliageQuality
    {
        get => clientSettings.FoliageQuality;

        private set
        {
            clientSettings.FoliageQuality = value;
            clientSettings.Save();
        }
    }

    /// <inheritdoc />
    [SuppressMessage("Performance", "CA1822:Mark members as static")]
    public string Category => Language.Graphics;

    /// <inheritdoc />
    [SuppressMessage("Performance", "CA1822:Mark members as static")]
    public string Description => Language.GraphicsSettingsDescription;

    /// <inheritdoc />
    public IEnumerable<Setting> Settings => settings;
}

