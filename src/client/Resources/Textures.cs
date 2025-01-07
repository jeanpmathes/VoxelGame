// <copyright file="Textures.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Client.Visuals.Textures;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Utilities.Resources;
using VoxelGame.Core.Visuals;
using VoxelGame.Core.Visuals.Meshables;

namespace VoxelGame.Client.Resources;

/// <summary>
///     The world texture resources.
/// </summary>
public class Textures : ResourceCatalog
{
    /// <summary>
    ///     Resource identifier for the block textures.
    /// </summary>
    public static readonly RID BlockID = RID.Named<TextureBundle>("Blocks");

    /// <summary>
    ///     Resource identifier for the fluid textures.
    /// </summary>
    public static readonly RID FluidID = RID.Named<TextureBundle>("Fluids");

    /// <summary>
    ///     Create an instance of the catalog.
    /// </summary>
    public Textures() : base([CreateBlockTextureLoader(), CreateFluidTextureLoader(), new TextureInfoProvider()]) {}

    private static TextureBundleLoader CreateBlockTextureLoader()
    {
        TextureBundleLoader loader = new(BlockID, resolution: 32, Meshing.MaxTextureCount, Image.MipmapAlgorithm.AveragingWithoutTransparency);

        loader.AddSource(FileSystem.GetResourceDirectory("Textures", "Blocks"));

        return loader;
    }

    private static TextureBundleLoader CreateFluidTextureLoader()
    {
        TextureBundleLoader loader = new(FluidID, resolution: 32, Meshing.MaxFluidTextureCount, Image.MipmapAlgorithm.AveragingWithTransparency);

        loader.AddSource(FileSystem.GetResourceDirectory("Textures", "Fluids"));

        return loader;
    }
}
