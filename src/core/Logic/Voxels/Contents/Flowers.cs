// <copyright file="Flowers.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Core.Logic.Contents;
using VoxelGame.Core.Logic.Voxels.Conventions;
using VoxelGame.Core.Resources.Language;

namespace VoxelGame.Core.Logic.Voxels;

/// <summary>
///     All sorts of flowers.
/// </summary>
public class Flowers(BlockBuilder builder) : Category(builder)
{
    /// <summary>
    ///     A simple red flower.
    /// </summary>
    public Flower RedFlower { get; } = builder.BuildFlower(new CID(nameof(RedFlower)), Language.FlowerRed);

    /// <summary>
    ///     A simple yellow flower.
    /// </summary>
    public Flower YellowFlower { get; } = builder.BuildFlower(new CID(nameof(YellowFlower)), Language.FlowerYellow);
}
