// <copyright file="CompositeModelled.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using OpenTK.Mathematics;
using VoxelGame.Annotations;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Logic.Attributes;

namespace VoxelGame.Core.Logic.Voxels.Behaviors.Visuals;

/// <summary>
///     Performs part selection for a <see cref="Modelled" /> block based on the <see cref="Composite" /> parts.
/// </summary>
public partial class CompositeModelled : BlockBehavior, IBehavior<CompositeModelled, BlockBehavior, Block>
{
    private readonly Composite composite;

    [Constructible]
    private CompositeModelled(Block subject) : base(subject)
    {
        composite = subject.Require<Composite>();
        subject.Require<Modelled>().Selector.ContributeFunction(GetSelector);
    }

    private Selector GetSelector(Selector original, State state)
    {
        Vector3i part = composite.GetPartPosition(state);
        
        // Maybe the stored models are not correct?
        part.X = composite.GetSize(state).X - part.X - 1;
        part.Z = composite.GetSize(state).Z - part.Z - 1;
        
        return original.WithPart(part);
    }
}
