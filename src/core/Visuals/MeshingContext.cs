// <copyright file="MeshingContext.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using OpenTK.Mathematics;
using VoxelGame.Core.Collections;
using VoxelGame.Core.Logic;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Core.Visuals;

/// <summary>
///     The context for section meshing.
/// </summary>
public class MeshingContext
{
    private readonly BlockMeshFaceHolder[] blockMeshFaceHolders;

    private readonly PooledList<uint> complexIndices = new(capacity: 16);
    private readonly PooledList<int> complexVertexData = new(capacity: 32);

    private readonly PooledList<float> complexVertexPositions = new(capacity: 64);

    private readonly PooledList<int> cropPlantVertexData = new(capacity: 16);
    private readonly PooledList<int> crossPlantVertexData = new(capacity: 16);

    private readonly Section current;
    private readonly Section?[] neighbors;

    private readonly VaryingHeightMeshFaceHolder[] opaqueFluidMeshFaceHolders;
    private readonly VaryingHeightMeshFaceHolder[] transparentFluidMeshFaceHolders;
    private readonly VaryingHeightMeshFaceHolder[] varyingHeightMeshFaceHolders;

    /// <summary>
    ///     Create a new block meshing context.
    /// </summary>
    /// <param name="section">The section that is meshed.</param>
    /// <param name="position">The position of the section.</param>
    /// <param name="world">The world the section is in.</param>
    public MeshingContext(Section section, SectionPosition position, World world)
    {
        current = section;
        neighbors = GetNeighborSections(world, position);

        blockMeshFaceHolders = CreateBlockMeshFaceHolders();
        varyingHeightMeshFaceHolders = CreateVaryingHeightMeshFaceHolders();
        opaqueFluidMeshFaceHolders = CreateVaryingHeightMeshFaceHolders();
        transparentFluidMeshFaceHolders = CreateVaryingHeightMeshFaceHolders();
    }

    /// <summary>
    ///     Get or set the complex vertex count.
    /// </summary>
    public uint ComplexVertexCount { get; set; }

    /// <summary>
    ///     The current block tint, used when the tint is set to neutral.
    /// </summary>
    public TintColor BlockTint { get; set; }

    /// <summary>
    ///     The current fluid tint, used when the tint is set to neutral.
    /// </summary>
    public TintColor FluidTint { get; set; }

    private static Section?[] GetNeighborSections(World world, SectionPosition position)
    {
        var neighborSections = new Section?[6];

        foreach (BlockSide side in BlockSide.All.Sides())
            neighborSections[(int) side] =
                world.GetSection(side.Offset(position));

        return neighborSections;
    }

    private static BlockMeshFaceHolder[] CreateBlockMeshFaceHolders()
    {
        var holders = new BlockMeshFaceHolder[6];

        holders[(int) BlockSide.Front] = new BlockMeshFaceHolder(BlockSide.Front);
        holders[(int) BlockSide.Back] = new BlockMeshFaceHolder(BlockSide.Back);
        holders[(int) BlockSide.Left] = new BlockMeshFaceHolder(BlockSide.Left);
        holders[(int) BlockSide.Right] = new BlockMeshFaceHolder(BlockSide.Right);
        holders[(int) BlockSide.Bottom] = new BlockMeshFaceHolder(BlockSide.Bottom);
        holders[(int) BlockSide.Top] = new BlockMeshFaceHolder(BlockSide.Top);

        return holders;
    }

    private static VaryingHeightMeshFaceHolder[] CreateVaryingHeightMeshFaceHolders()
    {
        var holders = new VaryingHeightMeshFaceHolder[6];

        holders[(int) BlockSide.Front] = new VaryingHeightMeshFaceHolder(BlockSide.Front);
        holders[(int) BlockSide.Back] = new VaryingHeightMeshFaceHolder(BlockSide.Back);
        holders[(int) BlockSide.Left] = new VaryingHeightMeshFaceHolder(BlockSide.Left);
        holders[(int) BlockSide.Right] = new VaryingHeightMeshFaceHolder(BlockSide.Right);
        holders[(int) BlockSide.Bottom] = new VaryingHeightMeshFaceHolder(BlockSide.Bottom);
        holders[(int) BlockSide.Top] = new VaryingHeightMeshFaceHolder(BlockSide.Top);

        return holders;
    }

    /// <summary>
    ///     Get the lists that can be filled with complex mesh data.
    /// </summary>
    public (PooledList<float> positions, PooledList<int> data, PooledList<uint> indices) GetComplexMeshLists()
    {
        return (complexVertexPositions, complexVertexData, complexIndices);
    }

    /// <summary>
    ///     Get the block mesh face holder for simple faces, given a block side.
    /// </summary>
    public BlockMeshFaceHolder GetSimpleBlockMeshFaceHolder(BlockSide side)
    {
        return blockMeshFaceHolders[(int) side];
    }

    /// <summary>
    ///     Get the block mesh face holder for varying height faces, given a block side.
    /// </summary>
    public VaryingHeightMeshFaceHolder GetVaryingHeightMeshFaceHolder(BlockSide side)
    {
        return varyingHeightMeshFaceHolders[(int) side];
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
    ///     Get a block from the current section or one of its neighbors.
    /// </summary>
    /// <param name="position">The position, in section-local coordinates.</param>
    /// <param name="side">The block side giving the neighbor to use if necessary.</param>
    /// <returns>The block or null if there is no block.</returns>
    public Block? GetBlock(Vector3i position, BlockSide side)
    {
        Block? block;

        if (IsPositionOutOfSection(position))
        {
            position = position.Mod(Section.Size);

            Section? neighbor = neighbors[(int) side];
            block = neighbor?.GetBlock(position);
        }
        else
        {
            block = current.GetBlock(position);
        }

        return block;
    }

    /// <summary>
    ///     Get a block and fluid from the current section or one of its neighbors.
    /// </summary>
    /// <param name="position">The position, in section-local coordinates.</param>
    /// <param name="side">The block side giving the neighbor to use if necessary.</param>
    /// <param name="level">The level of the fluid.</param>
    /// <returns>The block and fluid or null if there is nothing.</returns>
    public (Block block, Fluid fluid) GetBlockAndFluid(Vector3i position, BlockSide side, out int level)
    {
        Block? block;
        Fluid? fluid;

        level = -1;

        if (IsPositionOutOfSection(position))
        {
            position = position.Mod(Section.Size);

            Section? neighbor = neighbors[(int) side];
            block = neighbor?.GetBlock(position);
            fluid = neighbor?.GetFluid(position, out level);
        }
        else
        {
            block = current.GetBlock(position);
            fluid = current.GetFluid(position, out level);
        }

        return (block ?? Block.Air, fluid ?? Fluid.None);
    }

    /// <summary>
    ///     Get a block from the current section or one of its neighbors.
    /// </summary>
    /// <param name="position">The position, in section-local coordinates.</param>
    /// <param name="side">The block side giving the neighbor to use if necessary.</param>
    /// <param name="data">Will receive the data of the block.</param>
    /// <returns>The block or null if there is no block.</returns>
    public Block? GetBlock(Vector3i position, BlockSide side, out uint data)
    {
        Block? block;
        data = 0;

        if (IsPositionOutOfSection(position))
        {
            position = position.Mod(Section.Size);

            Section? neighbor = neighbors[(int) side];
            block = neighbor?.GetBlock(position, out data);
        }
        else
        {
            block = current.GetBlock(position, out data);
        }

        return block;
    }

    private static bool IsPositionOutOfSection(Vector3i position)
    {
        return position.X is < 0 or >= Section.Size ||
               position.Y is < 0 or >= Section.Size ||
               position.Z is < 0 or >= Section.Size;
    }

    /// <summary>
    ///     Generate the section mesh data.
    /// </summary>
    public SectionMeshData GenerateMeshData()
    {
        // We build the mesh data for everything except complex meshes, as they are already in the correct format.

        PooledList<int> simpleVertexData = new(capacity: 2048);
        GenerateMesh(blockMeshFaceHolders, simpleVertexData);

        PooledList<int> varyingHeightVertexData = new(capacity: 8);
        PooledList<uint> varyingHeightIndices = new(capacity: 8);

        uint varyingHeightVertexCount = 0;

        GenerateMesh(
            varyingHeightMeshFaceHolders,
            ref varyingHeightVertexCount,
            varyingHeightVertexData,
            varyingHeightIndices);

        PooledList<int> opaqueFluidVertexData = new(capacity: 8);
        PooledList<uint> opaqueFluidIndices = new(capacity: 8);
        uint opaqueFluidVertexCount = 0;

        GenerateMesh(
            opaqueFluidMeshFaceHolders,
            ref opaqueFluidVertexCount,
            opaqueFluidVertexData,
            opaqueFluidIndices);

        PooledList<int> transparentFluidVertexData = new(capacity: 8);
        PooledList<uint> transparentFluidIndices = new(capacity: 8);
        uint transparentFluidVertexCount = 0;

        GenerateMesh(
            transparentFluidMeshFaceHolders,
            ref transparentFluidVertexCount,
            transparentFluidVertexData,
            transparentFluidIndices);

        return new SectionMeshData(
            simpleVertexData,
            complexVertexPositions,
            complexVertexData,
            complexIndices,
            varyingHeightVertexData,
            varyingHeightIndices,
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
        ReturnToPool(blockMeshFaceHolders);
        ReturnToPool(varyingHeightMeshFaceHolders);
        ReturnToPool(opaqueFluidMeshFaceHolders);
        ReturnToPool(transparentFluidMeshFaceHolders);
    }

    private static void GenerateMesh(BlockMeshFaceHolder[] holders, PooledList<int> data)
    {
        foreach (BlockMeshFaceHolder holder in holders) holder.GenerateMesh(data);
    }

    private static void GenerateMesh(VaryingHeightMeshFaceHolder[] holders, ref uint vertexCount,
        PooledList<int> vertexData, PooledList<uint> indexData)
    {
        foreach (VaryingHeightMeshFaceHolder holder in holders)
            holder.GenerateMesh(ref vertexCount, vertexData, indexData);
    }

    private static void ReturnToPool(BlockMeshFaceHolder[] holders)
    {
        foreach (BlockMeshFaceHolder holder in holders) holder.ReturnToPool();
    }

    private static void ReturnToPool(VaryingHeightMeshFaceHolder[] holders)
    {
        foreach (VaryingHeightMeshFaceHolder holder in holders) holder.ReturnToPool();
    }
}
