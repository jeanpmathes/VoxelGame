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
using VoxelGame.Graphics.Graphics;
using VoxelGame.Logging;
using Image = VoxelGame.Core.Visuals.Image;

namespace VoxelGame.Client.Visuals;

/// <summary>
///     Represents the combination of all sources (directories) that make up a texture bundle.
/// </summary>
public partial class TextureBundleLoader
{
    private readonly String name;
    private readonly Int32 resolution;
    private readonly Int32 maxTextures;
    private readonly Image.MipmapAlgorithm mipmap;

    private readonly Int32 mips;

    private readonly List<Image> data = [];
    private readonly Dictionary<String, Int32> indices = [];

    private Int32 count;

    /// <summary>
    ///     Create a new texture bundle loader.
    /// </summary>
    /// <param name="name">A name referring to the texture bundle being loaded.</param>
    /// <param name="resolution">
    ///     The resolution of each texture in the bundle. Must be a power of 2. Other resolutions will be
    ///     ignored.
    /// </param>
    /// <param name="maxTextures">The maximum number of textures that can be loaded into the bundle.</param>
    /// <param name="mipmap">The mipmap algorithm to use for the textures.</param>
    public TextureBundleLoader(String name, Int32 resolution, Int32 maxTextures, Image.MipmapAlgorithm mipmap)
    {
        Debug.Assert(resolution > 0 && (resolution & (resolution - 1)) == 0);

        this.name = name;
        this.resolution = resolution;
        this.maxTextures = maxTextures;
        this.mipmap = mipmap;

        mips = BitOperations.Log2((UInt32) resolution) + 1;

        var fallback = Image.CreateFallback(resolution);
        AddTexture(fallback);
    }

    /// <summary>
    ///     Load textures from a directory.
    /// </summary>
    /// <param name="directory">The directory containing the textures.</param>
    /// <param name="loadingContext">The loading context to report progress and errors to.</param>
    public void Load(DirectoryInfo directory, ILoadingContext loadingContext)
    {
        FileInfo[] files;

        try
        {
            files = directory.GetFiles("*.png");
        }
        catch (DirectoryNotFoundException)
        {
            loadingContext.ReportWarning($"{nameof(TextureBundle)}Part", directory, "Texture directory not found");

            return;
        }

        LoadFiles(files);

        loadingContext.ReportSuccess($"{nameof(TextureBundle)}Part", directory);
    }

    private void LoadFiles(FileInfo[] files)
    {
        foreach (FileInfo path in files)
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
                    LogImageSizeMismatch(logger, resolution, path);
                }
            }
            catch (FileNotFoundException e)
            {
                LogImageLoadFailed(logger, e, path);
            }
    }

    private void AddTexture(Image texture)
    {
        data.Add(texture);

        count++;

        PreprocessImage(data[^1]);

        data.AddRange(data[^1].GenerateMipmaps(mips, mipmap));
    }

    private static void PreprocessImage(Image image)
    {
        Int64 r = 0;
        Int64 g = 0;
        Int64 b = 0;
        Int64 count = 0;

        for (var x = 0; x < image.Width; x++)
        for (var y = 0; y < image.Height; y++)
        {
            Color pixel = image.GetPixel(x, y);

            if (pixel.A == 0) continue;

            r += pixel.R * pixel.R;
            g += pixel.G * pixel.G;
            b += pixel.B * pixel.B;

            count++;
        }

        Int32 GetAverage(Int64 sum)
        {
            return (Int32) Math.Sqrt(sum / (Double) count);
        }

        Color average = Color.FromArgb(alpha: 0, GetAverage(r), GetAverage(g), GetAverage(b));

        for (var x = 0; x < image.Width; x++)
        for (var y = 0; y < image.Height; y++)
        {
            if (image.GetPixel(x, y).A != 0) continue;

            image.SetPixel(x, y, average);
        }
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

    /// <summary>
    ///     Pack all loaded textures into a texture bundle.
    ///     This will cut off any textures that exceed the maximum number of textures.
    ///     The remaining textures will be loaded into a GPU texture array.
    /// </summary>
    /// <param name="client">The client to use for loading the textures.</param>
    /// <param name="loadingContext">The loading context to report progress and errors to.</param>
    /// <returns>The loaded texture bundle.</returns>
    public TextureBundle Pack(VoxelGame.Graphics.Core.Client client, ILoadingContext loadingContext)
    {
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

        TextureArray loadedTextureArray = TextureArray.Load(client, textures, count, mips);

        loadingContext.ReportSuccess(nameof(TextureBundle), name);

        return new TextureBundle(loadedTextureArray, indices);
    }

    #region LOGGING

    private static readonly ILogger logger = LoggingHelper.CreateLogger<TextureBundle>();

    [LoggerMessage(EventId = LogID.TextureBundleLoader + 0, Level = LogLevel.Critical, Message = "The number of textures found ({Count}) is higher than the number of textures ({Max}) that are allowed for this TextureBundle")]
    private static partial void LogTooManyTextures(ILogger logger, Int32 count, Int32 max);

    [LoggerMessage(EventId = LogID.TextureBundleLoader + 1, Level = LogLevel.Debug, Message = "The size of the image did not match the specified resolution ({Resolution}) and was not loaded: {Path}")]
    private static partial void LogImageSizeMismatch(ILogger logger, Int32 resolution, FileInfo path);

    [LoggerMessage(EventId = LogID.TextureBundleLoader + 2, Level = LogLevel.Error, Message = "The image could not be loaded: {Path}")]
    private static partial void LogImageLoadFailed(ILogger logger, Exception exception, FileInfo path);

    #endregion LOGGING
}
