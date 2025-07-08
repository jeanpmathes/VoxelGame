﻿// <copyright file="ChunkLoader.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Diagnostics.CodeAnalysis;
using VoxelGame.Core.Logic.Chunks;
using VoxelGame.Core.Utilities;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Core.Actors.Components;

/// <summary>
/// Loads chunks around an <see cref="Actor"/>.
/// </summary>
public class ChunkLoader : ActorComponent, IConstructible<Actor, ChunkLoader>
{
    [SuppressMessage("Usage", "CA2213:Disposable fields should be disposed", Justification = "Is only borrowed by this class.")]
    private readonly Transform transform;
    
    private Request? request;
    
    private ChunkLoader(Actor subject) : base(subject) 
    {
        transform = subject.GetRequiredComponent<Transform>();
    }

    /// <inheritdoc />
    public static ChunkLoader Construct(Actor input)
    {
        return new ChunkLoader(input);
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
