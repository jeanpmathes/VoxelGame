// <copyright file="TextureBundleLoader.cs" company="VoxelGame">
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
using System.Diagnostics;
using System.IO;
using VoxelGame.Core.Utilities.Resources;
using Image = VoxelGame.Core.Visuals.Image;

namespace VoxelGame.Client.Visuals.Textures;

/// <summary>
///     Represents the combination of all sources (directories) that make up a texture bundle.
///     When loading a texture bundle, all textures from the sources are loaded into a texture array.
///     This will cut off any textures that exceed the maximum number of textures.
/// </summary>
public sealed class TextureBundleLoader : IResourceLoader
{
    private readonly RID identifier;
    private readonly Int32 maxTextures;
    private readonly Image.MipmapAlgorithm mipmap;
    private readonly Int32 resolution;

    private readonly List<DirectoryInfo> sources = [];

    /// <summary>
    ///     Create a new texture bundle loader.
    /// </summary>
    /// <param name="identifier">The identifier of the resource.</param>
    /// <param name="resolution">
    ///     The resolution of each texture in the bundle. Must be a power of 2. Other resolutions will be
    ///     ignored.
    /// </param>
    /// <param name="maxTextures">The maximum number of textures that can be loaded into the bundle.</param>
    /// <param name="mipmap">The mipmap algorithm to use for the textures.</param>
    public TextureBundleLoader(RID identifier, Int32 resolution, Int32 maxTextures, Image.MipmapAlgorithm mipmap)
    {
        Debug.Assert(resolution > 0 && (resolution & (resolution - 1)) == 0);

        this.identifier = identifier;
        this.resolution = resolution;
        this.maxTextures = maxTextures;
        this.mipmap = mipmap;
    }

    String? ICatalogEntry.Instance => identifier.Instance;

    /// <inheritdoc />
    public IEnumerable<IResource> Load(IResourceContext context)
    {
        return context.Require<VoxelGame.Graphics.Core.Client>(client =>
        [
            Bundler
                .Bundle(sources, resolution, context)
                .Pack(client, identifier, maxTextures, resolution, mipmap)
        ]);
    }

    /// <summary>
    ///     Add a source directory to the texture bundle loader.
    /// </summary>
    /// <param name="directory">The source directory to add.</param>
    public void AddSource(DirectoryInfo directory)
    {
        sources.Add(directory);
    }
}
