// <copyright file="TextureArray.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using OpenTK.Mathematics;
using VoxelGame.Core.Visuals;
using VoxelGame.Graphics.Core;
using VoxelGame.Graphics.Objects;

namespace VoxelGame.Graphics.Graphics;

/// <summary>
///     Represents an array of textures, where all textures are the same size.
/// </summary>
public sealed class TextureArray : IEnumerable<Texture>
{
    private readonly Texture[] textures;
    private readonly ColorS[] dominantColors;

    private TextureArray(Texture[] textures, ColorS[] dominantColors)
    {
        this.textures = textures;
        this.dominantColors = dominantColors;
    }

    /// <summary>
    ///     Get the number of textures in the array.
    /// </summary>
    public Int32 Count => textures.Length;

    /// <inheritdoc />
    [MustDisposeResource]
    public IEnumerator<Texture> GetEnumerator()
    {
        return textures.AsEnumerable().GetEnumerator();
    }

    [MustDisposeResource]
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <summary>
    ///     Get the dominant color of the texture at the given index.
    ///     The dominant color is the color of the last mip-level.
    ///     If no color is available, the color will be black.
    /// </summary>
    /// <param name="index">The index of the texture.</param>
    /// <returns>The dominant color.</returns>
    public ColorS GetDominantColor(Int32 index)
    {
        return dominantColors[index];
    }

    /// <summary>
    ///     Load a new array texture. It will be filled with all textures found in the given directory.
    /// </summary>
    /// <param name="client">The client that will own the texture.</param>
    /// <param name="images">The textures to load. Mip-levels are grouped together.</param>
    /// <param name="count">The number of textures in the array, excluding mip-levels.</param>
    /// <param name="mips">The number of mip-levels that are included per base texture.</param>
    public static TextureArray Load(Client client, Span<Image> images, Int32 count, Int32 mips)
    {
        Debug.Assert(images.Length > 0);
        Debug.Assert(images.Length % mips == 0);
        Debug.Assert(images.Length == mips * count);

        var data = new Texture[count];
        var colors = new ColorS[count];

        // ReSharper disable once RedundantAssignment
        Vector2i size = images[index: 0].Size;

        for (var index = 0; index < count; index++)
        {
            Int32 begin = index * mips;
            Int32 end = begin + mips;

            Debug.Assert(images[begin].Size == size);
            data[index] = client.LoadTexture(images[begin..end]);

            Int32 last = end - 1;

            if (images[last].Size == (1, 1))
                colors[index] = images[last].GetPixel(x: 0, y: 0).ToColorS();
        }

        return new TextureArray(data, colors);
    }

    /// <summary>
    ///     Get the array as a span.
    /// </summary>
    /// <returns>The span.</returns>
    public Span<Texture> AsSpan()
    {
        return textures;
    }
}
