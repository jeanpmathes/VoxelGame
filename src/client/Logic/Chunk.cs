// <copyright file="Chunk.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using Microsoft.Extensions.Logging;
using OpenTK.Mathematics;
using VoxelGame.Core.Logic;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Visuals;
using VoxelGame.Logging;
using VoxelGame.Support.Data;

namespace VoxelGame.Client.Logic;

/// <summary>
///     A chunk of the world, specifically for the client.
/// </summary>
public partial class Chunk : Core.Logic.Chunk
{
    private Boolean hasMeshData;
    private BlockSides meshedSides;

    /// <summary>
    ///     Create a new client chunk.
    /// </summary>
    /// <param name="context">The context of the chunk.</param>
    public Chunk(ChunkContext context) : base(context, CreateSection) {}

    /// <summary>
    ///     Get the client world this chunk is in.
    /// </summary>
    public new World World => base.World.Cast();

    /// <inheritdoc />
    public override void Initialize(Core.Logic.World world, ChunkPosition position)
    {
        base.Initialize(world, position);

        hasMeshData = false;
        meshedSides = BlockSides.None;
    }

    /// <inheritdoc />
    public override void Reset()
    {
        base.Reset();

        hasMeshData = false;
        meshedSides = BlockSides.None;
    }

    private Section GetSection(Int32 index)
    {
        return GetSectionByIndex(index).Cast();
    }

    /// <summary>
    ///     Begin meshing the chunk.
    ///     If a chunk on meshing requests the neighbors to mesh, the parameter should be set accordingly.
    /// </summary>
    /// <param name="side">The side from which the meshing was requested, or <see cref="BlockSide.All"/> if not specified.</param>
    public void BeginMeshing(BlockSide side)
    {
        Throw.IfDisposed(disposed);

        if (!this.IsViableForMeshing()) return;

        State.RequestNextState(new Meshing(side),
            new Core.Logic.ChunkState.RequestDescription
            {
                AllowDuplicate = false,
                AllowSkipOnDeactivation = true,
                AllowDiscardOnRepeat = false
            });
    }

    private static Core.Logic.Section CreateSection()
    {
        return new Section();
    }

    /// <summary>
    ///     Create a mesh for a section of this chunk and activate it.
    ///     This method should only be called from the main thread.
    /// </summary>
    /// <param name="x">The x position of the section relative in this chunk.</param>
    /// <param name="y">The y position of the section relative in this chunk.</param>
    /// <param name="z">The z position of the section relative in this chunk.</param>
    /// <param name="context">The chunk meshing context.</param>
    public void CreateAndSetMesh(Int32 x, Int32 y, Int32 z, ChunkMeshingContext context)
    {
        Throw.IfDisposed(disposed);

        GetSection(LocalSectionToIndex(x, y, z)).CreateAndSetMesh(context);
    }

    /// <summary>
    ///     Process a chance to mesh the entire chunk.
    /// </summary>
    /// <returns>A target state if the chunk would like to mesh, null otherwise.</returns>
    public Core.Logic.ChunkState? ProcessMeshingOption()
    {
        Throw.IfDisposed(disposed);

        if (!this.IsViableForMeshing()) return null;

        BlockSides sides = ChunkMeshingContext.DetermineImprovementSides(this, meshedSides);

        if (sides == BlockSides.None) return null;

        foreach (BlockSide side in BlockSide.All.Sides())
        {
            BlockSides current = side.ToFlag();

            // If a side is not included, it means the chunk can't be meshed anyways.
            if (!sides.HasFlag(current)) continue;

            // While a neighbor could have changed while this chunk was inactive, skipping is safe:
            // - If some sections have changed, the incomplete section system will fix that.
            // - If the entire neighbor has changed, that chunk will miss the flag and fix that on its activation.
            if (meshedSides.HasFlag(current)) continue;

            // A chunk can only mesh if it exists.
            if (!World.TryGetChunk(side.Offset(Position), out Core.Logic.Chunk? chunk)) continue;

            chunk.Cast().BeginMeshing(side.Opposite());
        }

        return new Meshing(BlockSide.All);
    }

    /// <inheritdoc />
    protected override void OnActivation()
    {
        RecreateIncompleteSectionMeshes();
    }

    /// <inheritdoc />
    protected override void OnDeactivation()
    {
        DisableAllVfx();
    }

    /// <inheritdoc />
    protected override void OnNeighborActivation(Core.Logic.Chunk neighbor)
    {
        RecreateIncompleteSectionMeshes();
    }

    private void RecreateIncompleteSectionMeshes()
    {
        ChunkMeshingContext context = ChunkMeshingContext.UsingActive(this, SpatialMeshingFactory.Shared);

        for (var index = 0; index < SectionCount; index++) GetSection(index).RecreateIncompleteMesh(context);
    }

    /// <summary>
    ///     Set a section as incomplete, which means that it was meshed without all required neighbors.
    ///     Alternatively, it can also indicate that a section was not meshed when affected by a data change.
    ///     This method does not require any resource access as it should only be called from the main thread.
    /// </summary>
    /// <param name="local">The local position of the section.</param>
    /// <param name="sides">The sides that are missing for the section.</param>
    public void SetSectionAsIncomplete((Int32 x, Int32 y, Int32 z) local, BlockSides sides)
    {
        Throw.IfDisposed(disposed);

        GetLocalSection(local.x, local.y, local.z).Cast().SetAsIncomplete(sides);
    }

    private ChunkMeshData CreateMeshData(ChunkMeshingContext context)
    {
        if (logger.IsEnabled(LogLevel.Debug))
            LogStartedCreatingMeshData(logger, Position, context.AvailableSides.ToCompactString());

        var sectionMeshes = new SectionMeshData?[SectionCount];

        foreach (Int32 index in context.SectionIndices)
            sectionMeshes[index] = GetSection(index).CreateMeshData(context);

        if (logger.IsEnabled(LogLevel.Debug))
            LogFinishedCreatingMeshData(logger, Position, context.AvailableSides.ToCompactString());

        return context.CreateMeshData(sectionMeshes, meshedSides);
    }

    /// <summary>
    ///     Set the mesh data for this chunk.
    /// </summary>
    /// <param name="meshData">The mesh data to apply.</param>
    public void SetMeshData(ChunkMeshData meshData)
    {
        Throw.IfDisposed(disposed);

        hasMeshData = true;
        meshedSides = meshData.Sides;

        foreach (Int32 index in meshData.Indices)
            GetSection(index).SetMeshData(meshData.SectionMeshData[index]!);
    }

    /// <summary>
    ///     Enable and disable section renderers based on the frustum.
    /// </summary>
    /// <param name="frustum">The view frustum to use for culling.</param>
    public void CullSections(Frustum frustum)
    {
        Throw.IfDisposed(disposed);

        Box3d chunkBox = VMath.CreateBox3(Position.Center, Extents);

        const Double tolerance = 16.0;

        if (!hasMeshData || !frustum.IsBoxVisible(chunkBox, tolerance))
        {
            DisableAllVfx();

            return;
        }

        for (var x = 0; x < Size; x++)
        for (var y = 0; y < Size; y++)
        for (var z = 0; z < Size; z++)
        {
            SectionPosition sectionPosition = SectionPosition.From(Position, (x, y, z));
            Vector3d position = sectionPosition.FirstBlock;

            Box3d sectionBox = VMath.CreateBox3(position + Core.Logic.Section.Extents, Core.Logic.Section.Extents);
            Boolean visible = frustum.IsBoxVisible(sectionBox, tolerance);

            GetSection(LocalSectionToIndex(x, y, z)).SetVfxEnabledState(visible);
        }
    }

    private void DisableAllVfx()
    {
        for (var index = 0; index < SectionCount; index++) GetSection(index).SetVfxEnabledState(enabled: false);
    }

    #region LOGGING

    private static readonly ILogger logger = LoggingHelper.CreateLogger<Chunk>();

    [LoggerMessage(EventId = Events.ChunkOperation, Level = LogLevel.Debug, Message = "Started creating mesh data for chunk {Position} using [{AvailableSides}] neighbors", SkipEnabledCheck = true)]
    private static partial void LogStartedCreatingMeshData(ILogger logger, ChunkPosition position, String availableSides);

    [LoggerMessage(EventId = Events.ChunkOperation, Level = LogLevel.Debug, Message = "Finished creating mesh data for chunk {Position} using [{AvailableSides}] neighbors", SkipEnabledCheck = true)]
    private static partial void LogFinishedCreatingMeshData(ILogger logger, ChunkPosition position, String availableSides);

    #endregion LOGGING

    #region IDisposable Support

    private Boolean disposed;

    /// <inheritdoc />
    protected override void Dispose(Boolean disposing)
    {
        if (disposed) return;

        base.Dispose(disposing);

        disposed = true;
    }

    #endregion
}
