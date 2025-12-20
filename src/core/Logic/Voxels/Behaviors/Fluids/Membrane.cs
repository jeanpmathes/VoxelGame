// <copyright file="Membrane.cs" company="VoxelGame">
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
using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Behaviors.Aspects.Strategies;
using VoxelGame.Core.Logic.Attributes;
using VoxelGame.Core.Utilities.Units;
using Void = VoxelGame.Toolkit.Utilities.Void;

namespace VoxelGame.Core.Logic.Voxels.Behaviors.Fluids;

/// <summary>
///     Controls inflow into the block, allowing to filter out which fluids can pass through.
/// </summary>
public partial class Membrane : BlockBehavior, IBehavior<Membrane, BlockBehavior, Block>
{
    [Constructible]
    private Membrane(Block subject) : base(subject)
    {
        subject.Require<Fillable>().IsInflowAllowed.ContributeFunction(GetIsInflowAllowed);
    }

    /// <summary>
    ///     Only fluids with a viscosity less than this value can flow into the block.
    /// </summary>
    public ResolvedProperty<Viscosity> MaxViscosity { get; } = ResolvedProperty<Viscosity>.New<Minimum<Viscosity, Void>>(nameof(MaxViscosity), new Viscosity {MilliPascalSeconds = 65});

    /// <inheritdoc />
    public override void OnInitialize(BlockProperties properties)
    {
        MaxViscosity.Initialize(this);
    }

    private Boolean GetIsInflowAllowed(Boolean original, (World world, Vector3i position, State state, Side side, Fluid fluid) context)
    {
        (World _, Vector3i _, State _, Side _, Fluid fluid) = context;

        return fluid.Viscosity < MaxViscosity.Get();
    }
}
