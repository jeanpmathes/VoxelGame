// <copyright file="Spatial.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using OpenTK.Mathematics;
using VoxelGame.Support.Core;

namespace VoxelGame.Support.Objects;

/// <summary>
///     A native object that can be part of a space.
/// </summary>
public class Spatial : NativeObject
{
    private bool dirty = true;

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
        if (dirty || Space.HasAdjustmentChanged) Native.UpdateSpatialData(this, Space.GetAdjustedData(this));

        dirty = false;
    }
}
