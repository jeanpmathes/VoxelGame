// <copyright file="Vine.cs" company="VoxelGame">
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
using VoxelGame.Core.Logic.Elements.Behaviors.Combustion;
using VoxelGame.Core.Logic.Elements.Behaviors.Siding;
using VoxelGame.Core.Utilities;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Core.Logic.Elements.Behaviors.Nature;

/// <summary>
/// A flat block which grows downwards and can, as an alternative to a holding wall, also hang from other vines.
/// </summary>
public class Vine : BlockBehavior, IBehavior<Vine, BlockBehavior, Block>
{
    private const Int32 MaxAge = 8;
    
    private IAttribute<Int32> Age => age ?? throw Exceptions.NotInitialized(nameof(age));
    private IAttribute<Int32>? age;

    private readonly SingleSided siding;
    private readonly Attached attached;

    private Vine(Block subject) : base(subject)
    {
        subject.Require<Combustible>();
        
        siding = subject.Require<SingleSided>();
        
        attached = subject.Require<Attached>();
        attached.IsOtherwiseAttached.ContributeFunction(GetIsOtherwiseAttached);
    }

    /// <inheritdoc />
    public static Vine Construct(Block input)
    {
        return new Vine(input);
    }

    /// <inheritdoc />
    public override void DefineState(IStateBuilder builder)
    {
        age = builder.Define(nameof(age)).Int32(min: 0, MaxAge + 1).Attribute();
    }

    /// <inheritdoc />
    public override void SubscribeToEvents(IEventBus bus)
    {
        bus.Subscribe<Block.RandomUpdateMessage>(OnRandomUpdate);
        bus.Subscribe<Block.NeighborUpdateMessage>(OnNeighborUpdate);
    }

    private Boolean GetIsOtherwiseAttached(Boolean original, (World world, Vector3i position, State state) context)
    {
        (World world, Vector3i position, State state) = context;

        BlockInstance? above = world.GetBlock(position.Above());

        if (above == null) 
            return false;

        if (above.Value.Block != Subject)
            return false;

        return siding.GetSide(state) == above.Value.Block.Get<Vine>()?.siding.GetSide(above.Value.State);
    }
    
    private void OnRandomUpdate(Block.RandomUpdateMessage message)
    {
        Int32 currentAge = message.State.Get(Age);
        
        if (currentAge < MaxAge) 
        {
            message.World.SetBlock(new BlockInstance(message.State.With(Age, currentAge + 1)), message.Position);
        }
        else if (message.World.GetBlock(message.Position.Below())?.Block == Blocks.Instance.Core.Air) // todo: replace all air checks with checks for an empty behavior or maybe a new empty block flag, describe difference to replaceable
        {
            message.World.SetBlock(new BlockInstance(message.State.With(Age, value: 0)), message.Position.Below());
            message.World.SetBlock(new BlockInstance(message.State.With(Age, value: 0)), message.Position);
        }
    }
    
    private void OnNeighborUpdate(Block.NeighborUpdateMessage message)
    {
        if (message.Side != Side.Top)
            return;
        
        attached.CheckAttachment(message.World, message.Position, message.State);
    }
}
