// <copyright file="ReadOnlyListSlot.cs" company="VoxelGame">
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
using System.Collections;
using System.Collections.Generic;

namespace VoxelGame.GUI.Bindings;

/// <summary>
///     A read-only list-valued slot.
/// </summary>
/// <typeparam name="TItem">The item type.</typeparam>
public class ReadOnlyListSlot<TItem> : ICollectionSource<TItem>, IReadOnlyList<TItem>
{
    private readonly List<TItem> items = [];

    private readonly Slot<Int32> count;

    /// <summary>
    ///     Create a new instance of the <see cref="ReadOnlyListSlot{TItem}" /> class.
    /// </summary>
    public ReadOnlyListSlot()
    {
        count = new Slot<Int32>(value: 0, this);
    }

    /// <summary>
    ///     The number of items in the list.
    /// </summary>
    public ReadOnlySlot<Int32> Count => count;

    /// <inheritdoc />
    public event EventHandler<CollectionChangedEventArgs<TItem>>? CollectionChanged;

    /// <inheritdoc />
    public IEnumerator<TItem> GetEnumerator()
    {
        return items.GetEnumerator();
    }

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable) items).GetEnumerator();
    }

    /// <inheritdoc />
    Int32 IReadOnlyCollection<TItem>.Count => items.Count;

    /// <inheritdoc />
    public TItem this[Int32 index]
    {
        get => items[index];

        private protected set
        {
            TItem oldItem = items[index];

            if (Equals(oldItem, value))
                return;

            items[index] = value;

            CollectionChanged?.Invoke(this, CollectionChangedEventArgs<TItem>.Replaced([oldItem], [value], index));
        }
    }

    private protected void Add(TItem item)
    {
        items.Add(item);
        count.SetValue(items.Count);

        CollectionChanged?.Invoke(this, CollectionChangedEventArgs<TItem>.Added([item], items.Count - 1));
    }

    private protected void Clear()
    {
        IReadOnlyList<TItem> oldItems = items.ToArray();

        items.Clear();
        count.SetValue(0);

        CollectionChanged?.Invoke(this, CollectionChangedEventArgs<TItem>.Removed(oldItems, oldIndex: 0));
    }

    /// <summary>
    ///     Check if the list contains the specified item.
    /// </summary>
    /// <param name="item">The item to check for.</param>
    /// <returns>True if the item is in the list, false otherwise.</returns>
    public Boolean Contains(TItem item)
    {
        return items.Contains(item);
    }

    /// <summary>
    ///     Copy the items in the list to an array, starting at the specified index.
    /// </summary>
    /// <param name="array">The array to copy the items to.</param>
    /// <param name="arrayIndex">The index in the array to start copying to.</param>
    public void CopyTo(TItem[] array, Int32 arrayIndex)
    {
        items.CopyTo(array, arrayIndex);
    }

    private protected Boolean Remove(TItem item)
    {
        Int32 index = items.IndexOf(item);

        if (index == -1)
            return false;

        items.RemoveAt(index);
        count.SetValue(items.Count);

        CollectionChanged?.Invoke(this, CollectionChangedEventArgs<TItem>.Removed([item], index));

        return true;
    }

    /// <summary>
    ///     Get the index of the specified item in the list, or -1 if the item is not in the list.
    /// </summary>
    /// <param name="item">The item to get the index of.</param>
    /// <returns>The index of the item in the list, or -1 if the item is not in the list.</returns>
    public Int32 IndexOf(TItem item)
    {
        return items.IndexOf(item);
    }

    private protected void Insert(Int32 index, TItem item)
    {
        items.Insert(index, item);
        count.SetValue(items.Count);

        CollectionChanged?.Invoke(this, CollectionChangedEventArgs<TItem>.Added([item], index));
    }

    private protected void RemoveAt(Int32 index)
    {
        TItem item = items[index];

        items.RemoveAt(index);
        count.SetValue(items.Count);

        CollectionChanged?.Invoke(this, CollectionChangedEventArgs<TItem>.Removed([item], index));
    }

    private protected void Move(Int32 oldIndex, Int32 newIndex)
    {
        if (oldIndex == newIndex)
            return;

        TItem item = items[oldIndex];

        items.RemoveAt(oldIndex);
        items.Insert(newIndex, item);

        CollectionChanged?.Invoke(this, CollectionChangedEventArgs<TItem>.Moved([item], oldIndex, newIndex));
    }

    private protected void Sort(Comparison<TItem> comparison)
    {
        items.Sort(comparison);

        CollectionChanged?.Invoke(this, CollectionChangedEventArgs<TItem>.Reordered());
    }
}
