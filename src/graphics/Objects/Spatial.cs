// <copyright file="Spatial.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Runtime.InteropServices.Marshalling;
using OpenTK.Mathematics;
using VoxelGame.Graphics.Core;

namespace VoxelGame.Graphics.Objects;

/// <summary>
///     A native object that can be part of a space.
/// </summary>
[NativeMarshalling(typeof(SpatialMarshaller))]
public class Spatial : NativeObject
{
    private Boolean dirty = true;

    private Vector3d position = Vector3d.Zero;
    private Quaterniond rotation = Quaterniond.Identity;

    /// <summary>
    ///     Create a new native spatial object.
    /// </summary>
    /// <param name="nativePointer">The native pointer.</param>
    /// <param name="space">The space in which the object is.</param>
    protected Spatial(IntPtr nativePointer, Space space) : base(nativePointer, space.Client)
    {
        Space = space;
    }

    /// <summary>
    ///     Get the space in which the object is.
    /// </summary>
    private Space Space { get; }

    /// <summary>
    ///     Get or set the spatial object position.
    /// </summary>
    public Vector3d Position
    {
        get => position;
        set
        {
            position = value;
            dirty = true;
        }
    }

    /// <summary>
    ///     Get or set the spatial object rotation.
    /// </summary>
    public Quaterniond Rotation
    {
        get => rotation;
        set
        {
            rotation = value;
            dirty = true;
        }
    }

    internal override void Synchronize()
    {
        if (dirty || Space.HasAdjustmentChanged) NativeMethods.UpdateSpatialData(this, Space.GetAdjustedData(this));

        dirty = false;
    }
}

#pragma warning disable S3242
[CustomMarshaller(typeof(Spatial), MarshalMode.ManagedToUnmanagedIn, typeof(SpatialMarshaller))]
internal static class SpatialMarshaller
{
    internal static IntPtr ConvertToUnmanaged(Spatial managed)
    {
        return managed.Self;
    }

    internal static void Free(IntPtr unmanaged)
    {
        // Nothing to do here.
    }
}
#pragma warning restore S3242
