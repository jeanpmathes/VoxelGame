// <copyright file="MeshingContext.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System.Diagnostics;
using System.Runtime.CompilerServices;
using OpenTK.Mathematics;
using VoxelGame.Core.Collections;
using VoxelGame.Core.Logic;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Core.Visuals;

#pragma warning disable S4049

/// <summary>
///     The context for section meshing.
/// </summary>
public class MeshingContext
{
    // todo: evaluate initial capacity

    private readonly PooledList<SpatialVertex> basicOpaqueMesh = new(capacity: 2048);
    private readonly PooledList<SpatialVertex> basicTransparentMesh = new(capacity: 2048);

    private readonly PooledList<int> cropPlantVertexData = new(capacity: 16);
    private readonly PooledList<int> crossPlantVertexData = new(capacity: 16);

    private readonly Section current;
    private readonly Section?[] neighbors;

    private readonly VaryingHeightMeshFaceHolder[] opaqueFluidMeshFaceHolders;

    private readonly FullMeshFaceHolder[] opaqueFullBlockMeshFaceHolders;

    private readonly VaryingHeightMeshFaceHolder[] opaqueVaryingHeightBlockMeshFaceHolders;
    private readonly (TintColor block, TintColor fluid)[,] tintColors;
    private readonly VaryingHeightMeshFaceHolder[] transparentFluidMeshFaceHolders;
    private readonly FullMeshFaceHolder[] transparentFullBlockMeshFaceHolders;
    private readonly VaryingHeightMeshFaceHolder[] transparentVaryingHeightBlockMeshFaceHolders;

    /// <summary>
    ///     Create a new block meshing context.
    /// </summary>
    /// <param name="position">The position of the section.</param>
    /// <param name="context">The chunk meshing context of the chunk the section is in.</param>
    public MeshingContext(SectionPosition position, ChunkMeshingContext context)
    {
        Section? section = context.GetSection(position);
        Debug.Assert(section != null);
        current = section;

        neighbors = GetNeighborSections(position, context);
        tintColors = GetTintColors(position, context);

        opaqueFullBlockMeshFaceHolders = CreateFullMeshFaceHolders();
        transparentFullBlockMeshFaceHolders = CreateFullMeshFaceHolders();

        opaqueVaryingHeightBlockMeshFaceHolders = CreateVaryingHeightMeshFaceHolders();
        transparentVaryingHeightBlockMeshFaceHolders = CreateVaryingHeightMeshFaceHolders();

        opaqueFluidMeshFaceHolders = CreateVaryingHeightMeshFaceHolders();
        transparentFluidMeshFaceHolders = CreateVaryingHeightMeshFaceHolders();
    }

    /// <summary>
    ///     Get current block tint, used when the tint is set to neutral.
    /// </summary>
    /// <param name="position">The position, in section-local coordinates.</param>
    public TintColor GetBlockTint(Vector3i position)
    {
        return tintColors[position.X, position.Z].block;
    }

    /// <summary>
    ///     Get current fluid tint, used when the tint is set to neutral.
    /// </summary>
    /// <param name="position">The position, in section-local coordinates.</param>
    public TintColor GetFluidTint(Vector3i position)
    {
        return tintColors[position.X, position.Z].fluid;
    }

    private static Section?[] GetNeighborSections(SectionPosition position, ChunkMeshingContext context)
    {
        var neighborSections = new Section?[6];

        foreach (BlockSide side in BlockSide.All.Sides())
            neighborSections[(int) side] =
                context.GetSection(side.Offset(position));

        return neighborSections;
    }

    private static (TintColor block, TintColor fluid)[,] GetTintColors(SectionPosition position, ChunkMeshingContext context)
    {
        var colors = new (TintColor block, TintColor fluid)[Section.Size, Section.Size];

        for (var x = 0; x < Section.Size; x++)
        for (var z = 0; z < Section.Size; z++)
            colors[x, z] = context.Map.GetPositionTint(position.FirstBlock + new Vector3i(x, y: 0, z));

        return colors;
    }

    private static FullMeshFaceHolder[] CreateFullMeshFaceHolders()
    {
        var holders = new FullMeshFaceHolder[6];

        foreach (BlockSide side in BlockSide.All.Sides()) holders[(int) side] = new FullMeshFaceHolder(side);

        return holders;
    }

    private static VaryingHeightMeshFaceHolder[] CreateVaryingHeightMeshFaceHolders()
    {
        var holders = new VaryingHeightMeshFaceHolder[6];

        foreach (BlockSide side in BlockSide.All.Sides()) holders[(int) side] = new VaryingHeightMeshFaceHolder(side);

        return holders;
    }

    /// <summary>
    /// Get the list containing the basic mesh data.
    /// </summary>
    /// <param name="isOpaque">Whether the mesh is opaque or not.</param>
    /// <returns>The list containing the basic mesh data.</returns>
    public PooledList<SpatialVertex> GetBasicMesh(bool isOpaque)
    {
        return isOpaque ? basicOpaqueMesh : basicTransparentMesh;
    }

    /// <summary>
    ///     Get the block mesh face holder for (full) blocks.
    ///     This considers the side and whether it is opaque or not.
    /// </summary>
    public FullMeshFaceHolder GetFullBlockMeshFaceHolder(BlockSide side, bool isOpaque)
    {
        return isOpaque ? opaqueFullBlockMeshFaceHolders[(int) side] : transparentFullBlockMeshFaceHolders[(int) side];
    }

    /// <summary>
    ///     Get the block mesh face holder for varying height faces, given a block side.
    ///     This considers the side and whether it is opaque or not.
    /// </summary>
    public VaryingHeightMeshFaceHolder GetVaryingHeightBlockMeshFaceHolder(BlockSide side, bool isOpaque)
    {
        return isOpaque ? opaqueVaryingHeightBlockMeshFaceHolders[(int) side] : transparentVaryingHeightBlockMeshFaceHolders[(int) side];
    }

    /// <summary>
    ///     Get the fluid mesh face holders for varying height faces.
    /// </summary>
    public VaryingHeightMeshFaceHolder[] GetFluidMeshFaceHolders(bool isOpaque)
    {
        return isOpaque ? opaqueFluidMeshFaceHolders : transparentFluidMeshFaceHolders;
    }

    /// <summary>
    ///     Get the crop plant vertex data list.
    /// </summary>
    public PooledList<int> GetCropPlantVertexData()
    {
        return cropPlantVertexData;
    }

    /// <summary>
    ///     Get the cross plant vertex data list.
    /// </summary>
    public PooledList<int> GetCrossPlantVertexData()
    {
        return crossPlantVertexData;
    }

    /// <summary>
    ///     Get a block and fluid from the current section or one of its neighbors.
    /// </summary>
    /// <param name="position">The position, in section-local coordinates.</param>
    /// <param name="side">The block side giving the neighbor to use if necessary.</param>
    /// <returns>The block and fluid or null if there is nothing.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (BlockInstance block, FluidInstance fluid)? GetBlockAndFluid(Vector3i position, BlockSide side)
    {
        (BlockInstance block, FluidInstance fluid)? result;

        if (Section.IsInBounds(position.ToTuple()))
        {
            BlockInstance block = current.GetBlock(position);
            FluidInstance fluid = current.GetFluid(position);

            result = (block, fluid);
        }
        else
        {
            position = Section.ToLocalPosition(position);

            Section? neighbor = neighbors[(int) side];
            BlockInstance? block = neighbor?.GetBlock(position);
            FluidInstance? fluid = neighbor?.GetFluid(position);

            result = block != null && fluid != null ? (block.Value, fluid.Value) : null;
        }

        return result;
    }

    /// <summary>
    ///     Get a block from the current section or one of its neighbors.
    /// </summary>
    /// <param name="position">The position, in section-local coordinates.</param>
    /// <param name="side">The block side giving the neighbor to use if necessary.</param>
    /// <returns>The block or null if there is no block.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public BlockInstance? GetBlock(Vector3i position, BlockSide side)
    {
        BlockInstance? block;

        if (Section.IsInBounds(position.ToTuple()))
        {
            block = current.GetBlock(position);
        }
        else
        {
            position = Section.ToLocalPosition(position);

            Section? neighbor = neighbors[(int) side];
            block = neighbor?.GetBlock(position);
        }

        return block;
    }

    /// <summary>
    ///     Generate the section mesh data.
    /// </summary>
    public SectionMeshData GenerateMeshData()
    {
        // We build the mesh data for everything except complex meshes, as they are already in the correct format.

        GenerateMesh(opaqueFullBlockMeshFaceHolders, basicOpaqueMesh);
        GenerateMesh(transparentFullBlockMeshFaceHolders, basicTransparentMesh);

        GenerateMesh(opaqueVaryingHeightBlockMeshFaceHolders, basicOpaqueMesh);
        GenerateMesh(transparentVaryingHeightBlockMeshFaceHolders, basicTransparentMesh);

        PooledList<int> opaqueFluidVertexData = new(capacity: 8);
        PooledList<uint> opaqueFluidIndices = new(capacity: 8);
        uint opaqueFluidVertexCount = 0;

        // todo: uncomment when opaque fluids are implemented
        /*GenerateMesh(
            opaqueFluidMeshFaceHolders,
            ref opaqueFluidVertexCount,
            opaqueFluidVertexData,
            opaqueFluidIndices);*/

        PooledList<int> transparentFluidVertexData = new(capacity: 8);
        PooledList<uint> transparentFluidIndices = new(capacity: 8);
        uint transparentFluidVertexCount = 0;

        // todo: uncomment when transparent fluids are implemented
        /*GenerateMesh(
            transparentFluidMeshFaceHolders,
            ref transparentFluidVertexCount,
            transparentFluidVertexData,
            transparentFluidIndices);*/

        return new SectionMeshData(
            (basicOpaqueMesh, basicTransparentMesh),
            crossPlantVertexData,
            cropPlantVertexData,
            opaqueFluidVertexData,
            opaqueFluidIndices,
            transparentFluidVertexData,
            transparentFluidIndices);
    }

    /// <summary>
    ///     Return all pooled resources.
    /// </summary>
    public void ReturnToPool()
    {
        ReturnToPool(opaqueFullBlockMeshFaceHolders);
        ReturnToPool(transparentFullBlockMeshFaceHolders);

        ReturnToPool(opaqueVaryingHeightBlockMeshFaceHolders);
        ReturnToPool(transparentVaryingHeightBlockMeshFaceHolders);

        ReturnToPool(opaqueFluidMeshFaceHolders);
        ReturnToPool(transparentFluidMeshFaceHolders);
    }

    private static void GenerateMesh(FullMeshFaceHolder[] holders, PooledList<SpatialVertex> mesh)
    {
        foreach (FullMeshFaceHolder holder in holders) holder.GenerateMesh(mesh);
    }

    private static void GenerateMesh(VaryingHeightMeshFaceHolder[] holders, PooledList<SpatialVertex> mesh)
    {
        foreach (VaryingHeightMeshFaceHolder holder in holders)
            holder.GenerateMesh(mesh);
    }

    private static void ReturnToPool(FullMeshFaceHolder[] holders)
    {
        foreach (FullMeshFaceHolder holder in holders) holder.ReturnToPool();
    }

    private static void ReturnToPool(VaryingHeightMeshFaceHolder[] holders)
    {
        foreach (VaryingHeightMeshFaceHolder holder in holders) holder.ReturnToPool();
    }
}
