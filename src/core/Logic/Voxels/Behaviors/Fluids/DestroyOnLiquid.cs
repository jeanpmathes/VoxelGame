// <copyright file="DestroyOnLiquid.cs" company="VoxelGame">
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

using VoxelGame.Annotations.Attributes;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Behaviors.Aspects.Strategies;
using VoxelGame.Core.Behaviors.Events;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Core.Logic.Voxels.Behaviors.Fluids;

/// <summary>
///     Breaks when filled with more than a certain amount of liquid, by default any amount causes breaking.
///     This is a specialization of <see cref="DestroyOnFluid" />.
/// </summary>
public partial class DestroyOnLiquid : BlockBehavior, IBehavior<DestroyOnLiquid, BlockBehavior, Block>
{
    [Constructible]
    private DestroyOnLiquid(Block subject) : base(subject)
    {
        subject.Require<Fillable>();
    }

    /// <summary>
    ///     The threshold above which the block breaks when filled with liquid.
    /// </summary>
    public ResolvedProperty<FluidLevel> Threshold { get; } = ResolvedProperty<FluidLevel>.New<Minimum<FluidLevel, Void>>(nameof(Threshold), FluidLevel.None);

    /// <inheritdoc />
    public override void SubscribeToEvents(IEventBus bus)
    {
        bus.Subscribe<Block.IStateUpdateMessage>(OnStateUpdate);
        bus.Subscribe<Block.IPlacementCompletedMessage>(OnPlacementCompleted);
    }

    /// <inheritdoc />
    public override void OnInitialize(BlockProperties properties)
    {
        Threshold.Initialize(this);
    }

    /// <inheritdoc />
    protected override void OnValidate(IValidator validator)
    {
        if (Threshold.Get().IsFull)
            validator.ReportWarning("The threshold is set to full fluid level, rendering the behavior useless");
    }

    private void OnStateUpdate(Block.IStateUpdateMessage message)
    {
        if (message.NewState.Fluid.Fluid.IsLiquid && message.NewState.Fluid.Level > Threshold.Get())
            Subject.ScheduleDestroy(message.World, message.Position);
    }

    private void OnPlacementCompleted(Block.IPlacementCompletedMessage message)
    {
        Content? content = message.World.GetContent(message.Position);

        if (content is {Fluid.Fluid.IsLiquid: true} && content.Value.Fluid.Level > Threshold.Get())
            Subject.ScheduleDestroy(message.World, message.Position);
    }
}
