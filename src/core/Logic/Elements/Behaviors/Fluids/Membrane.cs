// <copyright file="Membrane.cs" company="VoxelGame">
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
/// Controls inflow into the block, allowing to filter out which fluids can pass through.
/// </summary>
public class Membrane : BlockBehavior, IBehavior<Membrane, BlockBehavior, Block>
{
    private Membrane(Block subject) : base(subject)
    {
        subject.Require<Fillable>().IsInflowAllowed.ContributeFunction(GetIsInflowAllowed);
        
        MaxViscosityInitializer = Aspect<Int32, Block>.New<Minimum<Int32, Block>>(nameof(MaxViscosityInitializer), this);
    }

    /// <summary>
    /// Only fluids with a viscosity less than this value can flow into the block.
    /// </summary>
    public Int32 MaxViscosity { get; private set; } = 1000;
    
    /// <summary>
    /// Aspect used to initialize the <see cref="MaxViscosity"/> property.
    /// </summary>
    public Aspect<Int32, Block> MaxViscosityInitializer { get; }
    
    /// <inheritdoc/>
    public static Membrane Construct(Block input)
    {
        return new Membrane(input);
    }

    /// <inheritdoc/>
    public override void OnInitialize(BlockProperties properties)
    {
        MaxViscosity = MaxViscosityInitializer.GetValue(original: 1000, Subject);
    }
    
    private Boolean GetIsInflowAllowed(Boolean original, (World world, Vector3i position, State state, Side side, Fluid fluid) context)
    {
        (World _, Vector3i _, State _, Side _, Fluid fluid) = context;
        
        return fluid.Viscosity < MaxViscosity;
    }
}
