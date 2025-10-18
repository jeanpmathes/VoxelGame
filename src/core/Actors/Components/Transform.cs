// <copyright file="Transform.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using OpenTK.Mathematics;
using VoxelGame.Annotations;
using VoxelGame.Core.Utilities;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Core.Actors.Components;

/// <summary>
///     Gives an <see cref="Actor" /> a position and orientation in the world.
/// </summary>
public partial class Transform : ActorComponent, IOrientable
{
    private Vector3d actualPosition;

    [Constructible]
    private Transform(Actor subject) : base(subject) {}

    /// <summary>
    ///     Get the rotation of the actor in the world.
    /// </summary>
    public Quaterniond Rotation { get; set; } = Quaterniond.Identity;

    /// <summary>
    ///     Get the forward vector of the actor.
    /// </summary>
    public Vector3d Forward => Rotation * Vector3d.UnitX;

    /// <summary>
    ///     Get the right vector of the actor.
    /// </summary>
    public Vector3d Right => Rotation * Vector3d.UnitZ;

    /// <inheritdoc />
    public Vector3d Position
    {
        get => actualPosition;
        set => actualPosition = value.ClampComponents(-Subject.World.Extents, Subject.World.Extents);
    }
}
