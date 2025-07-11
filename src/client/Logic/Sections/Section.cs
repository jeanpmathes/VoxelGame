﻿// <copyright file="Section.cs" company="VoxelGame">
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
using VoxelGame.Core.Visuals;
using VoxelGame.Logging;
using VoxelGame.Toolkit.Memory;
using VoxelGame.Toolkit.Utilities;

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
    private Sides missing;

    private SectionVFX? vfx;
    private Boolean vfxEnabled;

    /// <inheritdoc />
    public Section(NativeSegment<UInt32> blocks) : base(blocks) {}

    /// <inheritdoc />
    public override void Reset()
    {
        base.Reset();

        hasMesh = false;
        missing = Sides.All;

        vfxEnabled = false;

        if (vfx == null)
            return;

#pragma warning disable S2952 // Object is diposed in Dispose() too, but is set to null here and thus must be disposed here.
        vfx.TearDown();
        vfx.Dispose();
#pragma warning restore S2952

        vfx = null;
    }

    /// <summary>
    ///     Create a mesh for this section and activate it.
    /// </summary>
    /// <param name="world">The world the section is in.</param>
    /// <param name="context">The context to use for mesh creation.</param>
    public void CreateAndSetMesh(World world, ChunkMeshingContext context)
    {
        Throw.IfDisposed(disposed);

        Sides required = GetRequiredSides(Position);
        missing = required & ~context.AvailableSides & Sides.All;

        using SectionMeshData meshData = CreateMeshData(context);
        SetMeshDataInternal(world, meshData);
    }

    /// <summary>
    ///     Recreate and set the mesh if it is incomplete, which means that it was meshed without all required neighbors.
    /// </summary>
    /// <param name="world">The world the section is in.</param>
    /// <param name="context">The context to use for mesh creation.</param>
    public void RecreateIncompleteMesh(World world, ChunkMeshingContext context)
    {
        Throw.IfDisposed(disposed);

        if (missing == Sides.None) return;

        Sides required = GetRequiredSides(Position);

        if (context.AvailableSides.HasFlag(required)) CreateAndSetMesh(world, context);
    }

    /// <summary>
    ///     Set that the mesh of the section is incomplete.
    ///     This should only be called on the main thread.
    ///     No resource access is needed, as all written variables are only accessed from the main thread.
    /// </summary>
    /// <param name="sides">The sides that are missing for the section.</param>
    public void SetAsIncomplete(Sides sides)
    {
        Throw.IfDisposed(disposed);

        missing |= sides;
    }

    private static Sides GetRequiredSides(SectionPosition position)
    {
        var required = Sides.None;
        (Int32 x, Int32 y, Int32 z) = position.Local;

        if (x == 0) required |= Sides.Left;
        if (x == Chunk.Size - 1) required |= Sides.Right;

        if (y == 0) required |= Sides.Bottom;
        if (y == Chunk.Size - 1) required |= Sides.Top;

        if (z == 0) required |= Sides.Back;
        if (z == Chunk.Size - 1) required |= Sides.Front;

        return required;
    }

    /// <summary>
    ///     Create mesh data for this section.
    /// </summary>
    /// <param name="chunkContext">The chunk context to use.</param>
    /// <returns>The created mesh data.</returns>
    public SectionMeshData CreateMeshData(IChunkMeshingContext chunkContext)
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
                UInt32 content = GetContent(x, y, z);

                Decode(
                    content,
                    out Block currentBlock,
                    out UInt32 data,
                    out Fluid currentFluid,
                    out FluidLevel level,
                    out Boolean isStatic);

                IBlockMeshable meshable = currentBlock;
                meshable.CreateMesh((x, y, z), new BlockMeshInfo(Side.All, data, currentFluid), context);

                currentFluid.CreateMesh(
                    (x, y, z),
                    FluidMeshInfo.Fluid(currentBlock.AsInstance(data), level, Side.All, isStatic),
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
    /// <param name="world">The world the section is in.</param>
    /// <param name="meshData">The mesh data to use and activate.</param>
    public void SetMeshData(World world, SectionMeshData meshData)
    {
        Throw.IfDisposed(disposed);

        // While the mesh is not necessarily complete,
        // missing neighbours are the reponsibility of the level that created the passed mesh, e.g. the chunk.
        missing = Sides.None;

        SetMeshDataInternal(world, meshData);
    }

    /// <summary>
    ///     Set whether the vfx is enabled.
    /// </summary>
    public void SetVfxEnabledState(Boolean enabled)
    {
        Throw.IfDisposed(disposed);

        vfxEnabled = enabled;

        if (vfx != null) vfx.IsEnabled = enabled;
    }

    private void SetMeshDataInternal(World world, SectionMeshData meshData)
    {
        Throw.IfDisposed(disposed);

        Debug.Assert(hasMesh == meshData.IsFilled);

        if (vfx == null)
        {
            vfx = new SectionVFX(world.Space, Position.FirstBlock);
            vfx.SetUp();

            vfx.IsEnabled = vfxEnabled;
        }

        vfx.SetData(meshData);
    }

    #region DISPOSABLE

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

    #endregion DISPOSABLE
}
