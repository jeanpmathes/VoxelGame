// <copyright file="Crops.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Core.Logic.Contents;
using VoxelGame.Core.Logic.Voxels.Conventions;
using VoxelGame.Core.Resources.Language;

namespace VoxelGame.Core.Logic.Voxels.Contents;

/// <summary>
///     Crops grow on farmland and can be harvested for food or other resources.
/// </summary>
public class Crops(BlockBuilder builder) : Category(builder)
{
    /// <summary>
    ///     Flax is a crop plant that grows on farmland. It requires water to fully grow.
    /// </summary>
    public Crop Flax { get; } = builder.BuildDenseCrop(new CID(nameof(Flax)), Language.Flax);

    /// <summary>
    ///     Potatoes are a crop plant that grows on farmland. They require water to fully grow.
    /// </summary>
    public Crop Potato { get; } = builder.BuildDenseCrop(new CID(nameof(Potato)), Language.Potato);

    /// <summary>
    ///     Onions are a crop plant that grows on farmland. They require water to fully grow.
    /// </summary>
    public Crop Onion { get; } = builder.BuildDenseCrop(new CID(nameof(Onion)), Language.Onion);

    /// <summary>
    ///     Wheat is a crop plant that grows on farmland. It requires water to fully grow.
    /// </summary>
    public Crop Wheat { get; } = builder.BuildDenseCrop(new CID(nameof(Wheat)), Language.Wheat);

    /// <summary>
    ///     Maize is a crop plant that grows on farmland.
    ///     Maize grows two blocks high. It requires water to fully grow.
    /// </summary>
    public Crop Maize { get; } = builder.BuildDoubleCrop(new CID(nameof(Maize)), Language.Maize);

    /// <summary>
    ///     The pumpkin plant grows pumpkin fruits.
    /// </summary>
    public Crop Pumpkin { get; } = builder.BuildFruitCrop(new CID(nameof(Pumpkin)), (Language.PumpkinPlant, Language.Pumpkin));

    /// <summary>
    ///     The melon plant grows melon fruits.
    /// </summary>
    public Crop Melon { get; } = builder.BuildFruitCrop(new CID(nameof(Melon)), (Language.MelonPlant, Language.Melon));
}
