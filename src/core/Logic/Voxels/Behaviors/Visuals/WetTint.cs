// <copyright file="WetTint.cs" company="VoxelGame">
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
using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Behaviors.Aspects.Strategies;
using VoxelGame.Core.Logic.Voxels.Behaviors.Fluids;
using VoxelGame.Core.Visuals;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Core.Logic.Voxels.Behaviors.Visuals;

/// <summary>
///     Behavior that applies a wet tint to a block when it is wet.
/// </summary>
public partial class WetTint : BlockBehavior, IBehavior<WetTint, BlockBehavior, Block>
{
    [Constructible]
    private WetTint(Block subject) : base(subject)
    {
        subject.Require<Wet>();
        subject.Require<Meshed>().Tint.ContributeFunction((original, state) => state.Fluid?.IsLiquid == true ? WetColor.Get() : original);
    }

    /// <summary>
    ///     The color tint to apply when the block is wet.
    /// </summary>
    public ResolvedProperty<ColorS> WetColor { get; } = ResolvedProperty<ColorS>.New<Exclusive<ColorS, Void>>(nameof(WetColor), ColorS.LightGray);

    /// <inheritdoc />
    public override void OnInitialize(BlockProperties properties)
    {
        WetColor.Initialize(this);
    }
}
