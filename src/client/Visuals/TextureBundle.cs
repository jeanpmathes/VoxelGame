// <copyright file="TextureBundle.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.Drawing;
using VoxelGame.Core.Utilities.Resources;
using VoxelGame.Graphics.Graphics;

namespace VoxelGame.Client.Visuals;

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

    #region DISPOSING

    /// <inheritdoc />
    public void Dispose()
    {
        // Nothing to dispose.
    }

    #endregion DISPOSING

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
    public Color GetDominantColor(Int32 index)
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
