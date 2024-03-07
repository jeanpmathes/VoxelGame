// <copyright file="PooledListTest.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Core.Collections;
using Xunit;

namespace VoxelGame.Core.Tests.Collections;

public class PooledListTest
{
    [Fact]
    public void TestAddAndAccess()
    {
        using PooledList<int> list = new();

        list.Add(item: 1);
        list.Add(item: 2);
        list.Add(item: 3);

        Assert.Equal(expected: 1, list[index: 0]);
        Assert.Equal(expected: 2, list[index: 1]);
        Assert.Equal(expected: 3, list[index: 2]);

        Assert.Equal(expected: 3, list.Count);
    }

    [Fact]
    public void TestAddRange()
    {
        using PooledList<int> list = new();

        list.AddRange(collection: new[] {1, 2, 3});

        Assert.Equal(expected: 1, list[index: 0]);
        Assert.Equal(expected: 2, list[index: 1]);
        Assert.Equal(expected: 3, list[index: 2]);
    }

    [Fact]
    public void TestClear()
    {
        using PooledList<int> list = new();

        list.AddRange(collection: new[] {1, 2, 3});
        list.Clear();

        Assert.Empty(list);
    }

    [Fact]
    public void TestContainsAndRemove()
    {
        using PooledList<int> list = new();

        list.AddRange(collection: new[] {1, 2, 3});

        Assert.True(list.Contains(item: 1));
        Assert.True(list.Contains(item: 2));
        Assert.True(list.Contains(item: 3));
        Assert.False(list.Contains(item: 4));

        int index = list.IndexOf(item: 2);
        list.RemoveAt(index);

        Assert.False(list.Contains(item: 2));
        Assert.Equal(expected: 3, list[index: 1]);

        list.Remove(item: 3);

        Assert.False(list.Contains(item: 3));
        Assert.Single(list);
    }
}
