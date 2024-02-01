// <copyright file="Section.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

// ReSharper disable CommentTypo

using System;
using System.Diagnostics;
using VoxelGame.Client.Visuals;
using VoxelGame.Core.Logic;
using VoxelGame.Core.Utilities;
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
    [NonSerialized] private SectionVFX? renderer;

    /// <summary>
    ///     Create a new client section. The section is empty, requiring generation.
    /// </summary>
    public Section(SectionPosition position) : base(position)
    {
        renderer = new SectionVFX(Application.Client.Instance.Space, position.FirstBlock);
        renderer.SetUp();
    }

    /// <inheritdoc />
    public override void Setup(Core.Logic.Section loaded)
    {
        Throw.IfDisposed(disposed);

        blocks = loaded.Cast().blocks;

        // Loaded section is not disposed because this section takes ownership of the resources.
    }

    /// <summary>
    ///     Create a mesh for this section and activate it.
    /// </summary>
    /// <param name="context">The context to use for mesh creation.</param>
    public void CreateAndSetMesh(ChunkMeshingContext context)
    {
        Throw.IfDisposed(disposed);

        BlockSides required = GetRequiredSides(position);
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

        BlockSides required = GetRequiredSides(position);

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
    /// <param name="chunkContext">The chunk context to use.</param>
    /// <returns>The created mesh data.</returns>
    public SectionMeshData CreateMeshData(ChunkMeshingContext chunkContext)
    {
        Throw.IfDisposed(disposed);

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
    ///     Set whether the renderer is enabled.
    /// </summary>
    public void SetRendererEnabledState(bool enabled)
    {
        Throw.IfDisposed(disposed);

        Debug.Assert(renderer != null);

        renderer.IsEnabled = enabled;
    }

    private void SetMeshDataInternal(SectionMeshData meshData)
    {
        Throw.IfDisposed(disposed);

        Debug.Assert(renderer != null);
        Debug.Assert(hasMesh == meshData.IsFilled);

        renderer.SetData(meshData);
    }

    #region IDisposable Support

    private bool disposed;

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        if (disposed) return;

        if (disposing)
        {
            renderer?.TearDown();
            renderer?.Dispose();
        }

        base.Dispose(disposing);

        disposed = true;
    }

    #endregion IDisposable Support
}
