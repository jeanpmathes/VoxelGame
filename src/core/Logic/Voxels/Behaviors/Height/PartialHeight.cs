// <copyright file="PartialHeight.cs" company="VoxelGame">
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
using VoxelGame.Core.Physics;
using VoxelGame.Core.Visuals.Meshables;

namespace VoxelGame.Core.Logic.Voxels.Behaviors.Height;

/// <summary>
///     Behavior associated with the <see cref="Meshable.PartialHeight" /> meshing.
///     It declares a state-dependent height aspect of a block.
/// </summary>
public partial class PartialHeight : BlockBehavior, IBehavior<PartialHeight, BlockBehavior, Block>
{
    [Constructible]
    private PartialHeight(Block subject) : base(subject)
    {
        Height = Aspect<BlockHeight, State>.New<Exclusive<BlockHeight, State>>(nameof(Height), this);

        subject.BoundingVolume.ContributeFunction(GetBoundingVolume, exclusive: true);
    }
    
    /// <summary>
    ///     The height aspect.
    /// </summary>
    public Aspect<BlockHeight, State> Height { get; }

    private BoundingVolume GetBoundingVolume(BoundingVolume original, State state)
    {
        return BoundingVolume.BlockWithHeight(GetHeight(state).ToInt32());
    }

    /// <inheritdoc />
    protected override void OnValidate(IValidator validator)
    {
        if (!Subject.Is<Meshables.PartialHeight>())
            validator.ReportWarning("Partial height blocks should use the corresponding meshing behavior");
    }

    /// <summary>
    ///     Get the height of the block in the given state.
    /// </summary>
    /// <param name="state">The state of the block.</param>
    /// <returns>The height of the block.</returns>
    public BlockHeight GetHeight(State state)
    {
        return Height.GetValue(BlockHeight.Minimum, state);
    }

    /// <summary>
    ///     Get whether a side of the block is full in a given state.
    /// </summary>
    /// <param name="side">The side to check.</param>
    /// <param name="state">The state of the block.</param>
    /// <returns><c>true</c> if the side is full, <c>false</c> otherwise.</returns>
    public Boolean IsSideFull(Side side, State state)
    {
        if (side == Side.Bottom) return true;

        return GetHeight(state).IsFull;
    }

    /// <summary>
    ///     Get whether the block is full in a given state.
    /// </summary>
    /// <param name="state">The state of the block.</param>
    /// <returns><c>true</c> if the block is full, <c>false</c> otherwise.</returns>
    public Boolean IsFull(State state)
    {
        return GetHeight(state).IsFull;
    }
}
