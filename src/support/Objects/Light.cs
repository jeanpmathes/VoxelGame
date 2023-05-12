// <copyright file="Light.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using OpenTK.Mathematics;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Support.Objects;

/// <summary>
///     A directional light. The position is ignored.
/// </summary>
public class Light : SpatialObject
{
    private Vector3 direction = Vector3.Zero;
    private bool dirty = true;

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
            direction = value.ToVector3();
            dirty = true;
        }
    }

    /// <inheritdoc />
    internal override void Synchronize()
    {
        base.Synchronize();

        if (dirty)
        {
            Native.SetLightDirection(this, direction);
            dirty = false;
        }
    }
}
