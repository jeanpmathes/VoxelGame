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
using VoxelGame.Core.Resources.Language;
using VoxelGame.Core.Updates;
using VoxelGame.Core.WorldGeneration;
using VoxelGame.Logging;

namespace VoxelGame.Core.Logic
{
    public abstract class World : IDisposable
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

        public WorldInformation Information { get; }

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

        protected abstract Chunk CreateChunk(int x, int z);

        public abstract void Update(float deltaTime);

        protected void StartActivatingChunks()
        {
            foreach ((int x, int z) in positionsToActivate)
                if (!positionsActivating.Contains((x, z)) && !activeChunks.ContainsKey((x, z)))
                {
                    string pathToChunk = ChunkDirectory + $@"\x{x}z{z}.chunk";
                    bool isActivating;

                    // Check if a file for the chunk position exists
                    if (File.Exists(pathToChunk))
                    {
                        isActivating = positionsToLoad.Enqueue((x, z));
                    }
                    else
                    {
#pragma warning disable CA2000 // Dispose objects before losing scope
                        isActivating = chunksToGenerate.Enqueue(CreateChunk(x, z));
#pragma warning restore CA2000 // Dispose objects before losing scope
                    }

                    if (isActivating) positionsActivating.Add((x, z));
                }

            positionsToActivate.Clear();
        }

        protected void FinishGeneratingChunks()
        {
            if (chunkGenerateTasks.Count > 0)
                for (int i = chunkGenerateTasks.Count - 1; i >= 0; i--)
                    if (chunkGenerateTasks[i].IsCompleted)
                    {
                        Task completed = chunkGenerateTasks[i];
                        Chunk generatedChunk = chunksGenerating[completed.Id];

                        chunkGenerateTasks.RemoveAt(i);
                        chunksGenerating.Remove(completed.Id);

                        positionsActivating.Remove((generatedChunk.X, generatedChunk.Z));

                        if (completed.IsFaulted)
                            throw completed.Exception?.GetBaseException() ?? new NullReferenceException();

                        if (!activeChunks.ContainsKey((generatedChunk.X, generatedChunk.Z)) &&
                            !positionsToReleaseOnActivation.Remove((generatedChunk.X, generatedChunk.Z)))
                        {
                            activeChunks.Add((generatedChunk.X, generatedChunk.Z), generatedChunk);

                            ProcessNewlyActivatedChunk(generatedChunk);
                        }
                        else
                        {
                            generatedChunk.Dispose();
                        }
                    }
        }

        protected void StartGeneratingChunks()
        {
            while (chunksToGenerate.Count > 0 && chunkGenerateTasks.Count < MaxGenerationTasks)
            {
                Chunk current = chunksToGenerate.Dequeue();
                Task currentTask = current.GenerateTask(generator);

                chunkGenerateTasks.Add(currentTask);
                chunksGenerating.Add(currentTask.Id, current);
            }
        }

        protected void FinishLoadingChunks()
        {
            if (chunkLoadingTasks.Count > 0)
                for (int i = chunkLoadingTasks.Count - 1; i >= 0; i--)
                    if (chunkLoadingTasks[i].IsCompleted)
                    {
                        Task<Chunk?> completed = chunkLoadingTasks[i];
                        (int x, int z) = positionsLoading[completed.Id];

                        chunkLoadingTasks.RemoveAt(i);
                        positionsLoading.Remove(completed.Id);

                        positionsActivating.Remove((x, z));

                        if (completed.IsFaulted)
                        {
                            if (!positionsToReleaseOnActivation.Remove((x, z)) || !activeChunks.ContainsKey((x, z)))
                            {
                                logger.LogError(
                                    Events.ChunkLoadingError,
                                    completed.Exception!.GetBaseException(),
                                    "An exception occurred when loading the chunk ({X}|{Z}). " +
                                    "The chunk has been scheduled for generation",
                                    x,
                                    z);

#pragma warning disable CA2000 // Dispose objects before losing scope
                                if (chunksToGenerate.Enqueue(CreateChunk(x, z)))
#pragma warning restore CA2000 // Dispose objects before losing scope
                                    positionsActivating.Add((x, z));
                            }
                        }
                        else
                        {
                            Chunk? loadedChunk = completed.Result;

                            if (loadedChunk != null && !activeChunks.ContainsKey((x, z)))
                            {
                                if (!positionsToReleaseOnActivation.Remove((loadedChunk.X, loadedChunk.Z)))
                                {
                                    loadedChunk.Setup(this, UpdateCounter);
                                    activeChunks.Add((x, z), loadedChunk);

                                    ProcessNewlyActivatedChunk(loadedChunk);
                                }
                                else
                                {
                                    loadedChunk.Dispose();
                                }
                            }
                            else
                            {
                                logger.LogError(
                                    Events.ChunkLoadingError,
                                    "Position of the loaded chunk file for position ({X}|{Z}) did not match the requested position, " +
                                    "which can be caused by a renamed chunk file. " +
                                    "Position will be scheduled for generation",
                                    x,
                                    z);

#pragma warning disable CA2000 // Dispose objects before losing scope
                                if (chunksToGenerate.Enqueue(CreateChunk(x, z)))
#pragma warning restore CA2000 // Dispose objects before losing scope
                                    positionsActivating.Add((x, z));
                            }
                        }
                    }
        }

        protected void StartLoadingChunks()
        {
            while (positionsToLoad.Count > 0 && chunkLoadingTasks.Count < MaxLoadingTasks)
            {
                (int x, int z) = positionsToLoad.Dequeue();

                // If a chunk is already being loaded or saved no new loading task is needed
                if (!positionsLoading.ContainsValue((x, z)))
                {
                    if (!positionsSaving.Contains((x, z)))
                    {
                        string pathToChunk = ChunkDirectory + $@"\x{x}z{z}.chunk";
                        Task<Chunk?> currentTask = Chunk.LoadTask(pathToChunk, x, z);

                        chunkLoadingTasks.Add(currentTask);
                        positionsLoading.Add(currentTask.Id, (x, z));
                    }
                    else
                    {
                        positionsActivatingThroughSaving.Add((x, z));
                    }
                }
            }
        }

        protected void FinishSavingChunks()
        {
            if (chunkSavingTasks.Count > 0)
                for (int i = chunkSavingTasks.Count - 1; i >= 0; i--)
                    if (chunkSavingTasks[i].IsCompleted)
                    {
                        Task completed = chunkSavingTasks[i];
                        Chunk completedChunk = chunksSaving[completed.Id];

                        chunkSavingTasks.RemoveAt(i);
                        chunksSaving.Remove(completed.Id);
                        positionsSaving.Remove((completedChunk.X, completedChunk.Z));

                        // Check if the chunk should be activated and is not active and not requested to be released on activation; if true, the chunk will not be disposed
                        if ((positionsToActivate.Contains((completedChunk.X, completedChunk.Z)) ||
                             positionsActivating.Contains((completedChunk.X, completedChunk.Z)))
                            && !activeChunks.ContainsKey((completedChunk.X, completedChunk.Z))
                            && !positionsToReleaseOnActivation.Contains((completedChunk.X, completedChunk.Z)))
                        {
                            positionsToActivate.Remove((completedChunk.X, completedChunk.Z));

                            if (positionsActivatingThroughSaving.Remove((completedChunk.X, completedChunk.Z)))
                                positionsActivating.Remove((completedChunk.X, completedChunk.Z));

                            activeChunks.Add((completedChunk.X, completedChunk.Z), completedChunk);

                            ProcessNewlyActivatedChunk(completedChunk);
                        }
                        else
                        {
                            if (completed.IsFaulted)
                                logger.LogError(
                                    Events.ChunkSavingError,
                                    completed.Exception!.GetBaseException(),
                                    "An exception occurred when saving chunk ({X}|{Z}). " +
                                    "Chunk will be disposed without saving",
                                    completedChunk.X,
                                    completedChunk.Z);

                            if (positionsActivatingThroughSaving.Remove((completedChunk.X, completedChunk.Z)))
                                positionsActivating.Remove((completedChunk.X, completedChunk.Z));

                            positionsToReleaseOnActivation.Remove((completedChunk.X, completedChunk.Z));

                            completedChunk.Dispose();
                        }
                    }
        }

        protected abstract void ProcessNewlyActivatedChunk(Chunk activatedChunk);

        protected void StartSavingChunks()
        {
            while (chunksToSave.Count > 0 && chunkSavingTasks.Count < MaxSavingTasks)
            {
                Chunk current = chunksToSave.Dequeue();
                Task currentTask = current.SaveTask(ChunkDirectory);

                chunkSavingTasks.Add(currentTask);
                chunksSaving.Add(currentTask.Id, current);
                positionsSaving.Add((current.X, current.Z));
            }
        }

        /// <summary>
        ///     Requests the activation of a chunk. This chunk will either be loaded or generated.
        /// </summary>
        /// <param name="x">The x coordinates in chunk coordinates.</param>
        /// <param name="z">The z coordinates in chunk coordinates.</param>
        public void RequestChunk(int x, int z)
        {
            positionsToReleaseOnActivation.Remove((x, z));

            if (!positionsActivating.Contains((x, z)) && !activeChunks.ContainsKey((x, z)))
            {
                positionsToActivate.Add((x, z));

                logger.LogDebug(Events.ChunkRequest, "Chunk ({X}|{Z}) has been requested successfully", x, z);
            }
        }

        /// <summary>
        ///     Notifies the world that a chunk is no longer needed. The world decides if the chunk is deactivated.
        /// </summary>
        /// <param name="x">The x coordinates in chunk coordinates.</param>
        /// <param name="z">The z coordinates in chunk coordinates.</param>
        /// <returns>true if the chunk will be released; false if not.</returns>
        public bool ReleaseChunk(int x, int z)
        {
            // Check if the chunk can be released
            if (x == 0 && z == 0) return false; // The chunk at (0|0) cannot be released.

            var canRelease = false;

            // Check if the chunk exists
            if (activeChunks.TryGetValue((x, z), out Chunk? chunk))
            {
                activeChunks.Remove((x, z));
                chunksToSave.Enqueue(chunk);

                logger.LogDebug(Events.ChunkRelease, "Released chunk ({X}|{Z})", x, z);

                canRelease = true;
            }

            if (positionsActivating.Contains((x, z)))
            {
                positionsToReleaseOnActivation.Add((x, z));

                logger.LogDebug(Events.ChunkRelease, "Scheduled to release chunk ({X}|{Z}) after activation", x, z);

                canRelease = true;
            }

            if (positionsToActivate.Contains((x, z)))
            {
                positionsToActivate.Remove((x, z));

                logger.LogDebug(Events.ChunkRelease, "Removed chunk ({X}|{Z}) from activation list", x, z);

                canRelease = true;
            }

            return canRelease;
        }

        /// <summary>
        ///     Returns the block at a given position in block coordinates. The block is only searched in active chunks.
        /// </summary>
        /// <param name="position">The block position.</param>
        /// <param name="data">The block data at the position.</param>
        /// <returns>The Block at the given position or null if the block was not found.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Block? GetBlock(Vector3i position, out uint data)
        {
            return GetBlock(position, out data, out _, out _, out _);
        }

        /// <summary>
        ///     Returns the block at a given position in block coordinates. The block is only searched in active chunks.
        /// </summary>
        /// <param name="position">The block position.</param>
        /// <param name="data">The block data at the position.</param>
        /// <param name="liquid">The liquid at the position.</param>
        /// <param name="level">The liquid level of the position.</param>
        /// <param name="isStatic">If the liquid at that position is static.</param>
        /// <returns>The Block at x, y, z or null if the block was not found.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Block? GetBlock(Vector3i position, out uint data, out Liquid? liquid, out LiquidLevel level,
            out bool isStatic)
        {
            Chunk? chunk = GetChunkWithBlock(position);

            if (chunk != null)
            {
                uint val = chunk.GetSection(position.Y >> Section.SectionSizeExp).GetContent(position);

                Section.Decode(val, out Block block, out data, out liquid, out level, out isStatic);

                return block;
            }

            data = 0;
            liquid = null;
            level = 0;
            isStatic = false;

            return null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Liquid? GetLiquid(Vector3i position, out LiquidLevel level, out bool isStatic)
        {
            return GetPosition(position, out _, out level, out isStatic).liquid;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public (Block? block, Liquid? liquid) GetPosition(Vector3i position, out uint data, out LiquidLevel level,
            out bool isStatic)
        {
            Block? block = GetBlock(position, out data, out Liquid? liquid, out level, out isStatic);

            return (block, liquid);
        }

        /// <summary>
        ///     Sets a block in the world, adds the changed sections to the re-mesh set and sends block updates to the neighbors of
        ///     the changed block.
        /// </summary>
        /// <param name="block">The block which should be set at the position.</param>
        /// <param name="data">The block data which should be set at the position.</param>
        /// <param name="position">The block position.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetBlock(Block block, uint data, Vector3i position)
        {
            Liquid liquid = GetPosition(position, out _, out LiquidLevel level, out bool isStatic).liquid ??
                            Liquid.None;

            SetPosition(block, data, liquid, level, isStatic, position, tickLiquid: true);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetLiquid(Liquid liquid, LiquidLevel level, bool isStatic, Vector3i position)
        {
            Block block = GetBlock(position, out uint data, out _, out _, out _) ?? Block.Air;
            SetPosition(block, data, liquid, level, isStatic, position, tickLiquid: false);
        }

        /// <summary>
        ///     Set the <c>isStatic</c> flag of a liquid without causing any updates around this liquid.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void ModifyLiquid(bool isStatic, Vector3i position)
        {
            ModifyWorldData(position, ~Section.StaticMask, isStatic ? Section.StaticMask : 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SetPosition(Block block, uint data, Liquid liquid, LiquidLevel level, bool isStatic,
            Vector3i position, bool tickLiquid)
        {
            Chunk? chunk = GetChunkWithBlock(position);

            if (chunk == null) return;

            uint val = Section.Encode(block, data, liquid, level, isStatic);

            chunk.GetSection(position.Y >> Section.SectionSizeExp).SetContent(position, val);

            if (tickLiquid) liquid.TickNow(this, position, level, isStatic);

            // Block updates - Side is passed out of the perspective of the block receiving the block update.

            foreach (BlockSide side in BlockSide.All.Sides())
            {
                Vector3i neighborPosition = side.Offset(position);

                (Block? blockNeighbor, Liquid? liquidNeighbor) = GetPosition(
                    neighborPosition,
                    out data,
                    out _,
                    out isStatic);

                blockNeighbor?.BlockUpdate(this, neighborPosition, data, side.Opposite());
                liquidNeighbor?.TickSoon(this, neighborPosition, isStatic);
            }

            ProcessChangedSection(chunk, position);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetPosition(Block block, uint data, Liquid liquid, LiquidLevel level, bool isStatic,
            Vector3i position)
        {
            SetPosition(block, data, liquid, level, isStatic, position, tickLiquid: true);
        }

        protected abstract void ProcessChangedSection(Chunk chunk, Vector3i position);

        /// <summary>
        ///     Modify the data of a position, without causing any updates.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ModifyWorldData(Vector3i position, uint clearMask, uint addMask)
        {
            Chunk? chunk = GetChunkWithBlock(position);

            if (chunk == null) return;

            uint val = chunk.GetSection(position.Y >> Section.SectionSizeExp).GetContent(position);

            val &= clearMask;
            val |= addMask;

            chunk.GetSection(position.Y >> Section.SectionSizeExp).SetContent(position, val);

            ProcessChangedSection(chunk, position);
        }

        public void SetDefaultBlock(Vector3i position)
        {
            SetBlock(Block.Air, data: 0, position);
        }

        public void SetDefaultLiquid(Vector3i position)
        {
            SetLiquid(Liquid.None, LiquidLevel.Eight, isStatic: true, position);
        }

        /// <summary>
        ///     Gets an active chunk.
        /// </summary>
        /// <param name="x">The x position of the chunk in chunk coordinates.</param>
        /// <param name="z">The y position of the chunk in chunk coordinates.</param>
        /// <returns>The chunk at the given position or null if no active chunk was found.</returns>
        public Chunk? GetChunk(int x, int z)
        {
            activeChunks.TryGetValue((x, z), out Chunk? chunk);

            return chunk;
        }

        /// <summary>
        ///     Gets the chunk that contains the specified position. If the chunk is not active, null is returned.
        /// </summary>
        /// <param name="position">The position. The y component is ignored.</param>
        /// <returns>The chunk if it exists, null if not.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Chunk? GetChunkOfPosition(Vector3i position)
        {
            activeChunks.TryGetValue(
                (position.X >> Section.SectionSizeExp, position.Z >> Section.SectionSizeExp),
                out Chunk? chunk);

            return chunk;
        }

        /// <summary>
        ///     Gets a section of an active chunk.
        /// </summary>
        /// <param name="sectionPosition">The position of the section, in section coordinates.</param>
        /// <returns>The section at the given position or null if no section was found.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Section? GetSection(Vector3i sectionPosition)
        {
            (int x, int y, int z) = sectionPosition;

            if (activeChunks.TryGetValue((x, z), out Chunk? chunk) && y is >= 0 and < Chunk.VerticalSectionCount)
                return chunk.GetSection(y);

            return null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Chunk? GetChunkWithBlock(Vector3i blockPosition)
        {
            bool exists = activeChunks.TryGetValue(
                (blockPosition.X >> Section.SectionSizeExp, blockPosition.Z >> Section.SectionSizeExp),
                out Chunk? chunk);

            if (!exists || blockPosition.Y is < 0 or >= Chunk.ChunkHeight) return null;

            return chunk;
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
        ///     Saves all active chunks that are not currently saved.
        /// </summary>
        /// <returns>A task that represents all tasks saving the chunks.</returns>
        public Task Save()
        {
            Console.WriteLine(Language.AllChunksSaving);
            Console.WriteLine();

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

#pragma warning disable CA1051 // Do not declare visible instance fields

        /// <summary>
        ///     A set of chunk positions which are currently not active and should either be loaded or generated.
        /// </summary>
        protected readonly HashSet<(int x, int z)> positionsToActivate;

        /// <summary>
        ///     A set of chunk positions that are currently being activated. No new chunks for these positions should be created.
        /// </summary>
        protected readonly HashSet<(int, int)> positionsActivating;

        /// <summary>
        ///     A queue that contains all chunks that have to be generated.
        /// </summary>
        protected readonly UniqueQueue<Chunk> chunksToGenerate;

        /// <summary>
        ///     A list of chunk generation tasks.
        /// </summary>
        protected readonly List<Task> chunkGenerateTasks;

        /// <summary>
        ///     A dictionary containing all chunks that are currently generated, with the task id of their generating task as key.
        /// </summary>
        protected readonly Dictionary<int, Chunk> chunksGenerating;

        /// <summary>
        ///     A queue that contains all positions that have to be loaded.
        /// </summary>
        protected readonly UniqueQueue<(int x, int z)> positionsToLoad;

        /// <summary>
        ///     A list of chunk loading tasks.
        /// </summary>
        protected readonly List<Task<Chunk?>> chunkLoadingTasks;

        /// <summary>
        ///     A dictionary containing all chunk positions that are currently loaded, with the task id of their loading task as
        ///     key.
        /// </summary>
        protected readonly Dictionary<int, (int x, int z)> positionsLoading;

        /// <summary>
        ///     A dictionary that contains all active chunks.
        /// </summary>
        protected readonly Dictionary<ValueTuple<int, int>, Chunk> activeChunks;

        /// <summary>
        ///     A set of chunk positions that should be released on their activation.
        /// </summary>
        protected readonly HashSet<(int x, int z)> positionsToReleaseOnActivation;

        /// <summary>
        ///     A queue of chunks that should be saved and disposed.
        /// </summary>
        protected readonly UniqueQueue<Chunk> chunksToSave;

        /// <summary>
        ///     A list of chunk saving tasks.
        /// </summary>
        protected readonly List<Task> chunkSavingTasks;

        /// <summary>
        ///     A dictionary containing all chunks that are currently saved, with the task id of their saving task as key.
        /// </summary>
        protected readonly Dictionary<int, Chunk> chunksSaving;

        /// <summary>
        ///     A set containing all positions that are currently saved.
        /// </summary>
        protected readonly HashSet<(int x, int z)> positionsSaving;

        /// <summary>
        ///     A set of positions that have no task activating them and have to be activated by the saving code.
        /// </summary>
        protected readonly HashSet<(int x, int z)> positionsActivatingThroughSaving;

#pragma warning restore CA1051 // Do not declare visible instance fields

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