// <copyright file="ResourceIdentifier.cs" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2025 Jean Patrick Mathes
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

using System;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Core.Utilities.Resources;

/// <summary>
///     The type of resource.
/// </summary>
public class ResourceType
{
    /// <summary>
    ///     Some resource categories.
    /// </summary>
    public enum Category
    {
        /// <summary>
        ///     A meta category, e.g. for error resources.
        /// </summary>
        Meta,

        /// <summary>
        ///     Any texture resource.
        /// </summary>
        Texture,

        /// <summary>
        ///     Any 3D model resource.
        /// </summary>
        Model,

        /// <summary>
        ///     Any voxel data resource.
        /// </summary>
        Voxels,

        /// <summary>
        ///     Any text resource.
        /// </summary>
        Text,

        /// <summary>
        ///     Any font resource.
        /// </summary>
        Font,

        /// <summary>
        ///     Any code resource.
        /// </summary>
        Code,

        /// <summary>
        ///     Any runtime object resource.
        /// </summary>
        Object
    }

    private readonly String type;

    /// <summary>
    ///     Create a new resource type.
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
            _ => throw Exceptions.UnsupportedValue(category)
        };

        this.type = BuildType(categoryString, type, subType);
    }

    /// <summary>
    ///     Create a new resource type.
    /// </summary>
    /// <param name="category">The category of the resource.</param>
    /// <param name="type">The general type of the resource.</param>
    /// <param name="subType">The subtype of the resource.</param>
    public ResourceType(String category, String type, String? subType = null)
    {
        this.type = BuildType(category, type, subType);
    }

    private static String BuildType(String category, String type, String? subType)
    {
        return subType == null ? $"{category}/{type}" : $"{category}/{type}/{subType}";
    }

    /// <inheritdoc />
    public override String ToString()
    {
        return type;
    }
}
