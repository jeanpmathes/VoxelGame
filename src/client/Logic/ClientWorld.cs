// <copyright file="ClientWorld.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using Microsoft.Extensions.Logging;
using OpenToolkit.Mathematics;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using VoxelGame.Client.Rendering;
using VoxelGame.Core;
using VoxelGame.Core.Collections;
using VoxelGame.Core.Logic;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Client.Logic
{
    public class ClientWorld : Core.Logic.World
    {
        private static readonly ILogger logger = LoggingHelper.CreateLogger<ClientWorld>();

        protected int MaxMeshingTasks { get; } = Properties.client.Default.MaxMeshingTasks;
        protected int MaxMeshDataSends { get; } = Properties.client.Default.MaxMeshDataSends;

        /// <summary>
        /// A queue with chunks that have to be meshed completely, mainly new chunks.
        /// </summary>
        private readonly UniqueQueue<ClientChunk> chunksToMesh;

        /// <summary>
        /// A list of chunk meshing tasks,
        /// </summary>
        private readonly List<Task<SectionMeshData[]>> chunkMeshingTasks;

        /// <summary>
        /// A dictionary containing all chunks that are currently meshed, with the task id of their meshing task as key.
        /// </summary>
        private readonly Dictionary<int, ClientChunk> chunksMeshing;

        /// <summary>
        /// A list of chunks where the mesh data has to be set;
        /// </summary>
        private readonly List<(ClientChunk chunk, Task<SectionMeshData[]> chunkMeshingTask)> chunksToSendMeshData;

        /// <summary>
        /// A set of chunks with information on which sections of them are to mesh.
        /// </summary>
        private readonly HashSet<(ClientChunk chunk, int index)> sectionsToMesh;

        /// <summary>
        /// This constructor is meant for worlds that are new.
        /// </summary>
        public ClientWorld(string name, string path, int seed) : base(name, path, seed)
        {
            chunksToMesh = new UniqueQueue<ClientChunk>();
            chunkMeshingTasks = new List<Task<SectionMeshData[]>>(MaxMeshingTasks);
            chunksMeshing = new Dictionary<int, ClientChunk>(MaxMeshingTasks);
            chunksToSendMeshData = new List<(ClientChunk chunk, Task<SectionMeshData[]> chunkMeshingTask)>(MaxMeshDataSends);
            sectionsToMesh = new HashSet<(ClientChunk chunk, int index)>();
        }

        /// <summary>
        /// This constructor is meant for worlds that already exist.
        /// </summary>
        public ClientWorld(WorldInformation information, string path) : base(information, path)
        {
            chunksToMesh = new UniqueQueue<ClientChunk>();
            chunkMeshingTasks = new List<Task<SectionMeshData[]>>(MaxMeshingTasks);
            chunksMeshing = new Dictionary<int, ClientChunk>(MaxMeshingTasks);
            chunksToSendMeshData = new List<(ClientChunk chunk, Task<SectionMeshData[]> chunkMeshingTask)>(MaxMeshDataSends);
            sectionsToMesh = new HashSet<(ClientChunk chunk, int index)>();
        }

        public void Render()
        {
            if (IsReady)
            {
                List<(ClientSection section, Vector3 position)> renderList = new List<(ClientSection section, Vector3 position)>();

                // Fill the render list.
                for (int x = -Client.Player.LoadDistance; x <= Client.Player.LoadDistance; x++)
                {
                    for (int z = -Client.Player.LoadDistance; z <= Client.Player.LoadDistance; z++)
                    {
                        if (activeChunks.TryGetValue((Client.Player.ChunkX + x, Client.Player.ChunkZ + z), out Chunk? chunk))
                        {
                            ((ClientChunk)chunk).AddCulledToRenderList(Client.Player.Frustum, ref renderList);
                        }
                    }
                }

                for (int stage = 0; stage < SectionRenderer.DrawStageCount; stage++)
                {
                    if (renderList.Count == 0) break;

                    renderList[0].section.PrepareRender(stage);

                    for (int i = 0; i < renderList.Count; i++)
                    {
                        renderList[i].section.Render(stage, renderList[i].position);
                    }

                    renderList[0].section.FinishRender(stage);
                }

                // Render the player
                Client.Player.Render();
            }
        }

        protected override Chunk CreateChunk(int x, int z)
        {
            return new ClientChunk(x, z);
        }

        public override void Update(float deltaTime)
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

                Client.Player.Tick(deltaTime);

                // Mesh all listed sections.
                foreach ((Chunk chunk, int index) in sectionsToMesh)
                {
                    ((ClientChunk)chunk).CreateAndSetMesh(index);
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

        protected override void ProcessNewlyActivatedChunk(Chunk activatedChunk)
        {
            chunksToMesh.Enqueue((ClientChunk)activatedChunk);

            // Schedule to mesh the chunks around this chunk
            if (activeChunks.TryGetValue((activatedChunk.X + 1, activatedChunk.Z), out Chunk? neighbor))
            {
                chunksToMesh.Enqueue((ClientChunk)neighbor);
            }

            if (activeChunks.TryGetValue((activatedChunk.X - 1, activatedChunk.Z), out neighbor))
            {
                chunksToMesh.Enqueue((ClientChunk)neighbor);
            }

            if (activeChunks.TryGetValue((activatedChunk.X, activatedChunk.Z + 1), out neighbor))
            {
                chunksToMesh.Enqueue((ClientChunk)neighbor);
            }

            if (activeChunks.TryGetValue((activatedChunk.X, activatedChunk.Z - 1), out neighbor))
            {
                chunksToMesh.Enqueue((ClientChunk)neighbor);
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

                            chunksToSendMeshData.Add(((ClientChunk)meshedChunk, completed));
                        }
                    }
                }
            }
        }

        private void StartMeshingChunks()
        {
            while (chunksToMesh.Count > 0 && chunkMeshingTasks.Count < MaxMeshingTasks)
            {
                ClientChunk current = chunksToMesh.Dequeue();

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
                for (int count = 0; count < MaxMeshDataSends && chunkIndex < chunksToSendMeshData.Count; count++)
                {
                    (Chunk chunk, var chunkMeshingTask) = chunksToSendMeshData[chunkIndex];

                    if (((ClientChunk)chunk).SetMeshDataStep(chunkMeshingTask.Result))
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void ProcessChangedSection(Chunk chunk, int x, int y, int z)
        {
            sectionsToMesh.Add(((ClientChunk)chunk, y >> ChunkHeightExp));

            // Check if sections next to changed section have to be changed:

            // Next on y axis.
            if ((y & (Section.SectionSize - 1)) == 0 && (y - 1 >> ChunkHeightExp) >= 0)
            {
                sectionsToMesh.Add(((ClientChunk)chunk, y - 1 >> ChunkHeightExp));
            }
            else if ((y & (Section.SectionSize - 1)) == Section.SectionSize - 1 && (y + 1 >> ChunkHeightExp) < Chunk.ChunkHeight)
            {
                sectionsToMesh.Add(((ClientChunk)chunk, y + 1 >> ChunkHeightExp));
            }

            Chunk? neighbor;

            // Next on x axis.
            if ((x & (Section.SectionSize - 1)) == 0 && activeChunks.TryGetValue((x - 1 >> SectionSizeExp, z >> SectionSizeExp), out neighbor))
            {
                sectionsToMesh.Add(((ClientChunk)neighbor, y >> ChunkHeightExp));
            }
            else if ((x & (Section.SectionSize - 1)) == Section.SectionSize - 1 && activeChunks.TryGetValue((x + 1 >> SectionSizeExp, z >> SectionSizeExp), out neighbor))
            {
                sectionsToMesh.Add(((ClientChunk)neighbor, y >> ChunkHeightExp));
            }

            // Next on z axis.
            if ((z & (Section.SectionSize - 1)) == 0 && activeChunks.TryGetValue((x >> SectionSizeExp, z - 1 >> SectionSizeExp), out neighbor))
            {
                sectionsToMesh.Add(((ClientChunk)neighbor, y >> ChunkHeightExp));
            }
            else if ((z & (Section.SectionSize - 1)) == Section.SectionSize - 1 && activeChunks.TryGetValue((x >> SectionSizeExp, z + 1 >> SectionSizeExp), out neighbor))
            {
                sectionsToMesh.Add(((ClientChunk)neighbor, y >> ChunkHeightExp));
            }
        }
    }
}