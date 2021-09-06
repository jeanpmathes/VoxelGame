// <copyright file="Player.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using VoxelGame.Core.Logic;
using VoxelGame.Core.Physics;

namespace VoxelGame.Core.Entities
{
    public abstract class Player : PhysicsEntity
    {
        /// <summary>
        /// Gets the extents of how many chunks should be around this player.
        /// </summary>
        public int LoadDistance { get; } = 4;

        /// <summary>
        /// Gets whether this player has moved to a different chunk in the last frame.
        /// </summary>
        public bool ChunkHasChanged { get; private set; }

        /// <summary>
        /// The x coordinate of the current chunk this player is in.
        /// </summary>
        public int ChunkX { get; private set; }

        /// <summary>
        /// The z coordinate of the current chunk this player is in.
        /// </summary>
        public int ChunkZ { get; private set; }

        protected Player(World world, float mass, float drag, BoundingBox boundingBox) : base(
            world,
            mass,
            drag,
            boundingBox)
        {
            Position = World.Information.SpawnInformation.Position;

            // Request chunks around current position
            ChunkX = (int) Math.Floor(Position.X) >> Section.SectionSizeExp;
            ChunkZ = (int) Math.Floor(Position.Z) >> Section.SectionSizeExp;

            for (int x = -LoadDistance; x <= LoadDistance; x++)
            {
                for (int z = -LoadDistance; z <= LoadDistance; z++)
                {
                    World.RequestChunk(ChunkX + x, ChunkZ + z);
                }
            }
        }

        protected sealed override void Update(float deltaTime)
        {
            OnUpdate(deltaTime);

            // Check if the current chunk has changed and request new chunks if needed / release unneeded chunks.
            ChunkChange();
        }

        protected abstract void OnUpdate(float deltaTime);

        private void ChunkChange()
        {
            int currentChunkX = (int) Math.Floor(Position.X) >> Section.SectionSizeExp;
            int currentChunkZ = (int) Math.Floor(Position.Z) >> Section.SectionSizeExp;

            if (currentChunkX == ChunkX && currentChunkZ == ChunkZ)
            {
                return;
            }

            ChunkHasChanged = true;

            int deltaX = Math.Abs(currentChunkX - ChunkX);
            int deltaZ = Math.Abs(currentChunkZ - ChunkZ);

            int signX = (currentChunkX - ChunkX >= 0) ? 1 : -1;
            int signZ = (currentChunkZ - ChunkZ >= 0) ? 1 : -1;

            // Check if player moved completely out of claimed chunks
            if (deltaX > 2 * LoadDistance || deltaZ > 2 * LoadDistance)
            {
                for (int x = -LoadDistance; x <= LoadDistance; x++)
                {
                    for (int z = -LoadDistance; z <= LoadDistance; z++)
                    {
                        World.ReleaseChunk(ChunkX + x, ChunkZ + z);
                        World.RequestChunk(currentChunkX + x, currentChunkZ + z);
                    }
                }
            }
            else
            {
                for (int x = 0; x < deltaX; x++)
                {
                    for (int z = 0; z < (2 * LoadDistance) + 1; z++)
                    {
                        World.ReleaseChunk(
                            ChunkX + ((LoadDistance - x) * -signX),
                            ChunkZ + ((LoadDistance - z) * -signZ));

                        World.RequestChunk(
                            currentChunkX + ((LoadDistance - x) * signX),
                            currentChunkZ + ((LoadDistance - z) * signZ));
                    }
                }

                for (int z = 0; z < deltaZ; z++)
                {
                    for (int x = 0; x < (2 * LoadDistance) + 1; x++)
                    {
                        World.ReleaseChunk(
                            ChunkX + ((LoadDistance - x) * -signX),
                            ChunkZ + ((LoadDistance - z) * -signZ));

                        World.RequestChunk(
                            currentChunkX + ((LoadDistance - x) * signX),
                            currentChunkZ + ((LoadDistance - z) * signZ));
                    }
                }
            }

            ChunkX = currentChunkX;
            ChunkZ = currentChunkZ;
        }
    }
}