// <copyright file="Growing.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Annotations.Attributes;
using VoxelGame.Core.Actors;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Behaviors.Aspects.Strategies;
using VoxelGame.Core.Behaviors.Events;
using VoxelGame.Core.Logic.Attributes;
using VoxelGame.Core.Logic.Contents;
using VoxelGame.Core.Utilities;
using Void = VoxelGame.Toolkit.Utilities.Void;

namespace VoxelGame.Core.Logic.Voxels.Behaviors.Nature;

/// <summary>
///     A block that grows upwards and requires a specific ground block to be placed on.
/// </summary>
public partial class Growing : BlockBehavior, IBehavior<Growing, BlockBehavior, Block>
{
    private const Int32 MaxAge = 7;
    private const Int32 MaxHeight = 4;
    private Block requiredGround = null!;

    [Constructible]
    private Growing(Block subject) : base(subject)
    {
        subject.IsPlacementAllowed.ContributeFunction(GetIsPlacementAllowed);
    }

    [LateInitialization] private partial IAttributeData<Int32> Age { get; set; }

    /// <summary>
    ///     The required ground block.
    /// </summary>
    public ResolvedProperty<CID?> RequiredGround { get; } = ResolvedProperty<CID?>.New<Exclusive<CID?, Void>>(nameof(RequiredGround));

    /// <inheritdoc />
    public override void SubscribeToEvents(IEventBus bus)
    {
        bus.Subscribe<Block.INeighborUpdateMessage>(OnNeighborUpdate);
        bus.Subscribe<Block.IRandomUpdateMessage>(OnRandomUpdate);
    }

    /// <inheritdoc />
    public override void OnInitialize(BlockProperties properties)
    {
        RequiredGround.Initialize(this);
    }

    /// <inheritdoc />
    public override void DefineState(IStateBuilder builder)
    {
        Age = builder.Define(nameof(Age)).Int32(min: 0, MaxAge + 1).Attribute();
    }

    /// <inheritdoc />
    protected override void OnValidate(IValidator validator)
    {
        if (RequiredGround.Get() == null)
            validator.ReportWarning("No required ground block is set");

        if (RequiredGround.Get() == Subject.ContentID)
            validator.ReportWarning("The required ground block cannot be the same as the growing block itself");

        requiredGround = Blocks.Instance.SafelyTranslateContentID(RequiredGround.Get());

        if (requiredGround == Blocks.Instance.Core.Error && RequiredGround.Get() != Blocks.Instance.Core.Error.ContentID)
            validator.ReportWarning($"The required ground block '{RequiredGround}' could not be found");
    }

    private Boolean GetIsPlacementAllowed(Boolean original, (World world, Vector3i position, Actor? actor) context)
    {
        (World world, Vector3i position, Actor? _) = context;

        Block? ground = world.GetBlock(position.Below())?.Block;

        return ground == requiredGround || ground == Subject;
    }

    private void OnNeighborUpdate(Block.INeighborUpdateMessage message)
    {
        if (message.Side != Side.Bottom)
            return;

        Block? ground = message.World.GetBlock(message.Position.Below())?.Block;

        if (ground != requiredGround && ground != Subject)
            Subject.ScheduleDestroy(message.World, message.Position);
    }

    private void OnRandomUpdate(Block.IRandomUpdateMessage message)
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
