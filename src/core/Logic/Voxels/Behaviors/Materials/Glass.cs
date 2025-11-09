// <copyright file="Glass.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Annotations.Attributes;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Logic.Voxels.Behaviors.Connection;

namespace VoxelGame.Core.Logic.Voxels.Behaviors.Materials;

/// <summary>
///     Blocks made out of glass.
/// </summary>
public partial class Glass : BlockBehavior, IBehavior<Glass, BlockBehavior, Block>
{
    [Constructible]
    private Glass(Block subject) : base(subject)
    {
        subject.Require<Connectable>().Strength.Initializer.ContributeConstant(Connectable.Strengths.Thin);
    }

    /// <inheritdoc />
    public override void OnInitialize(BlockProperties properties)
    {
        properties.IsOpaque.ContributeConstant(value: false);
    }
}
