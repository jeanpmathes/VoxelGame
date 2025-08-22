// <copyright file="Crops.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Core.Logic.Elements.Conventions;
using VoxelGame.Core.Resources.Language;

namespace VoxelGame.Core.Logic.Elements;

/// <summary>
/// 
/// </summary>
/// <param name="builder"></param>
public class Crops(BlockBuilder builder) : Category(builder)
{
    // todo: go through crop textures, remove duplicated stages and adapt code so it takes number of stages as parameter

    /// <summary>
    ///     Flax is a crop plant that grows on farmland. It requires water to fully grow.
    /// </summary>
    public Crop Flax { get; } = builder.BuildDenseCrop(Language.Flax, nameof(Flax));
    
    /// <summary>
    ///     Potatoes are a crop plant that grows on farmland. They require water to fully grow.
    /// </summary>
    public Crop Potatoes { get; } = builder.BuildDenseCrop(Language.Potatoes, nameof(Potatoes));
    
    /// <summary>
    ///     Onions are a crop plant that grows on farmland. They require water to fully grow.
    /// </summary>
    public Crop Onions { get; } = builder.BuildDenseCrop(Language.Onions, nameof(Onions));
    
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
