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
    ///     Get the middle clipping distance, which is used as both near and far plane distance in different passes.
    /// </summary>
    public double MidClipping { get; }

    /// <summary>
    ///     Get the near clipping distance.
    /// </summary>
    public double NearClipping { get; }

    /// <summary>
    ///     Get the full view frustum of the view, from near to far plane.
    /// </summary>
    public Frustum FullFrustum { get; }

    /// <summary>
    ///     Get the near view frustum of the view, from near to mid plane.
    /// </summary>
    public Frustum NearFrustum { get; }


    /// <summary>
    ///     Get the far view frustum of the view, from mid to far plane.
    /// </summary>
    public Frustum FarFrustum { get; }

    /// <summary>
    ///     Get the view matrix.
    /// </summary>
    public Matrix4d ViewMatrix { get; }

    /// <summary>
    ///     Get the view's full projection matrix.
    /// </summary>
    public Matrix4d FullProjectionMatrix { get; }

    /// <summary>
    ///     Get the view's near projection matrix.
    /// </summary>
    public Matrix4d NearProjectionMatrix { get; }

    /// <summary>
    ///     Get the view's far projection matrix.
    /// </summary>
    public Matrix4d FarProjectionMatrix { get; }
}
