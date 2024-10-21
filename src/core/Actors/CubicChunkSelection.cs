// <copyright file="CubicChunkSelection.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using VoxelGame.Core.Logic;
using VoxelGame.Core.Logic.Chunks;

namespace VoxelGame.Core.Actors;

/// <summary>
///     Defines a moveable cubic selection of chunks.
/// </summary>
public class CubicChunkSelection
{
    private readonly Int32 extent;
    private ChunkPosition center;

    /// <summary>
    ///     Create a new cubic chunk selection.
    /// </summary>
    /// <param name="center">The center of the selection.</param>
    /// <param name="extent">The extent of the selection.</param>
    public CubicChunkSelection(ChunkPosition center, Int32 extent)
    {
        this.center = center;
        this.extent = extent;
    }

    /// <summary>
    ///     Get the center of the selection.
    /// </summary>
    public ChunkPosition Center => center;

    /// <summary>
    ///     Request the chunks in the selection.
    /// </summary>
    /// <param name="world">The world to request the chunks from.</param>
    public void Request(World world)
    {
        for (Int32 x = -extent; x <= extent; x++)
        for (Int32 y = -extent; y <= extent; y++)
        for (Int32 z = -extent; z <= extent; z++)
            world.RequestChunk(center.Offset(x, y, z));
    }

    /// <summary>
    ///     Release the chunks in the selection.
    /// </summary>
    /// <param name="world">The world to release the chunks from.</param>
    public void Release(World world)
    {
        for (Int32 x = -extent; x <= extent; x++)
        for (Int32 y = -extent; y <= extent; y++)
        for (Int32 z = -extent; z <= extent; z++)
            world.ReleaseChunk(center.Offset(x, y, z));
    }

    /// <summary>
    ///     Move the selection to a new center.
    ///     This will request the new chunks and release the old ones.
    /// </summary>
    /// <param name="newCenter">The new center of the selection.</param>
    /// <param name="world">The world to request the chunks from.</param>
    public void Move(ChunkPosition newCenter, World world)
    {
        if (center == newCenter)
            return;

        Int32 deltaX = Math.Abs(newCenter.X - center.X);
        Int32 deltaY = Math.Abs(newCenter.Y - center.Y);
        Int32 deltaZ = Math.Abs(newCenter.Z - center.Z);

        // Check if player moved completely out of claimed chunks
        if (deltaX > 2 * extent || deltaY > 2 * extent || deltaZ > 2 * extent)
            ReleaseAndRequestAll(newCenter, world);
        else ReleaseAndRequestShifting(newCenter, world, deltaX, deltaY, deltaZ);

        center = newCenter;
    }

    /// <summary>
    ///     Release all previously claimed chunks and request all chunks around the player.
    /// </summary>
    private void ReleaseAndRequestAll(ChunkPosition newCenter, World world)
    {
        for (Int32 x = -extent; x <= extent; x++)
        for (Int32 y = -extent; y <= extent; y++)
        for (Int32 z = -extent; z <= extent; z++)
        {
            world.ReleaseChunk(center.Offset(x, y, z));
            world.RequestChunk(newCenter.Offset(x, y, z));
        }
    }

    /// <summary>
    ///     Release and request chunks around the player using a shifted window.
    /// </summary>
    private void ReleaseAndRequestShifting(ChunkPosition newCenter, World world,
        Int32 deltaX, Int32 deltaY, Int32 deltaZ)
    {
        Int32 signX = newCenter.X - center.X >= 0 ? 1 : -1;
        Int32 signY = newCenter.Y - center.Y >= 0 ? 1 : -1;
        Int32 signZ = newCenter.Z - center.Z >= 0 ? 1 : -1;

        DoRequests(deltaX, 2 * extent + 1, 2 * extent + 1);
        DoRequests(2 * extent + 1, deltaY, 2 * extent + 1);
        DoRequests(2 * extent + 1, 2 * extent + 1, deltaZ);

        void DoRequests(Int32 xMax, Int32 yMax, Int32 zMax)
        {
            if (xMax == 0 || yMax == 0 || zMax == 0) return;

            for (var x = 0; x < xMax; x++)
            for (var y = 0; y < yMax; y++)
            for (var z = 0; z < zMax; z++)
            {
                world.ReleaseChunk(
                    center.Offset(
                        (extent - x) * -signX,
                        (extent - y) * -signY,
                        (extent - z) * -signZ));

                world.RequestChunk(
                    newCenter.Offset(
                        (extent - x) * signX,
                        (extent - y) * signY,
                        (extent - z) * signZ));
            }
        }
    }
}
