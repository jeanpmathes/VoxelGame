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
using VoxelGame.Core.Collections;
using VoxelGame.Core.Logic;
using VoxelGame.Logging;

namespace VoxelGame.Client.Logic
{
    public class ClientWorld : Core.Logic.World
    {
        private static readonly ILogger logger = LoggingHelper.CreateLogger<ClientWorld>();

        private readonly System.Diagnostics.Stopwatch readyStopwatch = System.Diagnostics.Stopwatch.StartNew();

        private static int MaxMeshingTasks { get; } = Properties.client.Default.MaxMeshingTasks;
        private static int MaxMeshDataSends { get; } = Properties.client.Default.MaxMeshDataSends;

        /// <summary>
        /// A queue with chunks that have to be meshed completely, mainly new chunks.
        /// </summary>
        private readonly UniqueQueue<ClientChunk> chunksToMesh = new UniqueQueue<ClientChunk>();

        /// <summary>
        /// A list of chunk meshing tasks,
        /// </summary>
        private readonly List<Task<SectionMeshData[]>> chunkMeshingTasks =
            new List<Task<SectionMeshData[]>>(MaxMeshingTasks);

        /// <summary>
        /// A dictionary containing all chunks that are currently meshed, with the task id of their meshing task as key.
        /// </summary>
        private readonly Dictionary<int, ClientChunk> chunksMeshing = new Dictionary<int, ClientChunk>(MaxMeshingTasks);

        /// <summary>
        /// A list of chunks where the mesh data has to be set.
        /// </summary>
        private readonly List<(ClientChunk chunk, Task<SectionMeshData[]> chunkMeshingTask)> chunksToSendMeshData =
            new List<(ClientChunk chunk, Task<SectionMeshData[]> chunkMeshingTask)>();

        /// <summary>
        /// A set of chunks with information on which sections of them are to mesh.
        /// </summary>
        private readonly HashSet<(ClientChunk chunk, int index)> sectionsToMesh =
            new HashSet<(ClientChunk chunk, int index)>();

        /// <summary>
        /// This constructor is meant for worlds that are new.
        /// </summary>
        public ClientWorld(string name, string path, int seed) : base(name, path, seed) {}

        /// <summary>
        /// This constructor is meant for worlds that already exist.
        /// </summary>
        public ClientWorld(WorldInformation information, string path) : base(information, path) {}

        public void Render()
        {
            if (!IsReady) return;

            List<(ClientSection section, Vector3 position)> renderList =
                new List<(ClientSection section, Vector3 position)>();

            // Fill the render list.
            for (int x = -Application.Client.Player.LoadDistance; x <= Application.Client.Player.LoadDistance; x++)
            {
                for (int z = -Application.Client.Player.LoadDistance; z <= Application.Client.Player.LoadDistance; z++)
                {
                    if (activeChunks.TryGetValue(
                        (Application.Client.Player.ChunkX + x, Application.Client.Player.ChunkZ + z),
                        out Chunk? chunk))
                    {
                        ((ClientChunk) chunk).AddCulledToRenderList(Application.Client.Player.Frustum, renderList);
                    }
                }
            }

            // Render the collected sections.
            for (var stage = 0; stage < SectionRenderer.DrawStageCount; stage++)
            {
                if (renderList.Count == 0) break;

                SectionRenderer.PrepareStage(stage);

                for (var i = 0; i < renderList.Count; i++)
                {
                    renderList[i].section.Render(stage, renderList[i].position);
                }

                SectionRenderer.FinishStage(stage);
            }

            // Render the player
            Application.Client.Player.Render();
        }

        protected override Chunk CreateChunk(int x, int z)
        {
            return new ClientChunk(this, x, z, UpdateCounter);
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

                Application.Client.Player.Tick(deltaTime);

                // Mesh all listed sections.
                foreach ((Chunk chunk, int index) in sectionsToMesh)
                {
                    ((ClientChunk) chunk).CreateAndSetMesh(index);
                }

                sectionsToMesh.Clear();
            }
            else
            {
                if (activeChunks.Count >= 25 && activeChunks.ContainsKey((0, 0)))
                {
                    IsReady = true;

                    readyStopwatch.Stop();
                    double readyTime = readyStopwatch.Elapsed.TotalSeconds;

                    logger.LogInformation($"The world is ready after {readyTime}s.");
                }
            }

            FinishSavingChunks();
            StartSavingChunks();
        }

        protected override void ProcessNewlyActivatedChunk(Chunk activatedChunk)
        {
            chunksToMesh.Enqueue((ClientChunk) activatedChunk);

            // Schedule to mesh the chunks around this chunk
            if (activeChunks.TryGetValue((activatedChunk.X + 1, activatedChunk.Z), out Chunk? neighbor))
            {
                chunksToMesh.Enqueue((ClientChunk) neighbor);
            }

            if (activeChunks.TryGetValue((activatedChunk.X - 1, activatedChunk.Z), out neighbor))
            {
                chunksToMesh.Enqueue((ClientChunk) neighbor);
            }

            if (activeChunks.TryGetValue((activatedChunk.X, activatedChunk.Z + 1), out neighbor))
            {
                chunksToMesh.Enqueue((ClientChunk) neighbor);
            }

            if (activeChunks.TryGetValue((activatedChunk.X, activatedChunk.Z - 1), out neighbor))
            {
                chunksToMesh.Enqueue((ClientChunk) neighbor);
            }
        }

        private void FinishMeshingChunks()
        {
            if (chunkMeshingTasks.Count == 0) return;

            for (int i = chunkMeshingTasks.Count - 1; i >= 0; i--)
            {
                if (chunkMeshingTasks[i].IsCompleted)
                {
                    Task<SectionMeshData[]> completed = chunkMeshingTasks[i];
                    Chunk meshedChunk = chunksMeshing[completed.Id];

                    if (chunkMeshingTasks[i].IsFaulted)
                    {
                        Exception e = completed.Exception?.GetBaseException() ?? new NullReferenceException();

                        logger.LogCritical(
                            Events.ChunkMeshingError,
                            e,
                            "An exception occurred when meshing the chunk ({x}|{z}). The exception will be re-thrown.",
                            meshedChunk.X,
                            meshedChunk.Z);

                        throw e;
                    }
                    else
                    {
                        chunkMeshingTasks.RemoveAt(i);
                        chunksMeshing.Remove(completed.Id);

                        StartSendingMeshToChunk((ClientChunk) meshedChunk, completed);
                    }
                }
            }
        }

        private void StartSendingMeshToChunk(ClientChunk chunk, Task<SectionMeshData[]> completed)
        {
            // If there is already mesh data for this chunk, it is no longer up to date and can be discarded.

            int index = chunksToSendMeshData.FindIndex(entry => ReferenceEquals(entry.chunk, chunk));

            if (index != -1)
            {
                (ClientChunk chunk, Task<SectionMeshData[]> chunkMeshingTask) entry = chunksToSendMeshData[index];
                chunksToSendMeshData.RemoveAt(index);

                foreach (SectionMeshData data in entry.chunkMeshingTask.Result)
                {
                    data.Discard();
                }
            }

            chunksToSendMeshData.Add((chunk, completed));
        }

        private void StartMeshingChunks()
        {
            while (chunksToMesh.Count > 0 && chunkMeshingTasks.Count < MaxMeshingTasks)
            {
                ClientChunk current = chunksToMesh.Dequeue();
                Task<SectionMeshData[]> currentTask = current.CreateMeshDataTask();

                chunkMeshingTasks.Add(currentTask);
                chunksMeshing.Add(currentTask.Id, current);
            }
        }

        private void SendMeshData()
        {
            if (chunksToSendMeshData.Count > 0)
            {
                var chunkIndex = 0;

                for (var count = 0; count < MaxMeshDataSends && chunkIndex < chunksToSendMeshData.Count; count++)
                {
                    (Chunk chunk, var chunkMeshingTask) = chunksToSendMeshData[chunkIndex];

                    var clientChunk = (ClientChunk) chunk;

                    if (clientChunk.DoMeshDataSetStep(chunkMeshingTask.Result))
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
            sectionsToMesh.Add(((ClientChunk) chunk, y >> Section.SectionSizeExp));

            // Check if sections next to changed section have to be changed:

            // Next on y axis.
            if ((y & (Section.SectionSize - 1)) == 0 && (y - 1 >> Section.SectionSizeExp) >= 0)
            {
                sectionsToMesh.Add(((ClientChunk) chunk, y - 1 >> Section.SectionSizeExp));
            }
            else if ((y & (Section.SectionSize - 1)) == Section.SectionSize - 1 &&
                     (y + 1 >> Section.SectionSizeExp) < Chunk.VerticalSectionCount)
            {
                sectionsToMesh.Add(((ClientChunk) chunk, y + 1 >> Section.SectionSizeExp));
            }

            Chunk? neighbor;

            // Next on x axis.
            if ((x & (Section.SectionSize - 1)) == 0 && activeChunks.TryGetValue(
                (x - 1 >> Section.SectionSizeExp, z >> Section.SectionSizeExp),
                out neighbor))
            {
                sectionsToMesh.Add(((ClientChunk) neighbor, y >> Section.SectionSizeExp));
            }
            else if ((x & (Section.SectionSize - 1)) == Section.SectionSize - 1 && activeChunks.TryGetValue(
                (x + 1 >> Section.SectionSizeExp, z >> Section.SectionSizeExp),
                out neighbor))
            {
                sectionsToMesh.Add(((ClientChunk) neighbor, y >> Section.SectionSizeExp));
            }

            // Next on z axis.
            if ((z & (Section.SectionSize - 1)) == 0 && activeChunks.TryGetValue(
                (x >> Section.SectionSizeExp, z - 1 >> Section.SectionSizeExp),
                out neighbor))
            {
                sectionsToMesh.Add(((ClientChunk) neighbor, y >> Section.SectionSizeExp));
            }
            else if ((z & (Section.SectionSize - 1)) == Section.SectionSize - 1 && activeChunks.TryGetValue(
                (x >> Section.SectionSizeExp, z + 1 >> Section.SectionSizeExp),
                out neighbor))
            {
                sectionsToMesh.Add(((ClientChunk) neighbor, y >> Section.SectionSizeExp));
            }
        }

        protected override void AddAllTasks(ref List<Task> tasks)
        {
            base.AddAllTasks(ref tasks);
            tasks.AddRange(chunkMeshingTasks);
        }
    }
}