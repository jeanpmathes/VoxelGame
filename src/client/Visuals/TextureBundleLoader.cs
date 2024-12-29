// <copyright file="TextureBundleLoader.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Extensions.Logging;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Utilities.Resources;
using VoxelGame.Graphics.Graphics;
using VoxelGame.Logging;
using Image = VoxelGame.Core.Visuals.Image;

namespace VoxelGame.Client.Visuals;

/// <summary>
///     Represents the combination of all sources (directories) that make up a texture bundle.
///     When loading a texture bundle, all textures from the sources are loaded into a texture array.
///     This will cut off any textures that exceed the maximum number of textures.
/// </summary>
public sealed partial class TextureBundleLoader : IResourceLoader
{
    private readonly RID identifier;
    private readonly Int32 resolution;
    private readonly Int32 maxTextures;
    private readonly Image.MipmapAlgorithm mipmap;

    private readonly Int32 mips;

    private readonly List<DirectoryInfo> sources = [];

    private readonly List<Image> data = [];
    private readonly Dictionary<String, Int32> indices = [];

    private Int32 count;

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

        mips = BitOperations.Log2((UInt32) resolution) + 1;

        var fallback = Image.CreateFallback(resolution);
        AddTexture(fallback);
    }

    String? ICatalogEntry.Instance => identifier.Instance;

    /// <inheritdoc />
    public IEnumerable<IResource> Load(IResourceContext context)
    {
        return context.Require<VoxelGame.Graphics.Core.Client>(client =>
        {
            LoadSources(context);

            Span<Image> textures = CollectionsMarshal.AsSpan(data);

            if (count > maxTextures)
            {
                LogTooManyTextures(logger, count, maxTextures);

                textures = textures[..maxTextures];
                count = maxTextures;

                Int32 maxIndex = maxTextures - 1;

                foreach ((String key, Int32 index) in indices)
                    if (index > maxIndex)
                        indices[key] = 0;
            }

            TextureArray array = TextureArray.Load(client, textures, count, mips);

            return [new TextureBundle(identifier, array, indices)];
        });
    }

    /// <summary>
    ///     Add a source directory from which to load textures.
    /// </summary>
    /// <param name="directory">The directory to add as a source.</param>
    public void AddSource(DirectoryInfo directory)
    {
        sources.Add(directory);
    }

    private void AddTexture(Image texture)
    {
        data.Add(texture);

        count++;

        data[^1].RecolorTransparency();

        data.AddRange(data[^1].GenerateMipmaps(mips, mipmap));
    }

    private void AddTextureIndices(FileInfo file, Int32 index, Int32 size)
    {
        String textureName = GetTextureName(file);

        indices[textureName] = index;

        if (size == 1) return;

        for (var offset = 0; offset < size; offset++)
            indices[$"{textureName}:{offset}"] = index + offset;
    }

    private static String GetTextureName(FileInfo file)
    {
        StringBuilder name = new();

        foreach (Char c in file.GetFileNameWithoutExtension())
            if (Char.IsLetterOrDigit(c) || c == '_')
                name.Append(c);

        return name.ToString();
    }

    private void LoadFiles(IEnumerable<FileInfo> files, IResourceContext context)
    {
        foreach (FileInfo path in files)
        {
            Exception? error = null;
            String? errorMessage = null;

            try
            {
                Image file = Image.LoadFromFile(path);

                if (file.Width % resolution == 0 && file.Height == resolution)
                {
                    Int32 textureCount = file.Width / resolution;
                    AddTextureIndices(path, count, textureCount);

                    for (var j = 0; j < textureCount; j++)
                        AddTexture(file.CreateCopy(new Rectangle(j * resolution, y: 0, resolution, resolution)));
                }
                else
                {
                    errorMessage = "Image size did not match the specified resolution";
                }
            }
            catch (FileNotFoundException exception)
            {
                error = exception;
                errorMessage = "Image file not found";
            }

            context.ReportDiscovery(ResourceTypes.TextureBundlePNG, RID.Path(path), error, errorMessage);
        }
    }

    private void LoadSources(IResourceContext context)
    {
        List<FileInfo> files = [];

        foreach (DirectoryInfo source in sources)
            try
            {
                files.AddRange(source.GetFiles("*.png"));
            }
            catch (DirectoryNotFoundException exception)
            {
                context.ReportWarning(this, "Texture directory not found", exception, source);
            }

        LoadFiles(files, context);
    }

    #region LOGGING

    private static readonly ILogger logger = LoggingHelper.CreateLogger<TextureBundle>();

    [LoggerMessage(EventId = LogID.TextureBundleLoader + 0,
        Level = LogLevel.Critical,
        Message = "The number of textures found ({Count}) is higher than the number of textures ({Max}) that are allowed for this TextureBundle")]
    private static partial void LogTooManyTextures(ILogger logger, Int32 count, Int32 max);

    [LoggerMessage(EventId = LogID.TextureBundleLoader + 1,
        Level = LogLevel.Debug,
        Message = "The size of the image did not match the specified resolution ({Resolution}) and was not loaded: {Path}")]
    private static partial void LogImageSizeMismatch(ILogger logger, Int32 resolution, FileInfo path);

    [LoggerMessage(EventId = LogID.TextureBundleLoader + 2,
        Level = LogLevel.Error,
        Message = "The image could not be loaded: {Path}")]
    private static partial void LogImageLoadFailed(ILogger logger, Exception exception, FileInfo path);

    #endregion LOGGING
}
