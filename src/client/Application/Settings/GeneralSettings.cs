// <copyright file="GeneralSettings.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.Drawing;
using VoxelGame.Core.Resources.Language;
using VoxelGame.Core.Utilities;
using VoxelGame.UI.Providers;
using VoxelGame.UI.Settings;

namespace VoxelGame.Client.Application.Settings;

/// <summary>
///     General game settings that are not part of any other settings category.
///     Changed settings in this class will be saved.
/// </summary>
public sealed class GeneralSettings : ISettingsProvider, IScaleProvider
{
    private readonly List<Setting> settings = [];

    internal GeneralSettings(Properties.Settings clientSettings)
    {
        ScaleOfUI = new Bindable<Single>(
            () => (Single) clientSettings.ScaleOfUI,
            f =>
            {
                clientSettings.ScaleOfUI = f;
                clientSettings.Save();
            });

        settings.Add(
            Setting.CreateFloatRangeSetting(
                this,
                Language.ScaleOfUI,
                ScaleOfUI.Accessors,
                min: 0.25f,
                max: 3f,
                step: 0.05f));

        CrosshairColor = new Bindable<Color>(
            () => clientSettings.CrosshairColor,
            color =>
            {
                clientSettings.CrosshairColor = color;
                clientSettings.Save();
            });

        settings.Add(
            Setting.CreateColorSetting(
                this,
                Language.CrosshairColor,
                CrosshairColor.Accessors));

        CrosshairScale = new Bindable<Single>(
            () => (Single) clientSettings.CrosshairScale,
            f =>
            {
                clientSettings.CrosshairScale = f;
                clientSettings.Save();
            });

        settings.Add(
            Setting.CreateFloatRangeSetting(
                this,
                Language.CrosshairScale,
                CrosshairScale.Accessors,
                min: 0f,
                max: 0.5f));

        DarkSelectionColor = new Bindable<Color>(
            () => clientSettings.DarkSelectionColor,
            color =>
            {
                clientSettings.DarkSelectionColor = color;
                clientSettings.Save();
            });

        settings.Add(
            Setting.CreateColorSetting(
                this,
                Language.SelectionBoxDarkColor,
                DarkSelectionColor.Accessors));

        BrightSelectionColor = new Bindable<Color>(
            () => clientSettings.BrightSelectionColor,
            color =>
            {
                clientSettings.BrightSelectionColor = color;
                clientSettings.Save();
            });

        settings.Add(
            Setting.CreateColorSetting(
                this,
                Language.SelectionBoxBrightColor,
                BrightSelectionColor.Accessors));

        MouseSensitivity = new Bindable<Single>(
            () => (Single) clientSettings.MouseSensitivity,
            f =>
            {
                clientSettings.MouseSensitivity = f;
                clientSettings.Save();
            });

        settings.Add(
            Setting.CreateFloatRangeSetting(
                this,
                Language.MouseSensitivity,
                MouseSensitivity.Accessors,
                min: 0f,
                max: 1f));
    }

    /// <summary>
    ///     The scale factor of the UI.
    /// </summary>
    private Bindable<Single> ScaleOfUI { get; }

    /// <summary>
    ///     The color of the crosshair.
    /// </summary>
    public Bindable<Color> CrosshairColor { get; }

    /// <summary>
    ///     Get or set the crosshair scale setting.
    /// </summary>
    public Bindable<Single> CrosshairScale { get; }

    /// <summary>
    ///     The color of the selection box on bright background.
    /// </summary>
    public Bindable<Color> DarkSelectionColor { get; }

    /// <summary>
    ///     The color of the selection box on dark background.
    /// </summary>
    public Bindable<Color> BrightSelectionColor { get; }

    /// <summary>
    ///     Get or set the mouse sensitivity setting.
    /// </summary>
    public Bindable<Single> MouseSensitivity { get; }

    /// <inheritdoc />
    Single IScaleProvider.Scale => ScaleOfUI;

    /// <inheritdoc />
    IDisposable IScaleProvider.Subscribe(Action<Single> action)
    {
        return ScaleOfUI.Bind(args => action(args.NewValue));
    }

    /// <inheritdoc />
    static String ISettingsProvider.Category => Language.General;

    /// <inheritdoc />
    static String ISettingsProvider.Description => Language.GeneralSettingsDescription;

    /// <inheritdoc />
    public IEnumerable<Setting> Settings => settings;
}
