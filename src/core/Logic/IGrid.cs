// <copyright file="IGrid.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using OpenTK.Mathematics;

namespace VoxelGame.Core.Logic;

/// <summary>
///     A <see cref="IGrid" /> which only allows reading.
/// </summary>
public interface IReadOnlyGrid
{
    /// <summary>
    ///     Get content at a given position.
    /// </summary>
    /// <param name="position">The position to get the content from.</param>
    /// <returns>The content at the given position, or null if the position is out of bounds.</returns>
    Content? GetContent(Vector3i position);
}

/// <summary>
///     Represents a grid of block positions, filled with content.
/// </summary>
public interface IGrid : IReadOnlyGrid
{
    /// <summary>
    ///     Set content at a given position. If the position is out of bounds, nothing happens.
    /// </summary>
    /// <param name="content">The content to set.</param>
    /// <param name="position">The position to set the content at.</param>
    void SetContent(Content content, Vector3i position);
}

