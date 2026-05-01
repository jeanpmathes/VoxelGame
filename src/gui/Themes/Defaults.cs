// <copyright file="Defaults.cs" company="VoxelGame">
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
using System.Drawing;
using VoxelGame.GUI.Controls;
using VoxelGame.GUI.Graphics;
using VoxelGame.GUI.Styles;
using VoxelGame.GUI.Utilities;
using Brush = VoxelGame.GUI.Graphics.Brush;

namespace VoxelGame.GUI.Themes;

/// <summary>
///     Default values for control styling and design.
/// </summary>
public static class Defaults
{
    /// <summary>
    ///     The default background brush.
    /// </summary>
    public static readonly Brush BackgroundBrush = new SolidColorBrush(Color.FromArgb(red: 28, green: 28, blue: 28));

    /// <summary>
    ///     The default background brush.
    /// </summary>
    public static readonly Brush TransparentBackgroundBrush = new SolidColorBrush(Color.FromArgb(red: 28, green: 28, blue: 28, alpha: 204));

    /// <summary>
    ///     The default darker / alternative background brush.
    /// </summary>
    public static readonly Brush BackgroundAlternativeBrush = new SolidColorBrush(Color.FromArgb(red: 23, green: 23, blue: 23));

    /// <summary>
    ///     The default foreground brush.
    /// </summary>
    public static readonly Brush ForegroundBrush = new SolidColorBrush(Color.FromArgb(red: 211, green: 211, blue: 211));

    /// <summary>
    ///     The default interactive background brush.
    /// </summary>
    public static readonly Brush InteractiveBackgroundBrush = ForegroundBrush;

    /// <summary>
    ///     The default interactive foreground brush.
    /// </summary>
    public static readonly Brush InteractiveForegroundBrush = BackgroundAlternativeBrush;

    /// <summary>
    ///     The default disabled background brush.
    /// </summary>
    public static readonly Brush DisabledBackgroundBrush = new SolidColorBrush(Color.FromArgb(red: 193, green: 193, blue: 193));

    /// <summary>
    ///     The default disabled foreground brush.
    /// </summary>
    public static readonly Brush DisabledForegroundBrush = new SolidColorBrush(Color.FromArgb(red: 112, green: 112, blue: 112));

    /// <summary>
    ///     The default radius of borders and similar.
    /// </summary>
    public static readonly RadiusF Radius = new(10.0f);

    /// <summary>
    ///     Create a default style for a <see cref="Canvas" />.
    /// </summary>
    /// <param name="isTransparent">Whether the canvas should use the transparent background color.</param>
    /// <returns>The created canvas style.</returns>
    public static Style<Canvas> CreateCanvasStyle(Boolean isTransparent)
    {
        return Styling.Create<Canvas>($"{nameof(Defaults)}.{nameof(Canvas)}", s => s.Set(c => c.Background, isTransparent ? TransparentBackgroundBrush : BackgroundBrush));
    }

    /// <summary>
    ///     Default values specific to <see cref="IButton" />s.
    /// </summary>
    public static class Button
    {
        /// <summary>
        ///     The background brush of a button when hovered.
        /// </summary>
        public static readonly Brush HoveredBackgroundBrush = new SolidColorBrush(Color.FromArgb(red: 241, green: 241, blue: 241));

        /// <summary>
        ///     The background brush of a button when pressed.
        /// </summary>
        public static readonly Brush PressedBackgroundBrush = new SolidColorBrush(Color.FromArgb(red: 187, green: 187, blue: 187));

        /// <summary>
        ///     The border brush of a button when focused.
        /// </summary>
        public static readonly Brush FocusedBorderBrush = new SolidColorBrush(Color.FromArgb(red: 0, green: 0, blue: 0));
    }
}
