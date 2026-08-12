// <copyright file="ActionStateChange.cs" company="VoxelGame">
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

namespace VoxelGame.Client.Inputs.Actions;

/// <summary>
///     Describes how the state of an <see cref="Action" /> changed since its previous update.
/// </summary>
/// <param name="IsDown">Whether the input combination is active.</param>
/// <param name="PressCount">The number of presses received since the previous update.</param>
/// <param name="ReleaseCount">The number of releases received since the previous update.</param>
internal readonly record struct ActionStateChange(Boolean IsDown, Int32 PressCount, Int32 ReleaseCount)
{
    /// <summary>
    ///     Get a state change that represents no active input or transitions.
    /// </summary>
    internal static ActionStateChange Neutral { get; } = new(IsDown: false, PressCount: 0, ReleaseCount: 0);
}
