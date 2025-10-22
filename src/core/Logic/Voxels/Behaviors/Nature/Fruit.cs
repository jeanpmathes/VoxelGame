// <copyright file="Fruit.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Annotations.Attributes;
using VoxelGame.Core.Behaviors;

namespace VoxelGame.Core.Logic.Voxels.Behaviors.Nature;

/// <summary>
///     A block that is grown by a plant as its fruit.
/// </summary>
public partial class Fruit : BlockBehavior, IBehavior<Fruit, BlockBehavior, Block>
{
    [Constructible]
    private Fruit(Block subject) : base(subject)
    {
        subject.Require<Grounded>();
    }
}
