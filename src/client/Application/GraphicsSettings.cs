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
    private readonly List<Setting> settings = new();

    internal GraphicsSettings(Settings clientSettings)
    {
        FoliageQuality = new Bindable<Quality>(
            () => clientSettings.FoliageQuality,
            quality =>
            {
                clientSettings.FoliageQuality = quality;
                clientSettings.Save();
            });

        settings.Add(
            Setting.CreateQualitySetting(
                this,
                Language.GraphicsFoliageQuality,
                FoliageQuality.Accessors));

        WindowSize = new Bindable<Vector2i>(
            () => clientSettings.WindowSize.ToVector2i(),
            size =>
            {
                clientSettings.WindowSize = new Size(size.X, size.Y);
                clientSettings.Save();
            });

        settings.Add(
            Setting.CreateSizeSetting(
                this,
                Language.GraphicsWindowSize,
                WindowSize.Accessors));

        RenderResolutionScale = new Bindable<float>(
            () => (float) clientSettings.RenderResolutionScale,
            scale =>
            {
                clientSettings.RenderResolutionScale = scale;
                clientSettings.Save();
            });

        settings.Add(
            Setting.CreateFloatRangeSetting(
                this,
                Language.GraphicsRenderResolutionScale,
                RenderResolutionScale.Accessors,
                min: 0.1f,
                max: 5f,
                percentage: true));
    }

    /// <summary>
    /// The rendering quality of the foliage.
    /// </summary>
    public Bindable<Quality> FoliageQuality { get; }

    /// <summary>
    ///     The initial window size.
    /// </summary>
    public Bindable<Vector2i> WindowSize { get; }

    /// <summary>
    ///     The render resolution scale.
    /// </summary>
    public Bindable<float> RenderResolutionScale { get; }

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
