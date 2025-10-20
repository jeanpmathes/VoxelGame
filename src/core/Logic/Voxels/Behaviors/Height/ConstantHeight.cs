// <copyright file="ConstantHeight.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Annotations;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Behaviors.Aspects.Strategies;
using Void = VoxelGame.Toolkit.Utilities.Void;

namespace VoxelGame.Core.Logic.Voxels.Behaviors.Height;

/// <summary>
///     Defines the partial block height of a block as a constant value.
/// </summary>
/// <seealso cref="PartialHeight" />
public partial class ConstantHeight : BlockBehavior, IBehavior<ConstantHeight, BlockBehavior, Block>
{
    [Constructible]
    private ConstantHeight(Block subject) : base(subject)
    {
        subject.Require<PartialHeight>().Height.ContributeFunction((_, _) => Height.Get(), exclusive: true);
    }

    /// <summary>
    ///     The constant height of the block.
    /// </summary>
    public ResolvedProperty<BlockHeight> Height { get; } = ResolvedProperty<BlockHeight>.New<Exclusive<BlockHeight, Void>>(nameof(Height), BlockHeight.Minimum);

    /// <inheritdoc />
    public override void OnInitialize(BlockProperties properties)
    {
        Height.Initialize(this);
    }
}
