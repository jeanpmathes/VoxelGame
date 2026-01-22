// <copyright file="Textures.cs" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2026 Jean Patrick Mathes
//      
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
//     
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//     
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <https://www.gnu.org/licenses/>.
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
