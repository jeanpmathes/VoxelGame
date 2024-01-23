// <copyright file="GraphicsSettings.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using OpenTK.Mathematics;
using Properties;
using VoxelGame.Core.Resources.Language;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Visuals;
using VoxelGame.UI.Providers;
using VoxelGame.UI.Settings;

namespace VoxelGame.Client.Application;

/// <summary>
///     Game settings concerning the game graphics and visuals.
/// </summary>
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public class GraphicsSettings : ISettingsProvider
{
    private readonly Settings clientSettings;

    private readonly List<Setting> settings = new();

    internal GraphicsSettings(Settings clientSettings)
    {
        this.clientSettings = clientSettings;

        settings.Add(
            Setting.CreateQualitySetting(
                this,
                Language.GraphicsFoliageQuality,
                () => FoliageQuality,
                quality => FoliageQuality = quality));

        settings.Add(
            Setting.CreateSizeSetting(
                this,
                Language.GraphicsWindowSize,
                () => WindowSize,
                size => WindowSize = size));

        settings.Add(
            Setting.CreateFloatRangeSetting(
                this,
                Language.GraphicsRenderResolutionScale,
                () => RenderResolutionScale,
                scale => RenderResolutionScale = scale,
                min: 0.1f,
                max: 5f,
                percentage: true));
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
    ///     Get or set the window size setting.
    /// </summary>
    public Vector2i WindowSize
    {
        get => clientSettings.WindowSize.ToVector2i();

        private set
        {
            clientSettings.WindowSize = new Size(value.X, value.Y);
            clientSettings.Save();
        }
    }

    /// <summary>
    ///     Get or set the render resolution scale setting.
    /// </summary>
    public float RenderResolutionScale
    {
        get => (float) clientSettings.RenderResolutionScale;

        private set
        {
            clientSettings.RenderResolutionScale = value;
            clientSettings.Save();
        }
    }

    /// <summary>
    ///     Get the visual configuration from the settings.
    /// </summary>
    public VisualConfiguration VisualConfiguration => new()
    {
        FoliageQuality = FoliageQuality
    };

    /// <inheritdoc />
    [SuppressMessage("Performance", "CA1822:Mark members as static")]
    public string Category => Language.Graphics;

    /// <inheritdoc />
    [SuppressMessage("Performance", "CA1822:Mark members as static")]
    public string Description => Language.GraphicsSettingsDescription;

    /// <inheritdoc />
    public IEnumerable<Setting> Settings => settings;
}
