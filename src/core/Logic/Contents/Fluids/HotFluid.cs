// <copyright file="HotFluid.cs" company="VoxelGame">
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
using VoxelGame.Core.Logic.Voxels;
using VoxelGame.Core.Logic.Voxels.Behaviors.Combustion;
using VoxelGame.Core.Utilities.Units;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Contents.Fluids;

/// <summary>
///     A fluid that can burn its surroundings.
/// </summary>
public class HotFluid : BasicFluid
{
    /// <summary>
    ///     Create a new <see cref="HotFluid" />.
    /// </summary>
    public HotFluid(String name, String namedID, Density density, Viscosity viscosity, Boolean hasNeutralTint,
        TID texture, RenderType renderType = RenderType.Opaque) :
        base(
            name,
            namedID,
            density,
            viscosity,
            hasNeutralTint,
            texture,
            renderType) {}

    /// <inheritdoc />
    protected override void ScheduledUpdate(World world, Vector3i position, FluidInstance instance)
    {
        if (world.GetBlock(position)?.Block.Get<Combustible>() is {} combustible) combustible.DoBurn(world, position, Blocks.Instance.Environment.Fire);

        BurnAround(world, position);

        base.ScheduledUpdate(world, position, instance);
    }

    /// <inheritdoc />
    internal override void DoRandomUpdate(World world, Vector3i position, FluidLevel level, Boolean isStatic)
    {
        BurnAround(world, position);
    }

    private static void BurnAround(World world, Vector3i position)
    {
        foreach (Side side in Side.All.Sides())
        {
            Vector3i offsetPosition = position.Offset(side);

            if (world.GetBlock(offsetPosition)?.Block.Get<Combustible>() is {} combustible &&
                combustible.DoBurn(world, offsetPosition, Blocks.Instance.Environment.Fire))
                Blocks.Instance.Environment.Fire.Place(world, offsetPosition);

        }
    }
}
