// <copyright file="Fillable.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Behaviors.Aspects.Strategies;
using VoxelGame.Core.Logic.Attributes;

namespace VoxelGame.Core.Logic.Elements.Behaviors.Fluids;

/// <summary>
/// Makes a block able to be filled with fluids.
/// </summary>
public class Fillable : BlockBehavior, IBehavior<Fillable, BlockBehavior, Block>
{
    private Fillable(Block subject) : base(subject)
    {
        IsFluidRenderedInitializer = Aspect<Boolean, Block>.New<ORing<Block>>(nameof(IsFluidRenderedInitializer), this);
        
        IsInflowAllowed = Aspect<Boolean, (World, Vector3i, State, Side, Fluid)>.New<ANDing<(World, Vector3i, State, Side, Fluid)>>(nameof(IsInflowAllowed), this);
        IsOutflowAllowed = Aspect<Boolean, (World, Vector3i, State, Side, Fluid)>.New<ANDing<(World, Vector3i, State, Side, Fluid)>>(nameof(IsOutflowAllowed), this);
    }

    /// <summary>
    /// Whether the fluid filling this block should be rendered.
    /// </summary>
    public Boolean IsFluidRendered { get; private set; } // todo: rename to IsFluidMeshed

    /// <summary>
    /// Aspect used to initialize the <see cref="IsFluidRendered"/> property.
    /// </summary>
    public Aspect<Boolean, Block> IsFluidRenderedInitializer { get; }

    /// <summary>
    /// Whether inflow is allowed through a certain side.
    /// </summary>
    public Aspect<Boolean, (World world, Vector3i position, State state, Side side, Fluid fluid)> IsInflowAllowed { get; }
    
    /// <summary>
    /// Whether outflow is allowed through a certain side.
    /// </summary>
    public Aspect<Boolean, (World world, Vector3i position, State state, Side side, Fluid fluid)> IsOutflowAllowed { get; }

    /// <inheritdoc/>
    public static Fillable Construct(Block input)
    {
        return new Fillable(input);
    }

    /// <inheritdoc/>
    public override void OnInitialize(BlockProperties properties)
    {
        IsFluidRendered = IsFluidRenderedInitializer.GetValue(original: true, Subject);
    }
    
    /// <summary>
    /// Check whether inflow is allowed through a certain side.
    /// </summary>
    /// <param name="world">The world in which the block is located.</param>
    /// <param name="position">The position of the block.</param>
    /// <param name="side">The side through which the inflow is being checked.</param>
    /// <param name="fluid">The fluid that is being checked for inflow.</param>
    /// <returns>True if inflow is allowed, false otherwise.</returns>
    public Boolean CanInflow(World world, Vector3i position, Side side, Fluid fluid)
    {
        BlockInstance? block = world.GetBlock(position);
        
        if (block is not { State: var state }) return false;
        if (state.Block != Subject) return false;
        
        return IsInflowAllowed.GetValue(original: true, (world, position, state, side, fluid));
    }
    
    /// <summary>
    /// Check whether outflow is allowed through a certain side.
    /// </summary>
    /// <param name="world">The world in which the block is located.</param>
    /// <param name="position">The position of the block.</param>
    /// <param name="side">The side through which the outflow is being checked.</param>
    /// <param name="fluid">The fluid that is being checked for outflow.</param>
    /// <returns>True if outflow is allowed, false otherwise.</returns>
    public Boolean CanOutflow(World world, Vector3i position, Side side, Fluid fluid)
    {
        BlockInstance? block = world.GetBlock(position);
        
        if (block is not { State: var state }) return false;
        if (state.Block != Subject) return false;
        
        return IsOutflowAllowed.GetValue(original: true, (world, position, state, side, fluid));
    }
}
