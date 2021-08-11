// <copyright file="PooledList.cs" company="VoxelGame">
//     Based on the implementation of System.Collections.Generic.List<T>
// </copyright>
// <author>pershingthesecond</author>
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;

namespace VoxelGame.Core.Collections
{
    [DebuggerDisplay("Count = {Count}")]
    public class PooledList<T>
    {
        private readonly ArrayPool<T> arrayPool;

        private T[] items;
        private int size;

        /// <summary>
        /// Initializes a new instance of the <see cref="PooledList{T}"/> class that is empty and has the default initial capacity.
        /// </summary>
        public PooledList()
        {
            arrayPool = ArrayPool<T>.Shared;
            items = Array.Empty<T>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PooledList{T}"/> class that is empty and has at least the specified initial capacity.
        /// </summary>
        /// <param name="capacity">The minimum number of elements that the new list can initially store. The</param>
        public PooledList(int capacity)
        {
            arrayPool = ArrayPool<T>.Shared;

            if (capacity < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(capacity), $"The value '{capacity}' is negative, which is not allowed.");
            }

            if (capacity == 0)
            {
                items = Array.Empty<T>();
            }
            else
            {
                items = arrayPool.Rent(capacity);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PooledList{T}"/> class that is empty, has at least the specified initial capacity and uses a specified <see cref="ArrayPool{T}"/>
        /// </summary>
        /// <param name="capacity">The minimum number of elements that the new list can initially store. The</param>
        /// <param name="arrayPool">The <see cref="ArrayPool{T}"/> to use.</param>
        public PooledList(int capacity, ArrayPool<T> arrayPool) : this(capacity)
        {
            this.arrayPool = arrayPool;
        }

        /// <summary>
        /// Gets or sets the minimum number of elements the internal data structure can hold without resizing.
        /// </summary>
        public int Capacity
        {
            get { return items.Length; }
            set
            {
                if (value < size)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), $"Value '{value}' is smaller than size '{size}'.");
                }

                if (value != items.Length)
                {
                    if (value > 0)
                    {
                        T[] newItems = arrayPool.Rent(value);

                        if (size > 0)
                        {
                            Array.Copy(items, 0, newItems, 0, size);
                        }

                        arrayPool.Return(items);

                        items = newItems;
                    }
                    else
                    {
                        if (items.Length > 0)
                        {
                            arrayPool.Return(items);
                        }

                        items = Array.Empty<T>();
                    }
                }
            }
        }

        /// <summary>
        /// Gets the number of elements contained in the <see cref="PooledList{T}"/>.
        /// </summary>
        public int Count
        {
            get { return size; }
        }

        /// <summary>
        /// Gets or sets the element at the specified index.
        /// </summary>
        public T this[int index]
        {
            get
            {
                if ((uint) index >= (uint) size)
                {
                    throw new ArgumentOutOfRangeException(nameof(index), $"The index has to be smaller then the size '{size}'.");
                }

                return items[index];
            }

            set
            {
                if ((uint) index >= (uint) size)
                {
                    throw new ArgumentOutOfRangeException(nameof(index), $"The index has to be smaller then the size '{size}'.");
                }

                items[index] = value;
            }
        }

        /// <summary>
        /// Adds an object to the end of the <see cref="PooledList{T}"/>.
        /// </summary>
        /// <param name="item">The object to be added to the end of the <see cref="PooledList{T}"/>. The value can be null for reference types.</param>
        public void Add(T item)
        {
            if (size == items.Length)
            {
                EnsureCapacity(size + 1);
            }

            items[size++] = item;
        }

        /// <summary>
        /// Adds the elements of the specified collection to the end of the <see cref="PooledList{T}"/>.
        /// </summary>
        /// <param name="collection">The collection whose elements should be added to the end of the <see cref="PooledList{T}"/>. The collection itself cannot be <c>null</c>, but it can contain elements that are <c>null</c>, if type <c>T</c> is a reference type.</param>
        public void AddRange(ICollection<T> collection)
        {
            if (collection == null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            int count = collection.Count;

            if (count > 0)
            {
                EnsureCapacity(size + count);

                collection.CopyTo(items, size);

                size += count;
            }
        }

        /// <summary>
        /// Adds the elements of the specified array to the end of the <see cref="PooledList{T}"/>.
        /// </summary>
        /// <param name="array">The array whose elements should be added to the end of the <see cref="PooledList{T}"/>. The array itself cannot be <c>null</c>, but it can contain elements that are <c>null</c>, if type <c>T</c> is a reference type.</param>
        /// <param name="add">The amount of elements to add.</param>
        public void AddRange(T[] array, int count)
        {
            if (array == null)
            {
                throw new ArgumentNullException(nameof(array));
            }

            if (count > 0)
            {
                EnsureCapacity(size + count);

                Array.Copy(array, 0, items, size, count);

                size += count;
            }
        }

        /// <summary>
        /// Adds the elements of another <see cref="PooledList{T}"/> to the end of this <see cref="PooledList{T}"/>.
        /// </summary>
        /// <param name="pooledList">The <see cref="PooledList{T}"/> whose elements should be added to the end of the <see cref="PooledList{T}"/>. It is not allowed to be null or equal to the list it should be added to.</param>
        public void AddRange(PooledList<T> pooledList)
        {
            if (pooledList == null)
            {
                throw new ArgumentNullException(nameof(pooledList));
            }

            if (this == pooledList)
            {
                throw new ArgumentException($"Adding '{this}' to itself is not allowed.", nameof(pooledList));
            }

            int count = pooledList.Count;

            if (count > 0)
            {
                EnsureCapacity(size + count);

                Array.Copy(pooledList.items, 0, items, size, count);

                size += count;
            }
        }

        /// <summary>
        /// Removes the element at the specified index of the <see cref="PooledList{T}"/>.
        /// </summary>
        public void RemoveAt(int index)
        {
            if ((uint) index >= (uint) size)
            {
                throw new ArgumentOutOfRangeException($"The index '{index}' is not allowed to be larger then the size of the list.");
            }

            size--;

            if (index < size)
            {
                Array.Copy(items, index + 1, items, index, size - index);
            }

            items[size] = default!;
        }

        private void EnsureCapacity(int min)
        {
            if (items.Length < min)
            {
                int newCapacity = (items.Length == 0) ? 4 : items.Length * 2;

                if ((uint) newCapacity > int.MaxValue)
                {
                    newCapacity = int.MaxValue;
                }

                if (newCapacity < min)
                {
                    newCapacity = min;
                }

                Capacity = newCapacity;
            }
        }

        /// <summary>
        /// Gives access to the internal array of this <see cref="PooledList{T}"/>. It has to be returned to the pool.
        /// </summary>
        public T[] ExposeArray()
        {
            return items;
        }

        /// <summary>
        /// Return the internal array of this <see cref="PooledList{T}"/> to the pool. After calling this method, the exposed array should no longer be used.
        /// </summary>
        public void ReturnToPool()
        {
            arrayPool.Return(items);
            items = Array.Empty<T>();

            size = 0;
        }
    }
}