// <copyright file="Position.cs" company="VoxelGame">
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

using System.Diagnostics.CodeAnalysis;
using OpenTK.Mathematics;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Client.Console;

/// <summary>
///     Represents a 3D position argument provided to a console command.
/// </summary>
/// <param name="Vector3d">The position vector.</param>
[SuppressMessage("ReSharper", "InconsistentNaming", Justification = "That is the name of the type.")]
public readonly record struct Position(Vector3d Vector3d)
{
    /// <summary>
    ///     Get this position in integer coordinates.
    /// </summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "That is the name of the type.")]
    public Vector3i Vector3i => Vector3d.Floor();
}
