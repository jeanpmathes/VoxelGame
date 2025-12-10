// <copyright file="Flowers.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Core.Logic.Contents;
using VoxelGame.Core.Logic.Voxels.Conventions;
using VoxelGame.Core.Resources.Language;

namespace VoxelGame.Core.Logic.Voxels.Contents;

/// <summary>
///     All sorts of flowers.
/// </summary>
public class Flowers(BlockBuilder builder) : Category(builder)
{
    /// <summary>
    ///     A simple red flower.
    /// </summary>
    public Flower FlowerRed { get; } = builder.BuildFlower(new CID(nameof(FlowerRed)), Language.FlowerRed);

    /// <summary>
    ///     A simple yellow flower.
    /// </summary>
    public Flower FlowerYellow { get; } = builder.BuildFlower(new CID(nameof(FlowerYellow)), Language.FlowerYellow);
}
