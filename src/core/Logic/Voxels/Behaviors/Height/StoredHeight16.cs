// <copyright file="StoredHeight16.cs" company="VoxelGame">
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
///     Defines the partial block height of a block as a stored attribute with 16 different states.
/// </summary>
/// <seealso cref="PartialHeight" />
public partial class StoredHeight16 : BlockBehavior, IBehavior<StoredHeight16, BlockBehavior, Block>
{
    [Constructible]
    private StoredHeight16(Block subject) : base(subject)
    {
        subject.Require<PartialHeight>().Height.ContributeFunction((_, state) => BlockHeight.FromInt32(state.Get(Height)), exclusive: true);
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
    public override void DefineState(IStateBuilder builder)
    {
        Height = builder
            .Define(nameof(Height))
            .Int32(BlockHeight.Minimum.ToInt32(), BlockHeight.Maximum.ToInt32() + 1)
            .Attribute(generationDefault: BlockHeight.Maximum.ToInt32());
    }

    /// <inheritdoc />
    public override void OnInitialize(BlockProperties properties)
    {
        PlacementHeight.Initialize(this);
    }

    private void OnModifyHeight(Modifiable.IModifyHeightMessage message)
    {
        State state = message.State;

        Int32 newHeight = (state.Get(Height) + 1) % (BlockHeight.Maximum.ToInt32() + 1);

        message.World.SetBlock(state.With(Height, newHeight), message.Position);
    }

    private State GetHeightedState(State original, BlockHeight height)
    {
        return height.IsNone ? original : original.With(Height, height.ToInt32());
    }
}
