// <copyright file="ScheduledTickManagerTests.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.IO;
using JetBrains.Annotations;
using VoxelGame.Core.Collections;
using VoxelGame.Core.Logic;
using VoxelGame.Core.Serialization;
using VoxelGame.Core.Updates;
using Xunit;

namespace VoxelGame.Core.Tests.Collections;

[TestSubject(typeof(ScheduledTickManager<>))]
[Collection("Logger")]
public class ScheduledTickManagerTests
{
    private static Int32 lastId;

    [Fact]
    public void TestBasicFunctionality()
    {
        UpdateCounter counter = new();
        ScheduledTickManager<TestTick> manager = new(maxTicksPerUpdate: 32, counter);

        TestTick tick1 = new(id: 1);
        manager.Add(tick1, tickOffset: 3);

        TestTick tick2 = new(id: 2);
        manager.Add(tick2, tickOffset: 2);

        TestTick tick3 = new(id: 3);
        manager.Add(tick3, tickOffset: 1);

        counter.Increment();
        manager.Process();
        Assert.Equal(expected: 3, lastId);

        counter.Increment();
        manager.Process();
        Assert.Equal(expected: 2, lastId);

        counter.Increment();
        manager.Process();
        Assert.Equal(expected: 1, lastId);
    }

    [Fact]
    public void TestMaxTicks()
    {
        UpdateCounter counter = new();
        ScheduledTickManager<TestTick> manager = new(maxTicksPerUpdate: 2, counter);

        TestTick tick1 = new(id: 1);
        manager.Add(tick1, tickOffset: 1);

        TestTick tick2 = new(id: 2);
        manager.Add(tick2, tickOffset: 1);

        TestTick tick3 = new(id: 3);
        manager.Add(tick3, tickOffset: 1);

        counter.Increment();
        manager.Process();
        Assert.Equal(expected: 2, lastId);

        counter.Increment();
        manager.Process();
        Assert.Equal(expected: 3, lastId);
    }

    [Fact]
    public void TestSerialization()
    {
        UpdateCounter counter = new();
        ScheduledTickManager<TestTick> manager = new(maxTicksPerUpdate: 32, counter);

        TestTick tick1 = new(id: 1);
        manager.Add(tick1, tickOffset: 3);

        TestTick tick2 = new(id: 2);
        manager.Add(tick2, tickOffset: 2);

        TestTick tick3 = new(id: 3);
        manager.Add(tick3, tickOffset: 1);

        using MemoryStream data = new();
        using BinarySerializer serializer = new(data, "");
        serializer.SerializeEntity(manager);

        data.Position = 0;
        using BinaryDeserializer deserializer = new(data, "");
        ScheduledTickManager<TestTick> newManager = new(maxTicksPerUpdate: 32, counter);
        deserializer.SerializeEntity(newManager);

        counter.Increment();
        newManager.Process();
        Assert.Equal(expected: 3, lastId);

        counter.Increment();
        newManager.Process();
        Assert.Equal(expected: 2, lastId);

        counter.Increment();
        newManager.Process();
        Assert.Equal(expected: 1, lastId);
    }

    private class TestTick(Int32 id) : ITickable
    {
        public TestTick() : this(id: -1) {}

        public void Tick(World world)
        {
            lastId = id;
        }

        public void Serialize(Serializer serializer)
        {
            serializer.Serialize(ref id);
        }
    }
}
