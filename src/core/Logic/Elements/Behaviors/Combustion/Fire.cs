// <copyright file="Fire.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using OpenTK.Mathematics;
using VoxelGame.Core.Actors;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Behaviors.Aspects.Strategies;
using VoxelGame.Core.Behaviors.Events;
using VoxelGame.Core.Logic.Attributes;
using VoxelGame.Core.Logic.Elements.Behaviors.Meshables;
using VoxelGame.Core.Logic.Elements.Behaviors.Visuals;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Utilities.Resources;
using VoxelGame.Core.Visuals;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Core.Logic.Elements.Behaviors.Combustion;

/// <summary>
/// Spreads and burns blocks.
/// </summary>
public class Fire : BlockBehavior, IBehavior<Fire, BlockBehavior, Block>
{
    private const UInt32 UpdateOffset = 150;
    private const UInt32 UpdateVariation = 25;
    
    private IAttribute<Boolean> Front => front ?? throw Exceptions.NotInitialized(nameof(front));
    private IAttribute<Boolean>? front;
    
    private IAttribute<Boolean> Back => back ?? throw Exceptions.NotInitialized(nameof(back));
    private IAttribute<Boolean>? back;
    
    private IAttribute<Boolean> Left => left ?? throw Exceptions.NotInitialized(nameof(left));
    private IAttribute<Boolean>? left;
    
    private IAttribute<Boolean> Right => right ?? throw Exceptions.NotInitialized(nameof(right));
    private IAttribute<Boolean>? right;
    
    private IAttribute<Boolean> Top => top ?? throw Exceptions.NotInitialized(nameof(top));
    private IAttribute<Boolean>? top;
    
    private Fire(Block subject) : base(subject)
    {
        subject.Require<Meshed>().IsAnimated.ContributeConstant(value: true);
        
        subject.Require<Complex>().Mesh.ContributeFunction(GetMesh);
        subject.BoundingVolume.ContributeFunction(GetBoundingVolume);
        
        subject.IsPlacementAllowed.ContributeFunction(GetIsPlacementAllowed);
        subject.PlacementState.ContributeFunction(GetPlacementState);
        
        ModelsInitializer = Aspect<(RID, RID, RID), Block>.New<Exclusive<(RID, RID, RID), Block>>(nameof(ModelsInitializer), this);
    }

    /// <inheritdoc/>
    public static Fire Construct(Block input)
    {
        return new Fire(input);
    }
    
    /// <summary>
    /// The models used for the block.
    /// </summary>
    public (RID complete, RID side, RID top) Models { get; private set; }
    
    /// <summary>
    /// Aspect used to initialize the <see cref="Models"/> property.
    /// </summary>
    public Aspect<(RID complete, RID side, RID top), Block> ModelsInitializer { get; }

    /// <inheritdoc/>
    public override void OnInitialize(BlockProperties properties)
    {
        properties.IsReplaceable.ContributeConstant(value: true);
        properties.IsUnshaded.ContributeConstant(value: true);
        properties.IsOpaque.ContributeConstant(value: false);

        Models = ModelsInitializer.GetValue(original: default, Subject);
    }

    /// <inheritdoc/>
    public override void DefineState(IStateBuilder builder)
    {
        front = builder.Define(nameof(front)).Boolean().Attribute();
        back = builder.Define(nameof(back)).Boolean().Attribute();
        left = builder.Define(nameof(left)).Boolean().Attribute();
        right = builder.Define(nameof(right)).Boolean().Attribute();
        top = builder.Define(nameof(top)).Boolean().Attribute();
    }

    /// <inheritdoc/>
    public override void SubscribeToEvents(IEventBus bus)
    {
        bus.Subscribe<Block.PlacementCompletedMessage>(OnPlacementCompleted);
        bus.Subscribe<Block.NeighborUpdateMessage>(OnNeighborUpdate);
        bus.Subscribe<Block.ScheduledUpdateMessage>(OnScheduledUpdate);
    }

    private BlockMesh GetMesh(BlockMesh original, (State state, ITextureIndexProvider textureIndexProvider, IBlockModelProvider blockModelProvider, VisualConfiguration visuals) context)
    {
        (State state, ITextureIndexProvider textureIndexProvider, IBlockModelProvider blockModelProvider, VisualConfiguration _) = context;
        
        BlockModel complete = blockModelProvider.GetModel(Models.complete);

        BlockModel side = blockModelProvider.GetModel(Models.side);
        BlockModel up = blockModelProvider.GetModel(Models.top);
        
        (BlockModel north, BlockModel east, BlockModel south, BlockModel west) =
            side.CreateAllOrientations(rotateTopAndBottomTexture: true); // todo: do not create all orientations, or at least only crete the other parts on demand

        Boolean any = IsAnySideBurning(state);

        if (!any) return complete.CreateMesh(textureIndexProvider);

        List<BlockModel> requiredModels = new(capacity: 5);

        if (state.Get(Front))
            requiredModels.Add(south);
            
        if (state.Get(Back))
            requiredModels.Add(north);
            
        if (state.Get(Left))
            requiredModels.Add(west);
            
        if (state.Get(Right))
            requiredModels.Add(east);
            
        if (state.Get(Top))
            requiredModels.Add(up);

        return BlockModel.GetCombinedMesh(textureIndexProvider, requiredModels.ToArray());
    }
    
    private BoundingVolume GetBoundingVolume(BoundingVolume original, State state)
    {
        Boolean any = IsAnySideBurning(state);
        
        if (!any) return BoundingVolume.Block;

        List<BoundingVolume> volumes = new(capacity: 5);
        
        if (state.Get(Front)) AddVolume(Side.Front);
        if (state.Get(Back)) AddVolume(Side.Back);
        if (state.Get(Left)) AddVolume(Side.Left);
        if (state.Get(Right)) AddVolume(Side.Right);
        if (state.Get(Top)) AddVolume(Side.Top);
        
        switch (volumes.Count)
        {
            case 0:
                return BoundingVolume.Block;
            
            case 1:
                return volumes[0];
            
            default:
                BoundingVolume parent = volumes[0];
                return new BoundingVolume(parent.Center, parent.Extents, volumes[1..].ToArray());
        }

        void AddVolume(Side side)
        {
            Vector3d offset = side.Direction().ToVector3() * 0.4f;

            volumes.Add(new BoundingVolume(
                new Vector3d(x: 0.5f, y: 0.5f, z: 0.5f) + offset,
                new Vector3d(x: 0.5f, y: 0.5f, z: 0.5f) - offset.Absolute()));
        }
    }
    
    private static Boolean GetIsPlacementAllowed(Boolean original, (World world, Vector3i position, Actor? actor) context)
    {
        (World world, Vector3i position, Actor? _) = context;

        if (world.GetBlock(position.Below())?.IsFullySolid == true)
            return true;
        
        return GetPlacementSides(world, position) != Sides.None;
    }
    
    private void OnPlacementCompleted(Block.PlacementCompletedMessage message)
    {
        Subject.ScheduleUpdate(message.World, message.Position, GetDelay(message.Position));
    }
    
    private void OnNeighborUpdate(Block.NeighborUpdateMessage message)
    {
        if (message.Side == Side.Bottom)
        {
            if (IsAnySideBurning(message.State)) return;
            
            Sides sides = GetPlacementSides(message.World, message.Position);
            
            UpdateOrDestroy(sides);
        }
        else
        {
            if (!IsSideBurning(message.State, message.Side)) return;
            if (message.World.GetBlock(message.Side.Offset(message.Position))?.IsFullySolid == true) return;
            
            Sides sides = GetSidesBurning(message.State);

            sides &= ~message.Side.ToFlag();
            
            UpdateOrDestroy(sides);
        }

        void UpdateOrDestroy(Sides sides)
        {
            if (sides == Sides.None)
            {
                Subject.Destroy(message.World, message.Position);
            }
            else
            {
                State state = SetSides(message.State, sides);
                message.World.SetBlock(new BlockInstance(state), message.Position);
            }
        }
    }
    
    private void OnScheduledUpdate(Block.ScheduledUpdateMessage message)
    {
        var canBurn = false;
        
        Sides sides = GetSidesBurning(message.State);

        if (!IsAnySideBurning(message.State))
        {
            canBurn |= BurnAt(message.Position.Below());
            sides = Sides.All;
        }

        foreach (Side side in Side.All.Sides())
        {
            if (side == Side.Bottom) continue;

            if (sides.HasFlag(side.ToFlag()))
            {
                canBurn |= BurnAt(side.Offset(message.Position));
            }
        }

        if (!canBurn)
        {
            Subject.Destroy(message.World, message.Position);
        }
        else
        {
            Subject.ScheduleUpdate(message.World, message.Position, GetDelay(message.Position));
        }

        Boolean BurnAt(Vector3i burnPosition)
        {
            if (message.World.GetBlock(burnPosition)?.Block.Get<Combustible>() is not {} combustible) 
                return false;

            if (!combustible.DoBurn(message.World, burnPosition, Subject)) 
                return true;

            if (message.World.GetBlock(burnPosition.Below())?.Block.Get<AshCoverable>() is {} coverable)
                coverable.CoverWithAsh(message.World, burnPosition.Below());

            Subject.Place(message.World, burnPosition);

            return true;
        }
    }
    
    private State GetPlacementState(State original, (World world, Vector3i position, Actor? actor) context)
    {
        (World world, Vector3i position, Actor? _) = context;
        
        Sides sides = GetPlacementSides(world, position);

        return SetSides(original, sides);
    }
    
    private State SetSides(State original, Sides sides)
    {
        original.Set(Front, sides.HasFlag(Sides.Front));
        original.Set(Back, sides.HasFlag(Sides.Back));
        original.Set(Left, sides.HasFlag(Sides.Left));
        original.Set(Right, sides.HasFlag(Sides.Right));
        original.Set(Top, sides.HasFlag(Sides.Top));
        
        return original;
    }

    private static Sides GetPlacementSides(World world, Vector3i position)
    {
        var sides = Sides.None;

        if (CheckSide(Side.Bottom)) return Sides.None;
        
        if (CheckSide(Side.Front)) sides |= Sides.Front;
        if (CheckSide(Side.Back)) sides |= Sides.Back;
        if (CheckSide(Side.Left)) sides |= Sides.Left;
        if (CheckSide(Side.Right)) sides |= Sides.Right;
        if (CheckSide(Side.Top)) sides |= Sides.Top;
        
        return sides;
        
        Boolean CheckSide(Side side)
        {
            BlockInstance? neighbor = world.GetBlock(side.Offset(position));
            return neighbor is {IsFullySolid: true};
        }
    }

    private Boolean IsAnySideBurning(State state)
    {
        return state.Get(Front) || state.Get(Back) || state.Get(Left) || state.Get(Right) || state.Get(Top);
    }
    
    private Boolean IsSideBurning(State state, Side side)
    {
        return side switch
        {
            Side.Front => state.Get(Front),
            Side.Back => state.Get(Back),
            Side.Left => state.Get(Left),
            Side.Right => state.Get(Right),
            Side.Top => state.Get(Top),
            Side.Bottom or Side.All => false, 
            _ => throw Exceptions.UnsupportedEnumValue(side)
        };
    }
    
    private Sides GetSidesBurning(State state)
    {
        var sides = Sides.None;
        
        if (state.Get(Front)) sides |= Sides.Front;
        if (state.Get(Back)) sides |= Sides.Back;
        if (state.Get(Left)) sides |= Sides.Left;
        if (state.Get(Right)) sides |= Sides.Right;
        if (state.Get(Top)) sides |= Sides.Top;

        return sides;
    }
    
    private static UInt32 GetDelay(Vector3i position)
    {
        return UpdateOffset +
               (BlockUtilities.GetPositionDependentNumber(position, UpdateVariation * 2) - UpdateVariation);
    }
}
