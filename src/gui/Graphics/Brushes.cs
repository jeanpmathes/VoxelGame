// <copyright file="Brushes.cs" company="VoxelGame">
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

namespace VoxelGame.GUI.Graphics;

/// <summary>
///     A utility to access useful predefined brushes.
/// </summary>
public static class Brushes
{
    /// <summary>
    ///     Gets a brush that is completely transparent.
    /// </summary>
    public static Brush Transparent { get; } = new TransparentBrush();

    /// <summary>
    ///     Gets a brush that is solid white.
    /// </summary>
    public static Brush White { get; } = new SolidColorBrush(Color.White);

    /// <summary>
    ///     Gets a brush that is solid black.
    /// </summary>
    public static Brush Black { get; } = new SolidColorBrush(Color.Black);

    /// <summary>
    ///     Gets a brush that is solid red.
    /// </summary>
    public static Brush Red { get; } = new SolidColorBrush(Color.Red);

    /// <summary>
    ///     Gets a brush that is solid green.
    /// </summary>
    public static Brush Green { get; } = new SolidColorBrush(Color.Green);

    /// <summary>
    ///     Gets a brush that is solid blue.
    /// </summary>
    public static Brush Blue { get; } = new SolidColorBrush(Color.Blue);

    /// <summary>
    ///     Get a brush to draw debug bounds with.
    /// </summary>
    public static Brush DebugBounds => Red;

    /// <summary>
    ///     Get a brush to draw debug margin outlines with.
    /// </summary>
    public static Brush DebugMargin => Blue;

    /// <summary>
    ///     Get a brush to draw debug padding outlines with.
    /// </summary>
    public static Brush DebugPadding => Green;
}
