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

using System.Drawing;
using VoxelGame.GUI.Controls;
using VoxelGame.GUI.Graphics;
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
    public static readonly Brush BackgroundBrush = new SolidColorBrush(Color.FromArgb(red: 45, green: 45, blue: 48));

    /// <summary>
    ///     The default foreground brush.
    /// </summary>
    public static readonly Brush ForegroundBrush = new SolidColorBrush(Color.FromArgb(red: 255, green: 255, blue: 255));

    /// <summary>
    ///     The default disabled foreground brush.
    /// </summary>
    public static readonly Brush DisabledForegroundBrush = new SolidColorBrush(Color.FromArgb(red: 62, green: 62, blue: 64));

    /// <summary>
    ///     Default values specific to <see cref="IButton" />s.
    /// </summary>
    public static class Button
    {
        /// <summary>
        ///     The background brush of a button when hovered.
        /// </summary>
        public static readonly Brush HoveredBackgroundBrush = new SolidColorBrush(Color.FromArgb(red: 62, green: 62, blue: 64));

        /// <summary>
        ///     The foreground brush of a button when hovered.
        /// </summary>
        public static readonly Brush HoveredForegroundBrush = new SolidColorBrush(Color.FromArgb(red: 0, green: 122, blue: 204));

        /// <summary>
        ///     The background brush of a button when pressed.
        /// </summary>
        public static readonly Brush PressedBackgroundBrush = new SolidColorBrush(Color.FromArgb(red: 0, green: 122, blue: 204));

        /// <summary>
        ///     The foreground brush of a button when pressed.
        /// </summary>
        public static readonly Brush PressedForegroundBrush = new SolidColorBrush(Color.FromArgb(red: 255, green: 255, blue: 255));

        /// <summary>
        ///     The border brush of a button when focused.
        /// </summary>
        public static readonly Brush FocusedBorderBrush = new SolidColorBrush(Color.FromArgb(red: 0, green: 0, blue: 0));
    }
}
