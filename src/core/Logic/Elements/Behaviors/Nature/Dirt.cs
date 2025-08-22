// <copyright file="Dirt.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Behaviors.Events;
using VoxelGame.Core.Logic.Elements.Behaviors.Combustion;
using VoxelGame.Core.Logic.Elements.Behaviors.Fluids;
using VoxelGame.Core.Utilities.Resources;

namespace VoxelGame.Core.Logic.Elements.Behaviors.Nature;

/// <summary>
/// Core behavior for all soil blocks.
/// Not only the soil block itself, but also blocks that contain significant amounts of soil.
/// </summary>
public class Dirt : BlockBehavior, IBehavior<Dirt, BlockBehavior, Block>
{
    private Dirt(Block subject) : base(subject)
    {
        subject.Require<Plantable>();
        subject.Require<Membrane>().MaxViscosityInitializer.ContributeConstant(value: 100);
    }
    
    /// <inheritdoc/>
    public static Dirt Construct(Block input)
    {
        return new Dirt(input);
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
            message.World.SetBlock(Blocks.Instance.Environment.Mud.AsInstance(), message.Position);
    }
    
    private static void OnGeneratorUpdate(Block.GeneratorUpdateMessage message)
    {
        if (message.Content.Fluid is {IsAnyWater: true, Level: FluidLevel.Eight})
            message.Content = new Content(Blocks.Instance.Environment.Mud);
    }
    
    private static void OnAshCover(AshCoverable.AshCoverMessage message)
    {
        message.World.SetBlock(Blocks.Instance.Environment.AshCoveredDirt.AsInstance(), message.Position);
    }

    /// <inheritdoc/>
    protected override void OnValidate(IResourceContext context)
    {
        if (!Subject.Has<Wet>())
        {
            context.ReportWarning(this, "Soil blocks must be able to get wet in some way, preferably with visual representation of that");
        }
    }
}
