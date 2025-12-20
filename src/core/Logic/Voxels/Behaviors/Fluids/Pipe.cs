// <copyright file="Pipe.cs" company="VoxelGame">
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

namespace VoxelGame.Core.Logic.Voxels.Behaviors.Fluids;

/// <summary>
///     Guides the flow of fluids.
/// </summary>
public partial class Pipe : BlockBehavior, IBehavior<Pipe, BlockBehavior, Block>
{
    [Constructible]
    private Pipe(Block subject) : base(subject)
    {
        subject.Require<Piped>();

        var fillable = subject.Require<Fillable>();
        fillable.IsFluidMeshed.Initializer.ContributeConstant(value: false);
        fillable.IsInflowAllowed.ContributeFunction(GetIsInflowOrOutflowAllowed);
        fillable.IsOutflowAllowed.ContributeFunction(GetIsInflowOrOutflowAllowed);

        OpenSides = Aspect<Sides, State>.New<Exclusive<Sides, State>>(nameof(OpenSides), this);
    }

    /// <summary>
    ///     Get the sides which are open in a given state.
    /// </summary>
    public Aspect<Sides, State> OpenSides { get; }

    private Boolean GetIsInflowOrOutflowAllowed(Boolean original, (World world, Vector3i position, State state, Side side, Fluid fluid) context)
    {
        (World _, Vector3i _, State state, Side side, Fluid _) = context;

        return OpenSides.GetValue(Sides.None, state).HasFlag(side.ToFlag());
    }
}
