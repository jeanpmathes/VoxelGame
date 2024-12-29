// <copyright file="ResourceIdentifier.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;

namespace VoxelGame.Core.Utilities.Resources;

/// <summary>
/// The type of resource.
/// </summary>
public class ResourceType
{
    /// <summary>
    /// Some resource categories.
    /// </summary>
    public enum Category
    {
        /// <summary>
        /// A meta category, e.g. for error resources.
        /// </summary>
        Meta,

        /// <summary>
        /// Any texture resource.
        /// </summary>
        Texture,

        /// <summary>
        /// Any 3D model resource.
        /// </summary>
        Model,

        /// <summary>
        /// Any voxel data resource.
        /// </summary>
        Voxels,

        /// <summary>
        /// Any text resource.
        /// </summary>
        Text,

        /// <summary>
        /// Any font resource.
        /// </summary>
        Font,

        /// <summary>
        /// Any code resource.
        /// </summary>
        Code,

        /// <summary>
        /// Any runtime object resource.
        /// </summary>
        Object
    }

    private readonly String type;

    /// <summary>
    /// Create a new resource type.
    /// </summary>
    /// <param name="category">The category of the resource.</param>
    /// <param name="type">The general type of the resource.</param>
    /// <param name="subType">The subtype of the resource.</param>
    public ResourceType(Category category, String type, String? subType = null)
    {
        String categoryString = category switch
        {
            Category.Texture => "texture",
            Category.Model => "model",
            Category.Voxels => "voxels",
            Category.Meta => "meta",
            Category.Text => "text",
            Category.Font => "font",
            Category.Code => "code",
            Category.Object => "object",
            _ => throw new ArgumentOutOfRangeException(nameof(category), category, message: null)
        };

        this.type = BuildType(categoryString, type, subType);
    }

    /// <summary>
    /// Create a new resource type.
    /// </summary>
    /// <param name="category">The category of the resource.</param>
    /// <param name="type">The general type of the resource.</param>
    /// <param name="subType">The subtype of the resource.</param>
    public ResourceType(String category, String type, String? subType = null)
    {
        this.type = BuildType(category, type, subType);
    }

    private static String BuildType(String category, String type, String? subType) =>
        subType == null ? $"{category}/{type}" : $"{category}/{type}/{subType}";

    /// <inheritdoc />
    public override String ToString() => type;
}
