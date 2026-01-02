// <copyright file="ChunkLoader.cs" company="VoxelGame">
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
using System.Diagnostics.CodeAnalysis;
using VoxelGame.Annotations.Attributes;
using VoxelGame.Core.Logic.Chunks;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Core.Actors.Components;

/// <summary>
///     Loads chunks around an <see cref="Actor" />.
/// </summary>
public partial class ChunkLoader : ActorComponent
{
    [SuppressMessage("Usage", "CA2213:Disposable fields should be disposed", Justification = "Is only borrowed by this class.")]
    private readonly Transform transform;

    private Request? request;

    [Constructible]
    private ChunkLoader(Actor subject) : base(subject)
    {
        transform = subject.GetRequiredComponent<Transform>();
    }

    /// <summary>
    ///     Gets the extents of how many chunks should be around the actor.
    /// </summary>
    public static Int32 LoadDistance => 5;

    /// <summary>
    ///     The position of the current chunk the actor is in.
    /// </summary>
    public ChunkPosition Chunk => request?.Position ?? default;

    /// <inheritdoc />
    public override void OnAdd()
    {
        ChunkPosition chunk = ChunkPosition.From(transform.Position.Floor());

        request = Subject.World.RequestChunk(chunk, Subject);
    }

    /// <inheritdoc />
    public override void OnRemove()
    {
        Subject.World.ReleaseChunk(request);

        request = null;
    }

    /// <inheritdoc />
    public override void OnLogicUpdate(Double deltaTime)
    {
        ChunkPosition newChunk = ChunkPosition.From(transform.Position.Floor());

        if (newChunk == Chunk) return;

        Subject.World.ReleaseChunk(request);
        request = Subject.World.RequestChunk(newChunk, Subject);
    }
}
