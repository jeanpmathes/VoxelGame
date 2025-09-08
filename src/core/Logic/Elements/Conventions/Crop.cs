// <copyright file="Crop.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Logic.Elements.Behaviors.Fluids;
using VoxelGame.Core.Logic.Elements.Behaviors.Nature;
using VoxelGame.Core.Logic.Elements.Behaviors.Nature.Plants;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Elements.Conventions;

/// <summary>
/// A crop, as defined by the <see cref="CropConvention"/>.
/// </summary>
public sealed class Crop(String namedID, BlockBuilder builder) : Convention<Crop>(namedID, builder)
{
    /// <summary>
    /// The plant corresponding to this crop.
    /// </summary>
    public required Block Plant { get; init; }
    
    /// <summary>
    /// The optional fruit block that is grown by the crop.
    /// </summary>
    public required Block? Fruit { get; init; }
}

/// <summary>
/// A convention on crops.
/// </summary>
public static class CropConvention
{
    /// <summary>
    /// Build a new dense crop.
    /// </summary>
    /// <param name="b">The block builder to use.</param>
    /// <param name="namedID">The named ID of the crop, used to create the block IDs.</param>
    /// <param name="name">The name of the crop, used for display purposes.</param>
    /// <returns>The created dense crop.</returns>
    public static Crop BuildDenseCrop(this BlockBuilder b, String namedID, String name)
    {
        return b.BuildConvention<Crop>(builder =>
        {
            String texture = namedID.PascalCaseToSnakeCase();

            return new Crop(namedID, builder)
            {
                Plant = builder
                    .BuildFoliageBlock($"{namedID}{nameof(Crop.Plant)}", name)
                    .WithTexture(TID.Block(texture))
                    .WithBehavior<DenseCropPlant>()
                    .WithBehavior<DestroyOnLiquid>(breaking => breaking.ThresholdInitializer.ContributeConstant(FluidLevel.Three))
                    .Complete(),

                Fruit = null
            };
        });
    }

    /// <summary>
    /// Build a new double crop.
    /// </summary>
    /// <param name="b">The block builder to use.</param>
    /// <param name="namedID">The named ID of the crop, used to create the block IDs.</param>
    /// <param name="name">The name of the crop, used for display purposes.</param>
    /// <returns>The created double crop.</returns>
    public static Crop BuildDoubleCrop(this BlockBuilder b, String namedID, String name)
    {
        return b.BuildConvention<Crop>(builder =>
        {
            String texture = namedID.PascalCaseToSnakeCase();

            return new Crop(namedID, builder)
            {
                Plant = builder
                    .BuildFoliageBlock($"{namedID}{nameof(Crop.Plant)}", name)
                    .WithTexture(TID.Block(texture))
                    .WithBehavior<DoubleCropPlant>()
                    .WithBehavior<DestroyOnLiquid>(breaking => breaking.ThresholdInitializer.ContributeConstant(FluidLevel.Three))
                    .Complete(),

                Fruit = null
            };
        });
    }

    /// <summary>
    /// Build a new fruit crop.
    /// </summary>
    /// <param name="b">The block builder to use.</param>
    /// <param name="namedID">The named ID of the crop, used to create the block IDs.</param>
    /// <param name="names">The names of the parts, used for display purposes.</param>
    /// <returns>The created fruit crop.</returns>
    public static Crop BuildFruitCrop(this BlockBuilder b, String namedID, (String plant, String fruit) names)
    {
        return b.BuildConvention<Crop>(builder =>
        {
            String texture = namedID.PascalCaseToSnakeCase();

            var fruitID = $"{namedID}{nameof(Crop.Fruit)}";

            return new Crop(namedID, builder)
            {
                Plant = builder
                    .BuildFoliageBlock($"{namedID}{nameof(Crop.Plant)}", names.plant)
                    .WithTexture(TID.Block($"{texture}_plant"))
                    .WithBehavior<FruitCropPlant>(plant => plant.FruitInitializer.ContributeConstant(fruitID))
                    .WithBehavior<DestroyOnLiquid>(breaking => breaking.ThresholdInitializer.ContributeConstant(FluidLevel.Three))
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
