// <copyright file="Chunk.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using OpenToolkit.Mathematics;
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VoxelGame.Core.WorldGeneration;
using VoxelGame.Core.Collections;
using VoxelGame.Core.Updates;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Core.Logic
{
    [Serializable]
    public abstract class Chunk : IDisposable
    {
        private static readonly ILogger logger = LoggingHelper.CreateLogger<Chunk>();

        public const int ChunkHeight = 32;

        /// <summary>
        /// The X position of this chunk in chunk units
        /// </summary>
        public int X { get; }

        /// <summary>
        /// The Y position of this chunk in chunk units
        /// </summary>
        public int Z { get; }

        /// <summary>
        /// Gets the position of the chunk as a point located in the center of the chunk.
        /// </summary>
        public Vector3 ChunkPoint { get => new Vector3((X * Section.SectionSize) + (Section.SectionSize / 2f), ChunkHeight * Section.SectionSize / 2f, (Z * Section.SectionSize) + (Section.SectionSize / 2f)); }

        public static Vector3 ChunkExtents { get => new Vector3(Section.SectionSize / 2f, ChunkHeight * Section.SectionSize / 2f, Section.SectionSize / 2f); }

#pragma warning disable CA1051 // Do not declare visible instance fields
        protected readonly Section[] sections = new Section[ChunkHeight];
#pragma warning restore CA1051 // Do not declare visible instance fields

        private readonly ScheduledTickManager<Block.BlockTick> blockTickManager;
        private readonly ScheduledTickManager<Liquid.LiquidTick> liquidTickManager;

        protected Chunk(int x, int z, UpdateCounter updateCounter)
        {
            X = x;
            Z = z;

            for (var y = 0; y < ChunkHeight; y++)
            {
#pragma warning disable S1699 // Constructors should only call non-overridable methods
#pragma warning disable CA2214 // Do not call overridable methods in constructors
                sections[y] = CreateSection();
#pragma warning restore CA2214 // Do not call overridable methods in constructors
#pragma warning restore S1699 // Constructors should only call non-overridable methods
            }

            blockTickManager = new ScheduledTickManager<Block.BlockTick>(Block.MaxLiquidTicksPerFrameAndChunk, updateCounter);
            liquidTickManager = new ScheduledTickManager<Liquid.LiquidTick>(Liquid.MaxLiquidTicksPerFrameAndChunk, updateCounter);
        }

        protected abstract Section CreateSection();

        /// <summary>
        /// Calls setup on all sections. This is required after loading.
        /// </summary>
        public void Setup(UpdateCounter updateCounter)
        {
            blockTickManager.Setup(updateCounter);
            liquidTickManager.Setup(updateCounter);

            for (var y = 0; y < ChunkHeight; y++)
            {
                sections[y].Setup();
            }
        }

        /// <summary>
        /// Loads a chunk from a file specified by the path. If the loaded chunk does not fit the x and z parameters, null is returned.
        /// </summary>
        /// <param name="path">The path to the chunk file to load and check. The path itself is not checked.</param>
        /// <param name="x">The x coordinate of the chunk.</param>
        /// <param name="z">The z coordinate of the chunk.</param>
        /// <returns>The loaded chunk if its coordinates fit the requirements; null if they don't.</returns>
        public static Chunk? Load(string path, int x, int z)
        {
            logger.LogDebug("Loading chunk for position: ({x}|{z})", x, z);

            Chunk chunk;

            using (Stream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                IFormatter formatter = new BinaryFormatter();
                chunk = (Chunk)formatter.Deserialize(stream);
            }

            // Checking the chunk
            if (chunk.X == x && chunk.Z == z)
            {
                return chunk;
            }
            else
            {
                logger.LogWarning("The file for the chunk at ({x}|{z}) was not valid as the position did not match.", x, z);

                return null;
            }
        }

        /// <summary>
        /// Runs a task that loads a chunk from a file specified by the path. If the loaded chunk does not fit the x and z parameters, null is returned.
        /// </summary>
        /// <param name="path">The path to the chunk file to load and check. The path itself is not checked.</param>
        /// <param name="x">The x coordinate of the chunk.</param>
        /// <param name="z">The z coordinate of the chunk.</param>
        /// <returns>A task containing the loaded chunk if its coordinates fit the requirements; null if they don't.</returns>
        public static Task<Chunk?> LoadTask(string path, int x, int z)
        {
            return Task.Run(() => Load(path, x, z));
        }

        /// <summary>
        /// Saves this chunk in the directory specified by the path.
        /// </summary>
        /// <param name="path">The path of the directory where this chunk should be saved.</param>
        public void Save(string path)
        {
            blockTickManager.Unload();
            liquidTickManager.Unload();

            string chunkFile = path + $"/x{X}z{Z}.chunk";

            logger.LogDebug("Saving the chunk ({x}|{z}) to: {path}", X, Z, chunkFile);

            using Stream stream = new FileStream(chunkFile, FileMode.Create, FileAccess.Write, FileShare.Read);
            IFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, this);

            blockTickManager.Load();
            liquidTickManager.Load();
        }

        /// <summary>
        /// Runs a task which saves this chunk in the directory specified by the path.
        /// </summary>
        /// <param name="path">The path of the directory where this chunk should be saved.</param>
        /// <returns>A task.</returns>
        public Task SaveTask(string path)
        {
            return Task.Run(() => Save(path));
        }

        public void Generate(IWorldGenerator generator)
        {
            logger.LogDebug("Generating the chunk ({x}|{z}) using the '{name}' generator.", X, Z, generator);

            for (int x = 0; x < Section.SectionSize; x++)
            {
                for (int z = 0; z < Section.SectionSize; z++)
                {
                    int y = 0;

                    foreach (Block block in generator.GenerateColumn(x + (X * Section.SectionSize), z + (Z * Section.SectionSize)))
                    {
                        sections[y >> 5][x, y & (Section.SectionSize - 1), z] = block.Id;

                        y++;
                    }
                }
            }
        }

        public Task GenerateTask(IWorldGenerator generator)
        {
            return Task.Run(() => Generate(generator));
        }

        internal void ScheduleBlockTick(Block.BlockTick tick, int tickOffset)
        {
            blockTickManager.Add(tick, tickOffset);
        }

        internal void ScheduleLiquidTick(Liquid.LiquidTick tick, int tickOffset)
        {
            liquidTickManager.Add(tick, tickOffset);
        }

        public void Tick()
        {
            blockTickManager.Process();
            liquidTickManager.Process();

            for (int y = 0; y < ChunkHeight; y++)
            {
                sections[y].Tick(X, y, Z);
            }
        }

        public Section GetSection(int y)
        {
            return sections[y];
        }

        public sealed override string ToString()
        {
            return $"Chunk ({X}|{Z})";
        }

        public sealed override bool Equals(object? obj)
        {
            if (obj is Chunk other)
            {
                return other.X == this.X && other.Z == this.Z;
            }
            else
            {
                return false;
            }
        }

        public sealed override int GetHashCode()
        {
            return HashCode.Combine(X, Z);
        }

        #region IDisposable Support

        protected abstract void Dispose(bool disposing);

        ~Chunk()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable Support
    }
}