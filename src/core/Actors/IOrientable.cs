// <copyright file="IOrientable.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using OpenTK.Mathematics;

namespace VoxelGame.Core.Actors;

/// <summary>
///     Something that has an orientation, given by a forward and right vector.
/// </summary>
public interface IOrientable
{
    /// <summary>
    ///     Gets the forward vector of this object.
    /// </summary>
    public Vector3d Forward { get; }

    /// <summary>
    ///     Gets the right vector of this object.
    /// </summary>
    public Vector3d Right { get; }

    /// <summary>
    ///     Gets the position of this object.
    /// </summary>
    public Vector3d Position { get; }
}
