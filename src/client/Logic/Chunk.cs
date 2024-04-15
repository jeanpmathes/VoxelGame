// <copyright file="Chunk.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Diagnostics.CodeAnalysis;
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
    private const Int32 MaxMeshDataStep = 16;
    private static readonly ILogger logger = LoggingHelper.CreateLogger<Chunk>();

    private Boolean hasMeshData;
    private Int32 meshDataIndex;
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
        meshDataIndex = 0;
        meshedSides = BlockSides.None;
    }

    /// <inheritdoc />
    public override void Reset()
    {
        base.Reset();

        hasMeshData = false;
        meshDataIndex = 0;
        meshedSides = BlockSides.None;
    }

    [SuppressMessage("Performance", "CA1822:Mark members as static")]
    private Section GetSection(Int32 index)
    {
        return GetSectionByIndex(index).Cast();
    }

    /// <summary>
    ///     Begin meshing the chunk.
    /// </summary>
    public void BeginMeshing()
    {
        Throw.IfDisposed(disposed);

        if (!IsFullyDecorated) return;

        State.RequestNextState<Meshing>(new Core.Logic.ChunkState.RequestDescription
        {
            AllowDuplicateStateByType = false,
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

        BlockSides sides = ChunkMeshingContext.DetermineImprovementSides(this, meshedSides);

        if (sides == BlockSides.None) return null;

        foreach (BlockSide side in BlockSide.All.Sides())
        {
            BlockSides current = side.ToFlag();

            if (!sides.HasFlag(current) || meshedSides.HasFlag(current) || !World.TryGetChunk(side.Offset(Position), out Core.Logic.Chunk? chunk)) continue;

            chunk.Cast().BeginMeshing();
        }

        return new Meshing();
    }

    /// <inheritdoc />
    protected override void OnActivation()
    {
        RecreateIncompleteSectionMeshes();
    }

    /// <inheritdoc />
    protected override void OnDeactivation()
    {
        DisableAllSectionRenderers();
    }

    /// <inheritdoc />
    protected override void OnNeighborActivation(Core.Logic.Chunk neighbor)
    {
        RecreateIncompleteSectionMeshes();
    }

    private void RecreateIncompleteSectionMeshes()
    {
        ChunkMeshingContext context = ChunkMeshingContext.UsingActive(this, SpatialMeshingFactory.Shared);

        for (var s = 0; s < SectionCount; s++) GetSection(s).RecreateIncompleteMesh(context);
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
        logger.LogDebug(Events.ChunkOperation, "Started creating mesh data for chunk {Position} using [{AvailableSides}] neighbors", Position, context.AvailableSides.ToCompactString());

        var sectionMeshes = new SectionMeshData[SectionCount];

        for (var s = 0; s < SectionCount; s++) sectionMeshes[s] = GetSection(s).CreateMeshData(context);

        meshDataIndex = 0;

        logger.LogDebug(Events.ChunkOperation, "Finished creating mesh data for chunk {Position} using [{AvailableSides}] neighbors", Position, context.AvailableSides.ToCompactString());

        return new ChunkMeshData(sectionMeshes, context.AvailableSides);
    }

    /// <summary>
    ///     Do a mesh data set-step. This will apply a part of the mesh data and activate the part.
    /// </summary>
    /// <param name="meshData">The mesh data to apply.</param>
    /// <returns>True if this step was the final step.</returns>
    public Boolean DoMeshDataSetStep(ChunkMeshData meshData)
    {
        Throw.IfDisposed(disposed);

        hasMeshData = false;
        meshedSides = meshData.Sides;

        for (var count = 0; count < MaxMeshDataStep; count++)
        {
            GetSection(meshDataIndex).SetMeshData(meshData.SectionMeshData[meshDataIndex]);

            // The index has reached the end, all sections have received their mesh data.
            if (meshDataIndex == SectionCount - 1)
            {
                hasMeshData = true;
                meshDataIndex = 0;

                return true;
            }

            meshDataIndex++;
        }

        return false;
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
            DisableAllSectionRenderers();

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

            GetSection(LocalSectionToIndex(x, y, z)).SetRendererEnabledState(visible);
        }
    }

    private void DisableAllSectionRenderers()
    {
        for (var index = 0; index < SectionCount; index++) GetSection(index).SetRendererEnabledState(enabled: false);
    }

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
