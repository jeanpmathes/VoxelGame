// <copyright file="GraphicsSettings.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Drawing;
using Microsoft.Extensions.Logging;
using OpenTK.Mathematics;
using VoxelGame.Core.Resources.Language;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Visuals;
using VoxelGame.Logging;
using VoxelGame.UI.Providers;
using VoxelGame.UI.Settings;

namespace VoxelGame.Client.Application.Settings;

/// <summary>
///     Game settings concerning the game graphics and visuals.
/// </summary>
public sealed class GraphicsSettings : SettingsBase, ISettingsProvider
{
    internal GraphicsSettings(Properties.Settings clientSettings)
    {
        FoliageQuality = new Bindable<Quality>(
            () => clientSettings.FoliageQuality,
            quality =>
            {
                clientSettings.FoliageQuality = quality;
                clientSettings.Save();
            });

        WindowSize = new Bindable<Vector2i>(
            () => new Vector2i(clientSettings.WindowSize.Width, clientSettings.WindowSize.Height),
            size =>
            {
                clientSettings.WindowSize = new Size(size.X, size.Y);
                clientSettings.Save();
            });

        RenderResolutionScale = new Bindable<Single>(
            () => (Single) clientSettings.RenderResolutionScale,
            scale =>
            {
                clientSettings.RenderResolutionScale = scale;
                clientSettings.Save();
            });

        PostProcessingAntiAliasingQuality = new Bindable<Quality>(
            () => clientSettings.PostProcessingAntiAliasingQuality,
            q =>
            {
                clientSettings.PostProcessingAntiAliasingQuality = q;
                clientSettings.Save();
            });

        RenderingAntiAliasingQuality = new Bindable<Quality>(
            () => clientSettings.RenderingAntiAliasingQuality,
            q =>
            {
                clientSettings.RenderingAntiAliasingQuality = q;
                clientSettings.Save();
            });

        AnisotropicFilteringQuality = new Bindable<Quality>(
            () => clientSettings.AnisotropicFilteringQuality,
            q =>
            {
                clientSettings.AnisotropicFilteringQuality = q;
                clientSettings.Save();
            });
    }

    /// <summary>
    ///     The rendering quality of the foliage.
    /// </summary>
    public Bindable<Quality> FoliageQuality { get; }

    /// <summary>
    ///     The initial window size.
    /// </summary>
    public Bindable<Vector2i> WindowSize { get; }

    /// <summary>
    ///     The render resolution scale.
    /// </summary>
    public Bindable<Single> RenderResolutionScale { get; }

    /// <summary>
    ///     The antialiasing quality level used during post-processing.
    /// </summary>
    public Bindable<Quality> PostProcessingAntiAliasingQuality { get; }

    /// <summary>
    ///     The antialiasing quality level used during ray-based rendering.
    /// </summary>
    public Bindable<Quality> RenderingAntiAliasingQuality { get; }

    /// <summary>
    ///     The anisotropic filtering quality level used during ray-based rendering.
    /// </summary>
    public Bindable<Quality> AnisotropicFilteringQuality { get; }

    /// <summary>
    ///     Get the visual configuration from the settings.
    /// </summary>
    public VisualConfiguration VisualConfiguration => new()
    {
        FoliageQuality = FoliageQuality
    };

    /// <inheritdoc />
    static String ISettingsProvider.Category => Language.Graphics;

    /// <inheritdoc />
    static String ISettingsProvider.Description => Language.GraphicsSettingsDescription;

    /// <summary>
    ///     Create the actual settings for the properties of this class.
    /// </summary>
    /// <param name="client">The client which is using these settings.</param>
    internal void CreateSettings(Graphics.Core.Client client)
    {
        AddSetting(nameof(FoliageQuality),
            Setting.CreateQualitySetting(
                this,
                Language.GraphicsFoliageQuality,
                FoliageQuality.Accessors));

        AddSetting(nameof(WindowSize),
            Setting.CreateSizeSetting(
                this,
                Language.GraphicsWindowSize,
                WindowSize.Accessors,
                () => client.Size));

        AddSetting(nameof(RenderResolutionScale),
            Setting.CreateFloatRangeSetting(
                this,
                Language.GraphicsRenderResolutionScale,
                RenderResolutionScale.Accessors,
                min: 0.5f,
                max: 5f,
                percentage: true,
                step: 0.5f));

        AddSetting(nameof(PostProcessingAntiAliasingQuality),
            Setting.CreateQualitySetting(
                this,
                Language.GraphicsPostProcessingAntiAliasingQuality,
                PostProcessingAntiAliasingQuality.Accessors));

        AddSetting(nameof(RenderingAntiAliasingQuality),
            Setting.CreateQualitySetting(
                this,
                Language.GraphicsRenderingAntiAliasingQuality,
                RenderingAntiAliasingQuality.Accessors));

        AddSetting(nameof(AnisotropicFilteringQuality),
            Setting.CreateQualitySetting(
                this,
                Language.GraphicsAnisotropicFilteringQuality,
                AnisotropicFilteringQuality.Accessors));
    }

    #region LOGGING

    private static readonly ILogger logger = LoggingHelper.CreateLogger<GraphicsSettings>();

    /// <inheritdoc />
    protected override ILogger Logger => logger;

    #endregion LOGGING
}
