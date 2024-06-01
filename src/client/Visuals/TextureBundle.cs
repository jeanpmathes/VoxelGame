// <copyright file="TextureBundle.cs" company="VoxelGame">
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
using VoxelGame.Core.Visuals;
using VoxelGame.Logging;
using VoxelGame.Support.Graphics;
using Image = VoxelGame.Core.Visuals.Image;

namespace VoxelGame.Client.Visuals;

/// <summary>
///     A list of textures that can be used by shaders.
///     Each texture has a name and index.
/// </summary>
public sealed partial class TextureBundle : ITextureIndexProvider, IDominantColorProvider
{
    /// <summary>
    ///     Use this texture name to get the fallback texture without causing a warning.
    /// </summary>
    private const String MissingTextureName = "missing_texture";
    
    private LoadingContext? loadingContext;

    private TextureBundle(TextureArray textureArray, Dictionary<String, Int32> textureIndices)
    {
        TextureArray = textureArray;
        TextureIndices = textureIndices;
    }

    private TextureArray TextureArray { get; }
    private Dictionary<String, Int32> TextureIndices { get; }

    /// <summary>
    ///     Get the number of textures in the bundle.
    /// </summary>
    public Int32 Count => TextureArray.Count;

    /// <inheritdoc />
    public Color GetDominantColor(Int32 index)
    {
        return TextureArray.GetDominantColor(index);
    }

    /// <inheritdoc />
    public Int32 GetTextureIndex(String name)
    {
        if (name == MissingTextureName) return 0;

        if (loadingContext == null)
        {
            LogLoadingDisabled(logger);
            
            return 0;
        }

        if (TextureIndices.TryGetValue(name, out Int32 value)) return value;

        loadingContext.ReportWarning("Texture", name, "Texture not found");

        return 0;
    }

    /// <summary>
    /// Get the arrays filling the texture slots.
    /// </summary>
    public static (TextureArray, TextureArray) GetTextureSlots(TextureBundle first, TextureBundle second)
    {
        return (first.TextureArray, second.TextureArray);
    }

    /// <summary>
    ///     Load a new texture bundle. It will be filled with all textures found in the given directory.
    /// </summary>
    /// <param name="client">The client that will own the texture.</param>
    /// <param name="loadingContext">The context in which loading is performed.</param>
    /// <param name="textureDirectory">The directory to load textures from.</param>
    /// <param name="resolution">
    ///     The resolution, i.e. both the width and height, of the textures. Textures that do not fit are
    ///     excluded.
    /// </param>
    /// <param name="maxTextures">The maximum number of textures to load.</param>
    /// <param name="mipmap">The algorithm to use for generating mipmaps.</param>
    public static TextureBundle Load(
        Support.Core.Client client, LoadingContext loadingContext, DirectoryInfo textureDirectory,
        Int32 resolution, Int32 maxTextures, Image.MipmapAlgorithm mipmap)
    {
        Debug.Assert(resolution > 0 && (resolution & (resolution - 1)) == 0);

        FileInfo[] texturePaths;

        try
        {
            texturePaths = textureDirectory.GetFiles("*.png");
        }
        catch (DirectoryNotFoundException)
        {
            texturePaths = Array.Empty<FileInfo>();

            loadingContext.ReportWarning(nameof(TextureArray), textureDirectory, "Texture directory not found");
        }

        // Create fallback texture.
        List<Image> extraTextures = [];
        var fallback = Image.CreateFallback(resolution);
        extraTextures.Add(fallback);

        // Load all textures, preprocess them and add them to the list.
        LoadingResult result = LoadImages(resolution, texturePaths, extraTextures, mipmap);
        Span<Image> textures = CollectionsMarshal.AsSpan(result.Images);

        // Check if the arrays could hold all textures.
        if (result.Count > maxTextures)
        {
            LogTooManyTextures(logger, extraTextures.Count, maxTextures);

            textures = textures[..maxTextures];

            Int32 maxIndex = maxTextures - 1;

            foreach ((String key, Int32 index) in result.Indices)
                if (index > maxIndex)
                    result.Indices[key] = 0;
        }

        TextureArray loadedTextureArray = TextureArray.Load(client, textures, Math.Min(result.Count, maxTextures), result.Mips);

        loadingContext.ReportSuccess(nameof(TextureArray), textureDirectory);

        return new TextureBundle(loadedTextureArray, result.Indices);
    }

    /// <summary>
    ///     Set the loading context. This will be used for reporting results.
    /// </summary>
    /// <param name="usedLoadingContext">The loading context to use.</param>
    public void EnableLoading(LoadingContext usedLoadingContext)
    {
        loadingContext = usedLoadingContext;
    }

    /// <summary>
    ///     Disable loading. This will prevent any further loading reports. Only the fallback texture will be available.
    /// </summary>
    public void DisableLoading()
    {
        loadingContext = null;
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

    /// <summary>
    ///     Loads all images specified by the paths into a. The bitmaps are split into smaller parts that are all sized
    ///     according to the resolution.
    /// </summary>
    /// <param name="resolution">The resolution of the textures. Both width and height should have this value.</param>
    /// <param name="paths">The paths to the images.</param>
    /// <param name="extraTextures">The textures to add to the list.</param>
    /// <param name="mipmap">The algorithm to use for generating mipmaps.</param>
    /// <remarks>
    ///     Textures provided have to have the height given by the resolution, and the width must be a multiple of it.
    /// </remarks>
    private static LoadingResult LoadImages(Int32 resolution, IEnumerable<FileInfo> paths, List<Image> extraTextures, Image.MipmapAlgorithm mipmap)
    {
        Dictionary<String, Int32> indices = new();
        var count = 0;
        Int32 mips = BitOperations.Log2((UInt32) resolution) + 1;

        List<Image> images = [];

        foreach (Image texture in extraTextures) AddTexture(texture);

        foreach (FileInfo path in paths)
            try
            {
                Image image = Image.LoadFromFile(path);

                if (image.Width % resolution == 0 &&
                    image.Height == resolution) // Check if image consists of correctly sized textures
                {
                    Int32 textureCount = image.Width / resolution;
                    AddTextureIndices(indices, path, count, textureCount);

                    for (var j = 0; j < textureCount; j++) AddTexture(image.CreateCopy(new Rectangle(j * resolution, y: 0, resolution, resolution)));
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

        return new LoadingResult(indices, images, count, mips);

        void AddTexture(Image texture)
        {
            images.Add(texture);

            count++;

            PreprocessImage(images[^1]);

            images.AddRange(images[^1].GenerateMipmaps(mips, mipmap));
        }
    }

    private static void AddTextureIndices(IDictionary<String, Int32> indices, FileInfo file, Int32 index, Int32 size)
    {
        String name = GetTextureName(file);

        indices[name] = index;

        if (size == 1) return;

        for (var offset = 0; offset < size; offset++)
            indices[$"{name}:{offset}"] = index + offset;
    }

    private static String GetTextureName(FileInfo file)
    {
        StringBuilder name = new();

        foreach (Char c in file.GetFileNameWithoutExtension())
            if (Char.IsLetterOrDigit(c) || c == '_')
                name.Append(c);

        return name.ToString();
    }

    private sealed record LoadingResult(Dictionary<String, Int32> Indices, List<Image> Images, Int32 Count, Int32 Mips);

    #region LOGGING

    private static readonly ILogger logger = LoggingHelper.CreateLogger<TextureBundle>();

    [LoggerMessage(EventId = Events.ResourceLoad, Level = LogLevel.Warning, Message = "Loading of textures is currently disabled, fallback will be used instead")]
    private static partial void LogLoadingDisabled(ILogger logger);

    [LoggerMessage(EventId = 0, Level = LogLevel.Critical, Message = "The number of textures found ({Count}) is higher than the number of textures ({Max}) that are allowed for this TextureBundle")]
    private static partial void LogTooManyTextures(ILogger logger, Int32 count, Int32 max);

    [LoggerMessage(EventId = 0, Level = LogLevel.Debug, Message = "The size of the image did not match the specified resolution ({Resolution}) and was not loaded: {Path}")]
    private static partial void LogImageSizeMismatch(ILogger logger, Int32 resolution, FileInfo path);

    [LoggerMessage(EventId = 0, Level = LogLevel.Error, Message = "The image could not be loaded: {Path}")]
    private static partial void LogImageLoadFailed(ILogger logger, Exception exception, FileInfo path);

    #endregion LOGGING
}
