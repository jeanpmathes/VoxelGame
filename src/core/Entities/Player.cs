// <copyright file="Player.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using VoxelGame.Core.Logic;
using VoxelGame.Core.Physics;

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
    /// <param name="drag">The drag that affects the player.</param>
    /// <param name="boundingVolume">The bounding box of the player.</param>
    protected Player(World world, float mass, float drag, BoundingVolume boundingVolume) : base(
        world,
        mass,
        drag,
        boundingVolume)
    {
        Position = World.SpawnPosition;

        // Request chunks around current position
        ChunkX = (int) Math.Floor(Position.X) >> Chunk.BlockSizeExp;
        ChunkY = (int) Math.Floor(Position.Y) >> Chunk.BlockSizeExp;
        ChunkZ = (int) Math.Floor(Position.Z) >> Chunk.BlockSizeExp;

        for (int x = -LoadDistance; x <= LoadDistance; x++)
        for (int y = -LoadDistance; y <= LoadDistance; y++)
        for (int z = -LoadDistance; z <= LoadDistance; z++)
            World.RequestChunk(ChunkX + x, ChunkY + y, ChunkZ + z);
    }

    /// <summary>
    ///     Gets the extents of how many chunks should be around this player.
    /// </summary>
    public static int LoadDistance => 1;

    /// <summary>
    ///     The x coordinate of the current chunk this player is in.
    /// </summary>
    public int ChunkX { get; private set; }

    /// <summary>
    ///     The y coordinate of the current chunk this player is in.
    /// </summary>
    public int ChunkY { get; private set; }

    /// <summary>
    ///     The z coordinate of the current chunk this player is in.
    /// </summary>
    public int ChunkZ { get; private set; }

    /// <inheritdoc />
    protected sealed override void Update(float deltaTime)
    {
        OnUpdate(deltaTime);
        ProcessChunkChange();
    }

    /// <summary>
    ///     Called every time the player is updated.
    /// </summary>
    /// <param name="deltaTime">The time since the last update cycle.</param>
    protected abstract void OnUpdate(float deltaTime);

    /// <summary>
    ///     Check if the current chunk has changed and request new chunks if needed / release unneeded chunks.
    /// </summary>
    private void ProcessChunkChange()
    {
        int currentChunkX = (int) Math.Floor(Position.X) >> Chunk.BlockSizeExp;
        int currentChunkY = (int) Math.Floor(Position.Y) >> Chunk.BlockSizeExp;
        int currentChunkZ = (int) Math.Floor(Position.Z) >> Chunk.BlockSizeExp;

        if (currentChunkX == ChunkX && currentChunkY == ChunkY && currentChunkZ == ChunkZ) return;

        int deltaX = Math.Abs(currentChunkX - ChunkX);
        int deltaY = Math.Abs(currentChunkY - ChunkY);
        int deltaZ = Math.Abs(currentChunkZ - ChunkZ);

        // Check if player moved completely out of claimed chunks
        if (deltaX > 2 * LoadDistance || deltaY > 2 * LoadDistance || deltaZ > 2 * LoadDistance)
            ReleaseAndRequestAll(currentChunkX, currentChunkY, currentChunkZ);
        else ReleaseAndRequestShifting(currentChunkX, currentChunkY, currentChunkZ, deltaX, deltaY, deltaZ);

        ChunkX = currentChunkX;
        ChunkY = currentChunkY;
        ChunkZ = currentChunkZ;
    }

    /// <summary>
    ///     Release all previously claimed chunks and request all chunks around the player.
    /// </summary>
    private void ReleaseAndRequestAll(int currentChunkX, int currentChunkY, int currentChunkZ)
    {
        for (int x = -LoadDistance; x <= LoadDistance; x++)
        for (int y = -LoadDistance; y <= LoadDistance; y++)
        for (int z = -LoadDistance; z <= LoadDistance; z++)
        {
            World.ReleaseChunk(ChunkX + x, ChunkY + y, ChunkZ + z);
            World.RequestChunk(currentChunkX + x, currentChunkY + y, currentChunkZ + z);
        }
    }

    /// <summary>
    ///     Release and request chunks around the player using a shifted window.
    /// </summary>
    private void ReleaseAndRequestShifting(int currentChunkX, int currentChunkY, int currentChunkZ, int deltaX,
        int deltaY, int deltaZ)
    {
        int signX = currentChunkX - ChunkX >= 0 ? 1 : -1;
        int signY = currentChunkY - ChunkY >= 0 ? 1 : -1;
        int signZ = currentChunkZ - ChunkZ >= 0 ? 1 : -1;

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
                    ChunkX + (LoadDistance - x) * -signX,
                    ChunkY + (LoadDistance - y) * -signY,
                    ChunkZ + (LoadDistance - z) * -signZ);

                World.RequestChunk(
                    currentChunkX + (LoadDistance - x) * signX,
                    currentChunkY + (LoadDistance - y) * signY,
                    currentChunkZ + (LoadDistance - z) * signZ);
            }
        }
    }
}
