// <copyright file="SpatialObject.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using OpenTK.Mathematics;

namespace VoxelGame.Support.Objects;

/// <summary>
///     A native object that can be part of a space.
/// </summary>
public class SpatialObject : NativeObject
{
    /// <summary>
    ///     Create a new native spatial object.
    /// </summary>
    /// <param name="nativePointer">The native pointer.</param>
    /// <param name="space">The space in which the object is.</param>
    protected SpatialObject(IntPtr nativePointer, Space space) : base(nativePointer, space.Client)
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
    public Vector3d Position { get; set; }

    /// <summary>
    ///     Get or set the spatial object rotation.
    /// </summary>
    public Quaterniond Rotation { get; set; }

    /// <inheritdoc />
    public override void Synchronize()
    {
        Native.UpdateSpatialObjectData(this, Space.GetAdjustedData(this));
    }
}

