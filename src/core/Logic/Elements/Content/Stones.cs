// <copyright file="Stones.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Core.Logic.Elements.Conventions;
using VoxelGame.Core.Resources.Language;

namespace VoxelGame.Core.Logic.Elements;

/// <summary>
/// All sorts of stone types. Stone occurs naturally in the world but can also be used for the construction of various things.
/// </summary>
public class Stones(BlockBuilder builder) : Category(builder)
{
    /// <summary>
    ///     Granite is found next to volcanic activity.
    ///     When carved, the patterns show geometric shapes.
    /// </summary>
    public Stone Granite { get; } = builder.BuildStone(Language.Granite, nameof(Granite));
    
    /// <summary>
    ///     Sandstone is found all over the world and especially in the desert.
    ///     When carved, the patterns depict the desert sun.
    /// </summary>
    public Stone Sandstone { get; } = builder.BuildStone(Language.Sandstone, nameof(Sandstone));
    
    /// <summary>
    ///     Limestone is found all over the world and especially in oceans.
    ///     When carved, the patterns depict the ocean and life within it.
    /// </summary>
    public Stone Limestone { get; } = builder.BuildStone(Language.Limestone, nameof(Limestone));
    
    /// <summary>
    ///     Marble is a rarer stone type.
    ///     When carved, the patterns depict an ancient temple.
    /// </summary>
    public Stone Marble { get; } = builder.BuildStone(Language.Marble, nameof(Marble));
    
    /// <summary>
    ///     Pumice is created when lava rapidly cools down, while being in contact with a lot of water.
    ///     When carved, the patterns depict heat rising from the earth.
    /// </summary>
    public Stone Pumice { get; } = builder.BuildStone(Language.Pumice, nameof(Pumice));
    
    /// <summary>
    ///     Obsidian is a dark type of stone, that forms from lava.
    ///     When carved, the patterns depict an ancient artifact.
    /// </summary>
    public Stone Obsidian { get; } = builder.BuildStone(Language.Obsidian, nameof(Obsidian));
}
