// <copyright file="ObjectPool.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System.Collections.Concurrent;

namespace VoxelGame.Core.Collections;

/// <summary>
///     A pool of objects.
/// </summary>
/// <typeparam name="T">The type of objects to pool.</typeparam>
public class ObjectPool<T> where T : class, new()
{
    private readonly ConcurrentBag<T> objects = new();

#pragma warning disable CA1000 // Do not declare static members on generic types
    /// <summary>
    ///     Get a global shared instance of an object pool for a given type.
    /// </summary>
    public static ObjectPool<T> Shared { get; } = new();
#pragma warning restore CA1000 // Do not declare static members on generic types

    /// <summary>
    ///     Get an object from the pool.
    /// </summary>
    /// <returns>An object, may not be cleaned.</returns>
    public T Get()
    {
        return objects.TryTake(out T? instance) ? instance : new T();
    }

    /// <summary>
    ///     Return an object to this pool.
    /// </summary>
    /// <param name="obj">The object to return.</param>
    public void Return(T obj)
    {
        objects.Add(obj);
    }
}
