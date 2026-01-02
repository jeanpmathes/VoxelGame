// <copyright file="IEntity.cs" company="VoxelGame">
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

namespace VoxelGame.Core.Serialization;

/// <summary>
///     Interface for entities that can be serialized.
///     An entity uses versioned serialization.
///     Implementors should be classes and not structs.
/// </summary>
public interface IEntity
{
    /// <summary>
    ///     Get the current version of the entity.
    /// </summary>
    static abstract UInt32 CurrentVersion { get; }

    /// <summary>
    ///     Serialize the entity.
    /// </summary>
    /// <param name="serializer">The serializer to use.</param>
    /// <param name="header">The header of the entity.</param>
    void Serialize(Serializer serializer, Header header);

    /// <summary>
    ///     Header of an entity.
    /// </summary>
    /// <param name="Version">The version of the entity.</param>
    record struct Header(UInt32 Version);
}
