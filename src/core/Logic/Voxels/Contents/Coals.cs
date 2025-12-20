// <copyright file="Coals.cs" company="VoxelGame">
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

using VoxelGame.Core.Logic.Contents;
using VoxelGame.Core.Logic.Voxels.Conventions;
using VoxelGame.Core.Resources.Language;

namespace VoxelGame.Core.Logic.Voxels.Contents;

/// <summary>
///     Different types of coal. All three types can be found in the world.
/// </summary>
public class Coals(BlockBuilder builder) : Category(builder)
{
    /// <summary>
    ///     Lignite is a type of coal.
    ///     It is the lowest rank of coal but can be found near the surface.
    /// </summary>
    public Coal Lignite { get; } = builder.BuildCoal(new CID(nameof(Lignite)), Language.CoalLignite);

    /// <summary>
    ///     Bituminous coal is a type of coal.
    ///     It is of medium rank and is the most abundant type of coal.
    /// </summary>
    public Coal BituminousCoal { get; } = builder.BuildCoal(new CID(nameof(BituminousCoal)), Language.CoalBituminous);

    /// <summary>
    ///     Anthracite is a type of coal.
    ///     It is the highest rank of coal and is the hardest and most carbon-rich.
    /// </summary>
    public Coal Anthracite { get; } = builder.BuildCoal(new CID(nameof(Anthracite)), Language.CoalAnthracite);
}
