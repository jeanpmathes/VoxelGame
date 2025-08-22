// <copyright file="Flower.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Logic.Elements.Behaviors.Fluids;
using VoxelGame.Core.Logic.Elements.Behaviors.Nature.Plants;
using VoxelGame.Core.Logic.Elements.Behaviors.Visuals;
using VoxelGame.Core.Resources.Language;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Elements.Conventions;

/// <summary>
/// A flower, as defined by the <see cref="FlowerConvention"/>.
/// </summary>
public sealed class Flower(String namedID, BlockBuilder builder) : Convention<Flower>(namedID, builder)
{
    /// <summary>
    /// The short variant of this flower.
    /// </summary>
    public required Block Short { get; init; }
    
    /// <summary>
    /// The tall variant of this flower.
    /// </summary>
    public required Block Tall { get; init; }
}

/// <summary>
/// A convention on flowers.
/// </summary>
public static class FlowerConvention
{
    /// <summary>
    /// Build a new flower.
    /// </summary>
    /// <param name="b">The block builder to use.</param>
    /// <param name="name">The name of the flower, used for display purposes.</param>
    /// <param name="namedID">The named ID of the flower, used to create the block IDs.</param>
    /// <returns>The created flower.</returns>
    public static Flower BuildFlower(this BlockBuilder b, String name, String namedID)
    {
        return b.BuildConvention<Flower>(builder =>
        {
            String texture = namedID.PascalCaseToSnakeCase();

            return new Flower(namedID, builder)
            {
                Short = builder
                    .BuildFoliageBlock(name, $"{namedID}{nameof(Flower.Short)}")
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
                    .BuildFoliageBlock($"{name} ({nameof(Language.Tall)})", $"{namedID}{nameof(Flower.Tall)}")
                    .WithTexture(TID.Block($"{texture}_tall"))
                    .WithBehavior<NeutralTint>()
                    .WithBehavior<DoubleCrossPlant>()
                    .WithBehavior<DestroyOnLiquid>(breaking => breaking.ThresholdInitializer.ContributeConstant(FluidLevel.Five))
                    .Complete(),
            };
        });
    }
}
