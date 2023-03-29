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
using Microsoft.Extensions.Logging;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Visuals;
using VoxelGame.Logging;

namespace VoxelGame.Client.Rendering;

/// <summary>
///     Represents an array texture.
/// </summary>
public sealed class ArrayTexture : IDisposable, ITextureIndexProvider
{
    /// <summary>
    ///     Use this texture name to get the fallback texture without causing a warning.
    /// </summary>
    public const string MissingTextureName = "missing_texture";

    private static readonly ILogger logger = LoggingHelper.CreateLogger<ArrayTexture>();

    private readonly Dictionary<string, int> textureIndices = new();

    private int arrayCount;
    private int[] handles = null!;

    private LoadingContext? loadingContext;

    /// <summary>
    ///     Create a new array texture. It will be filled with all textures found in the given directory.
    /// </summary>
    /// <param name="loadingContext">The context in which loading is performed.</param>
    /// <param name="textureDirectory">The directory to load textures from.</param>
    /// <param name="resolution">The resolution of the array. Textures that do not fit are excluded.</param>
    /// <param name="useCustomMipmapGeneration">
    ///     True if custom mipmap generation should be used instead of the standard OpenGL
    ///     one. The custom algorithm is better for textures with complete transparency.
    /// </param>
    /// <param name="parameters">Optional texture parameters.</param>
    public ArrayTexture(LoadingContext loadingContext, DirectoryInfo textureDirectory, int resolution, bool useCustomMipmapGeneration, TextureParameters? parameters)
    {
        // todo: port to DirectX 

        Initialize(loadingContext, textureDirectory, resolution, useCustomMipmapGeneration, parameters);
    }

    /// <summary>
    ///     The size of a single array texture unit.
    /// </summary>
    public static int UnitSize => 2048;

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

    /// <summary>
    ///     Bind this array to the texture units.
    /// </summary>
    public void Use()
    {
        // for (var i = 0; i < arrayCount; i++) GL.BindTextureUnit(textureUnits[i] - TextureUnit.Texture0, handles[i]);
    }

    internal void SetWrapMode()
    {
        // for (var i = 0; i < arrayCount; i++)
        // {
        //     GL.BindTextureUnit(textureUnits[i] - TextureUnit.Texture0, handles[i]);
        //
        //     GL.TextureParameter(handles[i], TextureParameterName.TextureWrapS, (int) mode);
        //     GL.TextureParameter(handles[i], TextureParameterName.TextureWrapT, (int) mode);
        // }
    }

    private void Initialize(LoadingContext initialLoadingContext,
        DirectoryInfo textureDirectory, int resolution, bool useCustomMipmapGeneration, TextureParameters? parameters)
    {
        Debug.Assert(resolution > 0 && (resolution & (resolution - 1)) == 0);

        /*
        arrayCount = units.Length;

        textureUnits = units;
        handles = new int[arrayCount];

        GetHandles(handles);

        FileInfo[] texturePaths;

        try
        {
            texturePaths = textureDirectory.GetFiles("*.png");
        }
        catch (DirectoryNotFoundException)
        {
            texturePaths = Array.Empty<FileInfo>();

            initialLoadingContext.ReportWarning(Events.MissingDepository, nameof(ArrayTexture), textureDirectory, "Texture directory not found");
        }

        List<Bitmap> textures = new();

        // Create fall back texture.
        Bitmap fallback = Texture.CreateFallback(resolution);
        textures.Add(fallback);

        // Split all images into separate bitmaps and create a list.
        LoadBitmaps(resolution, texturePaths, textures);
        PreprocessBitmaps(textures);

        // Check if the arrays could hold all textures
        if (textures.Count > UnitSize * handles.Length)
        {
            logger.LogCritical(
                "The number of textures found ({Count}) is higher than the number of textures ({Max}) that are allowed for an ArrayTexture using {Units} units",
                textures.Count,
                UnitSize * handles.Length,
                units.Length);

            textures = textures.GetRange(index: 0, UnitSize * handles.Length);
        }

        Count = textures.Count;

        var loadedTextures = 0;
        var currentUnit = 0;

        while (loadedTextures < textures.Count)
        {
            int remainingTextures = textures.Count - loadedTextures;

            SetupArrayTexture(
                handles[currentUnit],
                units[currentUnit],
                resolution,
                textures,
                loadedTextures,
                loadedTextures + (remainingTextures < UnitSize ? remainingTextures : UnitSize),
                useCustomMipmapGeneration);

            parameters?.SetTextureParameters(handles[currentUnit]);

            loadedTextures += UnitSize;
            currentUnit++;
        }

        // Cleanup
        foreach (Bitmap bitmap in textures) bitmap.Dispose();

        initialLoadingContext.ReportSuccess(Events.ResourceLoad, nameof(ArrayTexture), textureDirectory);
        */
    }

    private static void PreprocessBitmaps(List<Bitmap> textures)
    {
        foreach (Bitmap texture in textures) PreprocessBitmap(texture);
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

    private void GetHandles(int[] arr)
    {
        // GL.CreateTextures(TextureTarget.Texture2DArray, arrayCount, arr);
    }

    /*
    private static void SetupArrayTexture(int handle, TextureUnit unit, int resolution,
        IReadOnlyList<Bitmap> textures,
        int startIndex, int length, bool useCustomMipmapGeneration)
    {
        var levels = (int) Math.Log(resolution, newBase: 2);

        GL.BindTextureUnit(unit - TextureUnit.Texture0, handle);

        // Allocate storage for array
        GL.TextureStorage3D(handle, levels, SizedInternalFormat.Rgba8, resolution, resolution, length);

        using Bitmap container = new(resolution, resolution * length, PixelFormat.Format32bppArgb);

        // Combine all textures into one
        for (int i = startIndex; i < length; i++)
        {
            textures[i].RotateFlip(RotateFlipType.RotateNoneFlipY);
            PlaceBitmap(textures[i], container, x: 0, i * resolution);
        }

        // Upload pixel data to array
        BitmapData data = container.LockBits(
            new Rectangle(x: 0, y: 0, container.Width, container.Height),
            ImageLockMode.ReadOnly,
            PixelFormat.Format32bppArgb);

        GL.TextureSubImage3D(
            handle,
            level: 0,
            xoffset: 0,
            yoffset: 0,
            zoffset: 0,
            resolution,
            resolution,
            length,
            OpenTK.Graphics.OpenGL4.PixelFormat.Bgra,
            PixelType.UnsignedByte,
            data.Scan0);

        container.UnlockBits(data);

        // Generate mipmaps for array
        if (!useCustomMipmapGeneration) GL.GenerateTextureMipmap(handle);
        else GenerateMipmapWithoutTransparencyMixing(handle, container, levels, length);

        // Set texture parameters for array
        GL.TextureParameter(
            handle,
            TextureParameterName.TextureMinFilter,
            (int) TextureMinFilter.NearestMipmapNearest);

        GL.TextureParameter(handle, TextureParameterName.TextureMagFilter, (int) TextureMagFilter.Nearest);

        GL.TextureParameter(handle, TextureParameterName.TextureWrapS, (int) TextureWrapMode.Repeat);
        GL.TextureParameter(handle, TextureParameterName.TextureWrapT, (int) TextureWrapMode.Repeat);
    }
    */

    /// <summary>
    ///     Loads all bitmaps specified by the paths into the list. The bitmaps are split into smaller parts that are all sized
    ///     according to the resolution.
    /// </summary>
    /// <remarks>
    ///     Textures provided have to have the height given by the resolution, and the width must be a multiple of it.
    /// </remarks>
    private void LoadBitmaps(int resolution, IReadOnlyCollection<FileInfo> paths, ICollection<Bitmap> bitmaps)
    {
        if (paths.Count == 0) return;

        var texIndex = 1;

        foreach (FileInfo path in paths)
            try
            {
                using Bitmap bitmap = new(path.FullName);

                if (bitmap.Width % resolution == 0 &&
                    bitmap.Height == resolution) // Check if image consists of correctly sized textures
                {
                    int textureCount = bitmap.Width / resolution;
                    textureIndices.Add(path.GetFileNameWithoutExtension(), texIndex);

                    for (var j = 0; j < textureCount; j++)
                    {
                        bitmaps.Add(
                            bitmap.Clone(
                                new Rectangle(j * resolution, y: 0, resolution, resolution),
                                PixelFormat.Format32bppArgb));

                        texIndex++;
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
    }

    private static void PlaceBitmap(Bitmap source, Bitmap destination, int x, int y)
    {
        for (var i = 0; i < source.Width; i++)
        for (var j = 0; j < source.Height; j++)
            destination.SetPixel(x + i, y + j, source.GetPixel(i, j));
    }

    private static void GenerateMipmapWithoutTransparencyMixing(int handle, Bitmap baseLevel, int levels,
        int length)
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
            UploadPixelData(handle, lowerLevel, lod, length);

            if (!upperLevel.Equals(baseLevel)) upperLevel.Dispose();

            upperLevel = lowerLevel;
        }

        if (!upperLevel.Equals(baseLevel)) upperLevel.Dispose();
    }

    private static void UploadPixelData(int handle, Bitmap bitmap, int lod, int length)
    {
        // todo: maybe this is a good point where upload to DirectX could be added

        BitmapData data = bitmap.LockBits(
            new Rectangle(x: 0, y: 0, bitmap.Width, bitmap.Height),
            ImageLockMode.ReadOnly,
            PixelFormat.Format32bppArgb);

        /*GL.TextureSubImage3D(
            handle,
            lod,
            xoffset: 0,
            yoffset: 0,
            zoffset: 0,
            bitmap.Width,
            bitmap.Width,
            length,
            OpenTK.Graphics.OpenGL4.PixelFormat.Bgra,
            PixelType.UnsignedByte,
            data.Scan0);*/

        bitmap.UnlockBits(data);
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

    #region IDisposable Support

    private bool disposed;

    private void Dispose(bool disposing)
    {
        if (disposed) return;

        if (disposing)
            for (var i = 0; i < arrayCount; i++)
                ; //GL.DeleteTexture(handles[i]); todo: freeing still important
        else
            logger.LogWarning(
                Events.UndeletedTexture,
                "Texture disposed by GC without freeing storage");

        disposed = true;
    }

    /// <summary>
    ///     Finalizer.
    /// </summary>
    ~ArrayTexture()
    {
        Dispose(disposing: false);
    }

    /// <summary>
    ///     Dispose of this texture.
    /// </summary>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    #endregion IDisposable Support
}

