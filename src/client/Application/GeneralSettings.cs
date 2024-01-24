﻿// <copyright file="GeneralSettings.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System.Collections.Generic;
using System.Drawing;
using Properties;
using VoxelGame.Core.Resources.Language;
using VoxelGame.UI.Providers;
using VoxelGame.UI.Settings;

namespace VoxelGame.Client.Application;

/// <summary>
///     General game settings that are not part of any other settings category.
///     Changed settings in this class will be saved.
/// </summary>
public class GeneralSettings : ISettingsProvider
{
    private readonly List<Setting> settings = new();

    internal GeneralSettings(Settings clientSettings)
    {
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

        CrosshairScale = new Bindable<float>(
            () => (float) clientSettings.CrosshairScale,
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

        MouseSensitivity = new Bindable<float>(
            () => (float) clientSettings.MouseSensitivity,
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
    /// The color of the crosshair.
    /// </summary>
    public Bindable<Color> CrosshairColor { get; }

    /// <summary>
    ///     Get or set the crosshair scale setting.
    /// </summary>
    public Bindable<float> CrosshairScale { get; }

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
    public Bindable<float> MouseSensitivity { get; }

    /// <inheritdoc />
    string ISettingsProvider.Category => Language.General;

    /// <inheritdoc />
    string ISettingsProvider.Description => Language.GeneralSettingsDescription;

    /// <inheritdoc />
    public IEnumerable<Setting> Settings => settings;
}
