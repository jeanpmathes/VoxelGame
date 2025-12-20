// <copyright file="RequestAlgorithmTests.cs" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2025 Jean Patrick Mathes
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
using System.Linq;
using JetBrains.Annotations;
using OpenTK.Mathematics;
using VoxelGame.Core.Logic.Chunks;
using VoxelGame.Core.Tests.Actor;
using VoxelGame.Core.Utilities;
using Xunit;

namespace VoxelGame.Core.Tests.Logic.Chunks;

[TestSubject(typeof(RequestAlgorithm))]
public class RequestAlgorithmTests
{
    private static IEnumerable<ChunkPosition> GetPositionsInManhattanRange(Int32 range)
    {
        for (Int32 x = -range; x <= range; x++)
        for (Int32 y = -range; y <= range; y++)
        for (Int32 z = -range; z <= range; z++)
            if (MathTools.Manhattan((x, y, z), Vector3i.Zero) <= range)
                yield return new ChunkPosition(x, y, z);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    public void RequestAlgorithm_ShouldCalculateCorrectPositionsInManhattanRange(Int32 range)
    {
        ChunkPosition center = new(x: 0, y: 0, z: 0);

        IEnumerable<ChunkPosition> naive = GetPositionsInManhattanRange(range).ToHashSet();
        IEnumerable<ChunkPosition> optimized = RequestAlgorithm.GetPositionsInManhattanRange(center, range).ToHashSet();

        Assert.Equal(naive, optimized);
    }

    private static IEnumerable<ChunkPosition> GetAffected(ChunkPosition center)
    {
        return GetPositionsInManhattanRange(RequestLevel.Range)
            .Select(position => center.Offset(position.X, position.Y, position.Z));
    }

    [Fact]
    public void RequestAlgorithm_ShouldLoadAllChunksAroundSingleRequest()
    {
        MockActor actor = new();
        ChunkPosition center = new(x: 0, y: 0, z: 0);

        MockChunkSystem system = new();
        RequestAlgorithm algorithm = new(system.GetOptional, system.GetRequired);

        HashSet<Request> pending = [new(center, actor)];
        HashSet<Request> released = [];

        algorithm.Process(pending, released);

        Assert.Equal(RequestLevel.Highest, system.GetLevel(center));
        Assert.Equal(GetAffected(center).Count(), system.GetNumberOfChunksToLoad());
    }

    [Fact]
    public void RequestAlgorithm_ShouldUnloadAllChunksWhenSingleRequestIsRemoved()
    {
        MockActor actor = new();
        ChunkPosition center = new(x: 0, y: 0, z: 0);

        MockChunkSystem system = new();
        RequestAlgorithm algorithm = new(system.GetOptional, system.GetRequired);

        HashSet<Request> pending = [new(center, actor)];
        HashSet<Request> released = [];

        algorithm.Process(pending, released);

        Assert.Equal(RequestLevel.Highest, system.GetLevel(center));
        Assert.Equal(GetAffected(center).Count(), system.GetNumberOfChunksToLoad());

        released = pending;
        pending = [];

        algorithm.Process(pending, released);

        Assert.Equal(RequestLevel.Lowest, system.GetLevel(center));
        Assert.Equal(expected: 0, system.GetNumberOfChunksToLoad());
    }

    [Fact]
    public void RequestAlgorithm_ShouldNotChangeChunkStateWhenOneOfTwoRequestsAtPositionIsRemoved()
    {
        MockActor actor1 = new();
        MockActor actor2 = new();
        ChunkPosition center = new(x: 0, y: 0, z: 0);

        MockChunkSystem system = new();
        RequestAlgorithm algorithm = new(system.GetOptional, system.GetRequired);

        HashSet<Request> pending = [new(center, actor1), new(center, actor2)];
        HashSet<Request> released = [];

        algorithm.Process(pending, released);

        Assert.Equal(RequestLevel.Highest, system.GetLevel(center));
        Assert.Equal(GetAffected(center).Count(), system.GetNumberOfChunksToLoad());

        released = pending.Take(count: 1).ToHashSet();
        pending = [];

        algorithm.Process(pending, released);

        Assert.Equal(RequestLevel.Highest, system.GetLevel(center));
        Assert.Equal(GetAffected(center).Count(), system.GetNumberOfChunksToLoad());
    }

    [Fact]
    public void RequestAlgorithm_ShouldUnloadChunksWhenAllRequestsAreRemoved()
    {
        MockActor actor = new();
        ChunkPosition center = new(x: 0, y: 0, z: 0);

        MockChunkSystem system = new();
        RequestAlgorithm algorithm = new(system.GetOptional, system.GetRequired);

        HashSet<Request> pending = [new(center, actor), new(center, actor)];
        HashSet<Request> released = [];

        algorithm.Process(pending, released);

        Assert.Equal(RequestLevel.Highest, system.GetLevel(center));
        Assert.Equal(GetAffected(center).Count(), system.GetNumberOfChunksToLoad());

        released = pending;
        pending = [];

        algorithm.Process(pending, released);

        Assert.Equal(RequestLevel.Lowest, system.GetLevel(center));
        Assert.Equal(expected: 0, system.GetNumberOfChunksToLoad());
    }

    [Fact]
    public void RequestAlgorithm_ShouldKeepChunksLoadedWhenReplacingRequest()
    {
        MockActor actor = new();
        ChunkPosition center = new(x: 0, y: 0, z: 0);

        MockChunkSystem system = new();
        RequestAlgorithm algorithm = new(system.GetOptional, system.GetRequired);

        HashSet<Request> pending = [new(center, actor)];
        HashSet<Request> released = [];

        algorithm.Process(pending, released);

        Assert.Equal(RequestLevel.Highest, system.GetLevel(center));
        Assert.Equal(GetAffected(center).Count(), system.GetNumberOfChunksToLoad());

        released = pending;
        pending = [new Request(center, actor)];

        algorithm.Process(pending, released);

        Assert.Equal(RequestLevel.Highest, system.GetLevel(center));
        Assert.Equal(GetAffected(center).Count(), system.GetNumberOfChunksToLoad());
    }

    [Fact]
    public void RequestAlgorithm_ShouldLoadAllChunksAroundMultipleRequests()
    {
        MockActor actor = new();
        ChunkPosition center = new(x: 0, y: 0, z: 0);
        ChunkPosition other = new(x: 1, y: 0, z: 0);

        MockChunkSystem system = new();
        RequestAlgorithm algorithm = new(system.GetOptional, system.GetRequired);

        HashSet<Request> pending = [new(center, actor)];
        HashSet<Request> released = [];

        algorithm.Process(pending, released);

        Assert.Equal(RequestLevel.Highest, system.GetLevel(center));
        Assert.Equal(GetAffected(center).Count(), system.GetNumberOfChunksToLoad());

        pending = [new Request(other, actor)];
        released = [];

        algorithm.Process(pending, released);

        Assert.Equal(RequestLevel.Highest, system.GetLevel(other));
        Assert.Equal(RequestLevel.Highest, system.GetLevel(center));

        Assert.Equal(GetAffected(center).Union(GetAffected(other)).Count(), system.GetNumberOfChunksToLoad());
    }

    [Fact]
    public void RequestAlgorithm_ShouldNotUnloadAllChunksWhenOneOfManyRequestsIsRemoved()
    {
        MockActor actor = new();
        ChunkPosition center = new(x: 0, y: 0, z: 0);
        ChunkPosition other = new(x: 2, y: 0, z: 0);

        MockChunkSystem system = new();
        RequestAlgorithm algorithm = new(system.GetOptional, system.GetRequired);

        HashSet<Request> pending = [new(center, actor), new(other, actor)];
        HashSet<Request> released = [];

        algorithm.Process(pending, released);

        Assert.Equal(RequestLevel.Highest, system.GetLevel(center));
        Assert.Equal(RequestLevel.Highest, system.GetLevel(other));

        Assert.Equal(GetAffected(center).Union(GetAffected(other)).Count(), system.GetNumberOfChunksToLoad());

        released = pending.Where(request => request.Position == center).ToHashSet();
        pending = [];

        algorithm.Process(pending, released);

        Assert.Equal(RequestLevel.Highest, system.GetLevel(other));
        Assert.NotEqual(RequestLevel.Highest, system.GetLevel(center));

        Assert.Equal(GetAffected(other).Count(), system.GetNumberOfChunksToLoad());
    }

    private sealed class MockChunkSystem
    {
        private readonly Dictionary<ChunkPosition, Requests> chunks = new();

        public Requests? GetOptional(ChunkPosition position)
        {
            return chunks.GetValueOrDefault(position);
        }

        public Requests GetRequired(ChunkPosition position)
        {
            return chunks.GetOrAdd(position, new Requests(chunk: null));
        }

        public RequestLevel GetLevel(ChunkPosition position)
        {
            return chunks.GetValueOrDefault(position)?.Level ?? RequestLevel.Lowest;
        }

        public Int32 GetNumberOfChunksToLoad()
        {
            return chunks.Values.Count(requests => requests.Level.IsLoaded);
        }
    }
}
