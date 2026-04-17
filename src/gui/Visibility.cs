// <copyright file="Visibility.cs" company="VoxelGame">
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

namespace VoxelGame.GUI;

/// <summary>
///     The visibility of a control.
/// </summary>
public enum Visibility
{
    /// <summary>
    ///     The control is visible. Only visible controls can receive input and focus.
    /// </summary>
    Visible = 0,

    /// <summary>
    ///     The control is hidden, but still takes up space in the layout.
    /// </summary>
    Hidden = 1,

    /// <summary>
    ///     The control is collapsed and does not take up any space in the layout.
    /// </summary>
    Collapsed = 2
}

/// <summary>
///     Tools for working with visibility values.
/// </summary>
public static class Visibilities
{
    /// <summary>
    ///     Get a visibility value from a boolean. Visible if <c>true</c>, collapsed if <c>false</c>.
    /// </summary>
    /// <param name="visible">Whether the control should be visible.</param>
    /// <returns>A visibility value corresponding to the given boolean.</returns>
    public static Visibility FromBoolean(Boolean visible)
    {
        return visible ? Visibility.Visible : Visibility.Collapsed;
    }

    /// <summary>
    ///     Get the lower of two visibility values. The lower visibility is the one that is less visible.
    /// </summary>
    public static Visibility Lower(Visibility a, Visibility b)
    {
        return (Visibility) Math.Max((Int32) a, (Int32) b);
    }

    extension(Visibility visibility)
    {
        /// <summary>
        ///     Whether the element is visible.
        /// </summary>
        public Boolean IsVisible => visibility == Visibility.Visible;

        /// <summary>
        ///     Whether the element participates in layout. Hidden and visible elements do, collapsed elements don't.
        /// </summary>
        public Boolean IsLayouted => visibility != Visibility.Collapsed;
    }
}
