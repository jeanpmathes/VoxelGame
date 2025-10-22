// <copyright file="Soil.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Annotations.Attributes;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Behaviors.Events;
using VoxelGame.Core.Logic.Attributes;
using VoxelGame.Core.Logic.Voxels.Behaviors.Combustion;
using VoxelGame.Core.Logic.Voxels.Behaviors.Fluids;
using VoxelGame.Core.Utilities.Units;

namespace VoxelGame.Core.Logic.Voxels.Behaviors.Nature;

/// <summary>
///     Core behavior for all soil blocks.
///     Not only the soil block itself, but also blocks that contain significant amounts of soil.
/// </summary>
public partial class Soil : BlockBehavior, IBehavior<Soil, BlockBehavior, Block>
{
    [Constructible]
    private Soil(Block subject) : base(subject)
    {
        subject.Require<Plantable>();
        subject.Require<Membrane>().MaxViscosity.Initializer.ContributeConstant(new Viscosity { MilliPascalSeconds = 6.5 });
        subject.Require<Fillable>().IsFluidMeshed.Initializer.ContributeConstant(value: false);
    }

    /// <inheritdoc />
    public override void SubscribeToEvents(IEventBus bus)
    {
        bus.Subscribe<Block.IRandomUpdateMessage>(OnRandomUpdate);
        bus.Subscribe<Block.IGeneratorUpdateMessage>(OnGeneratorUpdate);

        bus.Subscribe<AshCoverable.IAshCoverMessage>(OnAshCover);
    }

    private static void OnRandomUpdate(Block.IRandomUpdateMessage message)
    {
        FluidInstance? fluid = message.World.GetFluid(message.Position);

        if (fluid is {IsAnyWater: true, Level.IsFull: true})
            message.World.SetContent(new Content(Blocks.Instance.Environment.Mud, Voxels.Fluids.Instance.None), message.Position);
    }

    private static void OnGeneratorUpdate(Block.IGeneratorUpdateMessage message)
    {
        if (message.Content.Fluid is {IsAnyWater: true, Level.IsFull: true})
            message.Content = new Content(Blocks.Instance.Environment.Mud);
    }

    private static void OnAshCover(AshCoverable.IAshCoverMessage message)
    {
        message.World.SetBlock(new State(Blocks.Instance.Environment.AshCoveredSoil), message.Position);
    }

    /// <inheritdoc />
    protected override void OnValidate(IValidator validator)
    {
        if (!Subject.Is<Wet>())
        {
            validator.ReportWarning("Soil blocks must be able to get wet in some way, preferably with visual representation of that");
        }
    }
}
