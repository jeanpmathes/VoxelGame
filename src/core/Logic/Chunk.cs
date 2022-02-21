// <copyright file="Chunk.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OpenToolkit.Mathematics;
using VoxelGame.Core.Collections;
using VoxelGame.Core.Generation;
using VoxelGame.Core.Updates;
using VoxelGame.Core.Utilities;
using VoxelGame.Logging;

namespace VoxelGame.Core.Logic
{
    /// <summary>
    ///     A chunk, a group of sections.
    /// </summary>
    [Serializable]
    public abstract class Chunk : IDisposable
    {
        /// <summary>
        ///     The number of sections in a chunk. The chunk is a large column of sections.
        /// </summary>
        public const int VerticalSectionCount = 64;

        private const int RandomTickBatchSize = VerticalSectionCount / 2;

        /// <summary>
        ///     The width of a chunk in blocks.
        /// </summary>
        public const int ChunkWidth = Section.SectionSize;

        /// <summary>
        ///     The height of a chunk in blocks.
        /// </summary>
        public const int ChunkHeight = Section.SectionSize * VerticalSectionCount;

        private static readonly ILogger logger = LoggingHelper.CreateLogger<Chunk>();

        /// <summary>
        ///     Result of <c>lb(VerticalSectionCount)</c> as int.
        /// </summary>
        public static readonly int VerticalSectionCountExp = (int) Math.Log(VerticalSectionCount, newBase: 2);

        private readonly ScheduledTickManager<Block.BlockTick> blockTickManager;
        private readonly ScheduledTickManager<Liquid.LiquidTick> liquidTickManager;

        /// <summary>
        ///     The sections in this chunk.
        /// </summary>
#pragma warning disable CA1051 // Do not declare visible instance fields
        protected readonly Section[] sections = new Section[VerticalSectionCount];
#pragma warning restore CA1051 // Do not declare visible instance fields

        /// <summary>
        ///     Create a new chunk.
        /// </summary>
        /// <param name="world">The world.</param>
        /// <param name="x">The x chunk coordinate.</param>
        /// <param name="z">The z chunk coordinate.</param>
        protected Chunk(World world, int x, int z)
        {
            World = world;

            X = x;
            Z = z;

            for (var y = 0; y < VerticalSectionCount; y++)
            {
#pragma warning disable S1699 // Constructors should only call non-overridable methods
#pragma warning disable CA2214 // Do not call overridable methods in constructors
                sections[y] = CreateSection();
#pragma warning restore CA2214 // Do not call overridable methods in constructors
#pragma warning restore S1699 // Constructors should only call non-overridable methods
            }

            blockTickManager = new ScheduledTickManager<Block.BlockTick>(
                Block.MaxBlockTicksPerFrameAndChunk,
                World,
                World.UpdateCounter);

            liquidTickManager = new ScheduledTickManager<Liquid.LiquidTick>(
                Liquid.MaxLiquidTicksPerFrameAndChunk,
                World,
                World.UpdateCounter);
        }

        /// <summary>
        ///     The X position of this chunk in chunk units
        /// </summary>
        public int X { get; }

        /// <summary>
        ///     The Y position of this chunk in chunk units
        /// </summary>
        public int Z { get; }

        /// <summary>
        ///     Gets the position of the chunk as a point located in the center of the chunk.
        /// </summary>
        public Vector3 ChunkPoint => new(
            X * ChunkWidth + ChunkWidth / 2f,
            ChunkHeight / 2f,
            Z * ChunkWidth + ChunkWidth / 2f);

        /// <summary>
        ///     The extents of a chunk.
        /// </summary>
        public static Vector3 ChunkExtents => new(ChunkWidth / 2f, ChunkHeight / 2f, ChunkWidth / 2f);

        /// <summary>
        ///     The world this chunk is in.
        /// </summary>
        [field: NonSerialized] protected World World { get; private set; }

        /// <summary>
        ///     Creates a section.
        /// </summary>
        protected abstract Section CreateSection();

        /// <summary>
        ///     Calls setup on all sections. This is required after loading.
        /// </summary>
        public void Setup(World world, UpdateCounter updateCounter)
        {
            World = world;

            blockTickManager.Setup(World, updateCounter);
            liquidTickManager.Setup(World, updateCounter);

            for (var y = 0; y < VerticalSectionCount; y++) sections[y].Setup(world);
        }

        /// <summary>
        ///     Loads a chunk from a file specified by the path. If the loaded chunk does not fit the x and z parameters, null is
        ///     returned.
        /// </summary>
        /// <param name="path">The path to the chunk file to load and check. The path itself is not checked.</param>
        /// <param name="x">The x coordinate of the chunk.</param>
        /// <param name="z">The z coordinate of the chunk.</param>
        /// <returns>The loaded chunk if its coordinates fit the requirements; null if they don't.</returns>
        [SuppressMessage(
            "ReSharper.DPA",
            "DPA0002: Excessive memory allocations in SOH",
            Justification = "Chunks are allocated here.")]
        public static Chunk? Load(string path, int x, int z)
        {
            logger.LogDebug(Events.ChunkOperation, "Loading chunk for position: ({X}|{Z})", x, z);

            Chunk chunk;

            using (Stream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                IFormatter formatter = new BinaryFormatter();

                chunk = (Chunk) formatter.Deserialize(
                    stream); // Allocation issue flagged here, remove suppression when serialization and deserialization is reworked.
            }

            // Checking the chunk
            if (chunk.X == x && chunk.Z == z) return chunk;

            logger.LogWarning(
                "File for the chunk at ({X}|{Z}) was invalid: position did not match",
                x,
                z);

            return null;
        }

        /// <summary>
        ///     Runs a task that loads a chunk from a file specified by the path. If the loaded chunk does not fit the x and z
        ///     parameters, null is returned.
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
        ///     Saves this chunk in the directory specified by the path.
        /// </summary>
        /// <param name="path">The path of the directory where this chunk should be saved.</param>
        public void Save(string path)
        {
            blockTickManager.Unload();
            liquidTickManager.Unload();

            string chunkFile = path + $"/x{X}z{Z}.chunk";

            logger.LogDebug(Events.ChunkOperation, "Saving the chunk ({X}|{Z}) to: {Path}", X, Z, chunkFile);

            using Stream stream = new FileStream(chunkFile, FileMode.Create, FileAccess.Write, FileShare.Read);
            IFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, this);

            blockTickManager.Load();
            liquidTickManager.Load();
        }

        /// <summary>
        ///     Runs a task which saves this chunk in the directory specified by the path.
        /// </summary>
        /// <param name="path">The path of the directory where this chunk should be saved.</param>
        /// <returns>A task.</returns>
        public Task SaveTask(string path)
        {
            return Task.Run(() => Save(path));
        }

        /// <summary>
        ///     Generate the chunk content.
        /// </summary>
        /// <param name="generator">The generator to use.</param>
        public void Generate(IWorldGenerator generator)
        {
            logger.LogDebug(
                Events.ChunkOperation,
                "Generating the chunk ({X}|{Z}) using '{Name}' generator",
                X,
                Z,
                generator);

            for (var x = 0; x < Section.SectionSize; x++)
            for (var z = 0; z < Section.SectionSize; z++)
            {
                var y = 0;

                foreach (Block block in generator.GenerateColumn(
                             x + X * Section.SectionSize,
                             z + Z * Section.SectionSize))
                {
                    sections[y >> Section.SectionSizeExp][x, y & (Section.SectionSize - 1), z] = block.Id;

                    y++;
                }
            }
        }

        /// <summary>
        ///     Run a chunk generation task.
        /// </summary>
        /// <param name="generator">The generator to use.</param>
        /// <returns>The task.</returns>
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

        /// <summary>
        /// </summary>
        public void Tick()
        {
            blockTickManager.Process();
            liquidTickManager.Process();

            int anchor = NumberGenerator.Random.Next(minValue: 0, VerticalSectionCount);

            for (var i = 0; i < RandomTickBatchSize; i++)
            {
                int y = (anchor + i) % VerticalSectionCount;
                sections[y].SendRandomUpdates((X, y, Z));
            }
        }

        /// <summary>
        ///     Get the section at the specified index.
        /// </summary>
        /// <param name="y">The section index. Must be in the range [0, VerticalSectionCount)</param>
        /// <returns>The section.</returns>
        public Section GetSection(int y)
        {
            return sections[y];
        }

        /// <inheritdoc />
        public sealed override string ToString()
        {
            return $"Chunk ({X}|{Z})";
        }

        /// <inheritdoc />
        public sealed override bool Equals(object? obj)
        {
            if (obj is Chunk other) return other.X == X && other.Z == Z;

            return false;
        }

        /// <inheritdoc />
        public sealed override int GetHashCode()
        {
            return HashCode.Combine(X, Z);
        }

        #region IDisposable Support

        /// <summary>
        ///     Dispose of this chunk.
        /// </summary>
        protected abstract void Dispose(bool disposing);

        /// <summary>
        ///     Finalizer.
        /// </summary>
        ~Chunk()
        {
            Dispose(disposing: false);
        }

        /// <summary>
        ///     Dispose of this chunk.
        /// </summary>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable Support
    }
}
