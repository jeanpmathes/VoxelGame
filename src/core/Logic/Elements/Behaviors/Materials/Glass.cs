// <copyright file="Glass.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Logic.Elements.Behaviors.Connection;

namespace VoxelGame.Core.Logic.Elements.Behaviors.Materials;

/// <summary>
///     Blocks made out of glass.
/// </summary>
public class Glass : BlockBehavior, IBehavior<Glass, BlockBehavior, Block>
{
    private Glass(Block subject) : base(subject)
    {
        subject.Require<Connectable>().StrengthInitializer.ContributeConstant(Connectable.Strengths.Thin);
    }

    /// <inheritdoc />
    public static Glass Construct(Block input)
    {
        return new Glass(input);
    }

    /// <inheritdoc />
    public override void OnInitialize(BlockProperties properties)
    {
        properties.IsOpaque.ContributeConstant(value: false);
    }
}
