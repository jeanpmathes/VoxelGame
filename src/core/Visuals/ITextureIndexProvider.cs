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
    ///     Get the texture index for the given texture.
    /// </summary>
    /// <param name="name">The name of the texture.</param>
    /// <param name="isBlock">Whether the texture is a block texture or a fluid texture.</param>
    /// <returns>The texture index.</returns>
    public Int32 GetTextureIndex(String name, Boolean isBlock);
}
