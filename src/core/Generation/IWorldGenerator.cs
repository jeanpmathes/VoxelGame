﻿// <copyright file="IWorldGenerator.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System.Collections.Generic;
using VoxelGame.Core.Logic;

namespace VoxelGame.Core.Generation;

/// <summary>
///     Generates a world.
/// </summary>
public interface IWorldGenerator
{
    /// <summary>
    ///     Get the map of the world.
    /// </summary>
    IMap Map { get; }

    /// <summary>
    ///     Generate a column of the world.
    /// </summary>
    /// <param name="x">The x position of the world.</param>
    /// <param name="z">The z position of the world.</param>
    /// <param name="heightRange">The height range (inclusive, exclusive) in which blocks should be generated.</param>
    /// <returns>The data in the column.</returns>
    IEnumerable<Content> GenerateColumn(int x, int z, (int start, int end) heightRange);

    /// <summary>
    ///     Decorate a section of the world.
    /// </summary>
    /// <param name="position">The position of the section.</param>
    /// <param name="sections">The section and all its neighbors.</param>
    #pragma warning disable S2368
    void DecorateSection(SectionPosition position, Section[,,] sections);
    #pragma warning restore S2368

    /// <summary>
    ///     Emit views of global generated data for debugging.
    /// </summary>
    /// <param name="path">A path to the debug directory.</param>
    void EmitViews(string path);
}
