﻿// <copyright file="TextureBundle.cs" company="VoxelGame">
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
public sealed class TextureBundle : ITextureIndexProvider
{
    /// <summary>
    ///     Use this texture name to get the fallback texture without causing a warning.
    /// </summary>
    private const string MissingTextureName = "missing_texture";

    private static readonly ILogger logger = LoggingHelper.CreateLogger<TextureBundle>();

    private LoadingContext? loadingContext;

    private TextureBundle(TextureArray textureArray, Dictionary<string, int> textureIndices)
    {
        TextureArray = textureArray;
        TextureIndices = textureIndices;
    }

    /// <summary>
    ///     The array that stores all textures.
    /// </summary>
    public TextureArray TextureArray { get; }

    private Dictionary<string, int> TextureIndices { get; }

    /// <summary>
    ///     Get the number of textures in the bundle.
    /// </summary>
    public int Count => TextureArray.Count;

    /// <inheritdoc />
    public int GetTextureIndex(string name)
    {
        if (name == MissingTextureName) return 0;

        if (loadingContext == null)
        {
            logger.LogWarning(Events.ResourceLoad, "Loading of textures is currently disabled, fallback will be used instead");

            return 0;
        }

        if (TextureIndices.TryGetValue(name, out int value)) return value;

        loadingContext.ReportWarning(Events.MissingResource, "Texture", name, "Texture not found");

        return 0;
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
        int resolution, int maxTextures, Image.MipmapAlgorithm mipmap)
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

            loadingContext.ReportWarning(Events.MissingDepository, nameof(TextureArray), textureDirectory, "Texture directory not found");
        }

        // Create fallback texture.
        List<Image> extraTextures = new();
        var fallback = Image.CreateFallback(resolution);
        extraTextures.Add(fallback);

        // Load all textures, preprocess them and add them to the list.
        LoadingResult result = LoadImages(resolution, texturePaths, extraTextures, mipmap);
        Span<Image> textures = CollectionsMarshal.AsSpan(result.Images);

        // Check if the arrays could hold all textures.
        if (result.Count > maxTextures)
        {
            logger.LogCritical(
                "The number of textures found ({Count}) is higher than the number of textures ({Max}) that are allowed for this TextureBundle",
                extraTextures.Count,
                maxTextures);

            textures = textures[..maxTextures];

            int maxIndex = maxTextures - 1;

            foreach ((string key, int index) in result.Indices)
                if (index > maxIndex)
                    result.Indices[key] = 0;
        }

        TextureArray loadedTextureArray = TextureArray.Load(client, textures, Math.Min(result.Count, maxTextures), result.Mips);

        loadingContext.ReportSuccess(Events.ResourceLoad, nameof(TextureArray), textureDirectory);

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
        long r = 0;
        long g = 0;
        long b = 0;
        long count = 0;

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

        int GetAverage(long sum)
        {
            return (int) Math.Sqrt(sum / (double) count);
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
    private static LoadingResult LoadImages(int resolution, IEnumerable<FileInfo> paths, List<Image> extraTextures, Image.MipmapAlgorithm mipmap)
    {
        Dictionary<string, int> indices = new();
        var count = 0;
        int mips = BitOperations.Log2((uint) resolution) + 1;

        List<Image> images = [];

        void AddTexture(Image texture)
        {
            images.Add(texture);

            count++;

            PreprocessImage(images[^1]);

            images.AddRange(images[^1].GenerateMipmaps(mips, mipmap));
        }

        foreach (Image texture in extraTextures) AddTexture(texture);

        foreach (FileInfo path in paths)
            try
            {
                Image image = Image.LoadFromFile(path);

                if (image.Width % resolution == 0 &&
                    image.Height == resolution) // Check if image consists of correctly sized textures
                {
                    int textureCount = image.Width / resolution;
                    indices.Add(path.GetFileNameWithoutExtension(), count);

                    for (var j = 0; j < textureCount; j++) AddTexture(image.CreateCopy(new Rectangle(j * resolution, y: 0, resolution, resolution)));
                }
                else
                {
                    logger.LogDebug(
                        "The size of the image did not match the specified resolution ({Resolution}) and was not loaded: {Path}",
                        resolution,
                        path);
                }
            }
            catch (FileNotFoundException e)
            {
                logger.LogError(e, "The image could not be loaded: {Path}", path);
            }

        return new LoadingResult(indices, images, count, mips);
    }

    private sealed record LoadingResult(Dictionary<string, int> Indices, List<Image> Images, int Count, int Mips);
}
