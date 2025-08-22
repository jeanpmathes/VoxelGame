// <copyright file="Connectable.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Behaviors.Aspects.Strategies;
using VoxelGame.Core.Logic.Attributes;
using VoxelGame.Core.Utilities.Resources;

namespace VoxelGame.Core.Logic.Elements.Behaviors.Connection;

/// <summary>
/// Allows blocks like fences, walls and such to connect to this block.
/// Connectivity may only occur on the lateral sides of the block.
/// </summary>
public class Connectable : BlockBehavior, IBehavior<Connectable, BlockBehavior, Block>
{
    private Connectable(Block subject) : base(subject)
    {
        StrengthInitializer = Aspect<Strengths, Block>.New<Exclusive<Strengths, Block>>(nameof(StrengthInitializer), this);
        
        IsConnectionAllowed = Aspect<Boolean, State>.New<ANDing<State>>(nameof(IsConnectionAllowed), this);
    }
    
    /// <inheritdoc/>
    public static Connectable Construct(Block input)
    {
        return new Connectable(input);
    }
    
    /// <summary>
    /// The strength of the connection of this block.
    /// </summary>
    public Strengths Strength { get; private set; } 
    
    /// <summary>
    /// Aspect used to initialize the <see cref="Strength"/> property.
    /// </summary>
    public Aspect<Strengths, Block> StrengthInitializer { get; }
    
    /// <summary>
    /// Whether connection to this block is allowed in the given state.
    /// </summary>
    public Aspect<Boolean, State> IsConnectionAllowed { get; }

    /// <inheritdoc/>
    public override void OnInitialize(BlockProperties properties)
    {
        Strength = StrengthInitializer.GetValue(Strengths.None, Subject);
    }

    /// <inheritdoc/>
    protected override void OnValidate(IResourceContext context)
    {
        if (Strength == Strengths.None)
            context.ReportWarning(this, "Connectable blocks should have at least one connection strength defined");
    }

    /// <summary>
    /// Check whether two given connection strengths can connect to each other.
    /// The order of the two connection strengths does not matter.
    /// </summary>
    /// <param name="a">The first connection strength.</param>
    /// <param name="b">The second connection strength.</param>
    /// <returns><c>true</c> if the two connection strengths can connect to each other, <c>false</c> otherwise.</returns>
    public static Boolean CanConnect(Strengths a, Strengths b)
    {
        return (a & b) != Strengths.None;
    }

    /// <summary>
    /// The connection strength of this block.
    /// </summary>
    [Flags]
    public enum Strengths
    {
        /// <summary>
        /// The block does not allow any connections.
        /// </summary>
        None = 0,
        
        /// <summary>
        /// The block allows thin connectable blocks.
        /// Thin connectable blocks are two sixteenth wide.
        /// </summary>
        Thin = 1 << 0, // todo: check that the measurement given here and below is correct
        
        /// <summary>
        /// The block allows wide connectable blocks.
        /// Wide connectable blocks are four sixteenth wide.
        /// Note that the connecting block might require this block to close of the connection surface.
        /// </summary>
        Wide = 1 << 1
    }
}
