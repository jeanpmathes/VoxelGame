﻿// <copyright file="SingleTextureLoader.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
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
    private readonly RID identifier;
    private readonly FileInfo path;
    private readonly Int32 fallbackResolution;

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
