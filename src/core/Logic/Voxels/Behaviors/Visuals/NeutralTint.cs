// <copyright file="NeutralTint.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Annotations.Attributes;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Voxels.Behaviors.Visuals;

/// <summary>
///     Gives a block a <see cref="ColorS.Neutral" /> tint.
/// </summary>
public partial class NeutralTint : BlockBehavior, IBehavior<NeutralTint, BlockBehavior, Block>
{
    [Constructible]
    private NeutralTint(Block subject) : base(subject)
    {
        subject.Require<Meshed>().Tint.ContributeConstant(ColorS.Neutral);
    }
}
