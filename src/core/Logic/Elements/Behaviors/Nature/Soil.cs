// <copyright file="Soil.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Behaviors.Events;
using VoxelGame.Core.Logic.Elements.Behaviors.Combustion;
using VoxelGame.Core.Logic.Elements.Behaviors.Fluids;

namespace VoxelGame.Core.Logic.Elements.Behaviors.Nature;

/// <summary>
/// Core behavior for all soil blocks.
/// Not only the soil block itself, but also blocks that contain significant amounts of soil.
/// </summary>
public class Soil : BlockBehavior, IBehavior<Soil, BlockBehavior, Block>
{
    private Soil(Block subject) : base(subject)
    {
        subject.Require<Plantable>();
        subject.Require<Membrane>().MaxViscosityInitializer.ContributeConstant(value: 100);
        subject.Require<Fillable>().IsFluidRenderedInitializer.ContributeConstant(value: false);
    }
    
    /// <inheritdoc/>
    public static Soil Construct(Block input)
    {
        return new Soil(input);
    }

    /// <inheritdoc/>
    public override void SubscribeToEvents(IEventBus bus)
    {
        bus.Subscribe<Block.RandomUpdateMessage>(OnRandomUpdate);
        bus.Subscribe<Block.GeneratorUpdateMessage>(OnGeneratorUpdate);
        
        bus.Subscribe<AshCoverable.AshCoverMessage>(OnAshCover);
    }

    private static void OnRandomUpdate(Block.RandomUpdateMessage message)
    {
        FluidInstance? fluid = message.World.GetFluid(message.Position);

        if (fluid is {IsAnyWater: true, Level: FluidLevel.Eight})
            message.World.SetContent(new Content(Blocks.Instance.Environment.Mud, Elements.Fluids.Instance.None), message.Position);
    }
    
    private static void OnGeneratorUpdate(Block.GeneratorUpdateMessage message)
    {
        if (message.Content.Fluid is {IsAnyWater: true, Level: FluidLevel.Eight})
            message.Content = new Content(Blocks.Instance.Environment.Mud);
    }
    
    private static void OnAshCover(AshCoverable.AshCoverMessage message)
    {
        message.World.SetBlock(Blocks.Instance.Environment.AshCoveredSoil.AsInstance(), message.Position);
    }

    /// <inheritdoc/>
    protected override void OnValidate(IValidator validator)
    {
        if (!Subject.Is<Wet>())
        {
            validator.ReportWarning("Soil blocks must be able to get wet in some way, preferably with visual representation of that");
        }
    }
}
