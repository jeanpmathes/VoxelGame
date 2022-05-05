// <copyright file="ClientWorld.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OpenTK.Mathematics;
using Properties;
using VoxelGame.Client.Entities;
using VoxelGame.Client.Rendering;
using VoxelGame.Core.Collections;
using VoxelGame.Core.Entities;
using VoxelGame.Core.Logic;
using VoxelGame.Core.Physics;
using VoxelGame.Logging;

namespace VoxelGame.Client.Logic;

/// <summary>
///     The game world, specifically for the client.
/// </summary>
public class ClientWorld : World
{
    private static readonly ILogger logger = LoggingHelper.CreateLogger<ClientWorld>();

    /// <summary>
    ///     A list of chunk meshing tasks,
    /// </summary>
    private readonly List<Task<SectionMeshData[]>> chunkMeshingTasks =
        new(MaxMeshingTasks);

    /// <summary>
    ///     A dictionary containing all chunks that are currently meshed, with the task id of their meshing task as key.
    /// </summary>
    private readonly Dictionary<int, ClientChunk> chunksMeshing = new(MaxMeshingTasks);

    /// <summary>
    ///     A queue with chunks that have to be meshed completely, mainly new chunks.
    /// </summary>
    private readonly UniqueQueue<ClientChunk> chunksToMesh = new();

    /// <summary>
    ///     A list of chunks where the mesh data has to be set.
    /// </summary>
    private readonly List<(ClientChunk chunk, SectionMeshData[] meshData)> chunksToSendMeshData =
        new();

    private readonly Stopwatch readyStopwatch = Stopwatch.StartNew();

    private readonly List<(ClientSection section, Vector3 position)> renderList = new();

    /// <summary>
    ///     A set of chunks with information on which sections of them are to mesh.
    /// </summary>
    private readonly HashSet<(ClientChunk chunk, (int x, int y, int z))> sectionsToMesh =
        new();

    private ClientPlayer? player;

    /// <summary>
    ///     This constructor is meant for worlds that are new.
    /// </summary>
    public ClientWorld(string name, string path, int seed) : base(name, path, seed) {}

    /// <summary>
    ///     This constructor is meant for worlds that already exist.
    /// </summary>
    public ClientWorld(WorldInformation information, string path) : base(information, path) {}

    private static int MaxMeshingTasks { get; } = Settings.Default.MaxMeshingTasks;
    private static int MaxMeshDataSends { get; } = Settings.Default.MaxMeshDataSends;

    /// <summary>
    ///     Add a client player to the world.
    /// </summary>
    /// <param name="newPlayer">The new player.</param>
    public void AddPlayer(ClientPlayer newPlayer)
    {
        player = newPlayer;
    }

    /// <summary>
    ///     Render this world and everything in it.
    /// </summary>
    public void Render()
    {
        if (!IsReady) return;

        renderList.Clear();

        Frustum frustum = player!.Frustum;

        // Fill the render list.
        for (int x = -Player.LoadDistance; x <= Player.LoadDistance; x++)
        for (int y = -Player.LoadDistance; y <= Player.LoadDistance; y++)
        for (int z = -Player.LoadDistance; z <= Player.LoadDistance; z++)
            if (TryGetChunk(
                    player!.ChunkX + x,
                    player!.ChunkY + y,
                    player!.ChunkZ + z,
                    out Chunk? chunk))
                ((ClientChunk) chunk).AddCulledToRenderList(frustum, renderList);

        // Render the collected sections.
        for (var stage = 0; stage < SectionRenderer.DrawStageCount; stage++)
        {
            if (renderList.Count == 0) break;

            SectionRenderer.PrepareStage(stage);

            for (var i = 0; i < renderList.Count; i++) renderList[i].section.Render(stage, renderList[i].position);

            SectionRenderer.FinishStage(stage);
        }

        // Render all players in this world
        player?.Render();
    }

    /// <inheritdoc />
    protected override Chunk CreateChunk(int x, int y, int z)
    {
        return new ClientChunk(this, x, y, z);
    }

    /// <inheritdoc />
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
            foreach (Chunk chunk in ActiveChunks) chunk.Tick();

            player!.Tick(deltaTime);

            // Mesh all listed sections.
            foreach ((Chunk chunk, (int x, int y, int z)) in sectionsToMesh)
                ((ClientChunk) chunk).CreateAndSetMesh(x, y, z);

            sectionsToMesh.Clear();
        }
        else
        {
            if (ActiveChunkCount >= 25 && IsChunkActive(x: 0, y: 0, z: 0))
            {
                IsReady = true;

                readyStopwatch.Stop();
                double readyTime = readyStopwatch.Elapsed.TotalSeconds;

                logger.LogInformation(Events.WorldState, "World ready after {ReadyTime}s", readyTime);
            }
        }

        FinishSavingChunks();
        StartSavingChunks();
    }

    /// <inheritdoc />
    protected override void ProcessNewlyActivatedChunk(Chunk activatedChunk)
    {
        chunksToMesh.Enqueue((ClientChunk) activatedChunk);

        // Schedule to mesh the chunks around this chunk
        if (TryGetChunk(activatedChunk.X + 1, activatedChunk.Y, activatedChunk.Z, out Chunk? neighbor))
            chunksToMesh.Enqueue((ClientChunk) neighbor);

        if (TryGetChunk(activatedChunk.X - 1, activatedChunk.Y, activatedChunk.Z, out neighbor))
            chunksToMesh.Enqueue((ClientChunk) neighbor);

        if (TryGetChunk(activatedChunk.X, activatedChunk.Y + 1, activatedChunk.Z, out neighbor))
            chunksToMesh.Enqueue((ClientChunk) neighbor);

        if (TryGetChunk(activatedChunk.X, activatedChunk.Y - 1, activatedChunk.Z, out neighbor))
            chunksToMesh.Enqueue((ClientChunk) neighbor);

        if (TryGetChunk(activatedChunk.X, activatedChunk.Y, activatedChunk.Z + 1, out neighbor))
            chunksToMesh.Enqueue((ClientChunk) neighbor);

        if (TryGetChunk(activatedChunk.X, activatedChunk.Y, activatedChunk.Z - 1, out neighbor))
            chunksToMesh.Enqueue((ClientChunk) neighbor);
    }

    private void FinishMeshingChunks()
    {
        if (chunkMeshingTasks.Count == 0) return;

        for (int i = chunkMeshingTasks.Count - 1; i >= 0; i--)
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
                        "An exception (critical) occurred when meshing the chunk ({X}|{Z}) and will be re-thrown",
                        meshedChunk.X,
                        meshedChunk.Z);

                    throw e;
                }

                chunkMeshingTasks.RemoveAt(i);
                chunksMeshing.Remove(completed.Id);

                StartSendingMeshToChunk((ClientChunk) meshedChunk, completed);
            }
    }

    private void StartSendingMeshToChunk(ClientChunk chunk, Task<SectionMeshData[]> completed)
    {
        // If there is already mesh data for this chunk, it is no longer up to date and can be discarded.

        int index = chunksToSendMeshData.FindIndex(entry => ReferenceEquals(entry.chunk, chunk));

        if (index != -1)
        {
            (ClientChunk clientChunk, SectionMeshData[] meshData) = chunksToSendMeshData[index];
            chunksToSendMeshData.RemoveAt(index);

            foreach (SectionMeshData data in meshData) data.Discard();

            clientChunk.ResetMeshDataSetSteps();
        }

        chunksToSendMeshData.Add((chunk, completed.Result));
    }

    private void StartMeshingChunks()
    {
        while (chunksToMesh.Count > 0 && chunkMeshingTasks.Count < MaxMeshingTasks)
        {
            ClientChunk current = chunksToMesh.Dequeue();
            Task<SectionMeshData[]> currentTask = current.CreateMeshDataAsync();

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
                (Chunk chunk, SectionMeshData[] meshData) = chunksToSendMeshData[chunkIndex];

                var clientChunk = (ClientChunk) chunk;

                if (clientChunk.DoMeshDataSetStep(meshData))
                    chunksToSendMeshData.RemoveAt(chunkIndex);
                else if (chunksToSendMeshData.Count > 1) chunkIndex++;
            }
        }
    }

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override void ProcessChangedSection(Chunk chunk, Vector3i position)
    {
        int ToLocal(int worldPosition)
        {
            return (worldPosition >> Section.SizeExp) & (Chunk.Size - 1);
        }

        (int chunkX, int chunkY, int chunkZ) = (ToLocal(position.X), ToLocal(position.Y), ToLocal(position.Z));

        sectionsToMesh.Add(((ClientChunk) chunk, (chunkX, chunkY, chunkZ)));

        // Check if sections next to changed section have to be changed:

        void CheckNeighbor(int x, int y, int z)
        {
            if (TryGetChunk(x, y, z, out Chunk? neighbor)) sectionsToMesh.Add(((ClientChunk) neighbor, (x, y, z)));
        }

        int xSectionPosition = position.X & (Section.Size - 1);

        if (xSectionPosition == 0) CheckNeighbor(ToLocal(position.X - 1), chunkX, chunkY);
        else if (xSectionPosition == Section.Size - 1) CheckNeighbor(ToLocal(position.X + 1), chunkX, chunkY);

        int ySectionPosition = position.Y & (Section.Size - 1);

        if (ySectionPosition == 0) CheckNeighbor(chunkX, ToLocal(position.Y - 1), chunkZ);
        else if (ySectionPosition == Section.Size - 1) CheckNeighbor(chunkX, ToLocal(position.Y + 1), chunkZ);

        int zSectionPosition = position.Z & (Section.Size - 1);

        if (zSectionPosition == 0) CheckNeighbor(chunkX, chunkY, ToLocal(position.Z - 1));
        else if (zSectionPosition == Section.Size - 1) CheckNeighbor(chunkX, chunkY, ToLocal(position.Z + 1));
    }

    /// <inheritdoc />
    protected override void AddAllTasks(IList<Task> tasks)
    {
        base.AddAllTasks(tasks);
        chunkMeshingTasks.ForEach(tasks.Add);
    }
}
