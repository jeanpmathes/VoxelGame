// <copyright file="ScheduledUpdateManagerTests.cs" company="VoxelGame">
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
using VoxelGame.Toolkit.Utilities.Constants;
using Xunit;

namespace VoxelGame.Core.Tests.Collections;

[TestSubject(typeof(ScheduledUpdateManager<,>))]
[Collection("Logger")]
public class ScheduledUpdateManagerTests
{
    private static Int32 lastId;

    [Fact]
    public void ScheduledUpdateManager_ShouldProcessAddedUpdates()
    {
        UpdateCounter counter = new();
        ScheduledUpdateManager<TestUpdate, Constant32> manager = new(counter);

        TestUpdate update1 = new(id: 1);
        manager.Add(update1, updateOffset: 3);

        TestUpdate update2 = new(id: 2);
        manager.Add(update2, updateOffset: 2);

        TestUpdate update3 = new(id: 3);
        manager.Add(update3, updateOffset: 1);

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
    public void ScheduledUpdateManager_ShouldMoveUpdatesAboveLimitToNextUpdate()
    {
        UpdateCounter counter = new();
        ScheduledUpdateManager<TestUpdate, Constant2> manager = new(counter);

        TestUpdate update1 = new(id: 1);
        manager.Add(update1, updateOffset: 1);

        TestUpdate update2 = new(id: 2);
        manager.Add(update2, updateOffset: 1);

        TestUpdate update3 = new(id: 3);
        manager.Add(update3, updateOffset: 1);

        counter.Increment();
        manager.Process();
        Assert.Equal(expected: 2, lastId);

        counter.Increment();
        manager.Process();
        Assert.Equal(expected: 3, lastId);
    }

    [Fact]
    public void ScheduledUpdateManager_ShouldPreserveStateAfterSerialization()
    {
        UpdateCounter counter = new();
        ScheduledUpdateManager<TestUpdate, Constant32> manager = new(counter);

        TestUpdate update1 = new(id: 1);
        manager.Add(update1, updateOffset: 3);

        TestUpdate update2 = new(id: 2);
        manager.Add(update2, updateOffset: 2);

        TestUpdate update3 = new(id: 3);
        manager.Add(update3, updateOffset: 1);

        using MemoryStream data = new();
        using BinarySerializer serializer = new(data, "");
        serializer.SerializeEntity(manager);

        data.Position = 0;
        using BinaryDeserializer deserializer = new(data, "");
        ScheduledUpdateManager<TestUpdate, Constant32> newManager = new(counter);
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

    [Fact]
    public void ScheduledUpdateManager_ShouldSubtractOffsetOnNormalization()
    {
        UpdateCounter counter = new();
        ScheduledUpdateManager<TestUpdate, Constant32> manager = new(counter);

        counter.Increment();
        counter.Increment();
        counter.Increment();

        TestUpdate update1 = new(id: 1);
        manager.Add(update1, updateOffset: 1);

        manager.Normalize();
        counter.Reset();

        counter.Increment();
        manager.Process();

        Assert.Equal(expected: 1, lastId);
    }

    // ReSharper disable once ClassNeverInstantiated.Local
    private record Constant32 : IConstantInt32
    {
        public static Int32 Value => 32;
    }

    // ReSharper disable once ClassNeverInstantiated.Local
    private record Constant2 : IConstantInt32
    {
        public static Int32 Value => 2;
    }

    private class TestUpdate(Int32 id) : IUpdateable
    {
        private Int32 id = id;

        public TestUpdate() : this(id: -1) {}

        public void Update(World world)
        {
            lastId = id;
        }

        public void Serialize(Serializer serializer)
        {
            serializer.Serialize(ref id);
        }
    }
}
