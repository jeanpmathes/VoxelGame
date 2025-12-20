// <copyright file="WindowSettings.cs" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2025 Jean Patrick Mathes
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
using OpenTK.Mathematics;

namespace VoxelGame.Graphics.Core;

/// <summary>
///     The initial window settings.
/// </summary>
public record WindowSettings
{
    /// <summary>
    ///     The title of the window.
    /// </summary>
    public String Title { get; init; } = "New Window";

    /// <summary>
    ///     The initial size of the window.
    /// </summary>
    public Vector2i Size { get; init; } = Vector2i.One;

    /// <summary>
    ///     The scale at which the world space is rendered.
    /// </summary>
    public Single RenderScale { get; init; } = 1.0f;

    /// <summary>
    ///     Gets a value indicating whether to enable special PIX Graphics.
    /// </summary>
    public Boolean SupportPIX { get; init; }

    /// <summary>
    ///     Gets a value indicating whether to use GBV.
    /// </summary>
    public Boolean UseGBV { get; init; }

    /// <summary>
    ///     Get a version of these settings with corrected values that are safe to use.
    /// </summary>
    public WindowSettings Corrected
        => this with {Size = new Vector2i(Math.Max(val1: 1, Size.X), Math.Max(val1: 1, Size.Y))};
}
