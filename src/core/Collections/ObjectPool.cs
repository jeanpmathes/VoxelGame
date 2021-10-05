// <copyright file="ObjectPool.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System.Collections.Concurrent;

namespace VoxelGame.Core.Collections
{
    public class ObjectPool<T> where T : class, new()
    {
        private readonly ConcurrentBag<T> objects = new();

#pragma warning disable CA1000 // Do not declare static members on generic types
        public static ObjectPool<T> Shared { get; } = new();
#pragma warning restore CA1000 // Do not declare static members on generic types

        public T Get()
        {
            return objects.TryTake(out T? instance) ? instance : new T();
        }

        public void Return(T obj)
        {
            objects.Add(obj);
        }
    }
}