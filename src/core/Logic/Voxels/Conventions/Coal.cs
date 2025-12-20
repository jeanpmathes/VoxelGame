// <copyright file="Coal.cs" company="VoxelGame">
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
using VoxelGame.Core.Logic.Contents;
using VoxelGame.Core.Logic.Voxels.Behaviors.Combustion;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Voxels.Conventions;

/// <summary>
///     A coal type, as defined by the <see cref="CoalConvention" />.
/// </summary>
public class Coal(CID contentID, BlockBuilder builder) : Convention<Coal>(contentID, builder)
{
    /// <summary>
    ///     The block that represents this coal type.
    /// </summary>
    public required Block Block { get; init; }
}

/// <summary>
///     A convention for coal types.
/// </summary>
public static class CoalConvention
{
    /// <summary>
    ///     Builds a new coal type.
    /// </summary>
    /// <param name="b">The block builder to use.</param>
    /// <param name="contentID">The content ID of the coal, used to create the block CIDs.</param>
    /// <param name="name">The name of the coal, used for display purposes.</param>
    /// <returns>The created coal type.</returns>
    public static Coal BuildCoal(this BlockBuilder b, CID contentID, String name)
    {
        return b.BuildConvention<Coal>(builder =>
        {
            String texture = contentID.Identifier.PascalCaseToSnakeCase();

            return new Coal(contentID, builder)
            {
                Block = builder
                    .BuildSimpleBlock(contentID, name)
                    .WithTextureLayout(TextureLayout.Uniform(TID.Block($"coal_{texture}")))
                    .WithBehavior<Combustible>()
                    .Complete()
            };
        });
    }
}
