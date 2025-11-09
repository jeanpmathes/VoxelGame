// <copyright file="Piped.cs" company="VoxelGame">
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
using VoxelGame.Toolkit.Utilities;
using Void = VoxelGame.Toolkit.Utilities.Void;

namespace VoxelGame.Core.Logic.Voxels.Behaviors.Fluids;

/// <summary>
///     Allows pipes to connect to this block.
/// </summary>
public partial class Piped : BlockBehavior, IBehavior<Piped, BlockBehavior, Block>
{
    /// <summary>
    ///     The tier of the pipe.
    /// </summary>
    public enum PipeTier
    {
        /// <summary>
        ///     Primitive pipes are made of basic materials.
        /// </summary>
        Primitive,

        /// <summary>
        ///     Industrial pipes are made of advanced materials.
        /// </summary>
        Industrial
    }

    [Constructible]
    private Piped(Block subject) : base(subject)
    {
        IsConnectionAllowed = Aspect<Boolean, (State state, Side side)>.New<ANDing<(State, Side)>>(nameof(IsConnectionAllowed), this);
    }

    /// <summary>
    ///     The tier of this block, which determines what pipes can connect to it.
    /// </summary>
    public ResolvedProperty<PipeTier> Tier { get; } = ResolvedProperty<PipeTier>.New<Exclusive<PipeTier, Void>>(nameof(Tier));

    /// <summary>
    ///     Whether connection to this block is allowed in the given state from the given side.
    /// </summary>
    public Aspect<Boolean, (State state, Side side)> IsConnectionAllowed { get; }

    /// <inheritdoc />
    public override void OnInitialize(BlockProperties properties)
    {
        Tier.Initialize(this);
    }

    /// <summary>
    ///     Get the diameter of the pipe for a given tier.
    /// </summary>
    /// <param name="tier">The tier of the pipe.</param>
    /// <returns>The diameter of the pipe in meters.</returns>
    public static Double GetPipeDiameter(PipeTier tier)
    {
        return tier switch
        {
            PipeTier.Primitive => 0.3125,
            PipeTier.Industrial => 0.375,
            _ => throw Exceptions.UnsupportedEnumValue(tier)
        };
    }

    /// <summary>
    ///     Check whether a pipe of given tier can connect to this block in the given state from the given side.
    /// </summary>
    /// <param name="state">The state of the block.</param>
    /// <param name="side">The side from which the pipe is trying to connect.</param>
    /// <param name="tier">The tier of the pipe trying to connect.</param>
    /// <returns><c>true</c> if the pipe can connect, <c>false</c> otherwise.</returns>
    public Boolean CanConnect(State state, Side side, PipeTier tier)
    {
        return tier == Tier.Get() && IsConnectionAllowed.GetValue(original: true, (state, side));
    }
}
