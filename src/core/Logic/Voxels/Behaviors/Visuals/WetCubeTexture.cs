// <copyright file="WetCubeTexture.cs" company="VoxelGame">
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
using VoxelGame.Core.Logic.Voxels.Behaviors.Fluids;
using VoxelGame.Core.Visuals;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Core.Logic.Voxels.Behaviors.Visuals;

/// <summary>
///     Behavior that swaps out the texture of a block when it is wet.
///     Uses the <see cref="CubeTextured" /> behavior.
/// </summary>
public partial class WetCubeTexture : BlockBehavior, IBehavior<WetCubeTexture, BlockBehavior, Block>
{
    [Constructible]
    private WetCubeTexture(Block subject) : base(subject)
    {
        subject.Require<Wet>();
        subject.Require<CubeTextured>().ActiveTexture.ContributeFunction((original, state) => state.Fluid?.IsLiquid == true ? WetTexture.Get() : original);
    }

    /// <summary>
    ///     The texture layout to use when the block is wet.
    /// </summary>
    public ResolvedProperty<TextureLayout> WetTexture { get; } = ResolvedProperty<TextureLayout>.New<Exclusive<TextureLayout, Void>>(nameof(WetTexture), TextureLayout.Uniform(TID.MissingTexture));

    /// <inheritdoc />
    public override void OnInitialize(BlockProperties properties)
    {
        WetTexture.Initialize(this);
    }
}
