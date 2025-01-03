// <copyright file="PooledListTests.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using JetBrains.Annotations;
using VoxelGame.Core.Collections;
using Xunit;

namespace VoxelGame.Core.Tests.Collections;

[TestSubject(typeof(PooledList<>))]
public class PooledListTests
{
    private static readonly Int32[] collection = [1, 2, 3];

    [Fact]
    public void PooledList_ShouldInsertItemAndAllowAccess()
    {
        using PooledList<Int32> list = new();

        list.Add(item: 1);
        list.Add(item: 2);
        list.Add(item: 3);

        Assert.Equal(expected: 1, list[index: 0]);
        Assert.Equal(expected: 2, list[index: 1]);
        Assert.Equal(expected: 3, list[index: 2]);

        Assert.Equal(expected: 3, list.Count);
    }

    [Fact]
    public void PooledList_ShouldInsertRangeOfItems()
    {
        using PooledList<Int32> list = new();

        list.AddRange(collection: collection);

        Assert.Equal(expected: 1, list[index: 0]);
        Assert.Equal(expected: 2, list[index: 1]);
        Assert.Equal(expected: 3, list[index: 2]);
    }

    [Fact]
    public void PooledList_ShouldClearItems()
    {
        using PooledList<Int32> list = new();

        list.AddRange(collection: collection);
        list.Clear();

        Assert.Empty(list);
    }

    [Fact]
    public void PooledList_ShouldContainAddedItemsAndNotContainRemovedItems()
    {
        using PooledList<Int32> list = new();

        list.AddRange(collection: collection);

        Assert.Contains(expected: 1, list);
        Assert.Contains(expected: 2, list);
        Assert.Contains(expected: 3, list);
        Assert.DoesNotContain(expected: 4, list);

        Int32 index = list.IndexOf(item: 2);
        list.RemoveAt(index);

        Assert.DoesNotContain(expected: 2, list);
        Assert.Equal(expected: 3, list[index: 1]);

        list.Remove(item: 3);

        Assert.DoesNotContain(expected: 3, list);
        Assert.Single(list);
    }
}
