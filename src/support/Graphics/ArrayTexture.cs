// <copyright file="ArrayTexture.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System.Drawing;
using Microsoft.Extensions.Logging;
using VoxelGame.Logging;
using VoxelGame.Support.Objects;

namespace VoxelGame.Support.Graphics;

/// <summary>
///     Represents an array texture of arbitrary size.
///     To achieve this, the texture is split into multiple textures.
/// </summary>
public sealed class ArrayTexture
{
    // todo: ensure that no texture units are mentioned in the wiki

    private static readonly ILogger logger = LoggingHelper.CreateLogger<ArrayTexture>();

    private readonly Texture[] parts;

    private ArrayTexture(Texture[] parts, int count)
    {
        this.parts = parts;

        Count = count;
    }

    /// <summary>
    ///     Get the number of textures in the array.
    /// </summary>
    public int Count { get; private set; }

    /// <summary>
    ///     Get the number of parts that make up this array texture.
    /// </summary>
    internal uint PartCount => (uint) parts.Length;

    /// <summary>
    ///     Load a new array texture. It will be filled with all textures found in the given directory.
    /// </summary>
    /// <param name="client">The client that will own the texture.</param>
    /// <param name="textures">The textures to load. Mip-levels are grouped together.</param>
    /// <param name="count">The number of textures in the array, excluding mip-levels.</param>
    /// <param name="mips">The number of mip-levels that are included per base texture.</param>
    public static ArrayTexture Load(Client client, Span<Bitmap> textures, int count, int mips)
    {
        int requiredParts = count / Texture.MaxArrayTextureDepth + 1;

        // Split the full texture list into parts and create the array textures.
        var data = new Texture[requiredParts];
        var currentPart = 0;
        var added = 0;

        int step = Texture.MaxArrayTextureDepth * mips;

        foreach (Bitmap texture in textures) texture.RotateFlip(RotateFlipType.Rotate180FlipX);

        while (added < textures.Length)
        {
            int next = Math.Min(added + step, textures.Length);
            data[currentPart] = client.LoadTexture(textures[added..next], mips);

            added = next;
            currentPart++;
        }

        return new ArrayTexture(data, count);
    }

    /// <summary>
    ///     Get the pointers to the sub-units of the array texture.
    ///     Note that the pointers each point to an array texture, not every single texture contained in the array.
    /// </summary>
    /// <returns>The pointers.</returns>
    internal IEnumerable<IntPtr> GetTexturePointers()
    {
        return parts.Select(p => p.Self);
    }
}
