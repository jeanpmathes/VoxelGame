// <copyright file="GraphicsSettings.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Properties;
using VoxelGame.Client.Rendering;
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
                min: 1));

        settings.Add(
            Setting.CreateIntegerSetting(
                this,
                Language.GraphicsAnisotropicFiltering,
                () => Anisotropy,
                i => Anisotropy = i,
                min: 1));

        settings.Add(
            Setting.CreateIntegerSetting(this, Language.GraphicsMaxFPS, () => MaxFPS, i => MaxFPS = i, min: 0));

        settings.Add(
            Setting.CreateQualitySetting(
                this,
                Language.GraphicsFoliageQuality,
                () => FoliageQuality,
                quality => FoliageQuality = quality));

        settings.Add(
            Setting.CreateBooleanSetting(
                this,
                Language.GraphicsUseFullscreenBorderless,
                () => UseFullscreenBorderless,
                b => UseFullscreenBorderless = b));
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
    ///     Get or set the anisotropic filtering value.
    /// </summary>
    public int Anisotropy
    {
        get => clientSettings.AnisotropicFiltering;

        private set
        {
            clientSettings.AnisotropicFiltering = value;
            clientSettings.Save();
        }
    }

    /// <summary>
    ///     Get or set the maximum FPS setting. This is the maximum FPS that are passed to the window on creation.
    /// </summary>
    public int MaxFPS
    {
        get => clientSettings.MaxFPS;

        private set
        {
            clientSettings.MaxFPS = value;
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

    /// <summary>
    ///     Get or set whether fullscreen borderless should be used instead of normal fullscreen.
    /// </summary>
    public bool UseFullscreenBorderless
    {
        get => clientSettings.UseFullscreenBorderless;

        private set
        {
            clientSettings.UseFullscreenBorderless = value;
            clientSettings.Save();

            Screen.UpdateScreenState();
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


