// <copyright file="Targeting.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Annotations.Attributes;
using VoxelGame.Core.Logic.Attributes;
using VoxelGame.Core.Logic.Voxels;

namespace VoxelGame.Core.Actors.Components;

/// <summary>
///     Contains the targeting information of an <see cref="Actor" />.
///     This does not perform any targeting logic, which requires other components.
/// </summary>
public partial class Targeting : ActorComponent
{
    [Constructible]
    private Targeting(Actor subject) : base(subject) {}

    /// <summary>
    ///     Whether something is currently targeted by the actor.
    /// </summary>
    public Boolean HasTarget { get; set; }

    /// <summary>
    ///     The targeted side, or <see cref="Logic.Voxels.Side.All" /> if no side is targeted.
    /// </summary>
    public Side Side { get; set; }

    /// <summary>
    ///     Get the block position targeted by the actor.
    ///     If the actor is not targeting a block, this will be null.
    /// </summary>
    public Vector3i? Position { get; set; }

    /// <summary>
    ///     The targeted block, or null if no block is targeted.
    /// </summary>
    public State? Block { get; set; }

    /// <summary>
    ///     The targeted fluid, or null if no fluid is targeted.
    /// </summary>
    public FluidInstance? Fluid { get; set; }
}

/// <summary>
///     Extensions for the <see cref="Targeting" /> component.
/// </summary>
public static class TargetingExtensions
{
    /// <summary>
    ///     Get the side targeted by the actor, or null if no side is targeted or the actor does not have a targeting
    ///     component.
    /// </summary>
    /// <param name="actor">The actor to check.</param>
    /// <returns>The side, or <c>null</c>.</returns>
    public static Side? GetTargetedSide(this Actor actor)
    {
        if (actor.GetComponent<Targeting>() is {} targeting)
        {
            return targeting.HasTarget ? targeting.Side : null;
        }

        return null;
    }
}
