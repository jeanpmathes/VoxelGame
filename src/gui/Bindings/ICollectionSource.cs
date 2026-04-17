// <copyright file="ICollectionSource.cs" company="VoxelGame">
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
using System.Collections.Generic;

namespace VoxelGame.GUI.Bindings;

/// <summary>
///     Interface for a collection-valued value source which allows to observe changes to the collection.
/// </summary>
public interface ICollectionSource<TItem> : IReadOnlyCollection<TItem>
{
    /// <summary>
    ///     Event raised when the collection changes. The event args will contain information about the change.
    /// </summary>
    public event EventHandler<CollectionChangedEventArgs<TItem>>? CollectionChanged;
}

/// <summary>
///     Actions that can occur that cause a collection to change.
/// </summary>
public enum CollectionChangeAction
{
    /// <summary>
    ///     An item or items were added to the collection.
    ///     The <see cref="CollectionChangedEventArgs{TItem}.NewItems" /> property will contain the new items that were added,
    ///     and the <see cref="CollectionChangedEventArgs{TItem}.OldItems" /> property will be empty.
    ///     The <see cref="CollectionChangedEventArgs{TItem}.NewIndex" /> property will indicate the index at which the new
    ///     item(s) were added, and the <see cref="CollectionChangedEventArgs{TItem}.OldIndex" /> property will be -1.
    /// </summary>
    Add,

    /// <summary>
    ///     An item or items were removed from the collection.
    ///     The <see cref="CollectionChangedEventArgs{TItem}.OldItems" /> property will contain the old items that were
    ///     removed, and the <see cref="CollectionChangedEventArgs{TItem}.NewItems" /> property will be empty.
    ///     The <see cref="CollectionChangedEventArgs{TItem}.OldIndex" /> property will indicate the index from which the old
    ///     item(s) were removed, and the <see cref="CollectionChangedEventArgs{TItem}.NewIndex" /> property will be -1.
    /// </summary>
    Remove,

    /// <summary>
    ///     An item or items in the collection were replaced with new item(s).
    ///     The <see cref="CollectionChangedEventArgs{TItem}.NewItems" /> property will contain the new items that were added,
    ///     and the <see cref="CollectionChangedEventArgs{TItem}.OldItems" /> property will contain the old items that were
    ///     removed.
    ///     The <see cref="CollectionChangedEventArgs{TItem}.NewIndex" /> property will indicate the index at which the new
    ///     item(s) were added, and the <see cref="CollectionChangedEventArgs{TItem}.OldIndex" /> property will have the same
    ///     value.
    /// </summary>
    Replace,

    /// <summary>
    ///     An item or items in the collection were moved from one index to another.
    ///     The <see cref="CollectionChangedEventArgs{TItem}.NewItems" /> property will contain the item(s) that were moved,
    ///     and the <see cref="CollectionChangedEventArgs{TItem}.OldItems" /> property will contain the same item(s).
    ///     The <see cref="CollectionChangedEventArgs{TItem}.NewIndex" /> property will indicate the new index of the moved
    ///     item(s), and the <see cref="CollectionChangedEventArgs{TItem}.OldIndex" /> property will indicate the old index of
    ///     the moved item(s).
    /// </summary>
    Move,

    /// <summary>
    ///     The order of items in the collection was changed, but no items were added, removed, or replaced - for example, the
    ///     collection was sorted.
    ///     The <see cref="CollectionChangedEventArgs{TItem}.NewItems" /> and
    ///     <see cref="CollectionChangedEventArgs{TItem}.OldItems" /> properties will both be empty, and the
    ///     <see cref="CollectionChangedEventArgs{TItem}.NewIndex" /> and
    ///     <see cref="CollectionChangedEventArgs{TItem}.OldIndex" /> properties will both be -1.
    /// </summary>
    Reorder
}

/// <summary>
///     Event args for the <see cref="ICollectionSource{TItem}.CollectionChanged" /> event, containing information about
///     the change that occurred to the collection.
/// </summary>
/// <typeparam name="TItem">The type of items in the collection.</typeparam>
public sealed class CollectionChangedEventArgs<TItem> : EventArgs
{
    /// <summary>
    ///     The type of change that occurred to the collection. This will indicate whether items were added, removed, replaced,
    ///     or if the collection was reset. The <see cref="NewItems" /> and <see cref="OldItems" /> properties will contain the
    ///     relevant items based on the action.
    /// </summary>
    public CollectionChangeAction Action { get; private init; }

    /// <summary>
    ///     The new items that were added to the collection or that replaced old items in the collection. The contents of this
    ///     property will depend on the value of the <see cref="Action" /> property.
    /// </summary>
    public IReadOnlyList<TItem> NewItems { get; private init; } = [];

    /// <summary>
    ///     The old items that were removed from the collection or that were replaced by new items in the collection. The
    ///     contents of this property will depend on the value of the <see cref="Action" /> property.
    /// </summary>
    public IReadOnlyList<TItem> OldItems { get; private init; } = [];

    /// <summary>
    ///     The new index of the item(s), meaning depending on the value of the <see cref="Action" /> property.
    /// </summary>
    public Int32 NewIndex { get; private init; } = -1;

    /// <summary>
    ///     The old index of the item(s), meaning depending on the value of the <see cref="Action" /> property.
    /// </summary>
    public Int32 OldIndex { get; private init; } = -1;

    /// <summary>
    ///     Create a new <see cref="CollectionChangedEventArgs{TItem}" /> instance for the
    ///     <see cref="CollectionChangeAction.Add" /> action.
    /// </summary>
    public static CollectionChangedEventArgs<TItem> Added(IReadOnlyList<TItem> newItems, Int32 newIndex)
    {
        return new CollectionChangedEventArgs<TItem>
        {
            Action = CollectionChangeAction.Add,
            NewItems = newItems,
            OldItems = [],
            NewIndex = newIndex,
            OldIndex = -1
        };
    }

    /// <summary>
    ///     Create a new <see cref="CollectionChangedEventArgs{TItem}" /> instance for the
    ///     <see cref="CollectionChangeAction.Remove" /> action.
    /// </summary>
    public static CollectionChangedEventArgs<TItem> Removed(IReadOnlyList<TItem> oldItems, Int32 oldIndex)
    {
        return new CollectionChangedEventArgs<TItem>
        {
            Action = CollectionChangeAction.Remove,
            NewItems = [],
            OldItems = oldItems,
            NewIndex = -1,
            OldIndex = oldIndex
        };
    }

    /// <summary>
    ///     Create a new <see cref="CollectionChangedEventArgs{TItem}" /> instance for the
    ///     <see cref="CollectionChangeAction.Replace" /> action.
    /// </summary>
    public static CollectionChangedEventArgs<TItem> Replaced(IReadOnlyList<TItem> newItems, IReadOnlyList<TItem> oldItems, Int32 index)
    {
        return new CollectionChangedEventArgs<TItem>
        {
            Action = CollectionChangeAction.Replace,
            NewItems = newItems,
            OldItems = oldItems,
            NewIndex = index,
            OldIndex = index
        };
    }

    /// <summary>
    ///     Create a new <see cref="CollectionChangedEventArgs{TItem}" /> instance for the
    ///     <see cref="CollectionChangeAction.Move" /> action.
    /// </summary>
    public static CollectionChangedEventArgs<TItem> Moved(IReadOnlyList<TItem> items, Int32 oldIndex, Int32 newIndex)
    {
        return new CollectionChangedEventArgs<TItem>
        {
            Action = CollectionChangeAction.Move,
            NewItems = items,
            OldItems = items,
            NewIndex = newIndex,
            OldIndex = oldIndex
        };
    }

    /// <summary>
    ///     Create a new <see cref="CollectionChangedEventArgs{TItem}" /> instance for the
    ///     <see cref="CollectionChangeAction.Reorder" /> action.
    /// </summary>
    public static CollectionChangedEventArgs<TItem> Reordered()
    {
        return new CollectionChangedEventArgs<TItem>
        {
            Action = CollectionChangeAction.Reorder,
            NewItems = [],
            OldItems = [],
            NewIndex = -1,
            OldIndex = -1
        };
    }
}
