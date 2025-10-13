// <copyright file="Category.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System.Collections.Generic;
using VoxelGame.Core.Logic.Contents;

namespace VoxelGame.Core.Logic.Voxels;

/// <summary>
///     A category of content elements.
/// </summary>
public class Category(BlockBuilder builder)
{
    /// <summary>
    ///     All content elements in this category.
    /// </summary>
    public IEnumerable<IContent> Contents => builder.Registry.RetrieveContent();
}
