// <copyright file="RequestAlgorithm.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;

namespace VoxelGame.Core.Logic.Chunks;

/// <summary>
///     The request algorithm that controls the chunk request levels.
/// </summary>
public class RequestAlgorithm
{
    private readonly Func<ChunkPosition, Requests?> getOptional;
    private readonly Func<ChunkPosition, Requests> getRequired;

    private readonly HashSet<ChunkPosition> newlyRequested = [];
    private readonly HashSet<ChunkPosition> noLongerRequested = [];

    private readonly HashSet<Requests> changed = [];

    private readonly HashSet<ChunkPosition> requested = [];

    /// <summary>
    ///     Create a new instance of the request algorithm.
    /// </summary>
    /// <param name="getOptional">Function to get the requests of a chunk, if the chunk is loaded.</param>
    /// <param name="getRequired">Function to get the requests of a chunk, creating the chunk if necessary.</param>
    public RequestAlgorithm(Func<ChunkPosition, Requests?> getOptional, Func<ChunkPosition, Requests> getRequired)
    {
        this.getOptional = getOptional;
        this.getRequired = getRequired;
    }

    /// <summary>
    ///     Process all pending requests and releases.
    /// </summary>
    /// <param name="pendingRequests">All requests that are pending.</param>
    /// <param name="pendingReleases">All requests that are pending to be released.</param>
    public void Process(HashSet<Request> pendingRequests, HashSet<Request> pendingReleases)
    {
        newlyRequested.Clear();
        noLongerRequested.Clear();

        AddPendingRequestsToChunks(pendingRequests);
        RemoveReleasedRequestsFromChunks(pendingReleases);

        RemoveNoLongerRequestedPositions();

        changed.Clear();

        UpdateForReleasedPositions();
        SpreadForNewlyRequested();

        foreach (Requests requests in changed)
            requests.ApplyLevel();
    }

    private void AddPendingRequestsToChunks(HashSet<Request> pendingRequests)
    {
        foreach (Request request in pendingRequests)
        {
            Requests requests = getRequired(request.Position);

            Boolean first = requests.AddRequest(request);

            if (!first) continue;

            newlyRequested.Add(request.Position);
            noLongerRequested.Remove(request.Position);
        }
    }

    private void RemoveReleasedRequestsFromChunks(HashSet<Request> pendingReleases)
    {
        foreach (Request request in pendingReleases)
        {
            Requests? requests = getOptional(request.Position);

            if (requests == null) continue;

            Boolean last = requests.RemoveRequest(request);

            if (!last) continue;

            noLongerRequested.Add(request.Position);
            newlyRequested.Remove(request.Position);
        }
    }

    private void RemoveNoLongerRequestedPositions()
    {
        foreach (ChunkPosition position in noLongerRequested) requested.Remove(position);
    }

    private void UpdateForReleasedPositions()
    {
        foreach (ChunkPosition position in noLongerRequested)
        foreach (ChunkPosition affected in GetPositionsInManhattanRange(position, RequestLevel.Range))
        {
            Requests? requests = getOptional(affected);

            if (requests == null || requests.IsRequested) continue;

            requests.ResetLevel();
            changed.Add(requests);

            Int32 distance = GetDistanceToNearestRequested(affected);

            if (distance > 2 * RequestLevel.Range) continue;

            requests.RaiseLevel(RequestLevel.Highest - distance);
        }
    }

    private Int32 GetDistanceToNearestRequested(ChunkPosition source)
    {
        var distance = Int32.MaxValue;

        if (requested.Contains(source))
            return 0;

        foreach (ChunkPosition target in requested)
            distance = Math.Min(distance, ChunkPosition.Manhattan(source, target));

        return distance;
    }

    private void SpreadForNewlyRequested()
    {
        foreach (ChunkPosition position in newlyRequested)
        {
            SpreadRequestLevel(position);

            requested.Add(position);
        }
    }

    private void SpreadRequestLevel(ChunkPosition center)
    {
        foreach (ChunkPosition affected in GetPositionsInManhattanRange(center, RequestLevel.Range))
        {
            Int32 distance = ChunkPosition.Manhattan(center, affected);
            RequestLevel level = RequestLevel.Highest - distance;

            Requests requests = getRequired(affected);
            requests.RaiseLevel(level);

            changed.Add(requests);
        }
    }

    /// <summary>
    ///     Get all chunk positions in a manhattan range around a center.
    /// </summary>
    /// <param name="center">The center.</param>
    /// <param name="range">The range. Must be non-negative.</param>
    /// <returns>All chunk positions in range.</returns>
    public static IEnumerable<ChunkPosition> GetPositionsInManhattanRange(ChunkPosition center, Int32 range)
    {
        for (Int32 z = -range; z <= range; z++)
        {
            Int32 xyRange = range + 1 - Math.Abs(z);
            Int32 yRange = 2 * xyRange - 1;

            for (var ry = 0; ry < yRange; ry++)
            {
                Int32 xRange = xyRange - ry % 2;

                Int32 xStart = -xyRange + (ry + 1) / 2 + 1;
                Int32 yStart = -(ry / 2);

                for (var rx = 0; rx < xRange; rx++)
                {
                    Int32 x = xStart + rx;
                    Int32 y = yStart + rx;

                    yield return center.Offset(x, y, z);
                }
            }
        }
    }
}
