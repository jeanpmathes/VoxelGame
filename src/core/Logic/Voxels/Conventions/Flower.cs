// <copyright file="Flower.cs" company="VoxelGame">
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
using VoxelGame.Core.Logic.Voxels.Behaviors;
using VoxelGame.Core.Logic.Voxels.Behaviors.Fluids;
using VoxelGame.Core.Logic.Voxels.Behaviors.Nature.Plants;
using VoxelGame.Core.Logic.Voxels.Behaviors.Visuals;
using VoxelGame.Core.Resources.Language;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Voxels.Conventions;

/// <summary>
///     A flower, as defined by the <see cref="FlowerConvention" />.
/// </summary>
public sealed class Flower(CID contentID, BlockBuilder builder) : Convention<Flower>(contentID, builder)
{
    /// <summary>
    ///     The short variant of this flower.
    /// </summary>
    public required Block Short { get; init; }

    /// <summary>
    ///     The tall variant of this flower.
    /// </summary>
    public required Block Tall { get; init; }
}

/// <summary>
///     A convention on flowers.
/// </summary>
public static class FlowerConvention
{
    /// <summary>
    ///     Build a new flower.
    /// </summary>
    /// <param name="b">The block builder to use.</param>
    /// <param name="contentID">The content ID of the flower, used to create the block CIDs.</param>
    /// <param name="name">The name of the flower, used for display purposes.</param>
    /// <returns>The created flower.</returns>
    public static Flower BuildFlower(this BlockBuilder b, CID contentID, String name)
    {
        return b.BuildConvention<Flower>(builder =>
        {
            String texture = contentID.Identifier.PascalCaseToSnakeCase();

            return new Flower(contentID, builder)
            {
                Short = builder
                    .BuildFoliageBlock(new CID($"{contentID}{nameof(Flower.Short)}"), name)
                    .WithTexture(TID.Block(texture))
                    .WithBehavior<NeutralTint>()
                    .WithBehavior<CrossPlant>(plant =>
                    {
                        plant.Height.Initializer.ContributeConstant(value: 0.5);
                        plant.Width.Initializer.ContributeConstant(value: 0.35);
                    })
                    .WithBehavior<DestroyOnLiquid>(breaking => breaking.Threshold.Initializer.ContributeConstant(FluidLevel.Four))
                    .WithBehavior<Replaceable>()
                    .Complete(),

                Tall = builder
                    .BuildFoliageBlock(new CID($"{contentID}{nameof(Flower.Tall)}"), $"{name} ({nameof(Language.Tall)})")
                    .WithTexture(TID.Block($"{texture}_tall"))
                    .WithBehavior<NeutralTint>()
                    .WithBehavior<DoubleCrossPlant>()
                    .WithBehavior<DestroyOnLiquid>(breaking => breaking.Threshold.Initializer.ContributeConstant(FluidLevel.Five))
                    .Complete()
            };
        });
    }
}
