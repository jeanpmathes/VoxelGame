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
    /// <inheritdoc />
    public BlockModel GetModel(RID identifier)
    {
        return GetResource(identifier);
    }

    /// <inheritdoc />
    protected override BlockModel CreateFallback()
    {
        return BlockModels.CreateFallback();
    }

    /// <inheritdoc />
    protected override BlockModel Copy(BlockModel resource)
    {
        return resource.Copy();
    }
}
