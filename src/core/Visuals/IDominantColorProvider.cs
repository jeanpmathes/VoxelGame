// <copyright file="IDominantColorProvider.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Drawing;
using VoxelGame.Core.Utilities.Resources;

namespace VoxelGame.Core.Visuals;

/// <summary>
///     Interface for providing the dominant color of textures.
/// </summary>
public interface IDominantColorProvider : IResourceProvider
{
    /// <summary>
    ///     Get the dominant color of the texture with the given index.
    /// </summary>
    /// <param name="index">The index of the texture.</param>
    /// <param name="isBlock">Whether the texture is a block texture or a fluid texture.</param>
    /// <returns>The dominant color.</returns>
    Color GetDominantColor(Int32 index, Boolean isBlock);
}
