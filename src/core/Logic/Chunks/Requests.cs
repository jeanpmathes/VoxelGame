// <copyright file="Requests.cs" company="VoxelGame">
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
using System.Diagnostics;
using VoxelGame.Core.Actors;
using VoxelGame.Core.Collections;

namespace VoxelGame.Core.Logic.Chunks;

/// <summary>
///     All requests and the computed request level of a chunk.
/// </summary>
public class Requests(Chunk? chunk)
{
    private readonly Bag<Actor> requesters = new(null!);

    /// <summary>
    ///     Whether the chunk is requested by at least one actor.
    /// </summary>
    public Boolean IsRequested => requesters.Count > 0;

    /// <summary>
    ///     Get the current request level.
    /// </summary>
    public RequestLevel Level { get; private set; } = RequestLevel.Lowest;

    /// <summary>
    ///     Get all requesters of the chunk.
    /// </summary>
    public IEnumerable<Actor> Requesters => requesters;

    /// <summary>
    ///     Add a request to the class.
    /// </summary>
    /// <param name="request">The request to add. Must not be added to any other class.</param>
    /// <returns>True if this is the only request, meaning the chunk is now requested.</returns>
    public Boolean AddRequest(Request request)
    {
        Debug.Assert(request.Index == null);

        request.Index = requesters.Add(request.Requester);

        Level = RequestLevel.Highest;

        return requesters.Count == 1;
    }

    /// <summary>
    ///     Remove a request from the class.
    /// </summary>
    /// <param name="request">The request to remove.</param>
    /// <returns>True if this was the last request, meaning the chunk is no longer requested.</returns>
    public Boolean RemoveRequest(Request request)
    {
        Debug.Assert(request.Index != null);

        requesters.RemoveAt(request.Index.Value);
        request.Index = null;

        return requesters.Count == 0;
    }

    /// <summary>
    ///     Reset the request level to the lowest if possible.
    /// </summary>
    public void ResetLevel()
    {
        if (IsRequested) return;

        Level = RequestLevel.Lowest;
    }

    /// <summary>
    ///     Raise the request level to the given level.
    /// </summary>
    public void RaiseLevel(RequestLevel neighborLevel)
    {
        if (IsRequested)
            return;

        if (neighborLevel > Level)
            Level = neighborLevel;
    }

    /// <summary>
    ///     Apply the current request level to the chunk.
    /// </summary>
    internal void ApplyLevel()
    {
        chunk?.OnRequestLevelApplied();
    }
}
