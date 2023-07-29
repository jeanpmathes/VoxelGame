// <copyright file="TextureAtlas.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
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
using VoxelGame.Support.Objects;

namespace VoxelGame.Client.Rendering;

/// <summary>
///     Represents an array texture.
/// </summary>
public sealed class ArrayTexture : ITextureIndexProvider
{
    // todo: ensure that no texture units are mentioned in the wiki
    // todo: move the actual ArrayTexture into VG.Support and keep only the loading logic with a TexturesResource : ITextureIndexProvider in VG.Client

    /// <summary>
    ///     Use this texture name to get the fallback texture without causing a warning.
    /// </summary>
    public const string MissingTextureName = "missing_texture";

    private static readonly ILogger logger = LoggingHelper.CreateLogger<ArrayTexture>();

    private readonly Texture[] parts;

    private readonly Dictionary<string, int> textureIndices;

    private LoadingContext? loadingContext;

    private ArrayTexture(Texture[] parts, Dictionary<string, int> textureIndices, int count)
    {
        this.parts = parts;
        this.textureIndices = textureIndices;

        Count = count;
    }

    /// <summary>
    ///     Get the number of textures in the array.
    /// </summary>
    public int Count { get; private set; }

    /// <inheritdoc />
    public int GetTextureIndex(string name)
    {
        if (name == MissingTextureName) return 0;

        if (loadingContext == null)
        {
            logger.LogWarning(Events.ResourceLoad, "Loading of textures is currently disabled, fallback will be used instead");

            return 0;
        }

        if (textureIndices.TryGetValue(name, out int value)) return value;

        loadingContext.ReportWarning(Events.MissingResource, "TextureIndex", name, "Texture not found");

        return 0;
    }

    /// <summary>
    ///     Load a new array texture. It will be filled with all textures found in the given directory.
    /// </summary>
    /// <param name="client">The client that will own the texture.</param>
    /// <param name="loadingContext">The context in which loading is performed.</param>
    /// <param name="textureDirectory">The directory to load textures from.</param>
    /// <param name="resolution">The resolution of the array. Textures that do not fit are excluded.</param>
    /// <param name="maxTextures">The maximum number of textures to load.</param>
    public static ArrayTexture Load(Support.Client client, LoadingContext loadingContext, DirectoryInfo textureDirectory, int resolution, int maxTextures)
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

            loadingContext.ReportWarning(Events.MissingDepository, nameof(ArrayTexture), textureDirectory, "Texture directory not found");
        }

        List<Bitmap> textures = new();

        // Create fallback texture.
        Bitmap fallback = Texture.CreateFallback(resolution);
        textures.Add(fallback);

        // Load all textures, preprocess them and add them to the list.
        Dictionary<string, int> indices = LoadBitmaps(resolution, texturePaths, textures);
        Span<Bitmap> subresources = CollectionsMarshal.AsSpan(textures);

        int requiredParts = (maxTextures - 1) / Texture.MaxArrayTextureDepth + 1;

        // Check if the arrays could hold all textures.
        if (textures.Count > maxTextures) // todo: divide by mipmap count here and in log, consider it in the slicing below (or find more elegant solution like second list)
        {
            logger.LogCritical(
                "The number of textures found ({Count}) is higher than the number of textures ({Max}) that are allowed for this ArrayTexture",
                textures.Count,
                maxTextures);

            subresources = subresources[..maxTextures];
        }

        int count = subresources.Length; // todo: divide by mipmap count here too

        // Split the full texture list into parts and create the array textures.
        var data = new Texture[requiredParts];
        var currentPart = 0;
        var added = 0;

        while (added < subresources.Length) // todo: consider mipmaps here too
        {
            int next = Math.Min(added + Texture.MaxArrayTextureDepth, subresources.Length);
            data[currentPart] = client.LoadTexture(subresources[added..next]);

            added = next;
            currentPart++;
        }

        // Cleanup.
        foreach (Bitmap bitmap in textures) bitmap.Dispose();

        loadingContext.ReportSuccess(Events.ResourceLoad, nameof(ArrayTexture), textureDirectory);

        return new ArrayTexture(data, indices, count);
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
    ///     Loads all bitmaps specified by the paths into the list. The bitmaps are split into smaller parts that are all sized
    ///     according to the resolution.
    /// </summary>
    /// <remarks>
    ///     Textures provided have to have the height given by the resolution, and the width must be a multiple of it.
    /// </remarks>
    private static Dictionary<string, int> LoadBitmaps(int resolution, IReadOnlyCollection<FileInfo> paths, IList<Bitmap> bitmaps)
    {
        Dictionary<string, int> indices = new();

        if (paths.Count == 0) return indices;

        var levels = (int) Math.Log(resolution, newBase: 2);
        var texIndex = 1;

        foreach (FileInfo path in paths)
            try
            {
                using Bitmap bitmap = new(path.FullName);

                if (bitmap.Width % resolution == 0 &&
                    bitmap.Height == resolution) // Check if image consists of correctly sized textures
                {
                    int textureCount = bitmap.Width / resolution;
                    indices.Add(path.GetFileNameWithoutExtension(), texIndex);

                    for (var j = 0; j < textureCount; j++)
                    {
                        bitmaps.Add(
                            bitmap.Clone(
                                new Rectangle(j * resolution, y: 0, resolution, resolution),
                                PixelFormat.Format32bppArgb));

                        texIndex++;

                        PreprocessBitmap(bitmaps[^1]);
                        GenerateMipmapWithoutTransparencyMixing(bitmaps[^1], levels); // todo: upload the mipmaps to the GPU (description must be adapted)
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

        return indices;
    }

    private static void GenerateMipmapWithoutTransparencyMixing(Bitmap baseLevel, int levels)
    {
        Bitmap upperLevel = baseLevel;

        for (var lod = 1; lod < levels; lod++)
        {
#pragma warning disable CA2000 // Dispose objects before losing scope
            Bitmap lowerLevel = new(upperLevel.Width / 2, upperLevel.Height / 2);
#pragma warning restore CA2000 // Dispose objects before losing scope

            // Create the lower level by averaging the upper level
            CreateLowerLevel(ref upperLevel, ref lowerLevel);

            // Upload pixel data to array
            // todo: here was the upload before, but what now? pass all data at once, in correct order according to subresource indexing

            if (!upperLevel.Equals(baseLevel)) upperLevel.Dispose();

            upperLevel = lowerLevel;
        }

        if (!upperLevel.Equals(baseLevel)) upperLevel.Dispose();
    }

    private static void CreateLowerLevel(ref Bitmap upperLevel, ref Bitmap lowerLevel)
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
}
