// <copyright file="Fillable.cs" company="VoxelGame">
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
using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Behaviors.Aspects.Strategies;
using VoxelGame.Core.Logic.Attributes;
using Void = VoxelGame.Toolkit.Utilities.Void;

namespace VoxelGame.Core.Logic.Voxels.Behaviors.Fluids;

/// <summary>
///     Makes a block able to be filled with fluids.
/// </summary>
public partial class Fillable : BlockBehavior, IBehavior<Fillable, BlockBehavior, Block>
{
    [Constructible]
    private Fillable(Block subject) : base(subject)
    {
        IsInflowAllowed = Aspect<Boolean, (World, Vector3i, State, Side, Fluid)>.New<LogicalAnd<(World, Vector3i, State, Side, Fluid)>>(nameof(IsInflowAllowed), this);
        IsOutflowAllowed = Aspect<Boolean, (World, Vector3i, State, Side, Fluid)>.New<LogicalAnd<(World, Vector3i, State, Side, Fluid)>>(nameof(IsOutflowAllowed), this);
    }

    /// <summary>
    ///     Whether the fluid filling this block should be meshed.
    /// </summary>
    public ResolvedProperty<Boolean> IsFluidMeshed { get; } = ResolvedProperty<Boolean>.New<ORing<Void>>(nameof(IsFluidMeshed), initial: true);

    /// <summary>
    ///     Whether inflow is allowed through a certain side.
    /// </summary>
    public Aspect<Boolean, (World world, Vector3i position, State state, Side side, Fluid fluid)> IsInflowAllowed { get; }

    /// <summary>
    ///     Whether outflow is allowed through a certain side.
    /// </summary>
    public Aspect<Boolean, (World world, Vector3i position, State state, Side side, Fluid fluid)> IsOutflowAllowed { get; }

    /// <inheritdoc />
    public override void OnInitialize(BlockProperties properties)
    {
        IsFluidMeshed.Initialize(this);
    }

    /// <summary>
    ///     Check whether inflow is allowed through a certain side.
    /// </summary>
    /// <param name="world">The world in which the block is located.</param>
    /// <param name="position">The position of the block.</param>
    /// <param name="side">The side through which the inflow is being checked.</param>
    /// <param name="fluid">The fluid that is being checked for inflow.</param>
    /// <returns>True if inflow is allowed, false otherwise.</returns>
    public Boolean CanInflow(World world, Vector3i position, Side side, Fluid fluid)
    {
        State? block = world.GetBlock(position);

        if (block == null) return false;
        if (block.Value.Block != Subject) return false;

        return IsInflowAllowed.GetValue(original: true, (world, position, block.Value, side, fluid));
    }

    /// <summary>
    ///     Check whether outflow is allowed through a certain side.
    /// </summary>
    /// <param name="world">The world in which the block is located.</param>
    /// <param name="position">The position of the block.</param>
    /// <param name="side">The side through which the outflow is being checked.</param>
    /// <param name="fluid">The fluid that is being checked for outflow.</param>
    /// <returns>True if outflow is allowed, false otherwise.</returns>
    public Boolean CanOutflow(World world, Vector3i position, Side side, Fluid fluid)
    {
        State? block = world.GetBlock(position);

        if (block == null) return false;
        if (block.Value.Block != Subject) return false;

        return IsOutflowAllowed.GetValue(original: true, (world, position, block.Value, side, fluid));
    }
}
