// <copyright file="World.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using System;
using System.Collections.Generic;
using Resources;
using System.Threading.Tasks;
using VoxelGame.WorldGeneration;
using VoxelGame.Collections;

namespace VoxelGame.Logic
{
    public class World
    {
        public const int ChunkExtents = 5;

        private readonly int sectionSizeExp = (int)Math.Log(Section.SectionSize, 2);
        private readonly int chunkHeightExp = (int)Math.Log(Chunk.ChunkHeight, 2);

        private const int maxGenerationTasks = 15;
        private const int maxMeshingTasks = 5;
        private const int maxMeshDataSends = 2;

        /// <summary>
        /// Gets whether this world is ready for physics ticking and rendering.
        /// </summary>
        public bool IsReady { get; private set; } = false;

        private readonly IWorldGenerator generator;

        /// <summary>
        /// A set of chunks which is currently not active and should either be loaded or generated.
        /// </summary>
        private readonly HashSet<Chunk> chunksToActivate = new HashSet<Chunk>();

        /// <summary>
        /// A set of chunk positions that are currently being activated. No new chunks for these positions should be created.
        /// </summary>
        private readonly HashSet<ValueTuple<int, int>> chunksActivating = new HashSet<ValueTuple<int, int>>();

        /// <summary>
        /// A queue that contains all chunks that have to be generated.
        /// </summary>
        private readonly Queue<Chunk> chunksToGenerate = new Queue<Chunk>();

        /// <summary>
        /// A list of chunk generation tasks,
        /// </summary>
        private readonly List<Task> chunkGenerateTasks = new List<Task>();

        /// <summary>
        /// A dictionary containing all chunks that are currently generated, with the task id of their generating task as key.
        /// </summary>
        private readonly Dictionary<int, Chunk> chunksGenerating = new Dictionary<int, Chunk>();

        /// <summary>
        /// A dictionary that contains all active chunks.
        /// </summary>
        private readonly Dictionary<ValueTuple<int, int>, Chunk> activeChunks = new Dictionary<ValueTuple<int, int>, Chunk>();

        /// <summary>
        /// A queue with chunks that have to be meshed completely, mainly new chunks.
        /// </summary>
        private readonly UniqueQueue<Chunk> chunksToMesh = new UniqueQueue<Chunk>();

        /// <summary>
        /// A list of chunk meshing tasks,
        /// </summary>
        private readonly List<Task<(float[][] verticesData, uint[][] indicesData)>> chunkMeshingTasks = new List<Task<(float[][] verticesData, uint[][] indicesData)>>();

        /// <summary>
        /// A dictionary containing all chunks that are currently meshed, with the task id of their meshing task as key.
        /// </summary>
        private readonly Dictionary<int, Chunk> chunksMeshing = new Dictionary<int, Chunk>();

        /// <summary>
        /// A list of chunks where the mesh data has to be set;
        /// </summary>
        private readonly List<(Chunk chunk, Task<(float[][] verticesData, uint[][] indicesData)> chunkMeshingTask)> chunksToSendMeshData = new List<(Chunk chunk, Task<(float[][] verticesData, uint[][] indicesData)> chunkMeshingTask)>();

        /// <summary>
        /// A set of chunks with information on which sections of them are to mesh.
        /// </summary>
        private readonly HashSet<(Chunk chunk, int index)> sectionsToMesh = new HashSet<(Chunk chunk, int index)>();

        /// <summary>
        /// A set of chunks that have to be rendered.
        /// </summary>
        private readonly HashSet<Chunk> chunksToRender = new HashSet<Chunk>();

        public World(IWorldGenerator generator)
        {
            this.generator = generator;

            for (int x = ChunkExtents / -2; x < (ChunkExtents / 2) + 1; x++)
            {
                for (int z = ChunkExtents / -2; z < (ChunkExtents / 2) + 1; z++)
                {
                    chunksToActivate.Add(new Chunk(x, z));
                }
            }

            foreach (Chunk chunk in chunksToActivate) chunksToGenerate.Enqueue(chunk);

            chunksToActivate.Clear();
        }

        public void FrameRender()
        {
            if (IsReady)
            {
                // Collect all chunks to render
                chunksToRender.UnionWith(activeChunks.Values);

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
            // Check if new chunks have to be activated
            if (Game.Player.ChunkHasChanged)
            {
                for (int x = Game.Player.RenderDistance / -2; x < (Game.Player.RenderDistance / 2) + 1; x++)
                {
                    for (int z = Game.Player.RenderDistance / -2; z < (Game.Player.RenderDistance / 2) + 1; z++)
                    {
                        if (!chunksActivating.Contains((Game.Player.ChunkX + x, Game.Player.ChunkZ + z)) && !activeChunks.ContainsKey((Game.Player.ChunkX + x, Game.Player.ChunkZ + z)))
                        {
                            chunksToActivate.Add(new Chunk(Game.Player.ChunkX + x, Game.Player.ChunkZ + z));
                        }
                    }
                }
            }

            // Handle chunks to activate
            foreach (Chunk toActivate in chunksToActivate)
            {
                chunksActivating.Add((toActivate.X, toActivate.Z));

                chunksToGenerate.Enqueue(toActivate);
            }

            chunksToActivate.Clear();

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

                        chunksActivating.Remove((generatedChunk.X, generatedChunk.Z));

                        activeChunks.Add((generatedChunk.X, generatedChunk.Z), generatedChunk);

                        chunksToMesh.Enqueue(generatedChunk);

                        // Schedule to mesh the chunks around this chunk
                        if (activeChunks.TryGetValue((generatedChunk.X + 1, generatedChunk.Z), out Chunk neighbor) && !chunksToMesh.Contains(neighbor))
                        {
                            chunksToMesh.Enqueue(neighbor);
                        }

                        if (activeChunks.TryGetValue((generatedChunk.X - 1, generatedChunk.Z), out neighbor) && !chunksToMesh.Contains(neighbor))
                        {
                            chunksToMesh.Enqueue(neighbor);
                        }

                        if (activeChunks.TryGetValue((generatedChunk.X, generatedChunk.Z + 1), out neighbor) && !chunksToMesh.Contains(neighbor))
                        {
                            chunksToMesh.Enqueue(neighbor);
                        }

                        if (activeChunks.TryGetValue((generatedChunk.X, generatedChunk.Z - 1), out neighbor) && !chunksToMesh.Contains(neighbor))
                        {
                            chunksToMesh.Enqueue(neighbor);
                        }
                    }
                    else if (chunkGenerateTasks[i].IsFaulted)
                    {
                        throw chunkGenerateTasks[i].Exception;
                    }
                }
            }

            // Start generating new chunks if necessary
            while (chunksToGenerate.Count > 0 && chunkGenerateTasks.Count < maxGenerationTasks)
            {
                Chunk current = chunksToGenerate.Dequeue();
                Task currentTask = current.GenerateAsync(generator);

                chunkGenerateTasks.Add(currentTask);
                chunksGenerating.Add(currentTask.Id, current);
            }

            // Check if meshing tasks have finished
            if (chunkMeshingTasks.Count > 0)
            {
                for (int i = chunkMeshingTasks.Count - 1; i >= 0; i--)
                {
                    if (chunkMeshingTasks[i].IsCompleted)
                    {
                        Task<(float[][] verticesData, uint[][] indicesData)> completed = chunkMeshingTasks[i];
                        Chunk meshedChunk = chunksMeshing[completed.Id];

                        chunkMeshingTasks.RemoveAt(i);
                        chunksMeshing.Remove(completed.Id);

                        chunksToSendMeshData.Add((meshedChunk, completed));
                    }
                }
            }

            // Start meshing the listed chunks
            while (chunksToMesh.Count > 0 && chunkMeshingTasks.Count < maxMeshingTasks)
            {
                Chunk current = chunksToMesh.Dequeue();

                Task<(float[][] verticesData, uint[][] indicesData)> currentTask = current.CreateMeshDataAsync();

                chunkMeshingTasks.Add(currentTask);
                chunksMeshing.Add(currentTask.Id, current);
            }

            //Send mesh data to the chunks
            if (chunksToSendMeshData.Count > 0)
            {
                for (int i = chunksToSendMeshData.Count - 1; i >= 0 && chunksToSendMeshData.Count - i <= maxMeshDataSends; i--)
                {
                    (Chunk chunk, Task<(float[][] verticesData, uint[][] indicesData)> chunkMeshingTask) = chunksToSendMeshData[i];

                    if (chunk.SetMeshDataStep(chunkMeshingTask.Result.verticesData, chunkMeshingTask.Result.indicesData))
                    {
                        chunksToSendMeshData.RemoveAt(i);
                    }
                }
            }

            if (IsReady)
            {
                // Mesh all listed sections
                foreach ((Chunk chunk, int index) in sectionsToMesh)
                {
                    chunk.CreateMesh(index);
                }

                sectionsToMesh.Clear();

                Game.Player.Tick(deltaTime);
            }
            else
            {
                if (activeChunks.Count >= 25 && activeChunks.ContainsKey((0, 0)))
                {
                    IsReady = true;

                    Console.WriteLine(Language.WorldIsReady);
                }
            }
        }

        /// <summary>
        /// Returns the block at a given position in block coordinates. The block is only searched in active chunks.
        /// </summary>
        /// <param name="x">The x position in block coordinates.</param>
        /// <param name="y">The y position in block coordinates.</param>
        /// <param name="z">The z position in block coordinates.</param>
        /// <returns>The Block at x, y, z or null if the block was not found.</returns>
        public Block GetBlock(int x, int y, int z)
        {
            if (activeChunks.TryGetValue((x >> sectionSizeExp, z >> sectionSizeExp), out Chunk chunk) && y >= 0 && y < Chunk.ChunkHeight * Section.SectionSize)
            {
                return chunk.GetSection(y >> chunkHeightExp)
                    [x & (Section.SectionSize - 1), y & (Section.SectionSize - 1), z & (Section.SectionSize - 1)];
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Sets a block in the world, adds the changed sections to the re-mesh set and sends block updates to the neighbors of the changed block.
        /// </summary>
        /// <param name="block">The block which should be set at the position.</param>
        /// <param name="x">The x position of the block to set.</param>
        /// <param name="y">The y position of the block to set.</param>
        /// <param name="z">The z position of the block to set.</param>
        public void SetBlock(Block block, int x, int y, int z)
        {
            if (activeChunks.TryGetValue((x >> sectionSizeExp, z >> sectionSizeExp), out Chunk chunk) && y >= 0 && y < Chunk.ChunkHeight * Section.SectionSize)
            {
                chunk.GetSection(y >> chunkHeightExp)
                    [x & (Section.SectionSize - 1), y & (Section.SectionSize - 1), z & (Section.SectionSize - 1)] = block;

                sectionsToMesh.Add((chunk, y >> chunkHeightExp));

                // Block updates
                GetBlock(x, y, z + 1)?.BlockUpdate(x, y, z + 1); // Front
                GetBlock(x, y, z - 1)?.BlockUpdate(x, y, z - 1); // Back
                GetBlock(x - 1, y, z)?.BlockUpdate(x - 1, y, z); // Left
                GetBlock(x + 1, y, z)?.BlockUpdate(x + 1, y, z); // Right
                GetBlock(x, y - 1, z)?.BlockUpdate(x, y - 1, z); // Bottom
                GetBlock(x, y + 1, z)?.BlockUpdate(x, y + 1, z); // Top

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
        public Chunk GetChunk(int x, int z)
        {
            activeChunks.TryGetValue((x, z), out Chunk chunk);
            return chunk;
        }

        /// <summary>
        /// Gets a section of an active chunk.
        /// </summary>
        /// <param name="x">The x position of the section in chunk coordinates.</param>
        /// <param name="y">The y position of the section in chunk coordinates.</param>
        /// <param name="z">The z position of the section in chunk coordinates.</param>
        /// <returns>The section at the given position or null if no section was found.</returns>
        public Section GetSection(int x, int y, int z)
        {
            if (activeChunks.TryGetValue((x, z), out Chunk chunk) && y >= 0 && y < Chunk.ChunkHeight)
            {
                return chunk.GetSection(y);
            }
            else
            {
                return null;
            }
        }
    }
}