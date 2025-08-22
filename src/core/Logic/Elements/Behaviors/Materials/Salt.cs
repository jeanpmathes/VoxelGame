// <copyright file="Salt.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Behaviors.Events;
using VoxelGame.Core.Logic.Attributes;
using VoxelGame.Core.Logic.Elements.Behaviors.Fluids;

namespace VoxelGame.Core.Logic.Elements.Behaviors.Materials;

/// <summary>
/// Salt is a solid material that can be put into fresh water to create salt water.
/// </summary>
public class Salt : BlockBehavior, IBehavior<Salt, BlockBehavior, Block>
{
    private Salt(Block subject) : base(subject)
    {
        subject.Require<Fillable>().IsInflowAllowed.ContributeFunction(GetIsInflowAllowed);
    }

    /// <inheritdoc/>
    public static Salt Construct(Block input)
    {
        return new Salt(input);
    }

    /// <inheritdoc/>
    public override void SubscribeToEvents(IEventBus bus)
    {
        bus.Subscribe<Block.ContentUpdateMessage>(OnContentUpdate);
    }

    private static Boolean GetIsInflowAllowed(Boolean original, (World world, Vector3i position, State state, Side side, Fluid fluid) context)
    {
        (World _, Vector3i _, State _, Side _, Fluid fluid) = context;

        return fluid.IsLiquid;
    }
    
    private void OnContentUpdate(Block.ContentUpdateMessage message)
    {
        if (message.NewContent.Fluid.IsEmpty) return;

        Subject.Destroy(message.World, message.Position);

        if (message.NewContent.Fluid is {Fluid: var fluid, Level: var level}
            && fluid == Elements.Fluids.Instance.FreshWater)
            message.World.SetFluid(Elements.Fluids.Instance.SeaWater.AsInstance(level), message.Position);
    }
}
