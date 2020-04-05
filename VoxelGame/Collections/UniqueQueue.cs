// <copyright file="UniqueQueue.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using System.Collections.Generic;

namespace VoxelGame.Collections
{
    /// <summary>
    /// A queue where every entry is unique.
    /// </summary>
    public class UniqueQueue<T> : IEnumerable<T>
    {
        private HashSet<T> hashSet;
        private Queue<T> queue;

        /// <summary>
        /// Initializes a new instance of the class <see cref="UniqueQueue{T}"/>.
        /// </summary>
        public UniqueQueue()
        {
            hashSet = new HashSet<T>();
            queue = new Queue<T>();
        }

        /// <summary>
        /// Gets the number of elements contained in <see cref="UniqueQueue{T}"/>.
        /// </summary>
        public int Count
        {
            get => hashSet.Count;
        }

        /// <summary>
        /// Adds the specified element to the <see cref="UniqueQueue{T}"/>.
        /// </summary>
        /// <param name="item">The element to add to the UniqueQueue.</param>
        /// <returns>true if the element is added to the collection; false if it is already present.</returns>
        public bool Enqueue(T item)
        {
            if (hashSet.Add(item))
            {
                queue.Enqueue(item);

                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Returns the object at the beginning without removing it.
        /// </summary>
        /// <returns>The object at the beginning.</returns>
        public T Peek()
        {
            return queue.Peek();
        }

        /// <summary>
        /// Removes and returns the object at the beginning of the <see cref="UniqueQueue{T}"/>.
        /// </summary>
        /// <returns>The object that is removed and returned.</returns>
        public T Dequeue()
        {
            T item = queue.Dequeue();
            hashSet.Remove(item);

            return item;
        }

        /// <summary>
        /// Determines whether a <see cref="UniqueQueue{T}"/> object contains the specified element.
        /// </summary>
        /// <param name="item">The element to locate in the <see cref="UniqueQueue{T}"/> object.</param>
        /// <returns>true if the <see cref="UniqueQueue{T}"/> contains the specified element; otherwise, false.</returns>
        public bool Contains(T item)
        {
            return hashSet.Contains(item);
        }

        /// <summary>
        /// Removes all objects from the <see cref="UniqueQueue{T}"/>.
        /// </summary>
        public void Clear()
        {
            hashSet.Clear();
            queue.Clear();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return queue.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return queue.GetEnumerator();
        }
    }
}
