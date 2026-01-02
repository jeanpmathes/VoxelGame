// <copyright file="Mud.cs" company="VoxelGame">
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
using VoxelGame.Core.Behaviors.Events;
using VoxelGame.Core.Logic.Attributes;
using VoxelGame.Core.Utilities.Units;

namespace VoxelGame.Core.Logic.Voxels.Behaviors.Nature;

/// <summary>
///     When <see cref="Soil" /> gets filled with too much water, it turns into <see cref="Mud" />.
/// </summary>
public partial class Mud : BlockBehavior, IBehavior<Mud, BlockBehavior, Block>
{
    private static readonly Temperature crackingTemperature = new() {DegreesCelsius = 35.0};

    [Constructible]
    private Mud(Block subject) : base(subject)
    {
        subject.Require<Plantable>();
    }

    /// <inheritdoc />
    public override void SubscribeToEvents(IEventBus bus)
    {
        bus.Subscribe<Block.IRandomUpdateMessage>(OnRandomUpdate);
        bus.Subscribe<Plantable.IGrowthAttemptMessage>(OnGrowthAttempt);
    }

    private static void OnRandomUpdate(Block.IRandomUpdateMessage message)
    {
        if (message.World.Map.GetTemperature(message.Position) < crackingTemperature)
            return;

        message.World.SetContent(Content.Create(Blocks.Instance.Environment.CrackedDriedMud), message.Position);
    }

    private static void OnGrowthAttempt(Plantable.IGrowthAttemptMessage message)
    {
        if (message.Fluid != Voxels.Fluids.Instance.FreshWater) return;

        FluidLevel remaining = FluidLevel.Full - message.Level;

        message.World.SetContent(remaining >= FluidLevel.One
                ? new Content(new State(Blocks.Instance.Environment.Soil), Voxels.Fluids.Instance.FreshWater.AsInstance(remaining))
                : Content.Create(Blocks.Instance.Environment.Soil),
            message.Position);

        message.MarkAsSuccessful();
    }
}
