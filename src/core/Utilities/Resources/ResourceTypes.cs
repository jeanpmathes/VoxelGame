// <copyright file="ResourceTypes.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

namespace VoxelGame.Core.Utilities.Resources;

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
    ///     The object type corresponding to command invokers.
    /// </summary>
    public static ResourceType CommandInvoker { get; } = new(ResourceType.Category.Object, "command_invoker");
}
