// <copyright file="ITextureIndexProvider.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using VoxelGame.Core.Utilities.Resources;

namespace VoxelGame.Core.Visuals;

/// <summary>
///     Can provide a texture index for a given texture.
/// </summary>
public interface ITextureIndexProvider : IResourceProvider
{
    /// <summary>
    ///     The index of the missing texture.
    ///     When loading textures, a client has to ensure that the missing texture is always present and at index 0.
    /// </summary>
    public const Int32 MissingTextureIndex = 0;

    /// <summary>
    ///     Get the texture index for the given texture.
    /// </summary>
    /// <param name="identifier">The texture identifier.</param>
    /// <returns>The texture index.</returns>
    public Int32 GetTextureIndex(TID identifier);
}
