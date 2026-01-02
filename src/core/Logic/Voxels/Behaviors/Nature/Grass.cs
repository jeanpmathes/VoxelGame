// <copyright file="Grass.cs" company="VoxelGame">
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
using OpenTK.Mathematics;
using VoxelGame.Annotations.Attributes;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Events;
using VoxelGame.Core.Logic.Attributes;
using VoxelGame.Core.Logic.Voxels.Behaviors.Combustion;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Core.Logic.Voxels.Behaviors.Nature;

/// <summary>
///     A special soil cover that spreads to blocks which are <see cref="GrassSpreadable" />.
/// </summary>
public partial class Grass : BlockBehavior, IBehavior<Grass, BlockBehavior, Block>
{
    [Constructible]
    private Grass(Block subject) : base(subject)
    {
        subject.Require<CoveredSoil>();
        subject.Require<Combustible>().BurnedState.ContributeFunction(GetBurnedState);
    }

    /// <inheritdoc />
    public override void SubscribeToEvents(IEventBus bus)
    {
        bus.Subscribe<Block.IRandomUpdateMessage>(OnRandomUpdate);
    }

    private void OnRandomUpdate(Block.IRandomUpdateMessage message)
    {
        for (Int32 yOffset = -1; yOffset <= 1; yOffset++)
            foreach (Orientation orientation in Orientations.All)
            {
                Vector3i position = message.Position.Offset(orientation) + Vector3i.UnitY * yOffset;

                if (message.World.GetBlock(position)?.Block.Get<GrassSpreadable>() is {} spreadable)
                    spreadable.SpreadGrass(message.World, position, Subject);
            }
    }

    private static State? GetBurnedState(State? original, (World world, Vector3i position, State state, Block fire) context)
    {
        return new State(Blocks.Instance.Environment.AshCoveredSoil);
    }
}
