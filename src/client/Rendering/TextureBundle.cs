// <copyright file="TextureBundle.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Visuals;
using VoxelGame.Logging;
using VoxelGame.Support.Graphics;
using VoxelGame.Support.Objects;

namespace VoxelGame.Client.Rendering;

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

        loadingContext.ReportWarning(Events.MissingResource, "TextureIndex", name, "Texture not found");

        return 0;
    }

    /// <summary>
    ///     Load a new texture bundle. It will be filled with all textures found in the given directory.
    /// </summary>
    /// <param name="client">The client that will own the texture.</param>
    /// <param name="loadingContext">The context in which loading is performed.</param>
    /// <param name="textureDirectory">The directory to load textures from.</param>
    /// <param name="resolution">The resolution of the textures. Textures that do not fit are excluded.</param>
    /// <param name="maxTextures">The maximum number of textures to load.</param>
    public static TextureBundle Load(Support.Client client, LoadingContext loadingContext, DirectoryInfo textureDirectory, int resolution, int maxTextures)
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
        List<Bitmap> extraTextures = new();
        Bitmap fallback = Texture.CreateFallback(resolution);
        extraTextures.Add(fallback);

        // Load all textures, preprocess them and add them to the list.
        using LoadingResult result = LoadBitmaps(resolution, texturePaths, extraTextures);
        Span<Bitmap> textures = CollectionsMarshal.AsSpan(result.Bitmaps);

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

        // Cleanup.
        foreach (Bitmap bitmap in extraTextures) bitmap.Dispose();

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

    private static void PreprocessBitmap(Bitmap texture)
    {
        long r = 0;
        long g = 0;
        long b = 0;
        long count = 0;

        for (var x = 0; x < texture.Width; x++)
        for (var y = 0; y < texture.Height; y++)
        {
            Color pixel = texture.GetPixel(x, y);

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

        for (var x = 0; x < texture.Width; x++)
        for (var y = 0; y < texture.Height; y++)
        {
            if (texture.GetPixel(x, y).A != 0) continue;

            texture.SetPixel(x, y, average);
        }
    }

    /// <summary>
    ///     Loads all bitmaps specified by the paths into a. The bitmaps are split into smaller parts that are all sized
    ///     according to the resolution.
    /// </summary>
    /// <remarks>
    ///     Textures provided have to have the height given by the resolution, and the width must be a multiple of it.
    /// </remarks>
    private static LoadingResult LoadBitmaps(int resolution, IReadOnlyCollection<FileInfo> paths, List<Bitmap> extraTextures)
    {
        Dictionary<string, int> indices = new();
        var count = 0;
        var mips = (int) Math.Log(resolution, newBase: 2);

        List<Bitmap> bitmaps = new();

        void AddTexture(Bitmap texture)
        {
            bitmaps.Add(texture);

            count++;

            PreprocessBitmap(bitmaps[^1]);

            List<Bitmap> mipmap = GenerateMipmap(bitmaps[^1], mips);
            bitmaps.AddRange(mipmap);
        }

        foreach (Bitmap texture in extraTextures) AddTexture(texture);

        foreach (FileInfo path in paths)
            try
            {
                using Bitmap bitmap = new(path.FullName);

                if (bitmap.Width % resolution == 0 &&
                    bitmap.Height == resolution) // Check if image consists of correctly sized textures
                {
                    int textureCount = bitmap.Width / resolution;
                    indices.Add(path.GetFileNameWithoutExtension(), count);

                    for (var j = 0; j < textureCount; j++)
                    {
                        Bitmap texture = bitmap.Clone(
                            new Rectangle(j * resolution, y: 0, resolution, resolution),
                            PixelFormat.Format32bppArgb);

                        AddTexture(texture);
                    }
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

        return new LoadingResult(indices, bitmaps, count, mips);
    }

    private static List<Bitmap> GenerateMipmap(Bitmap baseLevel, int levels)
    {
        Bitmap upperLevel = baseLevel;
        List<Bitmap> mipmap = new();

        for (var lod = 1; lod < levels; lod++)
        {
            Bitmap lowerLevel = new(upperLevel.Width / 2, upperLevel.Height / 2);

            // Create the lower level by averaging the upper level
            // todo: allow selecting which mipmap generation algorithm to use, and add a transparency mixing one (essentially default OpenGL behaviour)
            CreateLowerLevelWithoutTransparencyMixing(ref upperLevel, ref lowerLevel);

            mipmap.Add(lowerLevel);
            upperLevel = lowerLevel;
        }

        return mipmap;
    }

    private static void CreateLowerLevelWithoutTransparencyMixing(ref Bitmap upperLevel, ref Bitmap lowerLevel)
    {
        for (var w = 0; w < lowerLevel.Width; w++)
        for (var h = 0; h < lowerLevel.Height; h++)
        {
            Color c1 = upperLevel.GetPixel(w * 2, h * 2);
            Color c2 = upperLevel.GetPixel(w * 2 + 1, h * 2);
            Color c3 = upperLevel.GetPixel(w * 2, h * 2 + 1);
            Color c4 = upperLevel.GetPixel(w * 2 + 1, h * 2 + 1);

            int minAlpha = Math.Min(Math.Min(c1.A, c2.A), Math.Min(c3.A, c4.A));
            int maxAlpha = Math.Max(Math.Max(c1.A, c2.A), Math.Max(c3.A, c4.A));

            int one = c1.HasOpaqueness().ToInt();
            int two = c2.HasOpaqueness().ToInt();
            int three = c3.HasOpaqueness().ToInt();
            int four = c4.HasOpaqueness().ToInt();

            int relevantPixelCount = minAlpha != 0 ? 4 : one + two + three + four;
            (int, int, int, int) factors = (one, two, three, four);

            int alpha = relevantPixelCount == 0 ? 0 : maxAlpha;
            factors = relevantPixelCount == 0 ? (1, 1, 1, 1) : factors;

            Color average = Color.FromArgb(
                alpha,
                CalculateAveragedColorChannel(c1.R, c2.R, c3.R, c4.R, factors),
                CalculateAveragedColorChannel(c1.G, c2.G, c3.G, c4.G, factors),
                CalculateAveragedColorChannel(c1.B, c2.B, c3.B, c4.B, factors));

            lowerLevel.SetPixel(w, h, average);
        }
    }

    private static int CalculateAveragedColorChannel(int c1, int c2, int c3, int c4, (int, int, int, int) factors)
    {
        (int f1, int f2, int f3, int f4) = factors;
        double divisor = f1 + f2 + f3 + f4;

        int s1 = c1 * c1 * f1;
        int s2 = c2 * c2 * f2;
        int s3 = c3 * c3 * f3;
        int s4 = c4 * c4 * f4;

        return (int) Math.Sqrt((s1 + s2 + s3 + s4) / divisor);
    }

    private sealed record LoadingResult(Dictionary<string, int> Indices, List<Bitmap> Bitmaps, int Count, int Mips) : IDisposable
    {
        public void Dispose()
        {
            foreach (Bitmap bitmap in Bitmaps) bitmap.Dispose();
        }
    }
}
