// <copyright file="IView.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Core.Actors;
using VoxelGame.Core.Physics;

namespace VoxelGame.Graphics.Graphics;

/// <summary>
///     Defines a view into the world, providing data that describes that view and properties like the view frustum.
/// </summary>
public interface IView : IOrientable
{
    /// <summary>
    ///     Get the view frustum, from near to far plane.
    /// </summary>
    Frustum Frustum => Definition.Frustum;

    /// <summary>
    ///     Get the parameters that define the view.
    /// </summary>
    Parameters Definition { get; }

    /// <summary>
    ///     Get the parameters that define the view.
    /// </summary>
    record Parameters(Double FieldOfView, Double AspectRatio, (Double near, Double far) Clipping, Vector3d Position, (Vector3d front, Vector3d up, Vector3d right) Orientation)
    {
        /// <summary>
        ///     Create a frustum from the view parameters.
        /// </summary>
        public Frustum Frustum => new(FieldOfView, AspectRatio, Clipping, Position, Orientation.front, Orientation.up, Orientation.right);
    }
}
