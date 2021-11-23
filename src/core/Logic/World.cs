// <copyright file="World.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OpenToolkit.Mathematics;
using Properties;
using VoxelGame.Core.Collections;
using VoxelGame.Core.Updates;
using VoxelGame.Core.WorldGeneration;
using VoxelGame.Logging;

namespace VoxelGame.Core.Logic
{
    public abstract partial class World : IDisposable
    {
        private static readonly ILogger logger = LoggingHelper.CreateLogger<World>();

        private readonly IWorldGenerator generator;

        /// <summary>
        ///     This constructor is meant for worlds that are new.
        /// </summary>
        protected World(string name, string path, int seed) :
            this(
                new WorldInformation
                {
                    Name = name,
                    Seed = seed,
                    Creation = DateTime.Now,
                    Version = GameInformation.Instance.Version
                },
                path,
                path + "/Chunks",
                new ComplexGenerator(seed))
        {
            Information.Save(Path.Combine(WorldDirectory, "meta.json"));

            logger.LogInformation(Events.WorldIO, "Created new world");
        }

        /// <summary>
        ///     This constructor is meant for worlds that already exist.
        /// </summary>
        protected World(WorldInformation information, string path) :
            this(
                information,
                path,
                path + "/Chunks",
                new ComplexGenerator(information.Seed))
        {
            logger.LogInformation(Events.WorldIO, "Loaded existing world");
        }

        /// <summary>
        ///     Setup of readonly fields and non-optional steps.
        /// </summary>
        private World(WorldInformation information, string worldDirectory, string chunkDirectory,
            IWorldGenerator generator)
        {
            positionsToActivate = new HashSet<(int x, int z)>();
            positionsActivating = new HashSet<(int, int)>();
            chunksToGenerate = new UniqueQueue<Chunk>();
            chunkGenerateTasks = new List<Task>(MaxGenerationTasks);
            chunksGenerating = new Dictionary<int, Chunk>(MaxGenerationTasks);
            positionsToLoad = new UniqueQueue<(int x, int z)>();
            chunkLoadingTasks = new List<Task<Chunk?>>(MaxLoadingTasks);
            positionsLoading = new Dictionary<int, (int x, int z)>(MaxLoadingTasks);
            activeChunks = new Dictionary<ValueTuple<int, int>, Chunk>();
            positionsToReleaseOnActivation = new HashSet<(int x, int z)>();
            chunksToSave = new UniqueQueue<Chunk>();
            chunkSavingTasks = new List<Task>(MaxSavingTasks);
            chunksSaving = new Dictionary<int, Chunk>(MaxSavingTasks);
            positionsSaving = new HashSet<(int x, int z)>(MaxSavingTasks);
            positionsActivatingThroughSaving = new HashSet<(int x, int z)>();

            Information = information;

            WorldDirectory = worldDirectory;
            ChunkDirectory = chunkDirectory;
            this.generator = generator;

            UpdateCounter = new UpdateCounter();

            Setup();
        }

        private WorldInformation Information { get; }

        public UpdateCounter UpdateCounter { get; }

        protected int MaxGenerationTasks { get; } = Settings.Default.MaxGenerationTasks;
        protected int MaxLoadingTasks { get; } = Settings.Default.MaxLoadingTasks;

        protected int MaxSavingTasks { get; } = Settings.Default.MaxSavingTasks;

        protected string WorldDirectory { get; }
        protected string ChunkDirectory { get; }

        /// <summary>
        ///     Gets whether this world is ready for physics ticking and rendering.
        /// </summary>
        public bool IsReady { get; protected set; }

        private void Setup()
        {
            Directory.CreateDirectory(WorldDirectory);
            Directory.CreateDirectory(ChunkDirectory);

            positionsToActivate.Add((0, 0));
        }

        public abstract void Update(float deltaTime);

        /// <summary>
        ///     Returns the block instance at a given position in block coordinates. The block is only searched in active chunks.
        /// </summary>
        /// <param name="position">The block position.</param>
        /// <returns>The block instance at the given position or null if the block was not found.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BlockInstance? GetBlock(Vector3i position)
        {
            RetrieveContent(position, out Block? block, out uint data, out _, out _, out _);

            return block?.AsInstance(data);
        }

        /// <summary>
        /// Retrieve the content at a given position. The content can only be retrieved from active chunks.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <param name="block">The block at the given position.</param>
        /// <param name="data">The data of the block.</param>
        /// <param name="liquid">The liquid at the given position.</param>
        /// <param name="level">The level of the liquid.</param>
        /// <param name="isStatic">Whether the liquid is static.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void RetrieveContent(Vector3i position,
            out Block? block, out uint data,
            out Liquid? liquid, out LiquidLevel level, out bool isStatic)
        {
            Chunk? chunk = GetChunkWithPosition(position);

            if (chunk != null)
            {
                uint val = chunk.GetSection(position.Y >> Section.SectionSizeExp).GetContent(position);
                Section.Decode(val, out block, out data, out liquid, out level, out isStatic);

                return;
            }

            block = null;
            data = 0;
            liquid = null;
            level = 0;
            isStatic = false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LiquidInstance? GetLiquid(Vector3i position)
        {
            RetrieveContent(
                position,
                out Block? _,
                out uint _,
                out Liquid? liquid,
                out LiquidLevel level,
                out bool isStatic);

            return liquid?.AsInstance(level, isStatic);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public (BlockInstance? block, LiquidInstance? liquid) GetContent(Vector3i position)
        {
            RetrieveContent(
                position,
                out Block? block,
                out uint data,
                out Liquid? liquid,
                out LiquidLevel level,
                out bool isStatic);

            return (block?.AsInstance(data), liquid?.AsInstance(level, isStatic));
        }

        /// <summary>
        ///     Sets a block in the world, adds the changed sections to the re-mesh set and sends updates to the neighbors of
        ///     the changed block.
        /// </summary>
        /// <param name="block">The block which should be set at the position.</param>
        /// <param name="position">The block position.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetBlock(BlockInstance block, Vector3i position)
        {
            LiquidInstance? liquid = GetLiquid(position);

            if (liquid is null) return;

            SetContent(block, liquid, position, tickLiquid: true);
        }

        /// <summary>
        ///     Sets a liquid in the world, adds the changed sections to the re-mesh set and sends updates to the neighbors of the
        ///     changed block.
        /// </summary>
        /// <param name="liquid"></param>
        /// <param name="position"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetLiquid(LiquidInstance liquid, Vector3i position)
        {
            BlockInstance? block = GetBlock(position);

            if (block is null) return;

            SetContent(block, liquid, position, tickLiquid: true);
        }

        /// <summary>
        ///     Set the <c>isStatic</c> flag of a liquid without causing any updates around this liquid.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void ModifyLiquid(bool isStatic, Vector3i position)
        {
            ModifyWorldData(position, ~Section.StaticMask, isStatic ? Section.StaticMask : 0);
        }

        private void SetContent(BlockInstance block, LiquidInstance liquid, Vector3i position, bool tickLiquid)
        {
            SetContent(block.Block, block.Data, liquid.Liquid, liquid.Level, liquid.IsStatic, position, tickLiquid);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SetContent(Block block, uint data, Liquid liquid, LiquidLevel level, bool isStatic,
            Vector3i position, bool tickLiquid)
        {
            Chunk? chunk = GetChunkWithPosition(position);

            if (chunk == null) return;

            uint val = Section.Encode(block, data, liquid, level, isStatic);

            chunk.GetSection(position.Y >> Section.SectionSizeExp).SetContent(position, val);

            if (tickLiquid) liquid.TickNow(this, position, level, isStatic);

            // Block updates - Side is passed out of the perspective of the block receiving the block update.

            foreach (BlockSide side in BlockSide.All.Sides())
            {
                Vector3i neighborPosition = side.Offset(position);

                (BlockInstance? blockNeighbor, LiquidInstance? liquidNeighbor) = GetContent(neighborPosition);

                blockNeighbor?.Block.BlockUpdate(this, neighborPosition, data, side.Opposite());
                liquidNeighbor?.Liquid.TickSoon(this, neighborPosition, isStatic);
            }

            ProcessChangedSection(chunk, position);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetPosition(Block block, uint data, Liquid liquid, LiquidLevel level, bool isStatic,
            Vector3i position)
        {
            SetContent(block, data, liquid, level, isStatic, position, tickLiquid: true);
        }

        protected abstract void ProcessChangedSection(Chunk chunk, Vector3i position);

        /// <summary>
        ///     Modify the data of a position, without causing any updates.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ModifyWorldData(Vector3i position, uint clearMask, uint addMask)
        {
            Chunk? chunk = GetChunkWithPosition(position);

            if (chunk == null) return;

            uint val = chunk.GetSection(position.Y >> Section.SectionSizeExp).GetContent(position);

            val &= clearMask;
            val |= addMask;

            chunk.GetSection(position.Y >> Section.SectionSizeExp).SetContent(position, val);

            ProcessChangedSection(chunk, position);
        }

        public void SetDefaultBlock(Vector3i position)
        {
            SetBlock(BlockInstance.Default, position);
        }

        public void SetDefaultLiquid(Vector3i position)
        {
            SetLiquid(LiquidInstance.Default, position);
        }

        /// <summary>
        ///     Sets the spawn position of this world.
        /// </summary>
        /// <param name="position">The position to set as spawn.</param>
        public void SetSpawnPosition(Vector3 position)
        {
            Information.SpawnInformation = new SpawnInformation(position);

            logger.LogInformation(Events.WorldData, "World spawn position has been set to: {Position}", position);
        }

        /// <summary>
        ///     Get the spawn position of this world.
        /// </summary>
        /// <returns>The spawn position.</returns>
        public Vector3 GetSpawnPosition()
        {
            return Information.SpawnInformation.Position;
        }

        /// <summary>
        ///     Saves all active chunks that are not currently saved.
        /// </summary>
        /// <returns>A task that represents all tasks saving the chunks.</returns>
        public Task Save()
        {
            logger.LogInformation(Events.WorldIO, "Saving world");

            List<Task> savingTasks = new(activeChunks.Count);

            foreach (Chunk chunk in activeChunks.Values)
                if (!positionsSaving.Contains((chunk.X, chunk.Z)))
                    savingTasks.Add(chunk.SaveTask(ChunkDirectory));

            Information.Version = GameInformation.Instance.Version;

            savingTasks.Add(Task.Run(() => Information.Save(Path.Combine(WorldDirectory, "meta.json"))));

            return Task.WhenAll(savingTasks);
        }

        /// <summary>
        ///     Wait for all world tasks to finish.
        /// </summary>
        /// <returns>A task that is finished when all world tasks are finished.</returns>
        public Task FinishAll()
        {
            // This method is just a quick hack to fix a possible cause of crashes.
            // It would be better to also process the finished tasks.

            List<Task> tasks = new();
            AddAllTasks(tasks);

            return Task.WhenAll(tasks);
        }

        protected virtual void AddAllTasks(List<Task> tasks)
        {
            tasks.AddRange(chunkGenerateTasks);
            tasks.AddRange(chunkLoadingTasks);
            tasks.AddRange(chunkSavingTasks);
        }

        #region IDisposable Support

        private bool disposed;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    foreach (Chunk activeChunk in activeChunks.Values) activeChunk.Dispose();

                    foreach (Chunk generatingChunk in chunksGenerating.Values) generatingChunk.Dispose();

                    foreach (Chunk savingChunk in chunksSaving.Values) savingChunk.Dispose();
                }

                disposed = true;
            }
        }

        ~World()
        {
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable Support
    }
}