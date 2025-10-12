// <copyright file="Flower.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Logic.Contents;
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
                        plant.HeightInitializer.ContributeConstant(value: 0.5);
                        plant.WidthInitializer.ContributeConstant(value: 0.35);
                    })
                    .WithBehavior<DestroyOnLiquid>(breaking => breaking.ThresholdInitializer.ContributeConstant(FluidLevel.Four))
                    .WithProperties(flags => flags.IsReplaceable.ContributeConstant(value: true))
                    .Complete(),

                Tall = builder
                    .BuildFoliageBlock(new CID($"{contentID}{nameof(Flower.Tall)}"), $"{name} ({nameof(Language.Tall)})")
                    .WithTexture(TID.Block($"{texture}_tall"))
                    .WithBehavior<NeutralTint>()
                    .WithBehavior<DoubleCrossPlant>()
                    .WithBehavior<DestroyOnLiquid>(breaking => breaking.ThresholdInitializer.ContributeConstant(FluidLevel.Five))
                    .Complete()
            };
        });
    }
}
