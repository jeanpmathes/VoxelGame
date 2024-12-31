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
    /// <param name="identifier">The texture identifier.</param>
    /// <returns>The texture index.</returns>
    public Int32 GetTextureIndex(TID identifier);
}
