// <copyright file="Coals.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Core.Logic.Elements.Conventions;
using VoxelGame.Core.Resources.Language;

namespace VoxelGame.Core.Logic.Elements;

/// <summary>
/// Different types of coal. All three types can be found in the world.
/// </summary>
public class Coals(BlockBuilder builder) : Category(builder)
{
    /// <summary>
    ///     Lignite is a type of coal.
    ///     It is the lowest rank of coal but can be found near the surface.
    /// </summary>
    public Coal Lignite { get; } = builder.BuildCoal(nameof(Lignite), Language.CoalLignite);
    
    /// <summary>
    ///     Bituminous coal is a type of coal.
    ///     It is of medium rank and is the most abundant type of coal.
    /// </summary>
    public Coal BituminousCoal { get; } = builder.BuildCoal(nameof(BituminousCoal), Language.CoalBituminous);
    
    /// <summary>
    ///     Anthracite is a type of coal.
    ///     It is the highest rank of coal and is the hardest and most carbon-rich.
    /// </summary>
    public Coal Anthracite { get; } = builder.BuildCoal(nameof(Anthracite), Language.CoalAnthracite);
}
