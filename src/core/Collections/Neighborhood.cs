// <copyright file="Neighborhood.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using OpenTK.Mathematics;
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
    public const Int32 Length = 3;

    /// <summary>
    ///     The index of the center of the array.
    /// </summary>
    public static Vector3i Center => Vector3i.One;

    /// <summary>
    ///     Get all indices of the array.
    /// </summary>
    public static IEnumerable<(Int32 x, Int32 y, Int32 z)> Indices => VMath.Range3(Length, Length, Length);
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

    /// <summary>
    ///     Get or set the element at the center of the neighborhood, which is essentially the active element itself.
    /// </summary>
    public T Center
    {
        get => GetAt(Neighborhood.Center);
        set => SetAt(Neighborhood.Center, value);
    }
}
