// <copyright file="Neighborhood.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System.Collections.Generic;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Core.Collections;

/// <summary>
///     Utilities for working with neighborhoods.
/// </summary>
public static class Neighborhood
{
    /// <summary>
    ///     The length of a neighbor-array in one dimension.
    /// </summary>
    public const int Length = 3;

    /// <summary>
    ///     Get all indices of the array.
    /// </summary>
    public static IEnumerable<(int x, int y, int z)> Indices => VMath.Range3(Length, Length, Length);
}

/// <summary>
///     A 3x3x3 array, used to store the neighborhood of any voxel cell.
/// </summary>
/// <typeparam name="T">The type of the elements.</typeparam>
public class Neighborhood<T> : Array3D<T>
{
    /// <summary>
    ///     Create a new neighbourhood.
    /// </summary>
    public Neighborhood() : base(Neighborhood.Length) {}
}
