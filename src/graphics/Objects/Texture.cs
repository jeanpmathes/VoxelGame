// <copyright file="Texture.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System.Runtime.InteropServices.Marshalling;
using OpenTK.Mathematics;
using VoxelGame.Graphics.Core;

namespace VoxelGame.Graphics.Objects;

/// <summary>
///     A texture.
/// </summary>
[NativeMarshalling(typeof(TextureMarshaller))]
public class Texture : NativeObject
{
    private readonly Vector2i size;

    /// <summary>
    ///     Create a new texture from a native pointer.
    /// </summary>
    internal Texture(IntPtr nativePointer, Client client, Vector2i size) : base(nativePointer, client)
    {
        this.size = size;
    }

    /// <summary>
    ///     Gets the width of the texture.
    /// </summary>
    public Int32 Width => size.X;

    /// <summary>
    ///     Gets the height of the texture.
    /// </summary>
    public Int32 Height => size.Y;

    /// <summary>
    ///     Frees the texture. Not allowed in same frame as creation.
    /// </summary>
    public void Free()
    {
        Deregister();
        NativeMethods.FreeTexture(this);
    }
}

#pragma warning disable S3242
[CustomMarshaller(typeof(Texture), MarshalMode.ManagedToUnmanagedIn, typeof(TextureMarshaller))]
internal static class TextureMarshaller
{
    internal static IntPtr ConvertToUnmanaged(Texture managed)
    {
        return managed.Self;
    }

    internal static void Free(IntPtr unmanaged)
    {
        // Nothing to do here.
    }
}
#pragma warning restore S3242
