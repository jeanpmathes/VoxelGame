// <copyright file="Parameters.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;

namespace VoxelGame.Core.Generation.Dungeons;

/// <summary>
///     The parameters describing the dungeon generation.
/// </summary>
/// <param name="Levels">The number of vertical levels in the dungeon.</param>
/// <param name="Size">The size of a level, both in width and height, measured in areas.</param>
public record Parameters(Int32 Levels, Int32 Size) {}
