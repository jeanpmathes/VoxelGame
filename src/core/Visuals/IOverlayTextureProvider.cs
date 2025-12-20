// <copyright file="IOverlayTextureProvider.cs" company="VoxelGame">
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
using VoxelGame.Core.Logic.Voxels;

namespace VoxelGame.Core.Visuals;

/// <summary>
///     Describes an overlay texture.
/// </summary>
/// <param name="TextureIndex">The texture index, in the texture space of the content type.</param>
/// <param name="Tint">The tint color.</param>
/// <param name="IsAnimated">Whether the texture is animated.</param>
public record struct OverlayTexture(Int32 TextureIndex, ColorS Tint, Boolean IsAnimated);

/// <summary>
///     Provides an overlay texture index.
///     Blocks and fluids implementing this interface should be a full or varying height block for best effect.
/// </summary>
public interface IOverlayTextureProvider
{
    /// <summary>
    ///     Get the overlay texture.
    /// </summary>
    /// <param name="content">The content.</param>
    /// <returns>The overlay texture.</returns>
    OverlayTexture GetOverlayTexture(Content content);
}
