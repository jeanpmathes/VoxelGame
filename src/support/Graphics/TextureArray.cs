// <copyright file="TextureArray.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System.Diagnostics;
using System.Drawing;
using VoxelGame.Support.Core;
using VoxelGame.Support.Objects;

namespace VoxelGame.Support.Graphics;

/// <summary>
///     Represents an array of textures, where all textures are the same size.
/// </summary>
public sealed class TextureArray
{
    // todo: ensure that no texture units are mentioned in the wiki

    private readonly Texture[] textures;

    private TextureArray(Texture[] textures)
    {
        this.textures = textures;
    }

    /// <summary>
    ///     Get the number of textures in the array.
    /// </summary>
    public int Count => textures.Length;

    /// <summary>
    ///     Load a new array texture. It will be filled with all textures found in the given directory.
    /// </summary>
    /// <param name="client">The client that will own the texture.</param>
    /// <param name="bitmaps">The textures to load. Mip-levels are grouped together.</param>
    /// <param name="count">The number of textures in the array, excluding mip-levels.</param>
    /// <param name="mips">The number of mip-levels that are included per base texture.</param>
    public static TextureArray Load(Client client, Span<Bitmap> bitmaps, int count, int mips)
    {
        Debug.Assert(bitmaps.Length > 0);
        Debug.Assert(bitmaps.Length % mips == 0);
        Debug.Assert(bitmaps.Length == mips * count);

        // Split the full texture list into parts and create the array textures.
        var data = new Texture[count];

        foreach (Bitmap texture in bitmaps) texture.RotateFlip(RotateFlipType.Rotate180FlipX); // todo: check in PIX that textures have right orientation, change shader if necessary

        Size size = bitmaps[index: 0].Size;

        for (var index = 0; index < count; index++)
        {
            int begin = index * mips;
            int end = begin + mips;

            Debug.Assert(bitmaps[begin].Size == size);
            data[index] = client.LoadTexture(bitmaps[begin..end]);
        }

        return new TextureArray(data);
    }

    /// <summary>
    ///     Get the pointers to the sub-units of the array texture.
    ///     Note that the pointers each point to an array texture, not every single texture contained in the array.
    /// </summary>
    /// <returns>The pointers.</returns>
    internal IEnumerable<IntPtr> GetTexturePointers()
    {
        return textures.Select(p => p.Self);
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
