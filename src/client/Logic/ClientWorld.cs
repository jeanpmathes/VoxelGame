// <copyright file="ClientWorld.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using OpenTK.Mathematics;
using Properties;
using VoxelGame.Client.Entities;
using VoxelGame.Client.Rendering;
using VoxelGame.Core.Entities;
using VoxelGame.Core.Logic;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Visuals;
using VoxelGame.Logging;

namespace VoxelGame.Client.Logic;

/// <summary>
///     The game world, specifically for the client.
/// </summary>
public class ClientWorld : World
{
    private static readonly ILogger logger = LoggingHelper.CreateLogger<ClientWorld>();

    private readonly Stopwatch readyStopwatch = Stopwatch.StartNew();

    private readonly List<(ClientSection section, Vector3d position)> renderList = new();

    /// <summary>
    ///     A set of chunks with information on which sections of them are to mesh.
    /// </summary>
    private readonly HashSet<(ClientChunk chunk, (int x, int y, int z))> sectionsToMesh =
        new();

    private ClientPlayer? player;

    /// <summary>
    ///     This constructor is meant for worlds that are new.
    /// </summary>
    public ClientWorld(string path, string name, int seed) : base(path, name, seed)
    {
        Setup();
    }

    /// <summary>
    ///     This constructor is meant for worlds that already exist.
    /// </summary>
    public ClientWorld(string path, WorldInformation information) : base(path, information)
    {
        Setup();
    }

    /// <summary>
    ///     Get the max meshing task limit.
    /// </summary>
    public Limit MaxMeshingTasks { get; private set; } = null!;

    /// <summary>
    ///     Get the max mesh data send limit.
    /// </summary>
    public Limit MaxMeshDataSends { get; private set; } = null!;

    private void Setup()
    {
        MaxMeshingTasks = ChunkContext.DeclareBudget(Settings.Default.MaxMeshingTasks);
        MaxMeshDataSends = ChunkContext.DeclareBudget(Settings.Default.MaxMeshDataSends);
    }

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
        if (!IsActive) return;

        IView view = player!.View;

        Application.Client.Instance.Resources.Shaders.SetPlanes(view.NearClipping, view.FarClipping);
        PassContext context = new(view.ViewMatrix, view.ProjectionMatrix, view.Frustum);

        renderList.Clear();

        // Fill the render list.
        for (int x = -Player.LoadDistance; x <= Player.LoadDistance; x++)
        for (int y = -Player.LoadDistance; y <= Player.LoadDistance; y++)
        for (int z = -Player.LoadDistance; z <= Player.LoadDistance; z++)
        {
            Chunk? chunk = GetActiveChunk(player!.Chunk.Offset(x, y, z));

            if (chunk == null) continue;

            ((ClientChunk) chunk).AddCulledToRenderList(context.Frustum, renderList);
        }

        DoRenderPass(context);
    }

    private void DoRenderPass(PassContext context)
    {
        // Render the collected sections.
        for (var stage = 0; stage < SectionRenderer.DrawStageCount; stage++)
        {
            if (renderList.Count == 0) break;

            SectionRenderer.PrepareStage(stage, context);

            for (var i = 0; i < renderList.Count; i++) renderList[i].section.Render(stage, renderList[i].position);

            SectionRenderer.FinishStage(stage);
        }

        SectionRenderer.DrawFullscreenPasses();

        // Render all players in this world
        player?.Render();
    }

    /// <inheritdoc />
    protected override Chunk CreateChunk(ChunkPosition position, ChunkContext context)
    {
        return new ClientChunk(this, position, context);
    }

    /// <inheritdoc />
    public override void Update(double deltaTime)
    {
        UpdateChunks();

        void HandleActivating()
        {
            if (ActiveChunkCount < 3 * 3 * 3 || !IsChunkActive(ChunkPosition.Origin)) return;

            CurrentState = State.Active;

            readyStopwatch.Stop();
            double readyTime = readyStopwatch.Elapsed.TotalSeconds;

            logger.LogInformation(Events.WorldState, "World ready after {ReadyTime}s", readyTime);
        }

        void HandleActive()
        {
            // Tick objects in world.
            foreach (Chunk chunk in ActiveChunks) chunk.Tick();

            player!.Tick(deltaTime);

            // Mesh all listed sections.
            foreach ((Chunk chunk, (int x, int y, int z)) in sectionsToMesh)
                ((ClientChunk) chunk).CreateAndSetMesh(x, y, z, ChunkMeshingContext.FromActive(chunk));

            sectionsToMesh.Clear();
        }

        switch (CurrentState)
        {
            case State.Activating:
                HandleActivating();

                break;

            case State.Active:
                HandleActive();

                break;

            case State.Deactivating:
                ProcessDeactivation();

                break;

            default:
                Debug.Fail("Invalid world state.");

                break;
        }
    }

    /// <inheritdoc />
    protected override ChunkState ProcessNewlyActivatedChunk(Chunk activatedChunk)
    {
        if (activatedChunk.IsFullyDecorated)
        {
            if (TryGetChunk(activatedChunk.Position.Offset(x: 1, y: 0, z: 0), out Chunk? neighbor))
                ((ClientChunk) neighbor).BeginMeshing();

            if (TryGetChunk(activatedChunk.Position.Offset(x: -1, y: 0, z: 0), out neighbor))
                ((ClientChunk) neighbor).BeginMeshing();

            if (TryGetChunk(activatedChunk.Position.Offset(x: 0, y: 1, z: 0), out neighbor))
                ((ClientChunk) neighbor).BeginMeshing();

            if (TryGetChunk(activatedChunk.Position.Offset(x: 0, y: -1, z: 0), out neighbor))
                ((ClientChunk) neighbor).BeginMeshing();

            if (TryGetChunk(activatedChunk.Position.Offset(x: 0, y: 0, z: 1), out neighbor))
                ((ClientChunk) neighbor).BeginMeshing();

            if (TryGetChunk(activatedChunk.Position.Offset(x: 0, y: 0, z: -1), out neighbor))
                ((ClientChunk) neighbor).BeginMeshing();

            return new ClientChunk.Meshing();
        }

        ChunkState? decoration = activatedChunk.ProcessDecorationOption();

        return decoration ?? new Chunk.Hidden();
    }

    /// <inheritdoc />
    protected override ChunkState? ProcessActivatedChunk(Chunk activatedChunk)
    {
        return activatedChunk.ProcessDecorationOption();
    }

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override void ProcessChangedSection(Chunk chunk, Vector3i position)
    {
        sectionsToMesh.Add(((ClientChunk) chunk, SectionPosition.From(position).GetLocal()));

        // Check if sections next to changed section have to be changed:

        void CheckNeighbor(Vector3i neighborPosition)
        {
            Chunk? neighbor = GetActiveChunk(neighborPosition);

            if (neighbor == null) return;

            sectionsToMesh.Add(((ClientChunk) neighbor, SectionPosition.From(neighborPosition).GetLocal()));
        }

        int xSectionPosition = position.X & (Section.Size - 1);

        if (xSectionPosition == 0) CheckNeighbor(position - (1, 0, 0));
        else if (xSectionPosition == Section.Size - 1) CheckNeighbor(position + (1, 0, 0));

        int ySectionPosition = position.Y & (Section.Size - 1);

        if (ySectionPosition == 0) CheckNeighbor(position - (0, 1, 0));
        else if (ySectionPosition == Section.Size - 1) CheckNeighbor(position + (0, 1, 0));

        int zSectionPosition = position.Z & (Section.Size - 1);

        if (zSectionPosition == 0) CheckNeighbor(position - (0, 0, 1));
        else if (zSectionPosition == Section.Size - 1) CheckNeighbor(position + (0, 0, 1));
    }
}
