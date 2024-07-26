// <copyright file="Player.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using VoxelGame.Core.Logic;
using VoxelGame.Core.Logic.Chunks;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Core.Actors;

/// <summary>
///     A player that can interact with the world.
/// </summary>
public abstract class Player : PhysicsActor
{
    /// <summary>
    ///     Create a new player.
    /// </summary>
    /// <param name="world">The world the player is in.</param>
    /// <param name="mass">The mass the player has.</param>
    /// <param name="boundingVolume">The bounding box of the player.</param>
    protected Player(World world, Single mass, BoundingVolume boundingVolume) : base(
        world,
        mass,
        boundingVolume)
    {
        Position = World.SpawnPosition;
        Chunk = ChunkPosition.From(Position.Floor());

        for (Int32 x = -LoadDistance; x <= LoadDistance; x++)
        for (Int32 y = -LoadDistance; y <= LoadDistance; y++)
        for (Int32 z = -LoadDistance; z <= LoadDistance; z++)
            World.RequestChunk(Chunk.Offset(x, y, z));
    }

    /// <summary>
    ///     Gets the extents of how many chunks should be around this player.
    /// </summary>
    public static Int32 LoadDistance => 1; // todo: pick something >= 5

    /// <summary>
    ///     The position of the current chunk this player is in.
    /// </summary>
    public ChunkPosition Chunk { get; private set; }

    /// <inheritdoc />
    protected sealed override void Update(Double deltaTime)
    {
        OnUpdate(deltaTime);
        ProcessChunkChange();
    }

    /// <summary>
    ///     Called every time the player is updated.
    /// </summary>
    /// <param name="deltaTime">The time since the last update cycle.</param>
    protected abstract void OnUpdate(Double deltaTime);

    /// <summary>
    ///     Check if the current chunk has changed and request new chunks if needed / release unneeded chunks.
    /// </summary>
    private void ProcessChunkChange()
    {
        ChunkPosition currentChunk = ChunkPosition.From(Position.Floor());

        if (currentChunk == Chunk) return;

        Int32 deltaX = Math.Abs(currentChunk.X - Chunk.X);
        Int32 deltaY = Math.Abs(currentChunk.Y - Chunk.Y);
        Int32 deltaZ = Math.Abs(currentChunk.Z - Chunk.Z);

        // Check if player moved completely out of claimed chunks
        if (deltaX > 2 * LoadDistance || deltaY > 2 * LoadDistance || deltaZ > 2 * LoadDistance)
            ReleaseAndRequestAll(currentChunk);
        else ReleaseAndRequestShifting(currentChunk, deltaX, deltaY, deltaZ);

        Chunk = currentChunk;
    }

    /// <summary>
    ///     Release all previously claimed chunks and request all chunks around the player.
    /// </summary>
    private void ReleaseAndRequestAll(ChunkPosition currentChunk)
    {
        for (Int32 x = -LoadDistance; x <= LoadDistance; x++)
        for (Int32 y = -LoadDistance; y <= LoadDistance; y++)
        for (Int32 z = -LoadDistance; z <= LoadDistance; z++)
        {
            World.ReleaseChunk(Chunk.Offset(x, y, z));
            World.RequestChunk(currentChunk.Offset(x, y, z));
        }
    }

    /// <summary>
    ///     Release and request chunks around the player using a shifted window.
    /// </summary>
    private void ReleaseAndRequestShifting(ChunkPosition currentChunk, Int32 deltaX,
        Int32 deltaY, Int32 deltaZ)
    {
        Int32 signX = currentChunk.X - Chunk.X >= 0 ? 1 : -1;
        Int32 signY = currentChunk.Y - Chunk.Y >= 0 ? 1 : -1;
        Int32 signZ = currentChunk.Z - Chunk.Z >= 0 ? 1 : -1;

        DoRequests(deltaX, 2 * LoadDistance + 1, 2 * LoadDistance + 1);
        DoRequests(2 * LoadDistance + 1, deltaY, 2 * LoadDistance + 1);
        DoRequests(2 * LoadDistance + 1, 2 * LoadDistance + 1, deltaZ);

        void DoRequests(Int32 xMax, Int32 yMax, Int32 zMax)
        {
            if (xMax == 0 || yMax == 0 || zMax == 0) return;

            for (var x = 0; x < xMax; x++)
            for (var y = 0; y < yMax; y++)
            for (var z = 0; z < zMax; z++)
            {
                World.ReleaseChunk(
                    Chunk.Offset(
                        (LoadDistance - x) * -signX,
                        (LoadDistance - y) * -signY,
                        (LoadDistance - z) * -signZ));

                World.RequestChunk(
                    currentChunk.Offset(
                        (LoadDistance - x) * signX,
                        (LoadDistance - y) * signY,
                        (LoadDistance - z) * signZ));
            }
        }
    }
}
