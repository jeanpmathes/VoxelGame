// <copyright file="World.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using Microsoft.Extensions.Logging;
using OpenToolkit.Mathematics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using VoxelGame.Core.Collections;
using VoxelGame.Core.Resources.Language;
using VoxelGame.Core.Updates;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.WorldGeneration;

namespace VoxelGame.Core.Logic
{
    public abstract class World : IDisposable
    {
        private static readonly ILogger Logger = LoggingHelper.CreateLogger<World>();

        public WorldInformation Information { get; }

        public UpdateCounter UpdateCounter { get; }

        protected int MaxGenerationTasks { get; } = Properties.core.Default.MaxGenerationTasks;
        protected int MaxLoadingTasks { get; } = Properties.core.Default.MaxLoadingTasks;

        protected int MaxSavingTasks { get; } = Properties.core.Default.MaxSavingTasks;

        protected string WorldDirectory { get; }
        protected string ChunkDirectory { get; }

        protected int SectionSizeExp { get; } = (int)Math.Log(Section.SectionSize, 2);
        protected int ChunkHeightExp { get; } = (int)Math.Log(Chunk.ChunkHeight, 2);

        /// <summary>
        /// Gets whether this world is ready for physics ticking and rendering.
        /// </summary>
        public bool IsReady { get; protected set; }

        private readonly IWorldGenerator generator;

#pragma warning disable CA1051 // Do not declare visible instance fields

        /// <summary>
        /// A set of chunk positions which are currently not active and should either be loaded or generated.
        /// </summary>
        protected readonly HashSet<(int x, int z)> positionsToActivate;

        /// <summary>
        /// A set of chunk positions that are currently being activated. No new chunks for these positions should be created.
        /// </summary>
        protected readonly HashSet<(int, int)> positionsActivating;

        /// <summary>
        /// A queue that contains all chunks that have to be generated.
        /// </summary>
        protected readonly UniqueQueue<Chunk> chunksToGenerate;

        /// <summary>
        /// A list of chunk generation tasks.
        /// </summary>
        protected readonly List<Task> chunkGenerateTasks;

        /// <summary>
        /// A dictionary containing all chunks that are currently generated, with the task id of their generating task as key.
        /// </summary>
        protected readonly Dictionary<int, Chunk> chunksGenerating;

        /// <summary>
        /// A queue that contains all positions that have to be loaded.
        /// </summary>
        protected readonly UniqueQueue<(int x, int z)> positionsToLoad;

        /// <summary>
        /// A list of chunk loading tasks.
        /// </summary>
        protected readonly List<Task<Chunk?>> chunkLoadingTasks;

        /// <summary>
        /// A dictionary containing all chunk positions that are currently loaded, with the task id of their loading task as key.
        /// </summary>
        protected readonly Dictionary<int, (int x, int z)> positionsLoading;

        /// <summary>
        /// A dictionary that contains all active chunks.
        /// </summary>
        protected readonly Dictionary<ValueTuple<int, int>, Chunk> activeChunks;

        /// <summary>
        /// A set of chunk positions that should be released on their activation.
        /// </summary>
        protected readonly HashSet<(int x, int z)> positionsToReleaseOnActivation;

        /// <summary>
        /// A queue of chunks that should be saved and disposed.
        /// </summary>
        protected readonly UniqueQueue<Chunk> chunksToSave;

        /// <summary>
        /// A list of chunk saving tasks.
        /// </summary>
        protected readonly List<Task> chunkSavingTasks;

        /// <summary>
        /// A dictionary containing all chunks that are currently saved, with the task id of their saving task as key.
        /// </summary>
        protected readonly Dictionary<int, Chunk> chunksSaving;

        /// <summary>
        /// A set containing all positions that are currently saved.
        /// </summary>
        protected readonly HashSet<(int x, int z)> positionsSaving;

        /// <summary>
        /// A set of positions that have no task activating them and have to be activated by the saving code.
        /// </summary>
        protected readonly HashSet<(int x, int z)> positionsActivatingThroughSaving;

#pragma warning restore CA1051 // Do not declare visible instance fields

        /// <summary>
        /// This constructor is meant for worlds that are new.
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
                worldDirectory: path,
                chunkDirectory: path + "/Chunks",
                new ComplexGenerator(seed))
        {
            Information.Save(Path.Combine(WorldDirectory, "meta.json"));

            Logger.LogInformation("Created a new world.");
        }

        /// <summary>
        /// This constructor is meant for worlds that already exist.
        /// </summary>
        protected World(WorldInformation information, string path) :
            this(
                information,
                worldDirectory: path,
                chunkDirectory: path + "/Chunks",
                new ComplexGenerator(information.Seed))
        {
            Logger.LogInformation("Loaded an existing world.");
        }

        /// <summary>
        /// Setup of readonly fields and non-optional steps.
        /// </summary>
        private World(WorldInformation information, string worldDirectory, string chunkDirectory, IWorldGenerator generator)
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
            {
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

                    if (isActivating)
                    {
                        positionsActivating.Add((x, z));
                    }
                }
            }

            positionsToActivate.Clear();
        }

        protected void FinishGeneratingChunks()
        {
            if (chunkGenerateTasks.Count > 0)
            {
                for (int i = chunkGenerateTasks.Count - 1; i >= 0; i--)
                {
                    if (chunkGenerateTasks[i].IsCompleted)
                    {
                        Task completed = chunkGenerateTasks[i];
                        Chunk generatedChunk = chunksGenerating[completed.Id];

                        chunkGenerateTasks.RemoveAt(i);
                        chunksGenerating.Remove(completed.Id);

                        positionsActivating.Remove((generatedChunk.X, generatedChunk.Z));

                        if (completed.IsFaulted)
                        {
                            throw completed.Exception?.GetBaseException() ?? new NullReferenceException();
                        }
                        else if (!activeChunks.ContainsKey((generatedChunk.X, generatedChunk.Z)) && !positionsToReleaseOnActivation.Remove((generatedChunk.X, generatedChunk.Z)))
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
            {
                for (int i = chunkLoadingTasks.Count - 1; i >= 0; i--)
                {
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
                                Logger.LogError(LoggingEvents.ChunkLoadingError, completed.Exception!.GetBaseException(), "An exception occurred when loading the chunk ({x}|{z}). " +
                                    "The chunk has been scheduled for generation", x, z);

#pragma warning disable CA2000 // Dispose objects before losing scope
                                if (chunksToGenerate.Enqueue(CreateChunk(x, z)))
#pragma warning restore CA2000 // Dispose objects before losing scope
                                {
                                    positionsActivating.Add((x, z));
                                }
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
                                Logger.LogError(LoggingEvents.ChunkLoadingError, "The position of the loaded chunk file for position ({x}|{z}) did not match the requested position. " +
                                    "This may be the result of a renamed chunk file. " +
                                    "The position will be scheduled for generation.", x, z);

#pragma warning disable CA2000 // Dispose objects before losing scope
                                if (chunksToGenerate.Enqueue(CreateChunk(x, z)))
#pragma warning restore CA2000 // Dispose objects before losing scope
                                {
                                    positionsActivating.Add((x, z));
                                }
                            }
                        }
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
            {
                for (int i = chunkSavingTasks.Count - 1; i >= 0; i--)
                {
                    if (chunkSavingTasks[i].IsCompleted)
                    {
                        Task completed = chunkSavingTasks[i];
                        Chunk completedChunk = chunksSaving[completed.Id];

                        chunkSavingTasks.RemoveAt(i);
                        chunksSaving.Remove(completed.Id);
                        positionsSaving.Remove((completedChunk.X, completedChunk.Z));

                        // Check if the chunk should be activated and is not active and not requested to be released on activation; if true, the chunk will not be disposed
                        if ((positionsToActivate.Contains((completedChunk.X, completedChunk.Z)) || positionsActivating.Contains((completedChunk.X, completedChunk.Z)))
                            && !activeChunks.ContainsKey((completedChunk.X, completedChunk.Z))
                            && !positionsToReleaseOnActivation.Contains((completedChunk.X, completedChunk.Z)))
                        {
                            positionsToActivate.Remove((completedChunk.X, completedChunk.Z));

                            if (positionsActivatingThroughSaving.Remove((completedChunk.X, completedChunk.Z)))
                            {
                                positionsActivating.Remove((completedChunk.X, completedChunk.Z));
                            }

                            activeChunks.Add((completedChunk.X, completedChunk.Z), completedChunk);

                            ProcessNewlyActivatedChunk(completedChunk);
                        }
                        else
                        {
                            if (completed.IsFaulted)
                            {
                                Logger.LogError(LoggingEvents.ChunkSavingError, completed.Exception!.GetBaseException(), "An exception occurred when saving chunk ({x}|{z}). " +
                                    "The chunk will be disposed without saving.", completedChunk.X, completedChunk.Z);
                            }

                            if (positionsActivatingThroughSaving.Remove((completedChunk.X, completedChunk.Z)))
                            {
                                positionsActivating.Remove((completedChunk.X, completedChunk.Z));
                            }

                            positionsToReleaseOnActivation.Remove((completedChunk.X, completedChunk.Z));

                            completedChunk.Dispose();
                        }
                    }
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
        /// Requests the activation of a chunk. This chunk will either be loaded or generated.
        /// </summary>
        /// <param name="x">The x coordinates in chunk coordinates.</param>
        /// <param name="z">The z coordinates in chunk coordinates.</param>
        public void RequestChunk(int x, int z)
        {
            positionsToReleaseOnActivation.Remove((x, z));

            if (!positionsActivating.Contains((x, z)) && !activeChunks.ContainsKey((x, z)))
            {
                positionsToActivate.Add((x, z));

                Logger.LogDebug(LoggingEvents.ChunkRequest, "Chunk ({x}|{z}) has been requested successfully.", x, z);
            }
        }

        /// <summary>
        /// Notifies the world that a chunk is no longer needed. The world decides if the chunk is deactivated.
        /// </summary>
        /// <param name="x">The x coordinates in chunk coordinates.</param>
        /// <param name="z">The z coordinates in chunk coordinates.</param>
        /// <returns>true if the chunk will be released; false if not.</returns>
        public bool ReleaseChunk(int x, int z)
        {
            // Check if the chunk can be released
            if (x == 0 && z == 0)
            {
                return false; // The chunk at (0|0) cannot be released.
            }

            bool canRelease = false;

            // Check if the chunk exists
            if (activeChunks.TryGetValue((x, z), out Chunk? chunk))
            {
                activeChunks.Remove((x, z));
                chunksToSave.Enqueue(chunk);

                Logger.LogDebug(LoggingEvents.ChunkRelease, "Released chunk ({x}|{z}).", x, z);

                canRelease = true;
            }

            if (positionsActivating.Contains((x, z)))
            {
                positionsToReleaseOnActivation.Add((x, z));

                Logger.LogDebug(LoggingEvents.ChunkRelease, "Scheduled to release chunk ({x}|{z}) after activation.", x, z);

                canRelease = true;
            }

            if (positionsToActivate.Contains((x, z)))
            {
                positionsToActivate.Remove((x, z));

                Logger.LogDebug(LoggingEvents.ChunkRelease, "Chunk ({x}|{z}) has been removed from activation list.", x, z);

                canRelease = true;
            }

            return canRelease;
        }

        /// <summary>
        /// Returns the block at a given position in block coordinates. The block is only searched in active chunks.
        /// </summary>
        /// <param name="x">The x position in block coordinates.</param>
        /// <param name="y">The y position in block coordinates.</param>
        /// <param name="z">The z position in block coordinates.</param>
        /// <param name="data">The block data at the position.</param>
        /// <returns>The Block at x, y, z or null if the block was not found.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Block? GetBlock(int x, int y, int z, out uint data)
        {
            return GetBlock(x, y, z, out data, out _, out _, out _);
        }

        /// <summary>
        /// Returns the block at a given position in block coordinates. The block is only searched in active chunks.
        /// </summary>
        /// <param name="x">The x position in block coordinates.</param>
        /// <param name="y">The y position in block coordinates.</param>
        /// <param name="z">The z position in block coordinates.</param>
        /// <param name="data">The block data at the position.</param>
        /// <param name="liquid">The liquid at the position.</param>
        /// <param name="level">The liquid level of the position.</param>
        /// <param name="isStatic">If the liquid at that position is static.</param>
        /// <returns>The Block at x, y, z or null if the block was not found.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Block? GetBlock(int x, int y, int z, out uint data, out Liquid? liquid, out LiquidLevel level, out bool isStatic)
        {
            if (activeChunks.TryGetValue((x >> SectionSizeExp, z >> SectionSizeExp), out Chunk? chunk) && y >= 0 && y < Chunk.ChunkHeight * Section.SectionSize)
            {
                uint val = chunk.GetSection(y >> ChunkHeightExp)[x & (Section.SectionSize - 1), y & (Section.SectionSize - 1), z & (Section.SectionSize - 1)];
                Section.Decode(val, out Block block, out data, out liquid, out level, out isStatic);

                return block;
            }
            else
            {
                data = 0;
                liquid = null;
                level = 0;
                isStatic = false;

                return null;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Liquid? GetLiquid(int x, int y, int z, out LiquidLevel level, out bool isStatic)
        {
            return GetPosition(x, y, z, out _, out level, out isStatic).liquid;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public (Block? block, Liquid? liquid) GetPosition(int x, int y, int z, out uint data, out LiquidLevel level, out bool isStatic)
        {
            Block? block = GetBlock(x, y, z, out data, out Liquid? liquid, out level, out isStatic);
            return (block, liquid);
        }

        /// <summary>
        /// Sets a block in the world, adds the changed sections to the re-mesh set and sends block updates to the neighbors of the changed block.
        /// </summary>
        /// <param name="block">The block which should be set at the position.</param>
        /// <param name="data">The block data which should be set at the position.</param>
        /// <param name="x">The x position of the block to set.</param>
        /// <param name="y">The y position of the block to set.</param>
        /// <param name="z">The z position of the block to set.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetBlock(Block block, uint data, int x, int y, int z)
        {
            Liquid liquid = GetPosition(x, y, z, out _, out LiquidLevel level, out bool isStatic).liquid ?? Liquid.None;
            SetPosition(block, data, liquid, level, isStatic, x, y, z, true);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetLiquid(Liquid liquid, LiquidLevel level, bool isStatic, int x, int y, int z)
        {
            Block block = GetBlock(x, y, z, out uint data, out _, out _, out _) ?? Block.Air;
            SetPosition(block, data, liquid, level, isStatic, x, y, z, false);
        }

        /// <summary>
        /// Set the <c>isStatic</c> flag of a liquid without causing any updates around this liquid.
        /// </summary>
        /// <param name="isStatic"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void ModifyLiquid(bool isStatic, int x, int y, int z)
        {
            ModifyWorldData(x, y, z, ~Section.STATICMASK, isStatic ? Section.STATICMASK : 0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SetPosition(Block block, uint data, Liquid liquid, LiquidLevel level, bool isStatic, int x, int y, int z, bool tickLiquid)
        {
            if (!activeChunks.TryGetValue((x >> SectionSizeExp, z >> SectionSizeExp), out Chunk? chunk) || y < 0 || y >= Chunk.ChunkHeight * Section.SectionSize)
            {
                return;
            }

            uint val = Section.Encode(block, data, liquid, level, isStatic);
            chunk.GetSection(y >> ChunkHeightExp)[x & (Section.SectionSize - 1), y & (Section.SectionSize - 1), z & (Section.SectionSize - 1)] = val;

            if (tickLiquid) liquid.TickNow(this, x, y, z, level, isStatic);

            // Block updates - Side is passed out of the perspective of the block receiving the block update.

            (Block? blockNeighbour, Liquid? liquidNeighbour) = GetPosition(x, y, z + 1, out data, out _, out isStatic);
            blockNeighbour?.BlockUpdate(this, x, y, z + 1, data, BlockSide.Back);
            liquidNeighbour?.TickSoon(this, x, y, z + 1, isStatic);

            (blockNeighbour, liquidNeighbour) = GetPosition(x, y, z - 1, out data, out _, out isStatic);
            blockNeighbour?.BlockUpdate(this, x, y, z - 1, data, BlockSide.Front);
            liquidNeighbour?.TickSoon(this, x, y, z - 1, isStatic);

            (blockNeighbour, liquidNeighbour) = GetPosition(x - 1, y, z, out data, out _, out isStatic);
            blockNeighbour?.BlockUpdate(this, x - 1, y, z, data, BlockSide.Right);
            liquidNeighbour?.TickSoon(this, x - 1, y, z, isStatic);

            (blockNeighbour, liquidNeighbour) = GetPosition(x + 1, y, z, out data, out _, out isStatic);
            blockNeighbour?.BlockUpdate(this, x + 1, y, z, data, BlockSide.Left);
            liquidNeighbour?.TickSoon(this, x + 1, y, z, isStatic);

            (blockNeighbour, liquidNeighbour) = GetPosition(x, y - 1, z, out data, out _, out isStatic);
            blockNeighbour?.BlockUpdate(this, x, y - 1, z, data, BlockSide.Top);
            liquidNeighbour?.TickSoon(this, x, y - 1, z, isStatic);

            (blockNeighbour, liquidNeighbour) = GetPosition(x, y + 1, z, out data, out _, out isStatic);
            blockNeighbour?.BlockUpdate(this, x, y + 1, z, data, BlockSide.Bottom);
            liquidNeighbour?.TickSoon(this, x, y + 1, z, isStatic);

            ProcessChangedSection(chunk, x, y, z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetPosition(Block block, uint data, Liquid liquid, LiquidLevel level, bool isStatic, int x, int y, int z)
        {
            SetPosition(block, data, liquid, level, isStatic, x, y, z, true);
        }

        protected abstract void ProcessChangedSection(Chunk chunk, int x, int y, int z);

        /// <summary>
        /// Modify the data of a position, without causing any updates.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ModifyWorldData(int x, int y, int z, uint clearMask, uint addMask)
        {
            if (!activeChunks.TryGetValue((x >> SectionSizeExp, z >> SectionSizeExp), out Chunk? chunk) || y < 0 || y >= Chunk.ChunkHeight * Section.SectionSize)
            {
                return;
            }

            uint val = chunk.GetSection(y >> ChunkHeightExp)[x & (Section.SectionSize - 1), y & (Section.SectionSize - 1), z & (Section.SectionSize - 1)];

            val &= clearMask;
            val |= addMask;

            chunk.GetSection(y >> ChunkHeightExp)[x & (Section.SectionSize - 1), y & (Section.SectionSize - 1), z & (Section.SectionSize - 1)] = val;

            ProcessChangedSection(chunk, x, y, z);
        }

        public void SetDefaultBlock(int x, int y, int z)
        {
            SetBlock(Block.Air, 0, x, y, z);
        }

        public void SetDefaultLiquid(int x, int y, int z)
        {
            SetLiquid(Liquid.None, LiquidLevel.Eight, true, x, y, z);
        }

        /// <summary>
        /// Gets an active chunk.
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
        /// Gets the chunk that contains the specified position. If the chunk is not active, null is returned.
        /// </summary>
        /// <param name="x">The x position.</param>
        /// <param name="z">The y position.</param>
        /// <returns>The chunk if it exists, null if not.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Chunk? GetChunkOfPosition(int x, int z)
        {
            activeChunks.TryGetValue((x >> SectionSizeExp, z >> SectionSizeExp), out Chunk? chunk);
            return chunk;
        }

        /// <summary>
        /// Gets a section of an active chunk.
        /// </summary>
        /// <param name="chunkPosition">The position of the section, in chunk coordinates.</param>
        /// <returns>The section at the given position or null if no section was found.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Section? GetSection(Vector3i chunkPosition)
        {
            (int x, int y, int z) = chunkPosition;
            if (activeChunks.TryGetValue((x, z), out Chunk? chunk) && y >= 0 && y < Chunk.ChunkHeight)
            {
                return chunk.GetSection(y);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Sets the spawn position of this world.
        /// </summary>
        /// <param name="position">The position to set as spawn.</param>
        public void SetSpawnPosition(Vector3 position)
        {
            Information.SpawnInformation = new SpawnInformation(position);

            Logger.LogInformation("World spawn position has been set to: {position}", position);
        }

        /// <summary>
        /// Saves all active chunks that are not currently saved.
        /// </summary>
        /// <returns>A task that represents all tasks saving the chunks.</returns>
        public Task Save()
        {
            Console.WriteLine(Language.AllChunksSaving);
            Console.WriteLine();

            Logger.LogInformation("Saving world.");

            List<Task> savingTasks = new List<Task>(activeChunks.Count);

            foreach (Chunk chunk in activeChunks.Values)
            {
                if (!positionsSaving.Contains((chunk.X, chunk.Z)))
                {
                    savingTasks.Add(chunk.SaveTask(ChunkDirectory));
                }
            }

            Information.Version = GameInformation.Instance.Version;

            savingTasks.Add(Task.Run(() => Information.Save(Path.Combine(WorldDirectory, "meta.json"))));

            return Task.WhenAll(savingTasks);
        }

        /// <summary>
        /// Wait for all world tasks to finish.
        /// </summary>
        /// <returns>A task that is finished when all world tasks are finished.</returns>
        public Task FinishAll()
        {
            // This method is just a quick hack to fix a possible cause of crashes.
            // It would be better to also process the finished tasks.

            List<Task> tasks = new List<Task>();
            AddAllTasks(ref tasks);
            return Task.WhenAll(tasks);
        }

        protected virtual void AddAllTasks(ref List<Task> tasks)
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
                    foreach (Chunk activeChunk in activeChunks.Values)
                    {
                        activeChunk.Dispose();
                    }

                    foreach (Chunk generatingChunk in chunksGenerating.Values)
                    {
                        generatingChunk.Dispose();
                    }

                    foreach (Chunk savingChunk in chunksSaving.Values)
                    {
                        savingChunk.Dispose();
                    }
                }

                disposed = true;
            }
        }

        ~World()
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