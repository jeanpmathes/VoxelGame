// <copyright file="StoredHeight8.cs" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2026 Jean Patrick Mathes
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
using VoxelGame.Annotations.Attributes;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Behaviors.Aspects.Strategies;
using VoxelGame.Core.Behaviors.Events;
using VoxelGame.Core.Logic.Attributes;
using Void = VoxelGame.Toolkit.Utilities.Void;

namespace VoxelGame.Core.Logic.Voxels.Behaviors.Height;

/// <summary>
///     Defines the partial block height of a block as a stored attribute with 8 different states.
/// </summary>
/// <seealso cref="PartialHeight" />
public partial class StoredHeight8 : BlockBehavior, IBehavior<StoredHeight8, BlockBehavior, Block>
{
    /// <summary>
    ///     The minimum height that can be stored in this behavior.
    /// </summary>
    private static readonly Int32 minimumHeight = BlockHeight.Minimum.ToInt32() / 2;

    /// <summary>
    ///     The maximum height that can be stored in this behavior.
    /// </summary>
    private static readonly Int32 maximumHeight = BlockHeight.Maximum.ToInt32() / 2;

    [Constructible]
    private StoredHeight8(Block subject) : base(subject)
    {
        subject.Require<PartialHeight>().Height.ContributeFunction((_, state) => BlockHeight.FromInt32(state.Get(Height) * 2 + 1), exclusive: true);
        subject.Require<StoredHeight>().HeightedState.ContributeFunction(GetHeightedState);
    }

    [LateInitialization] private partial IAttributeData<Int32> Height { get; set; }

    /// <summary>
    ///     The preferred height of the block at placement.
    /// </summary>
    public ResolvedProperty<BlockHeight> PlacementHeight { get; } = ResolvedProperty<BlockHeight>.New<Exclusive<BlockHeight, Void>>(nameof(PlacementHeight));

    /// <inheritdoc />
    public override void SubscribeToEvents(IEventBus bus)
    {
        bus.Subscribe<Modifiable.IModifyHeightMessage>(OnModifyHeight);
    }

    /// <inheritdoc />
    public override void OnInitialize(BlockProperties properties)
    {
        PlacementHeight.Initialize(this);
    }

    /// <inheritdoc />
    public override void DefineState(IStateBuilder builder)
    {
        Height = builder
            .Define(nameof(Height))
            .Int32(minimumHeight, maximumHeight + 1)
            .Attribute(generationDefault: maximumHeight);
    }

    private void OnModifyHeight(Modifiable.IModifyHeightMessage message)
    {
        State newState = message.State.With(Height, (message.State.Get(Height) + 1) % (maximumHeight + 1));
        message.World.SetBlock(newState, message.Position);
    }

    private State GetHeightedState(State original, BlockHeight height)
    {
        return height.IsNone ? original : original.With(Height, height.ToInt32() / 2);
    }
}
