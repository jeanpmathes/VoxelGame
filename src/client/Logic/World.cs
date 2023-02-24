// <copyright file="World.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using OpenTK.Mathematics;
using Properties;
using VoxelGame.Client.Entities;
using VoxelGame.Client.Rendering;
using VoxelGame.Core.Logic;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Visuals;
using VoxelGame.Logging;

namespace VoxelGame.Client.Logic;

/// <summary>
///     The game world, specifically for the client.
/// </summary>
public class World : Core.Logic.World
{
    private static readonly ILogger logger = LoggingHelper.CreateLogger<World>();

    private readonly Stopwatch readyStopwatch = Stopwatch.StartNew();

    private readonly List<(Section section, Vector3d position)> renderList = new();

    /// <summary>
    ///     A set of chunks with information on which sections of them are to mesh.
    /// </summary>
    private readonly HashSet<(Chunk chunk, (int x, int y, int z))> sectionsToMesh =
        new();

    private Player? player;

    /// <summary>
    ///     This constructor is meant for worlds that are new.
    /// </summary>
    public World(DirectoryInfo path, string name, (int upper, int lower) seed) : base(path, name, seed)
    {
        Setup();
    }

    /// <summary>
    ///     This constructor is meant for worlds that already exist.
    /// </summary>
    public World(DirectoryInfo path, WorldInformation information) : base(path, information)
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
    public void AddPlayer(Player newPlayer)
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
        for (int x = -Core.Entities.Player.LoadDistance; x <= Core.Entities.Player.LoadDistance; x++)
        for (int y = -Core.Entities.Player.LoadDistance; y <= Core.Entities.Player.LoadDistance; y++)
        for (int z = -Core.Entities.Player.LoadDistance; z <= Core.Entities.Player.LoadDistance; z++)
        {
            Core.Logic.Chunk? chunk = GetActiveChunk(player!.Chunk.Offset(x, y, z));
            chunk?.Cast().AddCulledToRenderList(context.Frustum, renderList);
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
    protected override Core.Logic.Chunk CreateChunk(ChunkPosition position, ChunkContext context)
    {
        return new Chunk(this, position, context);
    }

    /// <inheritdoc />
    public override void Update(double deltaTime)
    {
        UpdateChunks();

        void HandleActivating()
        {
            if (ActiveChunkCount < 3 * 3 * 3) return;

            readyStopwatch.Stop();
            double readyTime = readyStopwatch.Elapsed.TotalSeconds;

            logger.LogInformation(Events.WorldState, "World ready after {ReadyTime}s", readyTime);

            CurrentState = State.Active;
        }

        void HandleActive()
        {
            // Tick objects in world.
            foreach (Core.Logic.Chunk chunk in ActiveChunks) chunk.Tick();

            player!.Tick(deltaTime);

            // Mesh all listed sections.
            foreach ((Chunk chunk, (int x, int y, int z)) in sectionsToMesh)
                chunk.CreateAndSetMesh(x, y, z, ChunkMeshingContext.UsingActive(chunk));

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
    protected override ChunkState ProcessNewlyActivatedChunk(Core.Logic.Chunk activatedChunk)
    {
        if (activatedChunk.IsFullyDecorated)
        {
            foreach (BlockSide side in BlockSide.All.Sides())
                if (TryGetChunk(side.Offset(activatedChunk.Position), out Core.Logic.Chunk? neighbor))
                {
                    neighbor.Cast().BeginMeshing();
                }

            return new Chunk.Meshing();
        }

        ChunkState? decoration = activatedChunk.ProcessDecorationOption();

        return decoration ?? new Core.Logic.Chunk.Hidden();
    }

    /// <inheritdoc />
    protected override ChunkState? ProcessActivatedChunk(Core.Logic.Chunk activatedChunk)
    {
        return activatedChunk.Cast().ProcessDecorationOption() ??
               activatedChunk.Cast().ProcessMeshingOption();
    }

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override void ProcessChangedSection(Core.Logic.Chunk chunk, Vector3i position)
    {
        sectionsToMesh.Add((chunk.Cast(), SectionPosition.From(position).Local));

        // Check if sections next to changed section have to be changed:

        void CheckNeighbor(Vector3i neighborPosition)
        {
            Core.Logic.Chunk? neighbor = GetActiveChunk(neighborPosition);

            if (neighbor == null) return;

            sectionsToMesh.Add((neighbor.Cast(), SectionPosition.From(neighborPosition).Local));
        }

        int xSectionPosition = position.X & (Core.Logic.Section.Size - 1);

        if (xSectionPosition == 0) CheckNeighbor(position - (1, 0, 0));
        else if (xSectionPosition == Core.Logic.Section.Size - 1) CheckNeighbor(position + (1, 0, 0));

        int ySectionPosition = position.Y & (Core.Logic.Section.Size - 1);

        if (ySectionPosition == 0) CheckNeighbor(position - (0, 1, 0));
        else if (ySectionPosition == Core.Logic.Section.Size - 1) CheckNeighbor(position + (0, 1, 0));

        int zSectionPosition = position.Z & (Core.Logic.Section.Size - 1);

        if (zSectionPosition == 0) CheckNeighbor(position - (0, 0, 1));
        else if (zSectionPosition == Core.Logic.Section.Size - 1) CheckNeighbor(position + (0, 0, 1));
    }
}
