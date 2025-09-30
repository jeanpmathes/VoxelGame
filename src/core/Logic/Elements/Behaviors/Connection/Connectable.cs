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

namespace VoxelGame.Core.Logic.Elements.Behaviors.Connection;

/// <summary>
///     Allows blocks like fences, walls and such to connect to this block.
///     Connectivity may only occur on the lateral sides of the block.
/// </summary>
public class Connectable : BlockBehavior, IBehavior<Connectable, BlockBehavior, Block>
{
    /// <summary>
    ///     The connection strength of this block.
    /// </summary>
    [Flags]
    public enum Strengths
    {
        /// <summary>
        ///     The block does not allow any connections.
        /// </summary>
        None = 0,

        /// <summary>
        ///     The block allows thin connectable blocks.
        ///     Thin connectable blocks are two sixteenth wide.
        /// </summary>
        Thin = 1 << 0, // todo: check that the measurement given here and below is correct

        /// <summary>
        ///     The block allows wide connectable blocks.
        ///     Wide connectable blocks are four sixteenth wide.
        ///     Note that the connecting block might require this block to close of the connection surface.
        /// </summary>
        Wide = 1 << 1,

        /// <summary>
        ///     The block allows all connectable blocks.
        ///     Blocks which are full and solid will typically use this.
        /// </summary>
        All = Thin | Wide
    }

    private Connectable(Block subject) : base(subject)
    {
        StrengthInitializer = Aspect<Strengths, Block>.New<Exclusive<Strengths, Block>>(nameof(StrengthInitializer), this);

        IsConnectionAllowed = Aspect<Boolean, (Side, State)>.New<ANDing<(Side, State)>>(nameof(IsConnectionAllowed), this);
    }

    /// <summary>
    ///     The strength of the connection of this block.
    /// </summary>
    public Strengths Strength { get; private set; }

    /// <summary>
    ///     Aspect used to initialize the <see cref="Strength" /> property.
    /// </summary>
    public Aspect<Strengths, Block> StrengthInitializer { get; }

    /// <summary>
    ///     Whether connection to this block is allowed in the given state.
    /// </summary>
    public Aspect<Boolean, (Side side, State state)> IsConnectionAllowed { get; }

    /// <inheritdoc />
    public static Connectable Construct(Block input)
    {
        return new Connectable(input);
    }

    /// <inheritdoc />
    public override void OnInitialize(BlockProperties properties)
    {
        Strength = StrengthInitializer.GetValue(Strengths.None, Subject);
    }

    /// <inheritdoc />
    protected override void OnValidate(IValidator validator)
    {
        if (Strength == Strengths.None)
            validator.ReportWarning("Connectable blocks should have at least one connection strength defined");
    }

    /// <summary>
    ///     Check whether another connectable can connect to this one on the given side.
    /// </summary>
    /// <param name="state">The state of this block.</param>
    /// <param name="other">The other connectable to check connection against.</param>
    /// <param name="side">The side of this block to check connection on.</param>
    /// <returns><c>true</c> if the blocks can connect, <c>false</c> otherwise.</returns>
    public Boolean CanConnect(State state, Side side, Connectable other)
    {
        if (!IsConnectionAllowed.GetValue(original: true, (side, state))) return false;

        Strengths a = Strength;
        Strengths b = other.Strength;

        return (a & b) != Strengths.None;
    }
}
