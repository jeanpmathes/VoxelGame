// <copyright file="CompositeModelled.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using OpenTK.Mathematics;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Logic.Attributes;

namespace VoxelGame.Core.Logic.Elements.Behaviors.Visuals;

/// <summary>
/// Performs part selection for a <see cref="Modelled"/> block based on the <see cref="Composite"/> parts.
/// </summary>
public class CompositeModelled : BlockBehavior, IBehavior<CompositeModelled, BlockBehavior, Block>
{
    private readonly Composite composite;
    
    private CompositeModelled(Block subject) : base(subject)
    {
        composite = subject.Require<Composite>();
        subject.Require<Modelled>().Selector.ContributeFunction(GetSelector);
    }

    /// <inheritdoc />
    public static CompositeModelled Construct(Block input)
    {
        return new CompositeModelled(input);
    }
    
    private Vector4i GetSelector(Vector4i original, State state)
    {
        return new Vector4i(composite.GetPartPosition(state), w: 0);
    }
}
