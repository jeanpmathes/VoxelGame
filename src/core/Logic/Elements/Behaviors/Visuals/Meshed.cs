// <copyright file="Meshed.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Behaviors.Aspects.Strategies;
using VoxelGame.Core.Logic.Attributes;
using VoxelGame.Core.Visuals;
using VoxelGame.Core.Visuals.Meshables;

namespace VoxelGame.Core.Logic.Elements.Behaviors.Visuals;

/// <summary>
/// Core behavior for blocks that are meshed.
/// These are all blocks except those with <see cref="Meshable.Unmeshed"/>.
/// The behavior provides aspects shared by all meshing types.
/// </summary>
public class Meshed : BlockBehavior, IBehavior<Meshed, BlockBehavior, Block>
{
    /// <summary>
    /// The tint aspect of the block.
    /// Can be used to colorize the block.
    /// </summary>
    public Aspect<ColorS, State> Tint { get; }
    
    /// <summary>
    /// Whether the block is animated.
    /// If true, this poses special requirements to the used textures depending on the meshing type.
    /// </summary>
    public Aspect<Boolean, State> IsAnimated { get; }

    private Meshed(Block subject) : base(subject)
    {
        Tint = Aspect<ColorS, State>.New<Mix<State>>(nameof(Tint), this);
        IsAnimated = Aspect<Boolean, State>.New<ORing<State>>(nameof(IsAnimated), this);
    }
    
    /// <inheritdoc/>
    public static Meshed Construct(Block input)
    {
        return new Meshed(input);
    }
}
