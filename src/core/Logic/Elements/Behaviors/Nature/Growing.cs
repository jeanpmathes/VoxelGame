// <copyright file="Growing.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Annotations;
using VoxelGame.Core.Actors;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Behaviors.Aspects.Strategies;
using VoxelGame.Core.Behaviors.Events;
using VoxelGame.Core.Logic.Attributes;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Core.Logic.Elements.Behaviors.Nature;

/// <summary>
/// A block that grows upwards and requires a specific ground block to be placed on.
/// </summary>
public partial class Growing  : BlockBehavior, IBehavior<Growing, BlockBehavior, Block>
{
    private Block requiredGround = null!;

    [LateInitialization]
    private partial IAttribute<Int32> Age { get; set; }

    private const Int32 MaxAge = 7;
    private const Int32 MaxHeight = 4;
    
    private Growing(Block subject) : base(subject)
    {
        RequiredGroundInitializer = Aspect<String?, Block>.New<Exclusive<String?, Block>>(nameof(RequiredGroundInitializer), this);
        
        subject.IsPlacementAllowed.ContributeFunction(GetIsPlacementAllowed);
    }

    /// <summary>
    /// The required ground block.
    /// </summary>
    public String? RequiredGround { get; private set; }
    
    /// <summary>
    /// Aspect used to initialize the <see cref="RequiredGround"/> property.
    /// </summary>
    public Aspect<String?, Block> RequiredGroundInitializer { get; }
    
    /// <inheritdoc/>
    public static Growing Construct(Block input)
    {
        return new Growing(input);
    }

    /// <inheritdoc/>
    public override void OnInitialize(BlockProperties properties)
    {
        RequiredGround = RequiredGroundInitializer.GetValue(original: null, Subject);
    }

    /// <inheritdoc/>
    public override void DefineState(IStateBuilder builder)
    {
        Age = builder.Define(nameof(Age)).Int32(min: 0, MaxAge + 1).Attribute();
    }

    /// <inheritdoc/>
    public override void SubscribeToEvents(IEventBus bus)
    {
        bus.Subscribe<Block.NeighborUpdateMessage>(OnNeighborUpdate);
        bus.Subscribe<Block.RandomUpdateMessage>(OnRandomUpdate);
    }

    /// <inheritdoc/>
    protected override void OnValidate(IValidator validator)
    {
        if (RequiredGround == null)
            validator.ReportWarning("No required ground block is set");
        
        if (RequiredGround == Subject.NamedID)
            validator.ReportWarning("The required ground block cannot be the same as the growing block itself");
        
        requiredGround = Blocks.Instance.SafelyTranslateNamedID(RequiredGround);
        
        if (requiredGround == Blocks.Instance.Core.Error && RequiredGround != Blocks.Instance.Core.Error.NamedID)
            validator.ReportWarning($"The required ground block '{RequiredGround}' could not be found");
    }
    
    private Boolean GetIsPlacementAllowed(Boolean original, (World world, Vector3i position, Actor? actor) context)
    {
        (World world, Vector3i position, Actor? _) = context;
        
        Block? ground = world.GetBlock(position.Below())?.Block;

        return ground == requiredGround || ground == Subject;
    }
    
    private void OnNeighborUpdate(Block.NeighborUpdateMessage message)
    {
        if (message.Side != Side.Bottom) 
            return;
        
        Block? ground = message.World.GetBlock(message.Position.Below())?.Block;
        
        if (ground != requiredGround && ground != Subject)
            Subject.ScheduleDestroy(message.World, message.Position);
    }
    
    private void OnRandomUpdate(Block.RandomUpdateMessage message)
    {
        Int32 currentAge = message.State.Get(Age);

        if (currentAge < MaxAge)
        {
            message.World.SetBlock(message.State.With(Age, currentAge + 1), message.Position);
        }
        else
        {
            if (message.World.GetBlock(message.Position.Above())?.IsReplaceable != true)
                return;
            
            var height = 0;

            for (var offset = 0; offset < MaxHeight; offset++)
                if (message.World.GetBlock(message.Position.Below(offset))?.Block == Subject) height += 1;
                else break;

            if (height >= MaxHeight) 
                return;

            if (Subject.Place(message.World, message.Position.Above()))
                message.World.SetBlock(message.State.With(Age, value: 0), message.Position);
        }
    }
}
