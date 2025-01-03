// <copyright file="NativeAllocatorTests.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using JetBrains.Annotations;
using VoxelGame.Toolkit.Memory;
using Xunit;

namespace VoxelGame.Toolkit.Tests.Memory;

[TestSubject(typeof(NativeAllocator))]
public class NativeAllocatorTests
{
    [Fact]
    public void AllocationShouldBeAccessibleMemory()
    {
        using NativeAllocator allocator = new();

        NativeAllocation<Int32> allocation = allocator.Allocate<Int32>(count: 1);
        NativeSegment<Int32> segment = allocation.Segment;

        segment[index: 0] = 42;

        Assert.Equal(expected: 1, segment.Count);
        Assert.Equal(expected: 42, segment[index: 0]);

        allocator.Deallocate(allocation);
    }
}
