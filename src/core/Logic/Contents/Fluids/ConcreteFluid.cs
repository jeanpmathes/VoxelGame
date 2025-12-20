// <copyright file="ConcreteFluid.cs" company="VoxelGame">
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
using VoxelGame.Core.Logic.Attributes;
using VoxelGame.Core.Logic.Voxels;
using VoxelGame.Core.Logic.Voxels.Behaviors;
using VoxelGame.Core.Utilities.Units;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Contents.Fluids;

/// <summary>
///     A concrete-like fluid that can harden to concrete blocks.
/// </summary>
public class ConcreteFluid : BasicFluid
{
    /// <summary>
    ///     Create a new <see cref="ConcreteFluid" />.
    /// </summary>
    /// <param name="name">The name of the fluid.</param>
    /// <param name="namedID">The named ID of the fluid.</param>
    /// <param name="density">The density of the fluid.</param>
    /// <param name="viscosity">The viscosity of the fluid.</param>
    /// <param name="texture">The texture of the fluid.</param>
    public ConcreteFluid(String name, String namedID, Density density, Viscosity viscosity, TID texture) :
        base(
            name,
            namedID,
            density,
            viscosity,
            hasNeutralTint: false,
            texture) {}

    /// <inheritdoc />
    internal override void DoRandomUpdate(World world, Vector3i position, FluidLevel level, Boolean isStatic)
    {
        if (!isStatic) return;
        if (!Blocks.Instance.Construction.Concrete.CanPlace(world, position)) return;

        world.SetDefaultFluid(position);

        State state = Blocks.Instance.Construction.Concrete.GetPlacementState(world, position);

        state = state.WithHeight(level.BlockHeight);

        world.SetBlock(state, position);
    }
}
