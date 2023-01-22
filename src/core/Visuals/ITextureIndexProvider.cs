// <copyright file="ITextureIndexProvider.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

namespace VoxelGame.Core.Visuals;

/// <summary>
///     Can provide a texture index for a given texture.
/// </summary>
public interface ITextureIndexProvider
{
    /// <summary>
    ///     Get the texture index for the given texture.
    /// </summary>
    /// <param name="name">The name of the texture.</param>
    /// <returns>The texture index.</returns>
    int GetTextureIndex(string name);
}

