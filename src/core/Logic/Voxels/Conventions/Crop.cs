// <copyright file="Crop.cs" company="VoxelGame">
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
using VoxelGame.Core.Logic.Contents;
using VoxelGame.Core.Logic.Voxels.Behaviors.Fluids;
using VoxelGame.Core.Logic.Voxels.Behaviors.Nature;
using VoxelGame.Core.Logic.Voxels.Behaviors.Nature.Plants;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Voxels.Conventions;

/// <summary>
///     A crop, as defined by the <see cref="CropConvention" />.
/// </summary>
public sealed class Crop(CID contentID, BlockBuilder builder) : Convention<Crop>(contentID, builder)
{
    /// <summary>
    ///     The plant corresponding to this crop.
    /// </summary>
    public required Block Plant { get; init; }

    /// <summary>
    ///     The optional fruit block that is grown by the crop.
    /// </summary>
    public required Block? Fruit { get; init; }
}

/// <summary>
///     A convention on crops.
/// </summary>
public static class CropConvention
{
    /// <summary>
    ///     Build a new dense crop.
    /// </summary>
    /// <param name="b">The block builder to use.</param>
    /// <param name="contentID">The content ID of the crop, used to create the block CIDs.</param>
    /// <param name="name">The name of the crop, used for display purposes.</param>
    /// <returns>The created dense crop.</returns>
    public static Crop BuildDenseCrop(this BlockBuilder b, CID contentID, String name)
    {
        return b.BuildConvention<Crop>(builder =>
        {
            String texture = contentID.Identifier.PascalCaseToSnakeCase();

            return new Crop(contentID, builder)
            {
                Plant = builder
                    .BuildFoliageBlock(new CID($"{contentID}{nameof(Crop.Plant)}"), name)
                    .WithTexture(TID.Block(texture))
                    .WithBehavior<DenseCropPlant>()
                    .WithBehavior<DestroyOnLiquid>(breaking => breaking.Threshold.Initializer.ContributeConstant(FluidLevel.Three))
                    .Complete(),

                Fruit = null
            };
        });
    }

    /// <summary>
    ///     Build a new double crop.
    /// </summary>
    /// <param name="b">The block builder to use.</param>
    /// <param name="contentID">The content ID of the crop, used to create the block CIDs.</param>
    /// <param name="name">The name of the crop, used for display purposes.</param>
    /// <returns>The created double crop.</returns>
    public static Crop BuildDoubleCrop(this BlockBuilder b, CID contentID, String name)
    {
        return b.BuildConvention<Crop>(builder =>
        {
            String texture = contentID.Identifier.PascalCaseToSnakeCase();

            return new Crop(contentID, builder)
            {
                Plant = builder
                    .BuildFoliageBlock(new CID($"{contentID}{nameof(Crop.Plant)}"), name)
                    .WithTexture(TID.Block(texture))
                    .WithBehavior<DoubleCropPlant>()
                    .WithBehavior<DestroyOnLiquid>(breaking => breaking.Threshold.Initializer.ContributeConstant(FluidLevel.Three))
                    .Complete(),

                Fruit = null
            };
        });
    }

    /// <summary>
    ///     Build a new fruit crop.
    /// </summary>
    /// <param name="b">The block builder to use.</param>
    /// <param name="contentID">The content ID of the crop, used to create the block CIDs.</param>
    /// <param name="names">The names of the parts, used for display purposes.</param>
    /// <returns>The created fruit crop.</returns>
    public static Crop BuildFruitCrop(this BlockBuilder b, CID contentID, (String plant, String fruit) names)
    {
        return b.BuildConvention<Crop>(builder =>
        {
            String texture = contentID.Identifier.PascalCaseToSnakeCase();

            var fruitID = new CID($"{contentID}{nameof(Crop.Fruit)}");

            return new Crop(contentID, builder)
            {
                Plant = builder
                    .BuildFoliageBlock(new CID($"{contentID}{nameof(Crop.Plant)}"), names.plant)
                    .WithTexture(TID.Block($"{texture}_plant"))
                    .WithBehavior<FruitCropPlant>(plant => plant.Fruit.Initializer.ContributeConstant(fruitID))
                    .WithBehavior<DestroyOnLiquid>(breaking => breaking.Threshold.Initializer.ContributeConstant(FluidLevel.Three))
                    .Complete(),

                Fruit = builder
                    .BuildSimpleBlock(fruitID, names.fruit)
                    .WithTextureLayout(TextureLayout.Column(TID.Block(texture, x: 0), TID.Block(texture, x: 1)))
                    .WithBehavior<Fruit>()
                    .Complete()
            };
        });
    }
}
