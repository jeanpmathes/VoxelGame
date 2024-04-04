// <copyright file="TextureArray.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System.Collections;
using System.Diagnostics;
using OpenTK.Mathematics;
using VoxelGame.Core.Visuals;
using VoxelGame.Support.Core;
using VoxelGame.Support.Objects;

namespace VoxelGame.Support.Graphics;

/// <summary>
///     Represents an array of textures, where all textures are the same size.
/// </summary>
public sealed class TextureArray : IEnumerable<Texture>
{
    private readonly Texture[] textures;

    private TextureArray(Texture[] textures)
    {
        this.textures = textures;
    }

    /// <summary>
    ///     Get the number of textures in the array.
    /// </summary>
    public Int32 Count => textures.Length;

    /// <inheritdoc />
    public IEnumerator<Texture> GetEnumerator()
    {
        return textures.AsEnumerable().GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
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

        // Split the full texture list into parts and create the array textures.
        var data = new Texture[count];

        // ReSharper disable once RedundantAssignment
        Vector2i size = images[index: 0].Size;

        for (var index = 0; index < count; index++)
        {
            Int32 begin = index * mips;
            Int32 end = begin + mips;

            Debug.Assert(images[begin].Size == size);
            data[index] = client.LoadTexture(images[begin..end]);
        }

        return new TextureArray(data);
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
