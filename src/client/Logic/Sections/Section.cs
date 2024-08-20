// <copyright file="Section.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

// ReSharper disable CommentTypo

using System;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using VoxelGame.Client.Visuals;
using VoxelGame.Core.Logic.Chunks;
using VoxelGame.Core.Logic.Elements;
using VoxelGame.Core.Logic.Sections;
using VoxelGame.Core.Profiling;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Visuals;
using VoxelGame.Logging;

namespace VoxelGame.Client.Logic.Sections;

/// <summary>
///     A section of the world, specifically for the client.
///     Sections do not know their exact position in the world.
/// </summary>
public class Section : Core.Logic.Sections.Section
{
    #region LOGGING

    private static readonly ILogger logger = LoggingHelper.CreateLogger<Section>();

    #endregion LOGGING

    private Boolean hasMesh;
    private BlockSides missing;
    private SectionVFX? vfx;

    /// <inheritdoc />
    public Section(ArraySegment<UInt32> blocks) : base(blocks) {}

    /// <inheritdoc />
    public override void Initialize(SectionPosition newPosition)
    {
        base.Initialize(newPosition);

        vfx = new SectionVFX(Application.Client.Instance.Space, Position.FirstBlock);
        vfx.SetUp();
    }

    /// <inheritdoc />
    public override void Reset()
    {
        base.Reset();

        hasMesh = false;
        missing = BlockSides.All;

        Debug.Assert(vfx != null);

#pragma warning disable S2952 // Object is diposed in Dispose() too, but is overridden here and thus must be disposed here.
        vfx.TearDown();
        vfx.Dispose();
#pragma warning restore S2952

        vfx = null;
    }

    /// <summary>
    ///     Create a mesh for this section and activate it.
    /// </summary>
    /// <param name="context">The context to use for mesh creation.</param>
    public void CreateAndSetMesh(ChunkMeshingContext context)
    {
        Throw.IfDisposed(disposed);

        BlockSides required = GetRequiredSides(Position);
        missing = required & ~context.AvailableSides & BlockSides.All;

        using SectionMeshData meshData = CreateMeshData(context);
        SetMeshDataInternal(meshData);
    }

    /// <summary>
    ///     Recreate and set the mesh if it is incomplete, which means that it was meshed without all required neighbors.
    /// </summary>
    /// <param name="context">The context to use for mesh creation.</param>
    public void RecreateIncompleteMesh(ChunkMeshingContext context)
    {
        Throw.IfDisposed(disposed);

        if (missing == BlockSides.None) return;

        BlockSides required = GetRequiredSides(Position);

        if (context.AvailableSides.HasFlag(required)) CreateAndSetMesh(context);
    }

    /// <summary>
    ///     Set that the mesh of the section is incomplete.
    ///     This should only be called on the main thread.
    ///     No resource access is needed, as all written variables are only accessed from the main thread.
    /// </summary>
    /// <param name="sides">The sides that are missing for the section.</param>
    public void SetAsIncomplete(BlockSides sides)
    {
        Throw.IfDisposed(disposed);

        missing |= sides;
    }

    private static BlockSides GetRequiredSides(SectionPosition position)
    {
        var required = BlockSides.None;
        (Int32 x, Int32 y, Int32 z) = position.Local;

        if (x == 0) required |= BlockSides.Left;
        if (x == Chunk.Size - 1) required |= BlockSides.Right;

        if (y == 0) required |= BlockSides.Bottom;
        if (y == Chunk.Size - 1) required |= BlockSides.Top;

        if (z == 0) required |= BlockSides.Back;
        if (z == Chunk.Size - 1) required |= BlockSides.Front;

        return required;
    }

    /// <summary>
    ///     Create mesh data for this section.
    /// </summary>
    /// <param name="chunkContext">The chunk context to use.</param>
    /// <returns>The created mesh data.</returns>
    public SectionMeshData CreateMeshData(ChunkMeshingContext chunkContext)
    {
        Throw.IfDisposed(disposed);

        using Timer? timer = logger.BeginTimedScoped("Section Meshing");

        MeshingContext context = new(Position, chunkContext);

        using (logger.BeginTimedSubScoped("Section Meshing Loop", timer))
        {
            for (var x = 0; x < Size; x++)
            for (var y = 0; y < Size; y++)
            for (var z = 0; z < Size; z++)
            {
                UInt32 value = blocks[(x << SizeExp2) + (y << SizeExp) + z];

                Decode(
                    value,
                    out Block currentBlock,
                    out UInt32 data,
                    out Fluid currentFluid,
                    out FluidLevel level,
                    out Boolean isStatic);

                IBlockMeshable meshable = currentBlock;
                meshable.CreateMesh((x, y, z), new BlockMeshInfo(BlockSide.All, data, currentFluid), context);

                currentFluid.CreateMesh(
                    (x, y, z),
                    FluidMeshInfo.Fluid(currentBlock.AsInstance(data), level, BlockSide.All, isStatic),
                    context);
            }
        }

        SectionMeshData meshData;

        using (logger.BeginTimedSubScoped("Section Meshing Generate", timer))
        {
            meshData = context.GenerateMeshData();
        }

        hasMesh = meshData.IsFilled;

        context.ReturnToPool();

        return meshData;
    }

    /// <summary>
    ///     Set the mesh data for this section. The mesh must be generated from this section.
    ///     Must be called from the main thread.
    /// </summary>
    /// <param name="meshData">The mesh data to use and activate.</param>
    public void SetMeshData(SectionMeshData meshData)
    {
        Throw.IfDisposed(disposed);

        // While the mesh is not necessarily complete,
        // missing neighbours are the reponsibility of the level that created the passed mesh, e.g. the chunk.
        missing = BlockSides.None;

        SetMeshDataInternal(meshData);
    }

    /// <summary>
    ///     Set whether the vfx is enabled.
    /// </summary>
    public void SetVfxEnabledState(Boolean enabled)
    {
        Throw.IfDisposed(disposed);

        Debug.Assert(vfx != null);

        vfx.IsEnabled = enabled;
    }

    private void SetMeshDataInternal(SectionMeshData meshData)
    {
        Throw.IfDisposed(disposed);

        Debug.Assert(vfx != null);
        Debug.Assert(hasMesh == meshData.IsFilled);

        vfx.SetData(meshData);
    }

    #region IDisposable Support

    private Boolean disposed;

    /// <inheritdoc />
    protected override void Dispose(Boolean disposing)
    {
        if (disposed) return;

        if (disposing)
        {
            vfx?.TearDown();
            vfx?.Dispose();
        }

        base.Dispose(disposing);

        disposed = true;
    }

    #endregion IDisposable Support
}
