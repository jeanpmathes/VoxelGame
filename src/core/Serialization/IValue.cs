// <copyright file="IValue.cs" company="VoxelGame">
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

namespace VoxelGame.Core.Serialization;

/// <summary>
///     Interface for values that can be serialized.
///     Values use simple serialization without versioning.
///     As no versioning is used, the contents should not change.
///     Implementors should provide a parameterless constructor.
/// </summary>
public interface IValue
{
    /// <summary>
    ///     Serialize the value.
    /// </summary>
    /// <param name="serializer">The serializer to use.</param>
    void Serialize(Serializer serializer);
}
