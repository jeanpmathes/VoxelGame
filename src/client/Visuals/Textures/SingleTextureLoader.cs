// <copyright file="SingleTextureLoader.cs" company="VoxelGame">
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
using System.Collections.Generic;
using System.IO;
using VoxelGame.Core.Utilities.Resources;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Client.Visuals.Textures;

/// <summary>
///     Loads a single texture.
/// </summary>
public sealed class SingleTextureLoader : IResourceLoader
{
    private readonly Int32 fallbackResolution;
    private readonly RID identifier;
    private readonly FileInfo path;

    /// <summary>
    ///     Creates a new <see cref="SingleTextureLoader" /> which loads a texture from the given path.
    /// </summary>
    /// <param name="path">The path to the texture.</param>
    /// <param name="fallbackResolution">The resolution to use if the texture could not be loaded.</param>
    public SingleTextureLoader(FileInfo path, Int32 fallbackResolution = 16)
    {
        this.path = path;
        this.fallbackResolution = fallbackResolution;

        identifier = RID.Path(path);
    }

    String? ICatalogEntry.Instance => identifier.Instance;

    /// <inheritdoc />
    public IEnumerable<IResource> Load(IResourceContext context)
    {
        return context.Require<Application.Client>(client =>
        {
            Image image;

            try
            {
                image = Image.LoadFromFile(path);
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or ArgumentException)
            {
                image = Image.CreateFallback(fallbackResolution);

                context.ReportWarning(this, "Loading image file failed", exception, path);
            }

            return [new SingleTexture(identifier, client.LoadTexture(image))];
        });
    }
}
