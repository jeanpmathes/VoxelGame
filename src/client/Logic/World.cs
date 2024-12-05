// <copyright file="World.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using OpenTK.Mathematics;
using VoxelGame.Client.Actors;
using VoxelGame.Client.Visuals;
using VoxelGame.Core.Logic;
using VoxelGame.Core.Logic.Chunks;
using VoxelGame.Core.Logic.Elements;
using VoxelGame.Core.Logic.Sections;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Profiling;
using VoxelGame.Graphics.Core;
using VoxelGame.Graphics.Data;
using VoxelGame.Logging;
using VoxelGame.Toolkit.Memory;
using Chunk = VoxelGame.Client.Logic.Chunks.Chunk;

namespace VoxelGame.Client.Logic;

/// <summary>
///     The game world, specifically for the client.
/// </summary>
public class World : Core.Logic.World
{
    private static readonly Vector3d sunLightDirection = Vector3d.Normalize(new Vector3d(x: -2, y: -3, z: -1));

    #region LOGGING

    private static readonly ILogger logger = LoggingHelper.CreateLogger<World>();

    #endregion LOGGING

    /// <summary>
    ///     A set of chunks with information on which sections of them are to mesh.
    /// </summary>
    private readonly HashSet<(Chunk chunk, (Int32 x, Int32 y, Int32 z))> sectionsToMesh = [];

    private readonly Space space;

    private readonly List<Core.Logic.Chunks.Chunk> chunksWithActors = [];

    private Player? player;

    /// <summary>
    ///     This constructor is meant for worlds that are new.
    /// </summary>
    public World(DirectoryInfo path, String name, (Int32 upper, Int32 lower) seed) : base(path, name, seed)
    {
        space = Application.Client.Instance.Space;

        SetUp();
    }

    /// <summary>
    ///     This constructor is meant for worlds that already exist.
    /// </summary>
    public World(WorldData data) : base(data)
    {
        space = Application.Client.Instance.Space;

        SetUp();
    }

    private void SetUp()
    {
        space.Light.Direction = sunLightDirection;

        State.Activated += OnActivation;
        State.Deactivated += OnDeactivation;
        State.Terminated += OnTermination;
    }

    /// <inheritdoc />
    protected override Core.Logic.Chunks.Chunk CreateChunk(NativeSegment<UInt32> blocks, ChunkContext context)
    {
        return new Chunk(blocks, context);
    }

    /// <summary>
    ///     Add a client player to the world.
    /// </summary>
    /// <param name="newPlayer">The new player.</param>
    public void AddPlayer(Player newPlayer)
    {
        player = newPlayer;

        player.OnAdd(this);
    }

    private void RemovePlayer()
    {
        player?.OnRemove();
        player = null;
    }

    /// <summary>
    ///     Render this world and everything in it.
    /// </summary>
    public void Render()
    {
        if (!State.IsActive) return;

        Frustum frustum = player!.View.Frustum;

        if (Program.IsDebug) Chunks.ForEachActive(chunk => chunk.Cast().CullSections(frustum));
        else
            // Rendering chunks even if they are used by an off-thread operation is safe.
            // The rendering resources are only modified on the main thread anyway.
            Chunks.ForEachComplete(chunk => chunk.Cast().CullSections(frustum));
    }

    /// <inheritdoc />
    public override void ActiveUpdate(Double deltaTime, Timer? updateTimer)
    {
        using (Timer? tickTimer = logger.BeginTimedSubScoped("World Update Ticks", updateTimer))
        {
            DoTicksOnEverything(deltaTime, tickTimer);
        }

        using (logger.BeginTimedSubScoped("World Update Meshing", updateTimer))
        {
            MeshAndClearSectionList();
        }
    }

    private void OnActivation(Object? sender, EventArgs e)
    {
        player?.OnActivate();
    }

    private void OnDeactivation(Object? sender, EventArgs e)
    {
        player?.OnDeactivate();
    }

    private void OnTermination(Object? sender, EventArgs e)
    {
        RemovePlayer();

        foreach (Core.Logic.Chunks.Chunk chunk in Chunks.All)
            chunk.Cast().HideAllSections();
    }

    private void DoTicksOnEverything(Double deltaTime, Timer? tickTimer)
    {
        chunksWithActors.Clear();

        using (logger.BeginTimedSubScoped("World Tick Chunks", tickTimer))
        {
            Chunks.ForEachActive(TickChunk);
        }

        using (logger.BeginTimedSubScoped("World Tick Actors", tickTimer))
        {
            #pragma warning disable S4158 // chunksWithActors is filled by calls to TickChunk
            foreach (Core.Logic.Chunks.Chunk chunk in chunksWithActors)
                chunk.TickActors(deltaTime);
            #pragma warning restore S4158
        }
    }

    private void TickChunk(Core.Logic.Chunks.Chunk chunk)
    {
        if (!chunk.IsRequestedToSimulate)
            return;

        chunk.Tick();

        if (chunk.HasActors)
            chunksWithActors.Add(chunk);
    }

    private void MeshAndClearSectionList()
    {
        foreach ((Chunk chunk, (Int32 x, Int32 y, Int32 z)) in sectionsToMesh)
        {
            using ChunkMeshingContext context = ChunkMeshingContext.UsingActive(chunk, SpatialMeshingFactory.Shared);
            chunk.CreateAndSetMesh(x, y, z, context);
        }

        sectionsToMesh.Clear();
    }

    /// <inheritdoc />
    protected override ChunkState? ProcessNewlyActivatedChunk(Core.Logic.Chunks.Chunk activatedChunk)
    {
        return activatedChunk.ProcessDecorationOption() ?? ProcessActivatedChunk(activatedChunk);
    }

    /// <inheritdoc />
    protected override ChunkState? ProcessActivatedChunk(Core.Logic.Chunks.Chunk activatedChunk)
    {
        ChunkState? state = activatedChunk.Cast().ProcessMeshingOption(out Boolean allowActivation);

        if (state != null) return state;

        return allowActivation
            ? new Core.Logic.Chunks.Chunk.Active()
            : null;
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

                Sides missing = direction.ToSide().ToFlag();
                neighbor.Cast().SetSectionAsIncomplete(SectionPosition.From(neighborPosition).Local, missing);
            }
        }
    }
}
