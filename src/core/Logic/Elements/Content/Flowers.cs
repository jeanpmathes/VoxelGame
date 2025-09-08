// <copyright file="Flowers.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Core.Logic.Elements.Conventions;
using VoxelGame.Core.Resources.Language;

namespace VoxelGame.Core.Logic.Elements;

/// <summary>
/// All sorts of flowers.
/// </summary>
public class Flowers(BlockBuilder builder) : Category(builder)
{
    /// <summary>
    ///     A simple red flower.
    /// </summary>
    public Flower FlowerRed { get; } = builder.BuildFlower(nameof(FlowerRed), Language.FlowerRed); // todo: rename to Red when separate class and adapt naming in convention to include flower
    
    /// <summary>
    ///     A simple yellow flower.
    /// </summary>
    public Flower FlowerYellow { get; } = builder.BuildFlower(nameof(FlowerYellow), Language.FlowerYellow); // todo: rename to Yellow when separate class and adapt naming in convention to include flower
}
