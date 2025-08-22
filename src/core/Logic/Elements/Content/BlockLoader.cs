﻿// <copyright file="BlockLoader.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using VoxelGame.Core.Utilities.Resources;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Elements;

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
            context.Require<IBlockModelProvider>(blockModelProvider =>
                context.Require<VisualConfiguration>(visualConfiguration => 
                    Blocks.Instance.Initialize(
                        textureIndexProvider, 
                        blockModelProvider, 
                        visualConfiguration, 
                        context))));
    }
}
