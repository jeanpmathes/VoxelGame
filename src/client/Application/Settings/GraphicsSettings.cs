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
                min: 0.1f,
                max: 5f,
                percentage: true,
                step: 0.1f));
    }

    #region LOGGING

    private static readonly ILogger logger = LoggingHelper.CreateLogger<GraphicsSettings>();

    /// <inheritdoc />
    protected override ILogger Logger => logger;

    #endregion LOGGING
}
