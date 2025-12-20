// <copyright file="Vine.cs" company="VoxelGame">
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
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Events;
using VoxelGame.Core.Logic.Attributes;
using VoxelGame.Core.Logic.Voxels.Behaviors.Combustion;
using VoxelGame.Core.Logic.Voxels.Behaviors.Siding;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Core.Logic.Voxels.Behaviors.Nature;

/// <summary>
///     A flat block which grows downwards and can, as an alternative to a holding wall, also hang from other vines.
/// </summary>
public partial class Vine : BlockBehavior, IBehavior<Vine, BlockBehavior, Block>
{
    private const Int32 MaxAge = 8;
    private readonly Attached attached;

    private readonly SingleSided siding;

    [Constructible]
    private Vine(Block subject) : base(subject)
    {
        subject.Require<Combustible>();

        siding = subject.Require<SingleSided>();

        attached = subject.Require<Attached>();
        attached.IsOtherwiseAttached.ContributeFunction(GetIsOtherwiseAttached);
    }

    [LateInitialization] private partial IAttributeData<Int32> Age { get; set; }

    /// <inheritdoc />
    public override void SubscribeToEvents(IEventBus bus)
    {
        bus.Subscribe<Block.IRandomUpdateMessage>(OnRandomUpdate);
        bus.Subscribe<Block.INeighborUpdateMessage>(OnNeighborUpdate);
    }

    /// <inheritdoc />
    public override void DefineState(IStateBuilder builder)
    {
        Age = builder.Define(nameof(Age)).Int32(min: 0, MaxAge + 1).Attribute();
    }

    private Boolean GetIsOtherwiseAttached(Boolean original, (World world, Vector3i position, State state) context)
    {
        (World world, Vector3i position, State state) = context;

        State? above = world.GetBlock(position.Above());

        if (above == null)
            return false;

        if (above.Value.Block != Subject)
            return false;

        return siding.GetSide(state) == above.Value.Block.Get<Vine>()?.siding.GetSide(above.Value);
    }

    private void OnRandomUpdate(Block.IRandomUpdateMessage message)
    {
        Int32 currentAge = message.State.Get(Age);

        if (currentAge < MaxAge)
        {
            message.World.SetBlock(message.State.With(Age, currentAge + 1), message.Position);
        }
        else if (message.World.GetBlock(message.Position.Below())?.Block.IsEmpty == true)
        {
            message.World.SetBlock(message.State.With(Age, value: 0), message.Position.Below());
            message.World.SetBlock(message.State.With(Age, value: 0), message.Position);
        }
    }

    private void OnNeighborUpdate(Block.INeighborUpdateMessage message)
    {
        if (message.Side != Side.Top)
            return;

        attached.CheckAttachment(message.World, message.Position, message.State);
    }
}
