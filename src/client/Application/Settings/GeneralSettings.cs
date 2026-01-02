// <copyright file="GeneralSettings.cs" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2026 Jean Patrick Mathes
//      
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
//     
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//     
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <https://www.gnu.org/licenses/>.
// </copyright>
// <author>jeanpmathes</author>

using System;
using Microsoft.Extensions.Logging;
using VoxelGame.Core.Resources.Language;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Visuals;
using VoxelGame.Logging;
using VoxelGame.UI.Providers;
using VoxelGame.UI.Settings;

namespace VoxelGame.Client.Application.Settings;

/// <summary>
///     General game settings that are not part of any other settings category.
///     Changed settings in this class will be saved.
/// </summary>
public sealed class GeneralSettings : SettingsBase, ISettingsProvider, IScaleProvider
{
    internal GeneralSettings(Properties.Settings clientSettings)
    {
        ScaleOfUI = new Bindable<Single>(
            () => (Single) clientSettings.ScaleOfUI,
            f =>
            {
                clientSettings.ScaleOfUI = f;
                clientSettings.Save();
            });

        AddSetting(nameof(ScaleOfUI),
            Setting.CreateFloatRangeSetting(
                this,
                Language.ScaleOfUI,
                ScaleOfUI.Accessors,
                min: 0.25f,
                max: 3f,
                step: 0.05f));

        CrosshairColor = new Bindable<ColorS>(
            () => ColorS.FromColor(clientSettings.CrosshairColor),
            color =>
            {
                clientSettings.CrosshairColor = color.ToColor();
                clientSettings.Save();
            });

        AddSetting(nameof(CrosshairColor),
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

        AddSetting(nameof(CrosshairScale),
            Setting.CreateFloatRangeSetting(
                this,
                Language.CrosshairScale,
                CrosshairScale.Accessors,
                min: 0f,
                max: 0.5f));

        DarkSelectionColor = new Bindable<ColorS>(
            () => ColorS.FromColor(clientSettings.DarkSelectionColor),
            color =>
            {
                clientSettings.DarkSelectionColor = color.ToColor();
                clientSettings.Save();
            });

        AddSetting(nameof(DarkSelectionColor),
            Setting.CreateColorSetting(
                this,
                Language.SelectionBoxDarkColor,
                DarkSelectionColor.Accessors));

        BrightSelectionColor = new Bindable<ColorS>(
            () => ColorS.FromColor(clientSettings.BrightSelectionColor),
            color =>
            {
                clientSettings.BrightSelectionColor = color.ToColor();
                clientSettings.Save();
            });

        AddSetting(nameof(BrightSelectionColor),
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

        AddSetting(nameof(MouseSensitivity),
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
    public Bindable<ColorS> CrosshairColor { get; }

    /// <summary>
    ///     Get or set the crosshair scale setting.
    /// </summary>
    public Bindable<Single> CrosshairScale { get; }

    /// <summary>
    ///     The color of the selection box on bright background.
    /// </summary>
    public Bindable<ColorS> DarkSelectionColor { get; }

    /// <summary>
    ///     The color of the selection box on dark background.
    /// </summary>
    public Bindable<ColorS> BrightSelectionColor { get; }

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

    #region LOGGING

    private static readonly ILogger logger = LoggingHelper.CreateLogger<GeneralSettings>();

    /// <inheritdoc />
    protected override ILogger Logger => logger;

    #endregion LOGGING
}
