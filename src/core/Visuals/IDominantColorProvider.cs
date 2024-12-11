// <copyright file="IDominantColorProvider.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Drawing;

namespace VoxelGame.Core.Visuals;

/// <summary>
///     Interface for providing the dominant color of textures.
/// </summary>
public interface IDominantColorProvider
{
    /// <summary>
    ///     Get the dominant color of the texture with the given index.
    /// </summary>
    /// <param name="index">The index of the texture.</param>
    /// <returns>The dominant color.</returns>
    Color GetDominantColor(Int32 index);
}
