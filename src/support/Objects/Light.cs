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
    private Vector3d direction = Vector3d.Zero;

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

            Native.SetLightDirection(this, value.ToVector3()); // todo: use sync instead of writing directly, but not simple sync as positional mode for light (comes later) does not fit that
            direction = value;
        }
    }
}
