// <copyright file="BlockLoader.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.Linq;
using VoxelGame.Core.Logic.Sections;
using VoxelGame.Core.Utilities.Resources;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Elements;

/// <summary>
///     Loads all blocks.
/// </summary>
public sealed class BlockLoader : IResourceLoader
{
    /// <summary>
    ///     The maximum amount of different blocks that can be registered.
    /// </summary>
    private const Int32 BlockLimit = 1 << Section.DataShift;

    String? ICatalogEntry.Instance => null;

    /// <inheritdoc />
    public IEnumerable<IResource> Load(IResourceContext context)
    {
        return context.Require<ITextureIndexProvider>(textureIndexProvider =>
            context.Require<IBlockModelProvider>(blockModelProvider =>
                context.Require<VisualConfiguration>(visualConfiguration =>
                {
                    if (Blocks.Instance.Count > BlockLimit)
                        context.ReportWarning(this, $"Not more than {BlockLimit} blocks are allowed, additional blocks will be ignored");

                    UInt32 id = 0;

                    foreach (Block block in Blocks.Instance.Content.Take(BlockLimit))
                        block.SetUp(id++, textureIndexProvider, blockModelProvider, visualConfiguration);

                    return Blocks.Instance.Content.Take(BlockLimit);
                })));
    }
}
