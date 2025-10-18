// <copyright file="PartialHeight.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using VoxelGame.Annotations;
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
    /// <summary>
    ///     The (inclusive) minimum height of a block with variable height, is actually more than just a 2D plane.
    /// </summary>
    public const Int32 MinimumHeight = 0;

    /// <summary>
    ///     The (inclusive) maximum height of a block with variable height, equivalent to a full block.
    /// </summary>
    public const Int32 MaximumHeight = 15;

    /// <summary>
    ///     Special constant to indicate that a block has no height.
    ///     Only allowed in certain situations, e.g. for some calculations.
    /// </summary>
    public const Int32 NoHeight = -1;

    [Constructible]
    private PartialHeight(Block subject) : base(subject)
    {
        Height = Aspect<Int32, State>.New<Exclusive<Int32, State>>(nameof(Height), this);

        subject.BoundingVolume.ContributeFunction(GetBoundingVolume, exclusive: true);
    }

    /// <summary>
    ///     The half height. A block with this height fills half of a position.
    /// </summary>
    public static Int32 HalfHeight => MaximumHeight / 2;

    /// <summary>
    ///     The height aspect, produced values must be in the range [<see cref="MinimumHeight" />, <see cref="MaximumHeight" />
    ///     ].
    /// </summary>
    public Aspect<Int32, State> Height { get; }

    private BoundingVolume GetBoundingVolume(BoundingVolume original, State state)
    {
        return BoundingVolume.BlockWithHeight(GetHeight(state));
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
    /// <returns>The height of the block, in the range [<see cref="MinimumHeight" />, <see cref="MaximumHeight" />].</returns>
    public Int32 GetHeight(State state)
    {
        return Height.GetValue(MinimumHeight, state);
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

        return GetHeight(state) == MaximumHeight;
    }

    /// <summary>
    ///     Get whether the block is full in a given state.
    /// </summary>
    /// <param name="state">The state of the block.</param>
    /// <returns><c>true</c> if the block is full, <c>false</c> otherwise.</returns>
    public Boolean IsFull(State state)
    {
        return GetHeight(state) == MaximumHeight;
    }
}
