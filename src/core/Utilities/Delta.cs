// <copyright file="Delta.cs" company="VoxelGame">
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

namespace VoxelGame.Core.Utilities;

/// <summary>
///     Utility struct holding the delta time of an update.
/// </summary>
/// <param name="RealTime">
///     The delta time, in real time seconds.
///     This time should be used only in some specific cases, such as player input handling.
/// </param>
/// <param name="Time">
///     The delta time, in scaled seconds.
///     This time should be used for most cases, especially physics and game logic.
/// </param>
public readonly record struct Delta(Double RealTime, Double Time);
