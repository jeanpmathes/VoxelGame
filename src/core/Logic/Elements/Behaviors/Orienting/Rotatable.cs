﻿// <copyright file="Rotatable.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Behaviors.Aspects.Strategies;
using VoxelGame.Core.Logic.Attributes;

namespace VoxelGame.Core.Logic.Elements.Behaviors.Orienting;

/// <summary>
/// Core behavior for blocks that can be rotated in some way.
/// </summary>
public class Rotatable : BlockBehavior, IBehavior<Rotatable, BlockBehavior, Block> // todo: use the Orientable behavior as a step above this, as it is more general
{
    private Rotatable(Block subject) : base(subject)
    {
        Rotation = Aspect<Side, State>.New<Exclusive<Side, State>>(nameof(Rotation), this);
    }
    
    /// <summary>
    /// Get which side of the block corresponds to the <see cref="Side.Front"/> in a given state.
    /// For example, if this returns <see cref="Side.Top"/>, then the block is oriented upwards in that state.
    /// </summary>
    public Aspect<Side, State> Rotation { get; }
    
    /// <inheritdoc />
    public static Rotatable Construct(Block input)
    {
        return new Rotatable(input);
    }
}
