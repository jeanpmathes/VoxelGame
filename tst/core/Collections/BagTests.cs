// <copyright file="BagTests.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using VoxelGame.Core.Collections;
using Xunit;
using Int32 = System.Int32;

namespace VoxelGame.Core.Tests.Collections;

[TestSubject(typeof(Bag<>))]
public class BagTests
{
    private const Int32 Gap = -1;

    [Fact]
    public void Bag_Add_ShouldInsertItemAndReturnIndex()
    {
        var bag = new Bag<Int32>(Gap);

        Int32 index = bag.Add(item: 10);

        Assert.Equal(expected: 10, bag[index]);
        Assert.Equal(expected: 1, bag.Count);
    }

    [Fact]
    public void Bag_Add_ShouldUseGapsWhenAvailable()
    {
        var bag = new Bag<Int32>(Gap)
        {
            10,
            20
        };

        bag.RemoveAt(index: 0);

        Int32 index = bag.Add(item: 30);

        Assert.Equal(expected: 0, index);
        Assert.Equal(expected: 30, bag[index: 0]);
        Assert.Equal(expected: 2, bag.Count);
    }

    [Fact]
    public void Bag_Remove_ShouldCreateGapAndReduceCount()
    {
        var bag = new Bag<Int32>(Gap);
        Int32 index = bag.Add(item: 10);

        bag.RemoveAt(index);

        Assert.Equal(Gap, bag[index]);
        Assert.Equal(expected: 0, bag.Count);
    }

    [Fact]
    public void Bag_Clear_ShouldRemoveAllItemsAndGaps()
    {
        var bag = new Bag<Int32>(Gap)
        {
            10,
            20
        };

        bag.RemoveAt(index: 0);

        bag.Clear();

        Assert.Equal(expected: 0, bag.Count);
        Assert.Throws<ArgumentOutOfRangeException>(() => bag[index: 0]);
    }

    [Fact]
    public void Bag_Apply_ShouldRemoveItemsBasedOnCondition()
    {
        var bag = new Bag<Int32>(Gap)
        {
            10,
            20,
            30
        };

        bag.Apply(x => x > 15);

        Assert.Equal(expected: 2, bag.Count);
        Assert.Equal(Gap, bag[index: 0]);
        Assert.Equal(expected: 20, bag[index: 1]);
        Assert.Equal(expected: 30, bag[index: 2]);
    }

    [Fact]
    public void Bag_Enumerator_ShouldIterateWithoutGaps()
    {
        var bag = new Bag<Int32>(Gap)
        {
            10,
            20,
            30,
            40,
            50
        };

        bag.RemoveAt(index: 0);
        bag.RemoveAt(index: 2);
        bag.RemoveAt(index: 4);

        List<Int32> items = bag.ToList();

        Assert.Equal(expected: 2, items.Count);
        Assert.Equal(expected: 20, items[index: 0]);
        Assert.Equal(expected: 40, items[index: 1]);
    }
}
