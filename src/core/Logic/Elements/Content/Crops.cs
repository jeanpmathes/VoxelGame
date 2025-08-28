// <copyright file="Crops.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Core.Logic.Elements.Conventions;
using VoxelGame.Core.Resources.Language;

namespace VoxelGame.Core.Logic.Elements;

/// <summary>
/// Crops grow on farmland and can be harvested for food or other resources.
/// </summary>
public class Crops(BlockBuilder builder) : Category(builder)
{
    /// <summary>
    ///     Flax is a crop plant that grows on farmland. It requires water to fully grow.
    /// </summary>
    public Crop Flax { get; } = builder.BuildDenseCrop(Language.Flax, nameof(Flax));
    
    /// <summary>
    ///     Potatoes are a crop plant that grows on farmland. They require water to fully grow.
    /// </summary>
    public Crop Potato { get; } = builder.BuildDenseCrop(Language.Potato, nameof(Potato));
    
    /// <summary>
    ///     Onions are a crop plant that grows on farmland. They require water to fully grow.
    /// </summary>
    public Crop Onion { get; } = builder.BuildDenseCrop(Language.Onion, nameof(Onion));
    
    /// <summary>
    ///     Wheat is a crop plant that grows on farmland. It requires water to fully grow.
    /// </summary>
    public Crop Wheat { get; } = builder.BuildDenseCrop(Language.Wheat, nameof(Wheat));
    
    /// <summary>
    ///     Maize is a crop plant that grows on farmland.
    ///     Maize grows two blocks high. It requires water to fully grow.
    /// </summary>
    public Crop Maize { get; } = builder.BuildDoubleCrop(Language.Maize, nameof(Maize));

    /// <summary>
    ///     The pumpkin plant grows pumpkin fruits.
    /// </summary>
    public Crop Pumpkin { get; } = builder.BuildFruitCrop((Language.PumpkinPlant, Language.Pumpkin), nameof(Pumpkin));
    
    /// <summary>
    ///     The melon plant grows melon fruits.
    /// </summary>
    public Crop Melon { get; } = builder.BuildFruitCrop((Language.MelonPlant, Language.Melon), nameof(Melon));
}
