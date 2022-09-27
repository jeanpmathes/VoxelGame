// <copyright file="IView.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using OpenTK.Mathematics;
using VoxelGame.Core.Physics;

namespace VoxelGame.Client.Rendering;

/// <summary>
///     Defines a view into the world, providing methods used to render the world.
/// </summary>
public interface IView
{
    /// <summary>
    ///     Get the far clipping distance.
    /// </summary>
    public double FarClipping { get; }

    /// <summary>
    ///     Get the near clipping distance.
    /// </summary>
    public double NearClipping { get; }

    /// <summary>
    ///     Get the view frustum, from near to far plane.
    /// </summary>
    public Frustum Frustum { get; }

    /// <summary>
    ///     Get the view matrix.
    /// </summary>
    public Matrix4d ViewMatrix { get; }

    /// <summary>
    ///     Get the view's projection matrix.
    /// </summary>
    public Matrix4d ProjectionMatrix { get; }
}
