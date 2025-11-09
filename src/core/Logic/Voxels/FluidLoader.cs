// <copyright file="FluidLoader.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.Linq;
using VoxelGame.Core.Utilities.Resources;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Voxels;

/// <summary>
///     Loads all fluids.
/// </summary>
public sealed class FluidLoader : IResourceLoader
{
    /// <summary>
    ///     The maximum amount of different fluids that can be registry.Registered.
    /// </summary>
    private const Int32 FluidLimit = 32;

    String? ICatalogEntry.Instance => null;

    /// <inheritdoc />
    public IEnumerable<IResource> Load(IResourceContext context)
    {
        return context.Require<ITextureIndexProvider>(textureIndexProvider =>
            context.Require<IDominantColorProvider>(dominantColorProvider =>
            {
                if (Fluids.Instance.Count > FluidLimit)
                    context.ReportWarning(this, $"Not more than {FluidLimit} fluids are allowed, additional fluids will be ignored");

                UInt32 id = 0;

                foreach (Fluid fluid in Fluids.Instance.Content.Take(FluidLimit))
                    fluid.SetUp(id++, textureIndexProvider, dominantColorProvider);

                _ = Fluids.ContactManager; // Ensure the contact manager is created.

                return Fluids.Instance.Content.Take(FluidLimit);
            }));
    }
}
