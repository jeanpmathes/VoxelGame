// <copyright file="Connecting.cs" company="VoxelGame">
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
using VoxelGame.Core.Behaviors.Events;
using VoxelGame.Core.Logic.Attributes;
using VoxelGame.Core.Utilities;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Core.Logic.Voxels.Behaviors.Connection;

/// <summary>
///     Connects to other connectable blocks along its lateral sides.
/// </summary>
public partial class Connecting : BlockBehavior, IBehavior<Connecting, BlockBehavior, Block>
{
    private readonly Connectable connectable;

    [Constructible]
    private Connecting(Block subject) : base(subject)
    {
        connectable = subject.Require<Connectable>();

        subject.PlacementState.ContributeFunction(GetPlacementState);
    }

    [LateInitialization] private partial IAttribute<Boolean> North { get; set; }

    [LateInitialization] private partial IAttribute<Boolean> East { get; set; }

    [LateInitialization] private partial IAttribute<Boolean> South { get; set; }

    [LateInitialization] private partial IAttribute<Boolean> West { get; set; }

    /// <inheritdoc />
    public override void SubscribeToEvents(IEventBus bus)
    {
        bus.Subscribe<Block.INeighborUpdateMessage>(OnNeighborUpdate);
    }

    /// <inheritdoc />
    public override void DefineState(IStateBuilder builder)
    {
        North = builder.Define(nameof(North)).Boolean().Attribute();
        East = builder.Define(nameof(East)).Boolean().Attribute();
        South = builder.Define(nameof(South)).Boolean().Attribute();
        West = builder.Define(nameof(West)).Boolean().Attribute();
    }

    private State GetPlacementState(State original, (World world, Vector3i position, Actor? actor) context)
    {
        (World world, Vector3i position, Actor? _) = context;

        State state = original;

        foreach (Orientation orientation in Orientations.All)
        {
            Vector3i neighborPosition = orientation.Offset(position);

            if (CanConnectTo(world, neighborPosition, orientation.ToSide()))
                state.Set(GetDirection(orientation), value: true);
        }

        return state;
    }

    private void OnNeighborUpdate(Block.INeighborUpdateMessage message)
    {
        if (!message.Side.IsLateral()) return;

        IAttribute<Boolean> side = GetDirection(message.Side.ToOrientation());
        Boolean canConnect = CanConnectTo(message.World, message.Side.Offset(message.Position), message.Side);

        if (message.State.Get(side) == canConnect) return;

        message.World.SetBlock(message.State.With(side, canConnect), message.Position);
    }

    private Boolean CanConnectTo(World world, Vector3i position, Side side)
    {
        State? other = world.GetBlock(position);

        return other?.Block.Get<Connectable>() is {} otherConnectable && otherConnectable.CanConnect(other.Value, side.Opposite(), connectable);
    }

    private IAttribute<Boolean> GetDirection(Orientation orientation)
    {
        return orientation switch
        {
            Orientation.North => North,
            Orientation.East => East,
            Orientation.South => South,
            Orientation.West => West,
            _ => throw Exceptions.UnsupportedEnumValue(orientation)
        };
    }

    /// <summary>
    ///     Get the connections of the block given its state.
    /// </summary>
    /// <param name="state">The state of the block.</param>
    /// <returns>The connections, <c>true</c> if connected, <c>false</c> if not for each of the lateral sides.</returns>
    public (Boolean north, Boolean east, Boolean south, Boolean west) GetConnections(State state)
    {
        return (state.Get(North),
            state.Get(East),
            state.Get(South),
            state.Get(West));
    }
}
