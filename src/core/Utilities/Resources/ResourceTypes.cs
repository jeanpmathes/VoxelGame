// <copyright file="ResourceTypes.cs" company="VoxelGame">
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

namespace VoxelGame.Core.Utilities.Resources;

#pragma warning disable S1192 // Same strings do not necessarily refer to the same thing here.

/// <summary>
///     Predefined resource types.
/// </summary>
public static class ResourceTypes
{
    /// <summary>
    ///     The meta directory resource type.
    /// </summary>
    public static ResourceType Directory { get; } = new(ResourceType.Category.Meta, "directory");

    /// <summary>
    ///     The default text resource type.
    /// </summary>
    public static ResourceType Text { get; } = new(ResourceType.Category.Text, "text");

    /// <summary>
    ///     The font bundle resource type.
    /// </summary>
    public static ResourceType FontBundle { get; } = new(ResourceType.Category.Font, "bundle");

    /// <summary>
    ///     The shader code resource type.
    /// </summary>
    public static ResourceType Shader { get; } = new(ResourceType.Category.Code, "shader");

    /// <summary>
    ///     The PNG subtype of a texture resource in a bundle.
    /// </summary>
    public static ResourceType TextureBundlePNG { get; } = new(ResourceType.Category.Texture, "bundle", "png");

    /// <summary>
    ///     The XML subtype of a texture resource in a bundle.
    /// </summary>
    public static ResourceType TextureBundleXML { get; } = new(ResourceType.Category.Texture, "bundle", "xml");

    /// <summary>
    ///     The resource bundle type.
    /// </summary>
    public static ResourceType TextureBundle { get; } = new(ResourceType.Category.Texture, "bundle");

    /// <summary>
    ///     The general texture resource type.
    /// </summary>
    public static ResourceType Texture { get; } = new(ResourceType.Category.Texture, "texture");

    /// <summary>
    ///     The general model resource type.
    /// </summary>
    public static ResourceType Model { get; } = new(ResourceType.Category.Model, "model");

    /// <summary>
    ///     The structure resource type.
    /// </summary>
    public static ResourceType Structure { get; } = new(ResourceType.Category.Voxels, "structure");

    /// <summary>
    ///     The GUI object resource type.
    /// </summary>
    public static ResourceType GUI { get; } = new(ResourceType.Category.Object, "gui");

    /// <summary>
    ///     The world decoration resource type.
    /// </summary>
    public static ResourceType WorldDecoration { get; } = new(ResourceType.Category.Object, "worldgen_decoration");

    /// <summary>
    ///     The world structure resource type.
    /// </summary>
    public static ResourceType WorldStructure { get; } = new(ResourceType.Category.Object, "worldgen_structure");

    /// <summary>
    ///     The biome resource type.
    /// </summary>
    public static ResourceType Biome { get; } = new(ResourceType.Category.Object, "worldgen", "biome");

    /// <summary>
    ///     The sub-biome resource type.
    /// </summary>
    public static ResourceType SubBiome { get; } = new(ResourceType.Category.Object, "worldgen", "sub-biome");

    /// <summary>
    ///     The biome distribution resource type.
    /// </summary>
    public static ResourceType BiomeDistribution { get; } = new(ResourceType.Category.Object, "worldgen_biome_distribution");

    /// <summary>
    ///     The world generator block palette resource type.
    /// </summary>
    public static ResourceType GeneratorPalette { get; } = new(ResourceType.Category.Object, "worldgen_palette");

    /// <summary>
    ///     The GUI skin resource type.
    /// </summary>
    public static ResourceType Skin { get; } = new(ResourceType.Category.Object, "gui_skin");

    /// <summary>
    ///     The object type corresponding to engine objects.
    /// </summary>
    public static ResourceType Engine { get; } = new(ResourceType.Category.Object, "engine");

    /// <summary>
    ///     The object type corresponding to blocks.
    /// </summary>
    public static ResourceType Block { get; } = new(ResourceType.Category.Object, "block");

    /// <summary>
    ///     The object type corresponding to fluids.
    /// </summary>
    public static ResourceType Fluid { get; } = new(ResourceType.Category.Object, "fluid");

    /// <summary>
    ///     The object type corresponding to content conventions.
    /// </summary>
    public static ResourceType Convention { get; } = new(ResourceType.Category.Object, "convention");

    /// <summary>
    ///     The object type corresponding to command invokers.
    /// </summary>
    public static ResourceType CommandInvoker { get; } = new(ResourceType.Category.Object, "command_invoker");

    /// <summary>
    ///     The object type corresponding to commands.
    /// </summary>
    public static ResourceType Command { get; } = new(ResourceType.Category.Object, "command");

    /// <summary>
    ///     The object type corresponding to texture modifiers.
    /// </summary>
    public static ResourceType Modifier { get; } = new(ResourceType.Category.Object, "texture_modifier");

    /// <summary>
    ///     The object type corresponding to texture combinators.
    /// </summary>
    public static ResourceType Combinator { get; } = new(ResourceType.Category.Object, "texture_combinator");
}
