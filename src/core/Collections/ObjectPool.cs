// <copyright file="ObjectPool.cs" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2026 Jean Patrick Mathes
//      
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
//     
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//     
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <https://www.gnu.org/licenses/>.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Concurrent;

namespace VoxelGame.Core.Collections;

/// <summary>
///     A pool of objects.
/// </summary>
/// <typeparam name="T">The type of objects to pool.</typeparam>
public class ObjectPool<T> where T : class
{
    private readonly Func<T> factory;
    private readonly ConcurrentBag<T> objects = [];

    /// <summary>
    ///     Create a new object pool.
    /// </summary>
    /// <param name="factory">A factory function to create new objects.</param>
    public ObjectPool(Func<T> factory)
    {
        this.factory = factory;
    }

    /// <summary>
    ///     Get the number of objects in the pool.
    /// </summary>
    public Int32 Count => objects.Count;

    /// <summary>
    ///     Get an object from the pool.
    /// </summary>
    /// <returns>An object, may not be cleaned.</returns>
    public T Get()
    {
        return objects.TryTake(out T? instance) ? instance : factory();
    }

    /// <summary>
    ///     Return an object to this pool.
    /// </summary>
    /// <param name="obj">The object to return.</param>
    public void Return(T obj)
    {
        objects.Add(obj);
    }

    /// <summary>
    ///     Clear the pool and return all objects.
    /// </summary>
    /// <returns>An array of all objects in the pool.</returns>
    public T[] Clear()
    {
        var arr = new T[objects.Count];

        objects.CopyTo(arr, index: 0);
        objects.Clear();

        return arr;
    }
}

/// <summary>
///     A simple object pool that uses the default constructor.
/// </summary>
/// <typeparam name="T">The type of objects to pool.</typeparam>
public class SimpleObjectPool<T> : ObjectPool<T> where T : class, new()
{
    /// <summary>
    ///     Create a new simple object pool.
    /// </summary>
    public SimpleObjectPool() : base(() => new T()) {}

    /// <summary>
    ///     Get a shared instance of this object pool.
    /// </summary>
    public static SimpleObjectPool<T> Shared { get; } = new();
}
