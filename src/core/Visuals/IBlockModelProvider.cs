// <copyright file="IBlockModelProvider.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Core.Utilities.Resources;

namespace VoxelGame.Core.Visuals;

/// <summary>
/// Provides loaded block models.
/// </summary>
public interface IBlockModelProvider : IResourceProvider
{
    /// <summary>
    /// Get the block model for a given identifier.
    /// </summary>
    /// <param name="identifier">The resource identifier.</param>
    /// <returns>The block model, or a fallback model if the model is not found.</returns>
    public BlockModel GetModel(RID identifier);
}
