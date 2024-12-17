// <copyright file="Player.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using VoxelGame.Core.Logic.Chunks;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Core.Actors;

/// <summary>
///     A player that can interact with the world.
/// </summary>
public abstract class Player : PhysicsActor
{
    private Request? request;

    /// <summary>
    ///     Create a new player.
    /// </summary>
    /// <param name="mass">The mass the player has.</param>
    /// <param name="boundingVolume">The bounding box of the player.</param>
    protected Player(Single mass, BoundingVolume boundingVolume) : base(mass, boundingVolume)
    {
        AddedToWorld += OnAddedToWorld;
        RemovedFromWorld += OnRemovedFromWorld;
    }

    /// <summary>
    ///     Gets the extents of how many chunks should be around this player.
    /// </summary>
    public static Int32 LoadDistance => 5;

    /// <summary>
    ///     The position of the current chunk this player is in.
    /// </summary>
    public ChunkPosition Chunk => request?.Position ?? throw new InvalidOperationException();

    private void OnAddedToWorld(Object? sender, EventArgs e)
    {
        Position = World.SpawnPosition;

        ChunkPosition chunk = ChunkPosition.From(Position.Floor());

        request = World.RequestChunk(chunk, this);
    }

    private void OnRemovedFromWorld(Object? sender, EventArgs e)
    {
        World.ReleaseChunk(request);

        request = null;
    }

    /// <inheritdoc />
    protected sealed override void OnLogicUpdate(Double deltaTime)
    {
        OnPlayerUpdate(deltaTime);

        ChunkPosition newChunk = ChunkPosition.From(Position.Floor());

        if (newChunk == Chunk) return;

        World.ReleaseChunk(request);
        request = World.RequestChunk(newChunk, this);
    }

    /// <summary>
    ///     Called every time the player is updated.
    /// </summary>
    /// <param name="deltaTime">The time since the last update cycle.</param>
    protected abstract void OnPlayerUpdate(Double deltaTime);
}
