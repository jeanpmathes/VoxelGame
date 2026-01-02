// <copyright file="TextureBundle.cs" company="VoxelGame">
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

using System;
using System.Collections.Generic;
using VoxelGame.Core.Utilities.Resources;
using VoxelGame.Core.Visuals;
using VoxelGame.Graphics.Graphics;

namespace VoxelGame.Client.Visuals.Textures;

/// <summary>
///     A list of textures that can be used by shaders.
///     Each texture has a name and index.
/// </summary>
public sealed class TextureBundle : IResource
{
    /// <summary>
    ///     Create a new texture bundle.
    /// </summary>
    /// <param name="identifier">The identifier of the resource.</param>
    /// <param name="textureArray">The loaded texture array.</param>
    /// <param name="textureIndices">A mapping of texture names to indices.</param>
    public TextureBundle(RID identifier, TextureArray textureArray, Dictionary<String, Int32> textureIndices)
    {
        TextureArray = textureArray;
        TextureIndices = textureIndices;

        Identifier = identifier;
        Type = ResourceTypes.TextureBundle;
    }

    private TextureArray TextureArray { get; }
    private Dictionary<String, Int32> TextureIndices { get; }

    /// <summary>
    ///     Get the number of textures in the bundle.
    /// </summary>
    public Int32 Count => TextureArray.Count;

    /// <inheritdoc />
    public RID Identifier { get; }

    /// <inheritdoc />
    public ResourceType Type { get; }

    #region DISPOSABLE

    /// <inheritdoc />
    public void Dispose()
    {
        // Nothing to dispose.
    }

    #endregion DISPOSABLE

    /// <summary>
    ///     Try getting the texture index of a texture by its name.
    /// </summary>
    /// <param name="name">The name of the texture.</param>
    /// <param name="index">The index of the texture.</param>
    /// <returns>True if the texture was found, false otherwise.</returns>
    public Boolean TryGetTextureIndex(String name, out Int32 index)
    {
        return TextureIndices.TryGetValue(name, out index);
    }

    /// <summary>
    ///     Get the dominant color of a texture.
    /// </summary>
    /// <param name="index">The index of the texture.</param>
    /// <returns>The dominant color of the texture.</returns>
    public ColorS GetDominantColor(Int32 index)
    {
        return TextureArray.GetDominantColor(index);
    }

    /// <summary>
    ///     Get the arrays filling the texture slots.
    /// </summary>
    public static (TextureArray, TextureArray) GetTextureSlots(TextureBundle first, TextureBundle second)
    {
        return (first.TextureArray, second.TextureArray);
    }
}
