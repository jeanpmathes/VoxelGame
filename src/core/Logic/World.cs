// <copyright file="World.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OpenTK.Mathematics;
using Properties;
using VoxelGame.Core.Collections;
using VoxelGame.Core.Generation;
using VoxelGame.Core.Updates;
using VoxelGame.Logging;

namespace VoxelGame.Core.Logic
{
    /// <summary>
    ///     The world class. Contains everything that is in the world, e.g. chunks, entities, etc.
    /// </summary>
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
                    Version = ApplicationInformation.Instance.Version
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

        /// <summary>
        ///     The update counter counting the world updates.
        /// </summary>
        public UpdateCounter UpdateCounter { get; }

        private int MaxGenerationTasks { get; } = Settings.Default.MaxGenerationTasks;
        private int MaxLoadingTasks { get; } = Settings.Default.MaxLoadingTasks;

        private int MaxSavingTasks { get; } = Settings.Default.MaxSavingTasks;

        /// <summary>
        ///     The directory in which this world is stored.
        /// </summary>
        private string WorldDirectory { get; }

        /// <summary>
        ///     The directory in which all chunks of this world are stored.
        /// </summary>
        private string ChunkDirectory { get; }

        /// <summary>
        ///     Gets whether this world is ready for physics ticking and rendering.
        /// </summary>
        protected bool IsReady { get; set; }

        /// <summary>
        ///     Get the world creation seed.
        /// </summary>
        public int Seed => Information.Seed;

        /// <summary>
        /// Get or set the spawn position in this world.
        /// </summary>
        public Vector3 SpawnPosition
        {
            get => Information.SpawnInformation.Position;
            set
            {
                Information.SpawnInformation = new SpawnInformation(value);
                logger.LogInformation(Events.WorldData, "World spawn position has been set to: {Position}", value);
            }
        }

        private void Setup()
        {
            Directory.CreateDirectory(WorldDirectory);
            Directory.CreateDirectory(ChunkDirectory);

            positionsToActivate.Add((0, 0));
        }

        /// <summary>
        ///     Called every update cycle.
        /// </summary>
        /// <param name="deltaTime">The time since the last update cycle.</param>
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

        /// <summary>
        ///     Get the liquid at a given position. The liquid can only be retrieved from active chunks.
        /// </summary>
        /// <param name="position">The position in the world.</param>
        /// <returns>The liquid instance, if there is any.</returns>
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

        /// <summary>
        ///     Get both the liquid and block instance at a given position.
        ///     The content can only be retrieved from active chunks.
        /// </summary>
        /// <param name="position">The world position.</param>
        /// <returns>The content, if there is any.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public (BlockInstance block, LiquidInstance liquid)? GetContent(Vector3i position)
        {
            RetrieveContent(
                position,
                out Block? block,
                out uint data,
                out Liquid? liquid,
                out LiquidLevel level,
                out bool isStatic);

            if (block == null || liquid == null) return null;

            Debug.Assert(block != null);
            Debug.Assert(liquid != null);

            return (block.AsInstance(data), liquid.AsInstance(level, isStatic));
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

            SetContent(block, liquid, position, tickLiquid: false);
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

                (BlockInstance, LiquidInstance)? content = GetContent(neighborPosition);

                if (content == null) continue;
                (BlockInstance blockNeighbor, LiquidInstance liquidNeighbor) = content.Value;

                blockNeighbor.Block.BlockUpdate(this, neighborPosition, blockNeighbor.Data, side.Opposite());
                liquidNeighbor.Liquid.TickSoon(this, neighborPosition, liquidNeighbor.IsStatic);
            }

            ProcessChangedSection(chunk, position);
        }

        /// <summary>
        ///     Set all data at a world position.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetPosition(Block block, uint data, Liquid liquid, LiquidLevel level, bool isStatic,
            Vector3i position)
        {
            SetContent(block, data, liquid, level, isStatic, position, tickLiquid: true);
        }

        /// <summary>
        ///     Process that a section was changed.
        /// </summary>
        /// <param name="chunk">The chunk containing the section.</param>
        /// <param name="position">The position of the block that caused the section change.</param>
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

        /// <summary>
        ///     Set a position to the default block.
        /// </summary>
        public void SetDefaultBlock(Vector3i position)
        {
            SetBlock(BlockInstance.Default, position);
        }

        /// <summary>
        ///     Set a position to the default liquid.
        /// </summary>
        public void SetDefaultLiquid(Vector3i position)
        {
            SetLiquid(LiquidInstance.Default, position);
        }

        /// <summary>
        ///     Force a random update at a position.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <returns>True if both the liquid and block at the position received a random update.</returns>
        public bool DoRandomUpdate(Vector3i position)
        {
            (BlockInstance, LiquidInstance)? content = GetContent(position);

            if (content == null) return false;
            (BlockInstance block, LiquidInstance liquid) = content.Value;

            block.Block.RandomUpdate(this, position, block.Data);
            liquid.Liquid.RandomUpdate(this, position, liquid.Level, liquid.IsStatic);

            return true;
        }

        /// <summary>
        ///     Saves all active chunks that are not currently saved.
        /// </summary>
        /// <returns>A task that represents all tasks saving the chunks.</returns>
        public Task SaveAsync()
        {
            logger.LogInformation(Events.WorldIO, "Saving world");

            List<Task> savingTasks = new(activeChunks.Count);

            foreach (Chunk chunk in activeChunks.Values)
                if (!positionsSaving.Contains((chunk.X, chunk.Z)))
                    savingTasks.Add(chunk.SaveAsync(ChunkDirectory));

            Information.Version = ApplicationInformation.Instance.Version;

            savingTasks.Add(Task.Run(() => Information.Save(Path.Combine(WorldDirectory, "meta.json"))));

            return Task.WhenAll(savingTasks);
        }

        /// <summary>
        ///     Wait for all world tasks to finish.
        /// </summary>
        /// <returns>A task that is finished when all world tasks are finished.</returns>
        public Task FinishAllAsync()
        {
            // This method is just a quick hack to fix a possible cause of crashes.
            // It would be better to also process the finished tasks.

            List<Task> tasks = new();
            AddAllTasks(tasks);

            return Task.WhenAll(tasks);
        }

        /// <summary>
        ///     Add all tasks to the list. This is used to wait for all tasks to finish when calling <see cref="FinishAllAsync" />.
        /// </summary>
        /// <param name="tasks">The task list.</param>
        protected virtual void AddAllTasks(IList<Task> tasks)
        {
            chunkGenerateTasks.ForEach(tasks.Add);
            chunkLoadingTasks.ForEach(tasks.Add);
            chunkSavingTasks.ForEach(tasks.Add);
        }

        #region IDisposable Support

        private bool disposed;

        /// <summary>
        ///     Dispose of the world.
        /// </summary>
        /// <param name="disposing">True when disposing intentionally.</param>
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

        /// <summary>
        ///     Finalizer.
        /// </summary>
        ~World()
        {
            Dispose(disposing: false);
        }

        /// <summary>
        ///     Dispose of the world.
        /// </summary>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable Support
    }
}
