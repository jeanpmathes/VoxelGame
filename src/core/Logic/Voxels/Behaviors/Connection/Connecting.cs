// <copyright file="Connecting.cs" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2025 Jean Patrick Mathes
//      
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
//     
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//     
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <https://www.gnu.org/licenses/>.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Annotations.Attributes;
using VoxelGame.Core.Actors;
using VoxelGame.Core.Behaviors;
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

    [LateInitialization] private partial IAttributeData<Boolean> North { get; set; }

    [LateInitialization] private partial IAttributeData<Boolean> East { get; set; }

    [LateInitialization] private partial IAttributeData<Boolean> South { get; set; }

    [LateInitialization] private partial IAttributeData<Boolean> West { get; set; }

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
            Vector3i neighborPosition = position.Offset(orientation);

            if (CanConnectTo(world, neighborPosition, orientation.ToSide()))
                state.Set(GetDirection(orientation), value: true);
        }

        return state;
    }

    private void OnNeighborUpdate(Block.INeighborUpdateMessage message)
    {
        if (!message.Side.IsLateral()) return;

        IAttributeData<Boolean> side = GetDirection(message.Side.ToOrientation());
        Boolean canConnect = CanConnectTo(message.World, message.Position.Offset(message.Side), message.Side);

        if (message.State.Get(side) == canConnect) return;

        message.World.SetBlock(message.State.With(side, canConnect), message.Position);
    }

    private Boolean CanConnectTo(World world, Vector3i position, Side side)
    {
        State? other = world.GetBlock(position);

        return other?.Block.Get<Connectable>() is {} otherConnectable && otherConnectable.CanConnect(other.Value, side.Opposite(), connectable);
    }

    private IAttributeData<Boolean> GetDirection(Orientation orientation)
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
