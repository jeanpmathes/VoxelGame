// <copyright file="Rotatable.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using VoxelGame.Annotations.Attributes;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Behaviors.Aspects.Strategies;
using VoxelGame.Core.Logic.Attributes;
using VoxelGame.Core.Logic.Voxels.Behaviors.Meshables;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Core.Logic.Voxels.Behaviors.Orienting;

/// <summary>
///     Core behavior for blocks that can be rotated in some way.
/// </summary>
public partial class Rotatable : BlockBehavior, IBehavior<Rotatable, BlockBehavior, Block>
{
    [Constructible]
    private Rotatable(Block subject) : base(subject)
    {
        subject.Require<Orientable>();
        
        subject.RequireIfPresent<RotatableSimpleBlock, Simple>();

        Axis = Aspect<Axis, State>.New<Exclusive<Axis, State>>(nameof(Axis), this);
        Turns = Aspect<Int32, State>.New<Exclusive<Int32, State>>(nameof(Turns), this);
    }

    /// <summary>
    ///     The axis around which the block is rotated in a given state.
    /// </summary>
    public Aspect<Axis, State> Axis { get; }

    /// <summary>
    ///     Get the number of 90° clockwise turns the block has undergone from its original orientation in a given state.
    /// </summary>
    public Aspect<Int32, State> Turns { get; }

    /// <summary>
    ///     Get the current rotation axis of the block in the given state.
    /// </summary>
    /// <param name="state">The state to get the axis from.</param>
    /// <returns>The current rotation axis of the block.</returns>
    public Axis GetAxis(State state)
    {
        return Axis.GetValue(Utilities.Axis.Y, state);
    }

    /// <summary>
    ///     Get the current number of 90° clockwise turns around <see cref="Axis" /> the block has undergone from its original
    ///     orientation in the given state.
    /// </summary>
    /// <param name="state">The state to get the number of turns from.</param>
    /// <returns>The current number of 90° clockwise turns around <see cref="Axis" />.</returns>
    public Int32 GetTurns(State state)
    {
        return Turns.GetValue(original: 0, state);
    }
}
