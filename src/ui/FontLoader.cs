// <copyright file="FontLoader.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using VoxelGame.Core.Utilities.Resources;
using VoxelGame.UI.Platform;
using VoxelGame.UI.Utilities;

namespace VoxelGame.UI;

/// <summary>
///     Loads the font bundle for the GUI.
/// </summary>
public sealed class FontLoader : IResourceLoader
{
    String? ICatalogEntry.Instance => null;

    /// <inheritdoc />
    public IEnumerable<IResource> Load(IResourceContext context)
    {
        return context.Require<IGwenGui>(gui => [new FontBundle(gui.Root.Skin)]);
    }
}
