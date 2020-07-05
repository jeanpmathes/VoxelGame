// <copyright file="World.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
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
        public bool IsReady { get; private set; } = false;

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
        /// A set of chunks that have to be rendered.
        /// </summary>
        private readonly HashSet<Chunk> chunksToRender;

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
                chunkDirectory: path + @"\Chunks",
                new ComplexGenerator(seed))
        {
            Information.Save(Path.Combine(worldDirectory, "meta.json"));
        }

        /// <summary>
        /// This constructor is meant for worlds that already exist.
        /// </summary>
        public World(WorldInformation information, string path) :
            this(
                information,
                worldDirectory: path,
                chunkDirectory: path + @"\Chunks",
                new ComplexGenerator(information.Seed))
        {
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
            chunksToRender = new HashSet<Chunk>();
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

        public void FrameRender()
        {
            if (IsReady)
            {
                // Collect all chunks to render
                Chunk playerChunk = activeChunks[(Game.Player.ChunkX, Game.Player.ChunkZ)];
                chunksToRender.Add(playerChunk);

                for (int x = -Game.Player.RenderDistance; x <= Game.Player.RenderDistance; x++)
                {
                    for (int z = -Game.Player.RenderDistance; z <= Game.Player.RenderDistance; z++)
                    {
                        if (activeChunks.TryGetValue((Game.Player.ChunkX + x, Game.Player.ChunkZ + z), out Chunk? chunk))
                        {
                            if (x == 0 && z == 0)
                            {
                                continue;
                            }

                            chunksToRender.Add(chunk);
                        }
                    }
                }

                // Render the listed chunks
                foreach (Chunk chunk in chunksToRender)
                {
                    chunk.Render();
                }

                chunksToRender.Clear();

                // Render the player
                Game.Player.Render();
            }
        }

        public void FrameUpdate(float deltaTime)
        {
            // Handle chunks to activate
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

            // Check if generation tasks have finished and add the generated chunks to the active chunks dictionary
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

            // Start generating new chunks if necessary
            while (chunksToGenerate.Count > 0 && chunkGenerateTasks.Count < maxGenerationTasks)
            {
                Chunk current = chunksToGenerate.Dequeue();
                Task currentTask = current.GenerateTask(generator);

                chunkGenerateTasks.Add(currentTask);
                chunksGenerating.Add(currentTask.Id, current);
            }

            // Check if loading tasks have finished
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
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.Write(
                                    $"{DateTime.Now} | ---- CHUNK LOADING ERROR -------------\n" +
                                    $"Position: ({x}|{z}) Exception: ({(completed.Exception?.GetBaseException().GetType().ToString()) ?? "EXCEPTION IS NULL"})\n" +
                                    $"{(completed.Exception?.GetBaseException().Message) ?? "EXCEPTION IS NULL"}\n" +
                                     "The position has been scheduled for generation.\n");
                                Console.ResetColor();

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
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine(
                                    $"{DateTime.Now} | ---- CHUNK LOADING ERROR -------------\n" +
                                    $"Position: ({x}|{z}) Exception:\n" +
                                     "The loaded file did not match the requested chunk. This may be the result of renamed chunk files.\n" +
                                     "The position has been scheduled for generation.");
                                Console.ResetColor();

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

            // Start loading new chunks if necessary
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

            // Check if meshing tasks have finished
            if (chunkMeshingTasks.Count > 0)
            {
                for (int i = chunkMeshingTasks.Count - 1; i >= 0; i--)
                {
                    if (chunkMeshingTasks[i].IsCompleted)
                    {
                        if (chunkMeshingTasks[i].IsFaulted)
                        {
                            Exception e = chunkMeshingTasks[i].Exception?.GetBaseException() ?? new NullReferenceException();

                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine(
                                $"{DateTime.Now} | ---- CHUNK MESHING ERROR -------------\n" +
                                 "Exception:\n" +
                                $"{e.Message}\n" +
                                 "Stack Trace:\n" +
                                $"{e.StackTrace}");
                            Console.ResetColor();

                            throw e;
                        }
                        else
                        {
                            var completed = chunkMeshingTasks[i];
                            Chunk meshedChunk = chunksMeshing[completed.Id];

                            chunkMeshingTasks.RemoveAt(i);
                            chunksMeshing.Remove(completed.Id);

                            chunksToSendMeshData.Add((meshedChunk, completed));
                        }
                    }
                }
            }

            // Start meshing the listed chunks
            while (chunksToMesh.Count > 0 && chunkMeshingTasks.Count < maxMeshingTasks)
            {
                Chunk current = chunksToMesh.Dequeue();

                var currentTask = current.CreateMeshDataTask();

                chunkMeshingTasks.Add(currentTask);
                chunksMeshing.Add(currentTask.Id, current);
            }

            //Send mesh data to the chunks
            if (chunksToSendMeshData.Count > 0)
            {
                int i = 0;
                for (int count = 0; count < maxMeshDataSends && i < chunksToSendMeshData.Count; count++)
                {
                    (Chunk chunk, var chunkMeshingTask) = chunksToSendMeshData[i];

                    if (chunk.SetMeshDataStep(chunkMeshingTask.Result))
                    {
                        chunksToSendMeshData.RemoveAt(i);
                    }
                    else
                    {
                        i++;
                    }
                }
            }

            if (IsReady)
            {
                // Tick objects in world

                foreach (Chunk chunk in activeChunks.Values)
                {
                    chunk.Tick();
                }

                Game.Player.Tick(deltaTime);

                // Mesh all listed sections
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

                    Console.WriteLine(Language.WorldIsReady);
                }
            }

            // Check if saving tasks have finished
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
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine(
                                    $"{DateTime.Now} | ---- CHUNK SAVING ERROR -------------\n" +
                                    $"Position: ({completedChunk.X}|{completedChunk.Z}) Exception: ({(completed.Exception?.GetBaseException().GetType().ToString()) ?? "EXCEPTION IS NULL"})\n" +
                                    $"{completed.Exception?.GetBaseException().Message ?? "EXCEPTION IS NULL"}\n" +
                                     "The chunk will be disposed without saving.");
                                Console.ResetColor();
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

            // Start saving chunks if necessary
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

                canRelease = true;
            }

            if (positionsActivating.Contains((x, z)))
            {
                positionsToReleaseOnActivation.Add((x, z));

                canRelease = true;
            }

            if (positionsToActivate.Contains((x, z)))
            {
                positionsToActivate.Remove((x, z));

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
        public Block? GetBlock(int x, int y, int z, out byte data)
        {
            if (activeChunks.TryGetValue((x >> sectionSizeExp, z >> sectionSizeExp), out Chunk? chunk) && y >= 0 && y < Chunk.ChunkHeight * Section.SectionSize)
            {
                ushort val = chunk.GetSection(y >> chunkHeightExp)[x & (Section.SectionSize - 1), y & (Section.SectionSize - 1), z & (Section.SectionSize - 1)];

                data = (byte)(val >> 11);
                return Block.TranslateID((ushort)(val & Section.BlockMask));
            }
            else
            {
                data = 0;
                return null;
            }
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
        public void SetBlock(Block block, byte data, int x, int y, int z)
        {
            if (activeChunks.TryGetValue((x >> sectionSizeExp, z >> sectionSizeExp), out Chunk? chunk) && y >= 0 && y < Chunk.ChunkHeight * Section.SectionSize)
            {
                chunk.GetSection(y >> chunkHeightExp)[x & (Section.SectionSize - 1), y & (Section.SectionSize - 1), z & (Section.SectionSize - 1)] = (ushort)((data << 11) | (block ?? Block.AIR).Id);
                sectionsToMesh.Add((chunk, y >> chunkHeightExp));

                // Block updates
                GetBlock(x, y, z + 1, out data)?.BlockUpdate(x, y, z + 1, data, BlockSide.Back);
                GetBlock(x, y, z - 1, out data)?.BlockUpdate(x, y, z - 1, data, BlockSide.Front);
                GetBlock(x - 1, y, z, out data)?.BlockUpdate(x - 1, y, z, data, BlockSide.Right);
                GetBlock(x + 1, y, z, out data)?.BlockUpdate(x + 1, y, z, data, BlockSide.Left);
                GetBlock(x, y - 1, z, out data)?.BlockUpdate(x, y - 1, z, data, BlockSide.Top);
                GetBlock(x, y + 1, z, out data)?.BlockUpdate(x, y + 1, z, data, BlockSide.Bottom);

                // Check if sections next to this section have to be changed

                // Next on y axis
                if ((y & (Section.SectionSize - 1)) == 0 && (y - 1 >> chunkHeightExp) >= 0)
                {
                    sectionsToMesh.Add((chunk, y - 1 >> chunkHeightExp));
                }
                else if ((y & (Section.SectionSize - 1)) == Section.SectionSize - 1 && (y + 1 >> chunkHeightExp) < Chunk.ChunkHeight)
                {
                    sectionsToMesh.Add((chunk, y + 1 >> chunkHeightExp));
                }

                // Next on x axis
                if ((x & (Section.SectionSize - 1)) == 0 && activeChunks.TryGetValue((x - 1 >> sectionSizeExp, z >> sectionSizeExp), out chunk))
                {
                    sectionsToMesh.Add((chunk, y >> chunkHeightExp));
                }
                else if ((x & (Section.SectionSize - 1)) == Section.SectionSize - 1 && activeChunks.TryGetValue((x + 1 >> sectionSizeExp, z >> sectionSizeExp), out chunk))
                {
                    sectionsToMesh.Add((chunk, y >> chunkHeightExp));
                }

                // Next on z axis
                if ((z & (Section.SectionSize - 1)) == 0 && activeChunks.TryGetValue((x >> sectionSizeExp, z - 1 >> sectionSizeExp), out chunk))
                {
                    sectionsToMesh.Add((chunk, y >> chunkHeightExp));
                }
                else if ((z & (Section.SectionSize - 1)) == Section.SectionSize - 1 && activeChunks.TryGetValue((x >> sectionSizeExp, z + 1 >> sectionSizeExp), out chunk))
                {
                    sectionsToMesh.Add((chunk, y >> chunkHeightExp));
                }
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
        }

        /// <summary>
        /// Saves all active chunks that are not currently saved.
        /// </summary>
        /// <returns>A task that represents all tasks saving the chunks.</returns>
        public Task Save()
        {
            List<Task> savingTasks = new List<Task>(activeChunks.Count);

            foreach (Chunk chunk in activeChunks.Values)
            {
                if (!positionsSaving.Contains((chunk.X, chunk.Z)))
                {
                    savingTasks.Add(chunk.SaveTask(chunkDirectory));
                }
            }

            Console.WriteLine(Language.AllChunksSaving);

            Information.Version = Program.Version;

            savingTasks.Add(Task.Run(() => Information.Save(Path.Combine(worldDirectory, "meta.json"))));

            return Task.WhenAll(savingTasks);
        }

        #region IDisposable Support

        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
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

                disposedValue = true;
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