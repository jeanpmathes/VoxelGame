// <copyright file="Light.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System.Runtime.InteropServices.Marshalling;
using OpenTK.Mathematics;
using VoxelGame.Graphics.Core;

namespace VoxelGame.Graphics.Objects;

/// <summary>
///     A directional light. The position is ignored.
/// </summary>
[NativeMarshalling(typeof(LightMarshaller))]
public class Light : Spatial
{
    private Vector3d direction = Vector3d.Zero;
    private Boolean dirty = true;

    /// <summary>
    ///     Create a new light.
    /// </summary>
    public Light(IntPtr nativePointer, Space space) : base(nativePointer, space) {}

    /// <summary>
    ///     Get or set the light direction. This is the direction in which the light is emitted.
    /// </summary>
    public Vector3d Direction
    {
        get => direction;
        set
        {
            if (value == direction) return;

            direction = value;
            dirty = true;
        }
    }

    /// <inheritdoc />
    internal override void Synchronize()
    {
        base.Synchronize();

        if (!dirty) return;

        dirty = false;

        NativeMethods.SetLightDirection(this, (Vector3) direction);
    }
}

#pragma warning disable S3242
[CustomMarshaller(typeof(Light), MarshalMode.ManagedToUnmanagedIn, typeof(LightMarshaller))]
internal static class LightMarshaller
{
    internal static IntPtr ConvertToUnmanaged(Light managed)
    {
        return managed.Self;
    }

    internal static void Free(IntPtr unmanaged)
    {
        // Nothing to do here.
    }
}
#pragma warning restore S3242
