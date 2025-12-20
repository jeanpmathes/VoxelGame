// <copyright file="ITextureIndexProvider.cs" company="VoxelGame">
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
using VoxelGame.Core.Utilities.Resources;

namespace VoxelGame.Core.Visuals;

/// <summary>
///     Can provide a texture index for a given texture.
/// </summary>
public interface ITextureIndexProvider : IResourceProvider
{
    /// <summary>
    ///     The index of the missing texture.
    ///     When loading textures, a client has to ensure that the missing texture is always present and at index 0.
    /// </summary>
    const Int32 MissingTextureIndex = 0;

    /// <summary>
    ///     Get the texture index for the given texture.
    /// </summary>
    /// <param name="identifier">The texture identifier.</param>
    /// <returns>The texture index.</returns>
    Int32 GetTextureIndex(TID identifier);
}
