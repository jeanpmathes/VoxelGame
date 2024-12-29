// <copyright file="BlockModelProvider.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Core.Utilities.Resources;

namespace VoxelGame.Core.Visuals;

/// <summary>
///     Provides loaded block models.
/// </summary>
public class BlockModelProvider : ResourceProvider<BlockModel>, IBlockModelProvider
{
    /// <summary>
    ///     Creates a new block model provider.
    /// </summary>
    public BlockModelProvider() : base(BlockModels.CreateFallback, model => model.Copy()) {}

    /// <inheritdoc />
    public BlockModel GetModel(RID identifier)
    {
        return GetResource(identifier);
    }
}
