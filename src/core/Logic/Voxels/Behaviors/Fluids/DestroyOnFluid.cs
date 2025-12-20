// <copyright file="DestroyOnFluid.cs" company="VoxelGame">
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

using VoxelGame.Annotations.Attributes;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Events;

namespace VoxelGame.Core.Logic.Voxels.Behaviors.Fluids;

/// <summary>
///     Breaks when filled with any fluid, both liquid and gas.
/// </summary>
public partial class DestroyOnFluid : BlockBehavior, IBehavior<DestroyOnFluid, BlockBehavior, Block>
{
    [Constructible]
    private DestroyOnFluid(Block subject) : base(subject)
    {
        subject.Require<Fillable>();
    }

    /// <inheritdoc />
    public override void SubscribeToEvents(IEventBus bus)
    {
        bus.Subscribe<Block.IStateUpdateMessage>(OnStateUpdate);
        bus.Subscribe<Block.IPlacementCompletedMessage>(OnPlacementCompleted);
    }

    private void OnStateUpdate(Block.IStateUpdateMessage message)
    {
        if (!message.NewState.Fluid.IsEmpty)
            Subject.ScheduleDestroy(message.World, message.Position);
    }

    private void OnPlacementCompleted(Block.IPlacementCompletedMessage message)
    {
        Content? content = message.World.GetContent(message.Position);

        if (content is {Fluid.IsEmpty: false})
            Subject.ScheduleDestroy(message.World, message.Position);
    }
}
