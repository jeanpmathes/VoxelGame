// <copyright file="Salt.cs" company="VoxelGame">
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
using VoxelGame.Core.Logic.Voxels.Behaviors.Fluids;

namespace VoxelGame.Core.Logic.Voxels.Behaviors.Materials;

/// <summary>
///     Salt is a solid material that can be put into fresh water to create salt water.
/// </summary>
public partial class Salt : BlockBehavior, IBehavior<Salt, BlockBehavior, Block>
{
    [Constructible]
    private Salt(Block subject) : base(subject)
    {
        subject.Require<Fillable>().IsInflowAllowed.ContributeFunction(GetIsInflowAllowed);
    }

    /// <inheritdoc />
    public override void SubscribeToEvents(IEventBus bus)
    {
        bus.Subscribe<Block.IStateUpdateMessage>(OnStateUpdate);
    }

    private static Boolean GetIsInflowAllowed(Boolean original, (World world, Vector3i position, State state, Side side, Fluid fluid) context)
    {
        (World _, Vector3i _, State _, Side _, Fluid fluid) = context;

        return fluid.IsLiquid;
    }

    private void OnStateUpdate(Block.IStateUpdateMessage message)
    {
        if (message.NewState.Fluid.IsEmpty) return;

        Subject.Destroy(message.World, message.Position);

        if (message.NewState.Fluid is {Fluid: var fluid, Level: var level}
            && fluid == Voxels.Fluids.Instance.FreshWater)
            message.World.SetFluid(Voxels.Fluids.Instance.SeaWater.AsInstance(level), message.Position);
    }
}
