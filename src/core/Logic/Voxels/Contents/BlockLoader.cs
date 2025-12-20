// <copyright file="BlockLoader.cs" company="VoxelGame">
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
using System.Collections.Generic;
using VoxelGame.Core.Utilities.Resources;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Voxels.Contents;

/// <summary>
///     Loads all blocks.
/// </summary>
public sealed class BlockLoader : IResourceLoader
{
    String? ICatalogEntry.Instance => null;

    /// <inheritdoc />
    public IEnumerable<IResource> Load(IResourceContext context)
    {
        return context.Require<ITextureIndexProvider>(textureIndexProvider =>
            context.Require<IDominantColorProvider>(dominantColorProvider =>
                context.Require<IModelProvider>(blockModelProvider =>
                    context.Require<VisualConfiguration>(visualConfiguration =>
                        Blocks.Instance.Initialize(
                            textureIndexProvider,
                            dominantColorProvider,
                            blockModelProvider,
                            visualConfiguration,
                            context)))));
    }
}
