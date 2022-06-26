// <copyright file="Player.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using VoxelGame.Core.Logic;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Core.Entities;

/// <summary>
///     A player, that can interact with the world.
/// </summary>
public abstract class Player : PhysicsEntity
{
    /// <summary>
    ///     Create a new player.
    /// </summary>
    /// <param name="world">The world the player is in.</param>
    /// <param name="mass">The mass the player has.</param>
    /// <param name="boundingVolume">The bounding box of the player.</param>
    protected Player(World world, float mass, BoundingVolume boundingVolume) : base(
        world,
        mass,
        boundingVolume)
    {
        Position = World.SpawnPosition;
        Chunk = ChunkPosition.From(Position.Floor());

        for (int x = -LoadDistance; x <= LoadDistance; x++)
        for (int y = -LoadDistance; y <= LoadDistance; y++)
        for (int z = -LoadDistance; z <= LoadDistance; z++)
            World.RequestChunk(Chunk.Offset(x, y, z));
    }

    /// <summary>
    ///     Gets the extents of how many chunks should be around this player.
    /// </summary>
    public static int LoadDistance => 1;

    /// <summary>
    ///     The position the current chunk this player is in.
    /// </summary>
    public ChunkPosition Chunk { get; private set; }

    /// <inheritdoc />
    protected sealed override void Update(double deltaTime)
    {
        OnUpdate(deltaTime);
        ProcessChunkChange();
    }

    /// <summary>
    ///     Called every time the player is updated.
    /// </summary>
    /// <param name="deltaTime">The time since the last update cycle.</param>
    protected abstract void OnUpdate(double deltaTime);

    /// <summary>
    ///     Check if the current chunk has changed and request new chunks if needed / release unneeded chunks.
    /// </summary>
    private void ProcessChunkChange()
    {
        ChunkPosition currentChunk = ChunkPosition.From(Position.Floor());

        if (currentChunk == Chunk) return;

        int deltaX = Math.Abs(currentChunk.X - Chunk.X);
        int deltaY = Math.Abs(currentChunk.Y - Chunk.Y);
        int deltaZ = Math.Abs(currentChunk.Z - Chunk.Z);

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
        for (int x = -LoadDistance; x <= LoadDistance; x++)
        for (int y = -LoadDistance; y <= LoadDistance; y++)
        for (int z = -LoadDistance; z <= LoadDistance; z++)
        {
            World.ReleaseChunk(Chunk.Offset(x, y, z));
            World.RequestChunk(currentChunk.Offset(x, y, z));
        }
    }

    /// <summary>
    ///     Release and request chunks around the player using a shifted window.
    /// </summary>
    private void ReleaseAndRequestShifting(ChunkPosition currentChunk, int deltaX,
        int deltaY, int deltaZ)
    {
        int signX = currentChunk.X - Chunk.X >= 0 ? 1 : -1;
        int signY = currentChunk.Y - Chunk.Y >= 0 ? 1 : -1;
        int signZ = currentChunk.Z - Chunk.Z >= 0 ? 1 : -1;

        DoRequests(deltaX, 2 * LoadDistance + 1, 2 * LoadDistance + 1);
        DoRequests(2 * LoadDistance + 1, deltaY, 2 * LoadDistance + 1);
        DoRequests(2 * LoadDistance + 1, 2 * LoadDistance + 1, deltaZ);

        void DoRequests(int xMax, int yMax, int zMax)
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
