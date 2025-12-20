// <copyright file="PreviousPosition.cs" company="VoxelGame">
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

using OpenTK.Mathematics;
using VoxelGame.Annotations.Attributes;
using VoxelGame.Core.Actors;

namespace VoxelGame.Client.Actors.Components;

/// <summary>
///     Stores the previous position of an actor, e.g. before a teleportation.
///     Is used only by the command system.
/// </summary>
public partial class PreviousPosition : ActorComponent
{
    [Constructible]
    private PreviousPosition(Actor subject) : base(subject) {}

    /// <summary>
    ///     Get the previous position of the actor, as set by the command system.
    /// </summary>
    public Vector3d Value { get; set; } = Vector3d.Zero;
}
