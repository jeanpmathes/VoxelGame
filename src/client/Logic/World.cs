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
using VoxelGame.Core.Actors;
using VoxelGame.Core.Collections;
using VoxelGame.Core.Logic;
using VoxelGame.Core.Logic.Chunks;
using VoxelGame.Core.Logic.Elements;
using VoxelGame.Core.Logic.Sections;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Profiling;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Utilities.Units;
using VoxelGame.Core.Visuals;
using VoxelGame.Logging;
using VoxelGame.Support.Core;
using VoxelGame.Support.Data;
using Chunk = VoxelGame.Client.Logic.Chunks.Chunk;

namespace VoxelGame.Client.Logic;

/// <summary>
///     The game world, specifically for the client.
/// </summary>
public partial class World : Core.Logic.World
{
    private static readonly Vector3d sunLightDirection = Vector3d.Normalize(new Vector3d(x: -2, y: -3, z: -1));
    private static readonly Int32 minLoadedChunksAtStart = VMath.Cube(Player.LoadDistance * 2 + 1);

    /// <summary>
    ///     A set of chunks with information on which sections of them are to mesh.
    /// </summary>
    private readonly HashSet<(Chunk chunk, (Int32 x, Int32 y, Int32 z))> sectionsToMesh = [];

    private readonly Space space;

    private Int64 worldUpdateCount;
    private Int64 chunkUpdateCount;
    private Actors.Player? player;

    /// <summary>
    ///     This constructor is meant for worlds that are new.
    /// </summary>
    public World(DirectoryInfo path, String name, (Int32 upper, Int32 lower) seed) : base(path, name, seed)
    {
        space = Application.Client.Instance.Space;

        Setup();
    }

    /// <summary>
    ///     This constructor is meant for worlds that already exist.
    /// </summary>
    public World(WorldData data) : base(data)
    {
        space = Application.Client.Instance.Space;

        Setup();
    }

    private void Setup()
    {
        space.Light.Direction = sunLightDirection;
    }

    /// <inheritdoc />
    protected override ChunkPool CreateChunkPool()
    {
        return new ChunkPool(ChunkContext, context => new Chunk(context));
    }

    /// <summary>
    ///     Add a client player to the world.
    /// </summary>
    /// <param name="newPlayer">The new player.</param>
    public void AddPlayer(Actors.Player newPlayer)
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
            foreach (Core.Logic.Chunks.Chunk chunk in ActiveChunks)
            {
                chunk.Cast().CullSections(frustum);
            }
        }
    }

    /// <summary>
    ///     Process an update step for this world.
    /// </summary>
    /// <param name="deltaTime">Time since the last update.</param>
    /// <param name="updateTimer">A timer for profiling.</param>
    public void Update(Double deltaTime, Timer? updateTimer)
    {
        using Timer? subTimer = logger.BeginTimedSubScoped("World Update", updateTimer);

        using (logger.BeginTimedSubScoped("World Update Chunks", subTimer))
        {
            UpdateChunkStates();
        }

        switch (CurrentState)
        {
            case State.Activating:
                ProcessActivating();

                break;

            case State.Active:
                ProcessActive();

                break;

            case State.Deactivating:
                ProcessDeactivation();

                break;

            default:
                Debug.Fail("Invalid world state.");

                break;
        }

        void ProcessActive()
        {
            using (Timer? tickTimer = logger.BeginTimedSubScoped("World Update Ticks", subTimer))
            {
                DoTicksOnEverything(deltaTime, tickTimer);
            }

            using (logger.BeginTimedSubScoped("World Update Meshing", subTimer))
            {
                MeshAndClearSectionList();
            }
        }

        void ProcessActivating()
        {
            worldUpdateCount += 1;
            chunkUpdateCount += ChunkStateUpdateCount;

            if (ActiveChunkCount < minLoadedChunksAtStart) return;

            Duration readyTime = timer?.Elapsed ?? default;
            LogWorldReady(logger, readyTime, worldUpdateCount, chunkUpdateCount);

            timer?.Dispose();
            timer = null;

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

    private void DoTicksOnEverything(Double deltaTime, Timer? tickTimer)
    {
        using (logger.BeginTimedSubScoped("World Tick Chunks", tickTimer))
        {
            foreach (Core.Logic.Chunks.Chunk chunk in ActiveChunks) chunk.Tick();
        }

        using (logger.BeginTimedSubScoped("World Tick Player", tickTimer))
        {
            player!.Tick(deltaTime);
        }
    }

    private void MeshAndClearSectionList()
    {
        foreach ((Chunk chunk, (Int32 x, Int32 y, Int32 z)) in sectionsToMesh)
            chunk.CreateAndSetMesh(x, y, z, ChunkMeshingContext.UsingActive(chunk, SpatialMeshingFactory.Shared));

        sectionsToMesh.Clear();
    }

    /// <inheritdoc />
    protected override ChunkState? ProcessNewlyActivatedChunk(Core.Logic.Chunks.Chunk activatedChunk)
    {
        ChunkState? decoration = activatedChunk.ProcessDecorationOption();

        if (decoration != null) return decoration;
        if (!activatedChunk.ShouldMeshAccordingToNeighborState()) return null;
        if (!activatedChunk.IsViableForMeshing()) return null;

        foreach (BlockSide side in BlockSide.All.Sides())
            if (TryGetChunk(side.Offset(activatedChunk.Position), out Core.Logic.Chunks.Chunk? neighbor))
                neighbor.Cast().BeginMeshing(side.Opposite());

        return new Chunk.Meshing(BlockSide.All);
    }

    /// <inheritdoc />
    protected override ChunkState ProcessActivatedChunk(Core.Logic.Chunks.Chunk activatedChunk)
    {
        Debug.Assert(activatedChunk.IsFullyDecorated);

        return activatedChunk.Cast().ProcessMeshingOption() ??
               new Core.Logic.Chunks.Chunk.Active();
    }

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override void ProcessChangedSection(Core.Logic.Chunks.Chunk chunk, Vector3i position)
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
    private void EnqueueMeshingForAllAffectedSections(Core.Logic.Chunks.Chunk chunk, Vector3i position)
    {
        sectionsToMesh.Add((chunk.Cast(), SectionPosition.From(position).Local));

        CheckAxis(axis: 0);
        CheckAxis(axis: 1);
        CheckAxis(axis: 2);

        void CheckAxis(Int32 axis)
        {
            Int32 axisSectionPosition = position[axis] & (Section.Size - 1);

            Vector3i direction = new()
            {
                [axis] = 1
            };

            if (axisSectionPosition == 0) CheckNeighbor(direction * -1);
            else if (axisSectionPosition == Section.Size - 1) CheckNeighbor(direction);
        }

        void CheckNeighbor(Vector3i direction)
        {
            Vector3i neighborPosition = position + direction;

            if (!TryGetChunk(ChunkPosition.From(neighborPosition), out Core.Logic.Chunks.Chunk? neighbor)) return;

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

    #region LOGGING

    private static readonly ILogger logger = LoggingHelper.CreateLogger<World>();

    [LoggerMessage(EventId = Events.WorldState, Level = LogLevel.Information, Message = "World ready after {ReadyTime}, using {WorldUpdates} world updates with {ChunkUpdates} chunk updates")]
    private static partial void LogWorldReady(ILogger logger, Duration readyTime, Int64 worldUpdates, Int64 chunkUpdates);

    #endregion LOGGING
}
