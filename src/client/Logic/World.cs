// <copyright file="World.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using OpenTK.Mathematics;
using Properties;
using VoxelGame.Core.Entities;
using VoxelGame.Core.Logic;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Visuals;
using VoxelGame.Logging;
using VoxelGame.Support.Core;
using VoxelGame.Support.Data;

namespace VoxelGame.Client.Logic;

/// <summary>
///     The game world, specifically for the client.
/// </summary>
public class World : Core.Logic.World
{
    private static readonly ILogger logger = LoggingHelper.CreateLogger<World>();

    private static readonly Vector3d sunLightDirection = Vector3d.Normalize(new Vector3d(x: -2, y: -3, z: -1));

    private static readonly int minLoadedChunksAtStart = Math.Max(VMath.Cube((Player.LoadDistance - 1) * 2 + 1), val2: 1);

    private readonly Stopwatch readyStopwatch = Stopwatch.StartNew();

    /// <summary>
    ///     A set of chunks with information on which sections of them are to mesh.
    /// </summary>
    private readonly HashSet<(Chunk chunk, (int x, int y, int z))> sectionsToMesh =
        new();

    private readonly Space space;

    private Entities.Player? player;

    /// <summary>
    ///     This constructor is meant for worlds that are new.
    /// </summary>
    public World(DirectoryInfo path, string name, (int upper, int lower) seed) : base(path, name, seed)
    {
        space = Application.Client.Instance.Space;

        Setup();
    }

    /// <summary>
    ///     This constructor is meant for worlds that already exist.
    /// </summary>
    public World(DirectoryInfo path, WorldInformation information) : base(path, information)
    {
        space = Application.Client.Instance.Space;

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

        space.Light.Direction = sunLightDirection;
    }

    /// <summary>
    ///     Add a client player to the world.
    /// </summary>
    /// <param name="newPlayer">The new player.</param>
    public void AddPlayer(Entities.Player newPlayer)
    {
        player = newPlayer;

        space.Light.Direction = sunLightDirection;
    }

    /// <summary>
    ///     Render this world and everything in it.
    /// </summary>
    public void Render()
    {
        if (!IsActive) return;

        Frustum frustum = player!.View.Frustum;

        CullActiveChunks();

        return;

        void CullActiveChunks()
        {
            for (int x = -Player.LoadDistance; x <= Player.LoadDistance; x++)
            for (int y = -Player.LoadDistance; y <= Player.LoadDistance; y++)
            for (int z = -Player.LoadDistance; z <= Player.LoadDistance; z++)
            {
                Core.Logic.Chunk? chunk = GetActiveChunk(player!.Chunk.Offset(x, y, z));
                chunk?.Cast().CullSections(frustum);
            }
        }
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

        void HandleActive()
        {
            DoTicksOnEverything(deltaTime);
            MeshAndClearSectionList();
        }

        void HandleActivating()
        {
            if (ActiveChunkCount < minLoadedChunksAtStart) return;

            readyStopwatch.Stop();
            double readyTime = readyStopwatch.Elapsed.TotalSeconds;

            logger.LogInformation(Events.WorldState, "World ready after {ReadyTime}s", readyTime);

            CurrentState = State.Active;

            OnActivation();
        }
    }

    private void OnActivation()
    {
        player?.OnActivate();
    }

    /// <inheritdoc />
    protected override void OnDeactivation()
    {
        player?.OnDeactivate();
    }

    private void DoTicksOnEverything(double deltaTime)
    {
        foreach (Core.Logic.Chunk chunk in ActiveChunks) chunk.Tick();

        player!.Tick(deltaTime);
    }

    private void MeshAndClearSectionList()
    {
        foreach ((Chunk chunk, (int x, int y, int z)) in sectionsToMesh)
            chunk.CreateAndSetMesh(x, y, z, ChunkMeshingContext.UsingActive(chunk, SpatialMeshingFactory.Shared));

        sectionsToMesh.Clear();
    }

    /// <inheritdoc />
    protected override ChunkState ProcessNewlyActivatedChunk(Core.Logic.Chunk activatedChunk)
    {
        ChunkState? decoration = activatedChunk.ProcessDecorationOption();

        if (decoration != null) return decoration;
        if (!activatedChunk.IsFullyDecorated) return new Core.Logic.Chunk.Hidden();

        foreach (BlockSide side in BlockSide.All.Sides())
            if (TryGetChunk(side.Offset(activatedChunk.Position), out Core.Logic.Chunk? neighbor))
                neighbor.Cast().BeginMeshing();

        return new Chunk.Meshing();
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
        EnqueueMeshingForAllAffectedSections(chunk, position);
    }

    /// <summary>
    ///     Find all sections that need to be meshed because of a block change in a section.
    ///     If the block position is on the edge of a section, the neighbor is also considered to be affected.
    /// </summary>
    /// <param name="chunk">The chunk in which the block change happened.</param>
    /// <param name="position">The position of the block change, in block coordinates.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void EnqueueMeshingForAllAffectedSections(Core.Logic.Chunk chunk, Vector3i position)
    {
        sectionsToMesh.Add((chunk.Cast(), SectionPosition.From(position).Local));

        CheckAxis(axis: 0);
        CheckAxis(axis: 1);
        CheckAxis(axis: 2);

        void CheckAxis(int axis)
        {
            int axisSectionPosition = position[axis] & (Core.Logic.Section.Size - 1);

            Vector3i direction = new()
            {
                [axis] = 1
            };

            if (axisSectionPosition == 0) CheckNeighbor(direction * -1);
            else if (axisSectionPosition == Core.Logic.Section.Size - 1) CheckNeighbor(direction);
        }

        void CheckNeighbor(Vector3i direction)
        {
            Vector3i neighborPosition = position + direction;

            if (!TryGetChunk(ChunkPosition.From(neighborPosition), out Core.Logic.Chunk? neighbor)) return;

            if (neighbor.IsActive)
            {
                sectionsToMesh.Add((neighbor.Cast(), SectionPosition.From(neighborPosition).Local));
            }
            else
            {
                // We set the section as incomplete.
                // The next time the neighbor chunk is activated (if it is), the section will be meshed.

                BlockSides missing = direction.ToBlockSide().ToFlag();
                neighbor.Cast().SetSectionAsIncomplete(SectionPosition.From(neighborPosition).Local, missing);
            }
        }
    }
}
