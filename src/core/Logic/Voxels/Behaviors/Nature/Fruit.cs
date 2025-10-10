// <copyright file="Fruit.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Core.Behaviors;

namespace VoxelGame.Core.Logic.Voxels.Behaviors.Nature;

/// <summary>
///     A block that is grown by a plant as its fruit.
/// </summary>
public class Fruit : BlockBehavior, IBehavior<Fruit, BlockBehavior, Block>
{
    private Fruit(Block subject) : base(subject)
    {
        subject.Require<Grounded>();
    }

    /// <inheritdoc />
    public static Fruit Construct(Block input)
    {
        return new Fruit(input);
    }
}
