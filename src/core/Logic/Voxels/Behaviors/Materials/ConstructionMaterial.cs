// <copyright file="ConstructionMaterial.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Annotations.Attributes;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Logic.Voxels.Behaviors.Connection;

namespace VoxelGame.Core.Logic.Voxels.Behaviors.Materials;

/// <summary>
///     Blocks used for basic construction.
/// </summary>
public partial class ConstructionMaterial : BlockBehavior, IBehavior<ConstructionMaterial, BlockBehavior, Block>
{
    [Constructible]
    private ConstructionMaterial(Block subject) : base(subject)
    {
        subject.Require<Connectable>().Strength.Initializer.ContributeConstant(Connectable.Strengths.All);
    }
}
