﻿// <copyright file="Section.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

// ReSharper disable CommentTypo

using System;
using System.Diagnostics;
using VoxelGame.Client.Rendering;
using VoxelGame.Core.Logic;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Client.Logic;

/// <summary>
///     A section of the world, specifically for the client.
///     Sections do not know their exact position in the world.
/// </summary>
[Serializable]
public class Section : Core.Logic.Section
{
    [NonSerialized] private bool hasMesh;
    [NonSerialized] private BlockSides missing;
    [NonSerialized] private SectionRenderer? renderer;

    /// <summary>
    ///     Create a new client section.
    /// </summary>
    public Section(SectionPosition position) : base(position)
    {
        renderer = null;
        hasMesh = false;
        disposed = false;
    }

    /// <inheritdoc />
    public override void Setup(Core.Logic.Section loaded)
    {
        blocks = loaded.Cast().blocks;
        renderer = new SectionRenderer(Application.Client.Instance.Space, position.FirstBlock);

        // Loaded section is not disposed because this section takes ownership of the resources.
    }

    /// <summary>
    ///     Create a mesh for this section and activate it.
    /// </summary>
    /// <param name="position">The position of the section.</param>
    /// <param name="context">The context to use for mesh creation.</param>
    public void CreateAndSetMesh(SectionPosition position, ChunkMeshingContext context)
    {
        BlockSides required = GetRequiredSides(position);
        missing = required & ~context.AvailableSides & BlockSides.All;

        SectionMeshData meshData = CreateMeshData(position, context);
        SetMeshDataInternal(meshData);
    }

    /// <summary>
    ///     Recreate and set the mesh if it is incomplete, which means that it was meshed without all required neighbors.
    /// </summary>
    /// <param name="position">The position of the section.</param>
    /// <param name="context">The context to use for mesh creation.</param>
    public void RecreateIncompleteMesh(SectionPosition position, ChunkMeshingContext context)
    {
        if (missing == BlockSides.None) return;

        BlockSides required = GetRequiredSides(position);

        if (context.AvailableSides.HasFlag(required)) CreateAndSetMesh(position, context);
    }

    private static BlockSides GetRequiredSides(SectionPosition position)
    {
        var required = BlockSides.None;
        (int x, int y, int z) = position.Local;

        if (x == 0) required |= BlockSides.Left;
        if (x == Core.Logic.Chunk.Size - 1) required |= BlockSides.Right;

        if (y == 0) required |= BlockSides.Bottom;
        if (y == Core.Logic.Chunk.Size - 1) required |= BlockSides.Top;

        if (z == 0) required |= BlockSides.Back;
        if (z == Core.Logic.Chunk.Size - 1) required |= BlockSides.Front;

        return required;
    }

    /// <summary>
    ///     Create mesh data for this section.
    /// </summary>
    /// <param name="position">The position of the section.</param>
    /// <param name="chunkContext">The chunk context to use.</param>
    /// <returns>The created mesh data.</returns>
    public SectionMeshData CreateMeshData(SectionPosition position, ChunkMeshingContext chunkContext)
    {
        MeshingContext context = new(position, chunkContext);

        for (var x = 0; x < Size; x++)
        for (var y = 0; y < Size; y++)
        for (var z = 0; z < Size; z++)
        {
            uint val = blocks[(x << SizeExp2) + (y << SizeExp) + z];

            Decode(
                val,
                out Block currentBlock,
                out uint data,
                out Fluid currentFluid,
                out FluidLevel level,
                out bool isStatic);

            IBlockMeshable meshable = currentBlock;
            meshable.CreateMesh((x, y, z), new BlockMeshInfo(BlockSide.All, data, currentFluid), context);

            currentFluid.CreateMesh(
                (x, y, z),
                FluidMeshInfo.Fluid(currentBlock.AsInstance(data), level, BlockSide.All, isStatic),
                context);
        }

        SectionMeshData meshData = context.GenerateMeshData();
        hasMesh = meshData.IsFilled;

        context.ReturnToPool();

        return meshData;
    }

    /// <summary>
    ///     Set the mesh data for this section. The mesh must be generated from this section.
    /// </summary>
    /// <param name="meshData">The mesh data to use and activate.</param>
    public void SetMeshData(SectionMeshData meshData)
    {
        // While the mesh is not necessarily complete,
        // missing neighbours are the reponsibility of the level that created the passed mesh, e.g. the chunk.
        missing = BlockSides.None;

        SetMeshDataInternal(meshData);
    }

    private void SetMeshDataInternal(SectionMeshData meshData)
    {
        Debug.Assert(renderer != null);
        Debug.Assert(hasMesh == meshData.IsFilled);

        renderer.SetData(meshData);
    }

    #region IDisposable Support

    [NonSerialized] private bool disposed;

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        if (!disposed)
        {
            if (disposing) renderer?.Dispose();

            disposed = true;
        }
    }

    #endregion IDisposable Support
}
