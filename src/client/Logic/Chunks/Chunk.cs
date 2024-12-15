// <copyright file="Chunk.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using Microsoft.Extensions.Logging;
using OpenTK.Mathematics;
using VoxelGame.Client.Visuals;
using VoxelGame.Core.Logic.Chunks;
using VoxelGame.Core.Logic.Elements;
using VoxelGame.Core.Logic.Sections;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Visuals;
using VoxelGame.Graphics.Data;
using VoxelGame.Logging;
using VoxelGame.Toolkit.Memory;
using VoxelGame.Toolkit.Utilities;
using Section = VoxelGame.Client.Logic.Sections.Section;

namespace VoxelGame.Client.Logic.Chunks;

/// <summary>
///     A chunk of the world, specifically for the client.
/// </summary>
public partial class Chunk : Core.Logic.Chunks.Chunk
{
    /// <summary>
    ///     Create a new client chunk.
    /// </summary>
    /// <param name="blocks">The block memory of the chunk.</param>
    /// <param name="context">The context of the chunk.</param>
    public Chunk(NativeSegment<UInt32> blocks, ChunkContext context) : base(context, blocks, CreateSection) {}

    /// <summary>
    ///     Get the client world this chunk is in.
    /// </summary>
    public new World World => base.World.Cast();

    /// <summary>
    ///     Whether this chunk currently has mesh data.
    /// </summary>
    public Boolean HasMeshData { get; private set; }

    /// <summary>
    ///     Get the sides that are currently meshed.
    /// </summary>
    public Sides MeshedSides { get; private set; }

    /// <inheritdoc />
    public override void Initialize(Core.Logic.World world, ChunkPosition position)
    {
        base.Initialize(world, position);

        HasMeshData = false;
        MeshedSides = Sides.None;
    }

    /// <inheritdoc />
    public override void Reset()
    {
        base.Reset();

        HasMeshData = false;
        MeshedSides = Sides.None;
    }

    private Section GetSection(Int32 index)
    {
        return GetSectionByIndex(index).Cast();
    }

    /// <summary>
    /// Ask a chunk to consider re-meshing because of a (newly) active neighbor.
    /// </summary>
    public void ReMesh()
    {
        Throw.IfDisposed(disposed);

        if (!this.IsAbleToMesh()) return;
        if (!this.IsReMeshingValuable()) return;

        // The hidden state will then try to activate, which then meshes if necessary.
        State.RequestNextState<Hidden>();
    }

    private static Core.Logic.Sections.Section CreateSection(NativeSegment<UInt32> blocks)
    {
        return new Section(blocks);
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
    ///     Process a chance to mesh the entire chunk on activation.
    /// </summary>
    /// <param name="allowActivation">Whether the chunk can be activated in the case that this method returns <c>null</c>.</param>
    /// <returns>A target state if the chunk should mesh, null otherwise.</returns>
    public Core.Logic.Chunks.ChunkState? ProcessMeshingOption(out Boolean allowActivation)
    {
        Throw.IfDisposed(disposed);

        allowActivation = false;

        if (!this.IsAbleToMesh()) return null;

        ChunkMeshingContext? context = ChunkMeshingContext.TryAcquire(this,
            SpatialMeshingFactory.Shared,
            out allowActivation);

        if (context == null) return null;

        foreach (Side side in Side.All.Sides())
            context.GetChunk(side)?.Cast().ReMesh();

        return new Meshing(context);
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
    protected override void OnNeighborActivation()
    {
        RecreateIncompleteSectionMeshes();
    }

    private void RecreateIncompleteSectionMeshes()
    {
        using ChunkMeshingContext context = ChunkMeshingContext.UsingActive(this, SpatialMeshingFactory.Shared);

        for (var index = 0; index < SectionCount; index++) GetSection(index).RecreateIncompleteMesh(context);
    }

    /// <summary>
    ///     Set a section as incomplete, which means that it was meshed without all required neighbors.
    ///     Alternatively, it can also indicate that a section was not meshed when affected by a data change.
    ///     This method does not require any resource access as it should only be called from the main thread.
    /// </summary>
    /// <param name="local">The local position of the section.</param>
    /// <param name="sides">The sides that are missing for the section.</param>
    public void SetSectionAsIncomplete((Int32 x, Int32 y, Int32 z) local, Sides sides)
    {
        Throw.IfDisposed(disposed);

        GetLocalSection(local.x, local.y, local.z).Cast().SetAsIncomplete(sides);
    }

    private ChunkMeshData CreateMeshData(ChunkMeshingContext context)
    {
        LogStartedCreatingMeshData(logger, Position, context);

        var sectionMeshes = new SectionMeshData?[SectionCount];

        foreach (Int32 index in context.SectionIndices)
            sectionMeshes[index] = GetSection(index).CreateMeshData(context);

        LogFinishedCreatingMeshData(logger, Position, context);

        return context.CreateMeshData(sectionMeshes, MeshedSides);
    }

    /// <summary>
    ///     Set the mesh data for this chunk.
    /// </summary>
    /// <param name="meshData">The mesh data to apply.</param>
    public void SetMeshData(ChunkMeshData meshData)
    {
        Throw.IfDisposed(disposed);

        if (logger.IsEnabled(LogLevel.Debug))
            LogSettingMeshData(logger, Position, MeshedSides.ToCompactString(), meshData.Sides.ToCompactString());

        HasMeshData = true;
        MeshedSides = meshData.Sides;

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

        if (!HasMeshData || !frustum.IsBoxVisible(chunkBox, tolerance))
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

            Box3d sectionBox = VMath.CreateBox3(position + Core.Logic.Sections.Section.Extents, Core.Logic.Sections.Section.Extents);
            Boolean visible = frustum.IsBoxVisible(sectionBox, tolerance);

            GetSection(LocalSectionToIndex(x, y, z)).SetVfxEnabledState(visible);
        }
    }

    /// <summary>
    ///     Hide all sections.
    ///     Equivalent to performing frustum culling with a frustum that does not see the chunk.
    /// </summary>
    public void HideAllSections()
    {
        for (var index = 0; index < SectionCount; index++)
            GetSection(index).SetVfxEnabledState(enabled: false);
    }

    private void DisableAllVfx()
    {
        for (var index = 0; index < SectionCount; index++)
            GetSection(index).SetVfxEnabledState(enabled: false);
    }

    #region LOGGING

    private static readonly ILogger logger = LoggingHelper.CreateLogger<Chunk>();

    [LoggerMessage(EventId = LogID.Chunk + 0, Level = LogLevel.Debug, Message = "Started creating mesh data for chunk {Position} using {Context}", SkipEnabledCheck = true)]
    private static partial void LogStartedCreatingMeshData(ILogger logger, ChunkPosition position, ChunkMeshingContext context);

    [LoggerMessage(EventId = LogID.Chunk + 1, Level = LogLevel.Debug, Message = "Finished creating mesh data for chunk {Position} using {Context}", SkipEnabledCheck = true)]
    private static partial void LogFinishedCreatingMeshData(ILogger logger, ChunkPosition position, ChunkMeshingContext context);

    [LoggerMessage(
        EventId = LogID.Chunk + 2,
        Level = LogLevel.Debug,
        Message = "Setting mesh data of chunk {Position}, changing meshed sides from [{OldSides}] to [{NewSides}]",
        SkipEnabledCheck = true)]
    private static partial void LogSettingMeshData(ILogger logger, ChunkPosition position, String oldSides, String newSides);

    #endregion LOGGING

    #region DISPOSABLE

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
