// <copyright file="Stones.cs" company="VoxelGame">
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
///     All sorts of stone types. Stone occurs naturally in the world but can also be used for the construction of various
///     things.
/// </summary>
public class Stones(BlockBuilder builder) : Category(builder)
{
    /// <summary>
    ///     Granite is found next to volcanic activity.
    ///     When carved, the patterns show geometric shapes.
    /// </summary>
    public Stone Granite { get; } = builder.BuildStone(new CID(nameof(Granite)), Language.Granite);

    /// <summary>
    ///     Sandstone is found all over the world and especially in the desert.
    ///     When carved, the patterns depict the desert sun.
    /// </summary>
    public Stone Sandstone { get; } = builder.BuildStone(new CID(nameof(Sandstone)), Language.Sandstone);

    /// <summary>
    ///     Limestone is found all over the world and especially in oceans.
    ///     When carved, the patterns depict the ocean and life within it.
    /// </summary>
    public Stone Limestone { get; } = builder.BuildStone(new CID(nameof(Limestone)), Language.Limestone);

    /// <summary>
    ///     Marble is a rarer stone type.
    ///     When carved, the patterns depict an ancient temple.
    /// </summary>
    public Stone Marble { get; } = builder.BuildStone(new CID(nameof(Marble)), Language.Marble);

    /// <summary>
    ///     Pumice is created when lava rapidly cools down, while being in contact with a lot of water.
    ///     When carved, the patterns depict heat rising from the earth.
    /// </summary>
    public Stone Pumice { get; } = builder.BuildStone(new CID(nameof(Pumice)), Language.Pumice);

    /// <summary>
    ///     Obsidian is a dark type of stone, that forms from lava.
    ///     When carved, the patterns depict an ancient artifact.
    /// </summary>
    public Stone Obsidian { get; } = builder.BuildStone(new CID(nameof(Obsidian)), Language.Obsidian);
}
