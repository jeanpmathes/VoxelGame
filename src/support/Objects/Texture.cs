// <copyright file="Texture.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System.Runtime.InteropServices.Marshalling;
using OpenTK.Mathematics;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Visuals;
using VoxelGame.Logging;
using VoxelGame.Support.Core;

namespace VoxelGame.Support.Objects;

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
    ///     Load a texture from a file. This is only allowed during the loading phase.
    /// </summary>
    /// <param name="client">The client instance, used to determine texture lifetime and to access the graphics API.</param>
    /// <param name="path">The path to the texture file.</param>
    /// <param name="loadingContext">The loading context.</param>
    /// <param name="fallbackResolution">The resolution to use for the fallback texture.</param>
    /// <returns></returns>
    public static Texture Load(Client client, FileInfo path, LoadingContext? loadingContext, Int32 fallbackResolution = 16)
    {
        Image image;

        try
        {
            image = Image.LoadFromFile(path);
            loadingContext?.ReportSuccess(Events.ResourceLoad, nameof(Texture), path);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or ArgumentException)
        {
            image = Image.CreateFallback(fallbackResolution);
            loadingContext?.ReportWarning(Events.MissingResource, nameof(Texture), path, exception);
        }

        Texture texture = client.LoadTexture(image);

        return texture;
    }

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
