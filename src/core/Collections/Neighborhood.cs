// <copyright file="Neighborhood.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK.Mathematics;
using VoxelGame.Core.Utilities;
using VoxelGame.Toolkit.Collections;

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
    ///     The total number of elements in the neighborhood.
    /// </summary>
    public const Int32 Count = Length * Length * Length;

    /// <summary>
    ///     The index of the center of the array.
    /// </summary>
    public static Vector3i Center => Vector3i.One;

    /// <summary>
    ///     The index of the first element of the array (lowest index).
    /// </summary>
    public static Vector3i First => Vector3i.Zero;

    /// <summary>
    ///     Get all indices of the array.
    /// </summary>
    public static IEnumerable<(Int32 x, Int32 y, Int32 z)> Indices { get; } = VMath.Range3(Length, Length, Length).ToArray();
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
        get => this[Neighborhood.Center];
        set => this[Neighborhood.Center] = value;
    }

    /// <summary>
    ///     Get or set the element at the first index of the neighborhood.
    /// </summary>
    public T First
    {
        get => this[Neighborhood.First];
        set => this[Neighborhood.First] = value;
    }
}
