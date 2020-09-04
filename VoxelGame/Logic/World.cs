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
using VoxelGame.Collections;
using VoxelGame.Rendering;
using VoxelGame.Resources.Language;
using VoxelGame.Utilities;
using VoxelGame.WorldGeneration;

namespace VoxelGame.Logic
{
    internal class World : IDisposable
    {
        private static readonly ILogger logger = Program.CreateLogger<World>();

        public const int ChunkExtents = 5;

        public WorldInformation Information { get; }

        private readonly int maxGenerationTasks = Config.GetInt("maxGenerationTasks", 15);
        private readonly int maxLoadingTasks = Config.GetInt("maxLoadingTasks", 15);
        private readonly int maxMeshingTasks = Config.GetInt("maxMeshingTasks", 5);
        private readonly int maxMeshDataSends = Config.GetInt("maxMeshDataSends", 2);
        private readonly int maxSavingTasks = Config.GetInt("maxSavingTasks", 15);

        private readonly string worldDirectory;
        private readonly string chunkDirectory;

        private readonly int sectionSizeExp = (int)Math.Log(Section.SectionSize, 2);
        private readonly int chunkHeightExp = (int)Math.Log(Chunk.ChunkHeight, 2);

        /// <summary>
        /// Gets whether this world is ready for physics ticking and rendering.
        /// </summary>
        public bool IsReady { get; private set; }

        private readonly IWorldGenerator generator;

        /// <summary>
        /// A set of chunk positions which are currently not active and should either be loaded or generated.
        /// </summary>
        private readonly HashSet<(int x, int z)> positionsToActivate;

        /// <summary>
        /// A set of chunk positions that are currently being activated. No new chunks for these positions should be created.
        /// </summary>
        private readonly HashSet<(int, int)> positionsActivating;

        /// <summary>
        /// A queue that contains all chunks that have to be generated.
        /// </summary>
        private readonly UniqueQueue<Chunk> chunksToGenerate;

        /// <summary>
        /// A list of chunk generation tasks.
        /// </summary>
        private readonly List<Task> chunkGenerateTasks;

        /// <summary>
        /// A dictionary containing all chunks that are currently generated, with the task id of their generating task as key.
        /// </summary>
        private readonly Dictionary<int, Chunk> chunksGenerating;

        /// <summary>
        /// A queue that contains all positions that have to be loaded.
        /// </summary>
        private readonly UniqueQueue<(int x, int z)> positionsToLoad;

        /// <summary>
        /// A list of chunk loading tasks.
        /// </summary>
        private readonly List<Task<Chunk?>> chunkLoadingTasks;

        /// <summary>
        /// A dictionary containing all chunk positions that are currently loaded, with the task id of their loading task as key.
        /// </summary>
        private readonly Dictionary<int, (int x, int z)> positionsLoading;

        /// <summary>
        /// A dictionary that contains all active chunks.
        /// </summary>
        private readonly Dictionary<ValueTuple<int, int>, Chunk> activeChunks;

        /// <summary>
        /// A queue with chunks that have to be meshed completely, mainly new chunks.
        /// </summary>
        private readonly UniqueQueue<Chunk> chunksToMesh;

        /// <summary>
        /// A list of chunk meshing tasks,
        /// </summary>
        private readonly List<Task<SectionMeshData[]>> chunkMeshingTasks;

        /// <summary>
        /// A dictionary containing all chunks that are currently meshed, with the task id of their meshing task as key.
        /// </summary>
        private readonly Dictionary<int, Chunk> chunksMeshing;

        /// <summary>
        /// A list of chunks where the mesh data has to be set;
        /// </summary>
        private readonly List<(Chunk chunk, Task<SectionMeshData[]> chunkMeshingTask)> chunksToSendMeshData;

        /// <summary>
        /// A set of chunks with information on which sections of them are to mesh.
        /// </summary>
        private readonly HashSet<(Chunk chunk, int index)> sectionsToMesh;

        /// <summary>
        /// A set of chunk positions that should be released on their activation.
        /// </summary>
        private readonly HashSet<(int x, int z)> positionsToReleaseOnActivation;

        /// <summary>
        /// A queue of chunks that should be saved and disposed.
        /// </summary>
        private readonly UniqueQueue<Chunk> chunksToSave;

        /// <summary>
        /// A list of chunk saving tasks.
        /// </summary>
        private readonly List<Task> chunkSavingTasks;

        /// <summary>
        /// A dictionary containing all chunks that are currently saved, with the task id of their saving task as key.
        /// </summary>
        private readonly Dictionary<int, Chunk> chunksSaving;

        /// <summary>
        /// A set containing all positions that are currently saved.
        /// </summary>
        private readonly HashSet<(int x, int z)> positionsSaving;

        /// <summary>
        /// A set of positions that have no task activating them and have to be activated by the saving code.
        /// </summary>
        private readonly HashSet<(int x, int z)> positionsActivatingThroughSaving;

        /// <summary>
        /// This constructor is meant for worlds that are new.
        /// </summary>
        public World(string name, string path, int seed) :
            this(
                new WorldInformation
                {
                    Name = name,
                    Seed = seed,
                    Creation = DateTime.Now,
                    Version = Program.Version
                },
                worldDirectory: path,
                chunkDirectory: path + "/Chunks",
                new ComplexGenerator(seed))
        {
            Information.Save(Path.Combine(worldDirectory, "meta.json"));

            logger.LogInformation("Created a new world.");
        }

        /// <summary>
        /// This constructor is meant for worlds that already exist.
        /// </summary>
        public World(WorldInformation information, string path) :
            this(
                information,
                worldDirectory: path,
                chunkDirectory: path + "/Chunks",
                new ComplexGenerator(information.Seed))
        {
            logger.LogInformation("Loaded an existing world.");
        }

        /// <summary>
        /// Setup of readonly fields and non-optional steps.
        /// </summary>
        private World(WorldInformation information, string worldDirectory, string chunkDirectory, IWorldGenerator generator)
        {
            positionsToActivate = new HashSet<(int x, int z)>();
            positionsActivating = new HashSet<(int, int)>();
            chunksToGenerate = new UniqueQueue<Chunk>();
            chunkGenerateTasks = new List<Task>(maxGenerationTasks);
            chunksGenerating = new Dictionary<int, Chunk>(maxGenerationTasks);
            positionsToLoad = new UniqueQueue<(int x, int z)>();
            chunkLoadingTasks = new List<Task<Chunk?>>(maxLoadingTasks);
            positionsLoading = new Dictionary<int, (int x, int z)>(maxLoadingTasks);
            activeChunks = new Dictionary<ValueTuple<int, int>, Chunk>();
            chunksToMesh = new UniqueQueue<Chunk>();
            chunkMeshingTasks = new List<Task<SectionMeshData[]>>(maxMeshingTasks);
            chunksMeshing = new Dictionary<int, Chunk>(maxMeshingTasks);
            chunksToSendMeshData = new List<(Chunk chunk, Task<SectionMeshData[]> chunkMeshingTask)>(maxMeshDataSends);
            sectionsToMesh = new HashSet<(Chunk chunk, int index)>();
            positionsToReleaseOnActivation = new HashSet<(int x, int z)>();
            chunksToSave = new UniqueQueue<Chunk>();
            chunkSavingTasks = new List<Task>(maxSavingTasks);
            chunksSaving = new Dictionary<int, Chunk>(maxSavingTasks);
            positionsSaving = new HashSet<(int x, int z)>(maxSavingTasks);
            positionsActivatingThroughSaving = new HashSet<(int x, int z)>();

            Information = information;

            this.worldDirectory = worldDirectory;
            this.chunkDirectory = chunkDirectory;
            this.generator = generator;

            Setup();
        }

        private void Setup()
        {
            Directory.CreateDirectory(worldDirectory);
            Directory.CreateDirectory(chunkDirectory);

            positionsToActivate.Add((0, 0));
        }

        public void Render()
        {
            if (IsReady)
            {
                // Collect all chunks to render
                for (int x = -Game.Player.RenderDistance; x <= Game.Player.RenderDistance; x++)
                {
                    for (int z = -Game.Player.RenderDistance; z <= Game.Player.RenderDistance; z++)
                    {
                        if (activeChunks.TryGetValue((Game.Player.ChunkX + x, Game.Player.ChunkZ + z), out Chunk? chunk))
                        {
                            chunk.RenderCulled(Game.Player.Frustum);
                        }
                    }
                }

                // Render the player
                Game.Player.Render();
            }
        }

        public void Update(float deltaTime)
        {
            StartActivatingChunks();

            FinishGeneratingChunks();
            StartGeneratingChunks();

            FinishLoadingChunks();
            StartLoadingChunks();

            FinishMeshingChunks();
            StartMeshingChunks();
            SendMeshData();

            if (IsReady)
            {
                // Tick objects in world.
                foreach (Chunk chunk in activeChunks.Values)
                {
                    chunk.Tick();
                }

                Game.Player.Tick(deltaTime);

                // Mesh all listed sections.
                foreach ((Chunk chunk, int index) in sectionsToMesh)
                {
                    chunk.CreateAndSetMesh(index);
                }

                sectionsToMesh.Clear();
            }
            else
            {
                if (activeChunks.Count >= 25 && activeChunks.ContainsKey((0, 0)))
                {
                    IsReady = true;

                    logger.LogInformation("The world is ready.");
                }
            }

            FinishSavingChunks();
            StartSavingChunks();
        }

        private void StartActivatingChunks()
        {
            foreach ((int x, int z) in positionsToActivate)
            {
                if (!positionsActivating.Contains((x, z)) && !activeChunks.ContainsKey((x, z)))
                {
                    string pathToChunk = chunkDirectory + $@"\x{x}z{z}.chunk";
                    bool isActivating;

                    // Check if a file for the chunk position exists
                    if (File.Exists(pathToChunk))
                    {
                        isActivating = positionsToLoad.Enqueue((x, z));
                    }
                    else
                    {
#pragma warning disable CA2000 // Dispose objects before losing scope
                        isActivating = chunksToGenerate.Enqueue(new Chunk(x, z));
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

        private void FinishGeneratingChunks()
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

                            chunksToMesh.Enqueue(generatedChunk);

                            // Schedule to mesh the chunks around this chunk
                            if (activeChunks.TryGetValue((generatedChunk.X + 1, generatedChunk.Z), out Chunk? neighbor))
                            {
                                chunksToMesh.Enqueue(neighbor);
                            }

                            if (activeChunks.TryGetValue((generatedChunk.X - 1, generatedChunk.Z), out neighbor))
                            {
                                chunksToMesh.Enqueue(neighbor);
                            }

                            if (activeChunks.TryGetValue((generatedChunk.X, generatedChunk.Z + 1), out neighbor))
                            {
                                chunksToMesh.Enqueue(neighbor);
                            }

                            if (activeChunks.TryGetValue((generatedChunk.X, generatedChunk.Z - 1), out neighbor))
                            {
                                chunksToMesh.Enqueue(neighbor);
                            }
                        }
                        else
                        {
                            generatedChunk.Dispose();
                        }
                    }
                }
            }
        }

        private void StartGeneratingChunks()
        {
            while (chunksToGenerate.Count > 0 && chunkGenerateTasks.Count < maxGenerationTasks)
            {
                Chunk current = chunksToGenerate.Dequeue();
                Task currentTask = current.GenerateTask(generator);

                chunkGenerateTasks.Add(currentTask);
                chunksGenerating.Add(currentTask.Id, current);
            }
        }

        private void FinishLoadingChunks()
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
                                logger.LogError(LoggingEvents.ChunkLoadingError, completed.Exception!.GetBaseException(), "An exception occurred when loading the chunk ({x}|{z}). " +
                                    "The chunk has been scheduled for generation", x, z);

#pragma warning disable CA2000 // Dispose objects before losing scope
                                if (chunksToGenerate.Enqueue(new Chunk(x, z)))
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
                                    loadedChunk.Setup();
                                    activeChunks.Add((x, z), loadedChunk);

                                    chunksToMesh.Enqueue(loadedChunk);

                                    // Schedule to mesh the chunks around this chunk
                                    if (activeChunks.TryGetValue((loadedChunk.X + 1, loadedChunk.Z), out Chunk? neighbor))
                                    {
                                        chunksToMesh.Enqueue(neighbor);
                                    }

                                    if (activeChunks.TryGetValue((loadedChunk.X - 1, loadedChunk.Z), out neighbor))
                                    {
                                        chunksToMesh.Enqueue(neighbor);
                                    }

                                    if (activeChunks.TryGetValue((loadedChunk.X, loadedChunk.Z + 1), out neighbor))
                                    {
                                        chunksToMesh.Enqueue(neighbor);
                                    }

                                    if (activeChunks.TryGetValue((loadedChunk.X, loadedChunk.Z - 1), out neighbor))
                                    {
                                        chunksToMesh.Enqueue(neighbor);
                                    }
                                }
                                else
                                {
                                    loadedChunk.Dispose();
                                }
                            }
                            else
                            {
                                logger.LogError(LoggingEvents.ChunkLoadingError, "The position of the loaded chunk file for position ({x}|{z}) did not match the requested position. " +
                                    "This may be the result of a renamed chunk file. " +
                                    "The position will be scheduled for generation.", x, z);

#pragma warning disable CA2000 // Dispose objects before losing scope
                                if (chunksToGenerate.Enqueue(new Chunk(x, z)))
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

        private void StartLoadingChunks()
        {
            while (positionsToLoad.Count > 0 && chunkLoadingTasks.Count < maxLoadingTasks)
            {
                (int x, int z) = positionsToLoad.Dequeue();

                // If a chunk is already being loaded or saved no new loading task is needed
                if (!positionsLoading.ContainsValue((x, z)))
                {
                    if (!positionsSaving.Contains((x, z)))
                    {
                        string pathToChunk = chunkDirectory + $@"\x{x}z{z}.chunk";
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

        private void FinishMeshingChunks()
        {
            if (chunkMeshingTasks.Count > 0)
            {
                for (int i = chunkMeshingTasks.Count - 1; i >= 0; i--)
                {
                    if (chunkMeshingTasks[i].IsCompleted)
                    {
                        var completed = chunkMeshingTasks[i];
                        Chunk meshedChunk = chunksMeshing[completed.Id];

                        if (chunkMeshingTasks[i].IsFaulted)
                        {
                            Exception e = completed.Exception?.GetBaseException() ?? new NullReferenceException();

                            logger.LogCritical(LoggingEvents.ChunkMeshingError, e, "An exception occurred when meshing the chunk ({x}|{z}). The exception will be re-thrown.", meshedChunk.X, meshedChunk.Z);

                            throw e;
                        }
                        else
                        {
                            chunkMeshingTasks.RemoveAt(i);
                            chunksMeshing.Remove(completed.Id);

                            chunksToSendMeshData.Add((meshedChunk, completed));
                        }
                    }
                }
            }
        }

        private void StartMeshingChunks()
        {
            while (chunksToMesh.Count > 0 && chunkMeshingTasks.Count < maxMeshingTasks)
            {
                Chunk current = chunksToMesh.Dequeue();

                var currentTask = current.CreateMeshDataTask();

                chunkMeshingTasks.Add(currentTask);
                chunksMeshing.Add(currentTask.Id, current);
            }
        }

        private void SendMeshData()
        {
            if (chunksToSendMeshData.Count > 0)
            {
                int chunkIndex = 0;
                for (int count = 0; count < maxMeshDataSends && chunkIndex < chunksToSendMeshData.Count; count++)
                {
                    (Chunk chunk, var chunkMeshingTask) = chunksToSendMeshData[chunkIndex];

                    if (chunk.SetMeshDataStep(chunkMeshingTask.Result))
                    {
                        chunksToSendMeshData.RemoveAt(chunkIndex);
                    }
                    else if (chunksToSendMeshData.Count > 1)
                    {
                        chunkIndex++;
                    }
                }
            }
        }

        private void FinishSavingChunks()
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

                            chunksToMesh.Enqueue(completedChunk);

                            // Schedule to mesh the chunks around this chunk
                            if (activeChunks.TryGetValue((completedChunk.X + 1, completedChunk.Z), out Chunk? neighbor))
                            {
                                chunksToMesh.Enqueue(neighbor);
                            }

                            if (activeChunks.TryGetValue((completedChunk.X - 1, completedChunk.Z), out neighbor))
                            {
                                chunksToMesh.Enqueue(neighbor);
                            }

                            if (activeChunks.TryGetValue((completedChunk.X, completedChunk.Z + 1), out neighbor))
                            {
                                chunksToMesh.Enqueue(neighbor);
                            }

                            if (activeChunks.TryGetValue((completedChunk.X, completedChunk.Z - 1), out neighbor))
                            {
                                chunksToMesh.Enqueue(neighbor);
                            }
                        }
                        else
                        {
                            if (completed.IsFaulted)
                            {
                                logger.LogError(LoggingEvents.ChunkSavingError, completed.Exception!.GetBaseException(), "An exception occurred when saving chunk ({x}|{z}). " +
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

        private void StartSavingChunks()
        {
            while (chunksToSave.Count > 0 && chunkSavingTasks.Count < maxSavingTasks)
            {
                Chunk current = chunksToSave.Dequeue();
                Task currentTask = current.SaveTask(chunkDirectory);

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

                logger.LogDebug(LoggingEvents.ChunkRequest, "Chunk ({x}|{z}) has been requested successfully.", x, z);
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

                logger.LogDebug(LoggingEvents.ChunkRelease, "Released chunk ({x}|{z}).", x, z);

                canRelease = true;
            }

            if (positionsActivating.Contains((x, z)))
            {
                positionsToReleaseOnActivation.Add((x, z));

                logger.LogDebug(LoggingEvents.ChunkRelease, "Scheduled to release chunk ({x}|{z}) after activation.", x, z);

                canRelease = true;
            }

            if (positionsToActivate.Contains((x, z)))
            {
                positionsToActivate.Remove((x, z));

                logger.LogDebug(LoggingEvents.ChunkRelease, "Chunk ({x}|{z}) has been removed from activation list.", x, z);

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
        /// <param name="liquid">The liquid id of the position.</param>
        /// <param name="level">The liquid level of the position.</param>
        /// <param name="isStatic">If the liquid at that position is static.</param>
        /// <returns>The Block at x, y, z or null if the block was not found.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Block? GetBlock(int x, int y, int z, out uint data, out uint liquid, out LiquidLevel level, out bool isStatic)
        {
            if (activeChunks.TryGetValue((x >> sectionSizeExp, z >> sectionSizeExp), out Chunk? chunk) && y >= 0 && y < Chunk.ChunkHeight * Section.SectionSize)
            {
                uint val = chunk.GetSection(y >> chunkHeightExp)[x & (Section.SectionSize - 1), y & (Section.SectionSize - 1), z & (Section.SectionSize - 1)];

                data = (val & Section.DATAMASK) >> Section.DATASHIFT;
                liquid = (val & Section.LIQUIDMASK) >> Section.LIQUIDSHIFT;
                level = (LiquidLevel)((val & Section.LEVELMASK) >> Section.LEVELSHIFT);
                isStatic = (val & Section.STATICMASK) != 0;

                return Block.TranslateID(val & Section.BLOCKMASK);
            }
            else
            {
                data = 0;
                liquid = 0;
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
            Block? block = GetBlock(x, y, z, out data, out uint liquidId, out level, out isStatic);
            Liquid? liquid = Liquid.TranslateID(liquidId);

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
            SetPosition(block, data, liquid, level, isStatic, x, y, z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetLiquid(Liquid liquid, LiquidLevel level, bool isStatic, int x, int y, int z)
        {
            Block block = GetBlock(x, y, z, out uint data, out _, out _, out _) ?? Block.Air;
            SetPosition(block, data, liquid, level, isStatic, x, y, z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetPosition(Block block, uint data, Liquid liquid, LiquidLevel level, bool isStatic, int x, int y, int z)
        {
            if (!activeChunks.TryGetValue((x >> sectionSizeExp, z >> sectionSizeExp), out Chunk? chunk) || y < 0 || y >= Chunk.ChunkHeight * Section.SectionSize)
            {
                return;
            }

            uint val = (uint)((((isStatic ? 1 : 0) << Section.STATICSHIFT) & Section.STATICMASK) | (((uint)level << Section.LEVELSHIFT) & Section.LEVELMASK) | ((liquid.Id << Section.LIQUIDSHIFT) & Section.LIQUIDMASK) | ((data << Section.DATASHIFT) & Section.DATAMASK) | (block.Id & Section.BLOCKMASK));
            chunk.GetSection(y >> chunkHeightExp)[x & (Section.SectionSize - 1), y & (Section.SectionSize - 1), z & (Section.SectionSize - 1)] = val;

            sectionsToMesh.Add((chunk, y >> chunkHeightExp));

            // Block updates - Side is passed out of the perspective of the block receiving the block update.
            GetBlock(x, y, z + 1, out data)?.BlockUpdate(x, y, z + 1, data, BlockSide.Back);
            GetBlock(x, y, z - 1, out data)?.BlockUpdate(x, y, z - 1, data, BlockSide.Front);
            GetBlock(x - 1, y, z, out data)?.BlockUpdate(x - 1, y, z, data, BlockSide.Right);
            GetBlock(x + 1, y, z, out data)?.BlockUpdate(x + 1, y, z, data, BlockSide.Left);
            GetBlock(x, y - 1, z, out data)?.BlockUpdate(x, y - 1, z, data, BlockSide.Top);
            GetBlock(x, y + 1, z, out data)?.BlockUpdate(x, y + 1, z, data, BlockSide.Bottom);

            // Check if sections next to this section have to be changed:

            // Next on y axis.
            if ((y & (Section.SectionSize - 1)) == 0 && (y - 1 >> chunkHeightExp) >= 0)
            {
                sectionsToMesh.Add((chunk, y - 1 >> chunkHeightExp));
            }
            else if ((y & (Section.SectionSize - 1)) == Section.SectionSize - 1 && (y + 1 >> chunkHeightExp) < Chunk.ChunkHeight)
            {
                sectionsToMesh.Add((chunk, y + 1 >> chunkHeightExp));
            }

            // Next on x axis.
            if ((x & (Section.SectionSize - 1)) == 0 && activeChunks.TryGetValue((x - 1 >> sectionSizeExp, z >> sectionSizeExp), out chunk))
            {
                sectionsToMesh.Add((chunk, y >> chunkHeightExp));
            }
            else if ((x & (Section.SectionSize - 1)) == Section.SectionSize - 1 && activeChunks.TryGetValue((x + 1 >> sectionSizeExp, z >> sectionSizeExp), out chunk))
            {
                sectionsToMesh.Add((chunk, y >> chunkHeightExp));
            }

            // Next on z axis.
            if ((z & (Section.SectionSize - 1)) == 0 && activeChunks.TryGetValue((x >> sectionSizeExp, z - 1 >> sectionSizeExp), out chunk))
            {
                sectionsToMesh.Add((chunk, y >> chunkHeightExp));
            }
            else if ((z & (Section.SectionSize - 1)) == Section.SectionSize - 1 && activeChunks.TryGetValue((x >> sectionSizeExp, z + 1 >> sectionSizeExp), out chunk))
            {
                sectionsToMesh.Add((chunk, y >> chunkHeightExp));
            }
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
            activeChunks.TryGetValue((x >> sectionSizeExp, z >> sectionSizeExp), out Chunk? chunk);
            return chunk;
        }

        /// <summary>
        /// Gets a section of an active chunk.
        /// </summary>
        /// <param name="x">The x position of the section in chunk coordinates.</param>
        /// <param name="y">The y position of the section in chunk coordinates.</param>
        /// <param name="z">The z position of the section in chunk coordinates.</param>
        /// <returns>The section at the given position or null if no section was found.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Section? GetSection(int x, int y, int z)
        {
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

            logger.LogInformation("World spawn position has been set to: {position}", position);
        }

        /// <summary>
        /// Saves all active chunks that are not currently saved.
        /// </summary>
        /// <returns>A task that represents all tasks saving the chunks.</returns>
        public Task Save()
        {
            Console.WriteLine(Language.AllChunksSaving);
            Console.WriteLine();

            logger.LogInformation("Saving world.");

            List<Task> savingTasks = new List<Task>(activeChunks.Count);

            foreach (Chunk chunk in activeChunks.Values)
            {
                if (!positionsSaving.Contains((chunk.X, chunk.Z)))
                {
                    savingTasks.Add(chunk.SaveTask(chunkDirectory));
                }
            }

            Information.Version = Program.Version;

            savingTasks.Add(Task.Run(() => Information.Save(Path.Combine(worldDirectory, "meta.json"))));

            return Task.WhenAll(savingTasks);
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