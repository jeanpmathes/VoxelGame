// <copyright file="Effect.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System.Runtime.InteropServices.Marshalling;
using VoxelGame.Graphics.Core;
using VoxelGame.Graphics.Data;

namespace VoxelGame.Graphics.Objects;

/// <summary>
///     An effect is a object positioned in 3D space that is rendered with a raster pipeline.
/// </summary>
[NativeMarshalling(typeof(EffectMarshaller))]
public class Effect : Drawable
{
    /// <summary>
    ///     Wrap a native mesh and drawable pointer.
    /// </summary>
    public Effect(IntPtr nativePointer, Space space) : base(nativePointer, space) {}

    /// <summary>
    ///     Set the new vertices for this effect.
    /// </summary>
    /// <param name="vertices">The new vertices.</param>
    public void SetNewVertices(Span<EffectVertex> vertices)
    {
        Native.SetEffectVertices(this, vertices);
    }
}

#pragma warning disable S3242
[CustomMarshaller(typeof(Effect), MarshalMode.ManagedToUnmanagedIn, typeof(EffectMarshaller))]
internal static class EffectMarshaller
{
    internal static IntPtr ConvertToUnmanaged(Effect managed)
    {
        return managed.Self;
    }

    internal static void Free(IntPtr unmanaged)
    {
        // Nothing to do here.
    }
}
#pragma warning restore S3242
