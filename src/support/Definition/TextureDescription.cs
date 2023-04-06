// <copyright file="TextureDescription.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System.Runtime.InteropServices;

namespace VoxelGame.Support.Definition;

/// <summary>
///     Describes a texture.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
#pragma warning disable S3898 // No equality comparison used.
public struct TextureDescription
#pragma warning disable S3898 // No equality comparison used.
{
    /// <summary>
    ///     The width of the texture.
    /// </summary>
    public uint Width;

    /// <summary>
    ///     The height of the texture.
    /// </summary>
    public uint Height;
}
