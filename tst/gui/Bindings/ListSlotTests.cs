// <copyright file="ListSlotTests.cs" company="VoxelGame">
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
using JetBrains.Annotations;
using VoxelGame.GUI.Bindings;
using VoxelGame.GUI.Tests.Utilities;
using Xunit;

namespace VoxelGame.GUI.Tests.Bindings;

[TestSubject(typeof(ListSlot<>))]
public class ListSlotTests
{
    [Fact]
    public void ListSlot_New_ShouldBeEmptyAndMutable()
    {
        ListSlot<String> slot = [];
        IList<String> list = slot;

        Assert.Empty(slot);
        AssertCount(slot, expected: 0);
        Assert.False(list.IsReadOnly);
    }

    [Fact]
    public void ListSlot_Add_ShouldAppendItemAndReportAddition()
    {
        ListSlot<String> slot = ["alpha"];
        EventObserver observer = EventObserver.Observe(slot);

        slot.Add("beta");

        Assert.Equal<String>(["alpha", "beta"], slot);
        AssertCount(slot, expected: 2);
        CollectionChangedEventArgs<String> args = observer.AssertObservation<CollectionChangedEventArgs<String>>(slot);
        AssertChange(args, CollectionChangeAction.Add, ["beta"], [], oldIndex: -1, newIndex: 1);
    }

    [Fact]
    public void ListSlot_Insert_ShouldPlaceItemAtRequestedIndexAndReportAddition()
    {
        ListSlot<String> slot = ["alpha", "gamma"];
        EventObserver observer = EventObserver.Observe(slot);

        slot.Insert(index: 1, "beta");

        Assert.Equal<String>(["alpha", "beta", "gamma"], slot);
        AssertCount(slot, expected: 3);
        CollectionChangedEventArgs<String> args = observer.AssertObservation<CollectionChangedEventArgs<String>>(slot);
        AssertChange(args, CollectionChangeAction.Add, ["beta"], [], oldIndex: -1, newIndex: 1);
    }

    [Fact]
    public void ListSlot_Remove_ShouldRemoveFirstMatchingItemAndReportRemoval()
    {
        ListSlot<String> slot = ["alpha", "beta", "alpha"];
        EventObserver observer = EventObserver.Observe(slot);

        Boolean wasRemoved = slot.Remove("alpha");

        Assert.True(wasRemoved);
        Assert.Equal<String>(["beta", "alpha"], slot);
        AssertCount(slot, expected: 2);
        CollectionChangedEventArgs<String> args = observer.AssertObservation<CollectionChangedEventArgs<String>>(slot);
        AssertChange(args, CollectionChangeAction.Remove, [], ["alpha"], oldIndex: 0, newIndex: -1);
    }

    [Fact]
    public void ListSlot_Remove_WithMissingItem_ShouldNotChangeListOrRaiseCollectionChanged()
    {
        ListSlot<String> slot = ["alpha", "beta"];
        EventObserver observer = EventObserver.Observe(slot);

        Boolean wasRemoved = slot.Remove("missing");

        Assert.False(wasRemoved);
        Assert.Equal<String>(["alpha", "beta"], slot);
        AssertCount(slot, expected: 2);
        Assert.Equal(expected: 0, observer.InvocationCount);
    }

    [Fact]
    public void ListSlot_RemoveAt_ShouldRemoveItemAtRequestedIndexAndReportRemoval()
    {
        ListSlot<String> slot = ["alpha", "beta", "gamma"];
        EventObserver observer = EventObserver.Observe(slot);

        slot.RemoveAt(index: 1);

        Assert.Equal<String>(["alpha", "gamma"], slot);
        AssertCount(slot, expected: 2);
        CollectionChangedEventArgs<String> args = observer.AssertObservation<CollectionChangedEventArgs<String>>(slot);
        AssertChange(args, CollectionChangeAction.Remove, [], ["beta"], oldIndex: 1, newIndex: -1);
    }

    [Fact]
    public void ListSlot_Clear_ShouldRemoveAllItemsAndReportRemoval()
    {
        ListSlot<String> slot = ["alpha", "beta", "gamma"];
        EventObserver observer = EventObserver.Observe(slot);

        slot.Clear();

        Assert.Empty(slot);
        AssertCount(slot, expected: 0);
        CollectionChangedEventArgs<String> args = observer.AssertObservation<CollectionChangedEventArgs<String>>(slot);
        AssertChange(args, CollectionChangeAction.Remove, [], ["alpha", "beta", "gamma"], oldIndex: 0, newIndex: -1);
    }

    [Fact]
    public void ListSlot_Clear_WhenEmpty_ShouldNotRaiseCollectionChanged()
    {
        ListSlot<String> slot = [];
        EventObserver observer = EventObserver.Observe(slot);

        slot.Clear();

        Assert.Empty(slot);
        AssertCount(slot, expected: 0);
        Assert.Equal(expected: 0, observer.InvocationCount);
    }

    [Fact]
    public void ListSlot_SetItem_ShouldReplaceItemAndReportOldAndNewItems()
    {
        ListSlot<String> slot = ["alpha", "beta"];
        EventObserver observer = EventObserver.Observe(slot);

        slot[1] = "gamma";

        Assert.Equal<String>(["alpha", "gamma"], slot);
        AssertCount(slot, expected: 2);
        CollectionChangedEventArgs<String> args = observer.AssertObservation<CollectionChangedEventArgs<String>>(slot);
        AssertChange(args, CollectionChangeAction.Replace, ["gamma"], ["beta"], oldIndex: 1, newIndex: 1);
    }

    [Fact]
    public void ListSlot_SetItem_WithEqualItem_ShouldNotRaiseCollectionChanged()
    {
        ListSlot<String> slot = ["alpha", "beta"];
        EventObserver observer = EventObserver.Observe(slot);
        String equalItem = String.Concat("be", "ta");

        slot[1] = equalItem;

        Assert.Equal<String>(["alpha", "beta"], slot);
        AssertCount(slot, expected: 2);
        Assert.Equal(expected: 0, observer.InvocationCount);
    }

    [Fact]
    public void ListSlot_Move_ShouldRelocateItemAndReportMove()
    {
        ListSlot<String> slot = ["alpha", "beta", "gamma", "delta"];
        EventObserver observer = EventObserver.Observe(slot);

        slot.Move(oldIndex: 1, newIndex: 3);

        Assert.Equal<String>(["alpha", "gamma", "delta", "beta"], slot);
        AssertCount(slot, expected: 4);
        CollectionChangedEventArgs<String> args = observer.AssertObservation<CollectionChangedEventArgs<String>>(slot);
        AssertChange(args, CollectionChangeAction.Move, ["beta"], ["beta"], oldIndex: 1, newIndex: 3);
    }

    [Fact]
    public void ListSlot_Move_ToSameIndex_ShouldNotRaiseCollectionChanged()
    {
        ListSlot<String> slot = ["alpha", "beta"];
        EventObserver observer = EventObserver.Observe(slot);

        slot.Move(oldIndex: 1, newIndex: 1);

        Assert.Equal<String>(["alpha", "beta"], slot);
        AssertCount(slot, expected: 2);
        Assert.Equal(expected: 0, observer.InvocationCount);
    }

    [Fact]
    public void ListSlot_Sort_ShouldOrderItemsAndReportReorder()
    {
        ListSlot<String> slot = ["charlie", "alpha", "bravo"];
        EventObserver observer = EventObserver.Observe(slot);

        slot.Sort(StringComparer.Ordinal.Compare);

        Assert.Equal<String>(["alpha", "bravo", "charlie"], slot);
        AssertCount(slot, expected: 3);
        CollectionChangedEventArgs<String> args = observer.AssertObservation<CollectionChangedEventArgs<String>>(slot);
        AssertChange(args, CollectionChangeAction.Reorder, [], [], oldIndex: -1, newIndex: -1);
    }

    private static void AssertCount(ListSlot<String> slot, Int32 expected)
    {
        Assert.Equal(expected, slot.Count.GetValue());
        Assert.Equal(expected, ((ICollection<String>) slot).Count);
    }

    private static void AssertChange(
        CollectionChangedEventArgs<String> args,
        CollectionChangeAction expectedAction,
        IReadOnlyList<String> expectedNewItems,
        IReadOnlyList<String> expectedOldItems,
        Int32 oldIndex,
        Int32 newIndex)
    {
        Assert.Equal(expectedAction, args.Action);
        Assert.Equal(expectedNewItems, args.NewItems);
        Assert.Equal(expectedOldItems, args.OldItems);
        Assert.Equal(oldIndex, args.OldIndex);
        Assert.Equal(newIndex, args.NewIndex);
    }
}
