// <copyright file="ConstructionMaterial.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Logic.Voxels.Behaviors.Connection;

namespace VoxelGame.Core.Logic.Voxels.Behaviors.Materials;

/// <summary>
///     Blocks used for basic construction.
/// </summary>
public class ConstructionMaterial : BlockBehavior, IBehavior<ConstructionMaterial, BlockBehavior, Block>
{
    private ConstructionMaterial(Block subject) : base(subject)
    {
        subject.Require<Connectable>().StrengthInitializer.ContributeConstant(Connectable.Strengths.All);
    }

    /// <inheritdoc />
    public static ConstructionMaterial Construct(Block input)
    {
        return new ConstructionMaterial(input);
    }
}
