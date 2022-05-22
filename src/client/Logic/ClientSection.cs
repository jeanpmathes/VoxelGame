// <copyright file="ClientSection.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

// ReSharper disable CommentTypo

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using OpenTK.Mathematics;
using VoxelGame.Client.Collections;
using VoxelGame.Client.Rendering;
using VoxelGame.Core.Collections;
using VoxelGame.Core.Logic;
using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Client.Logic;

/// <summary>
///     A section of the world, specifically for the client.
///     Sections do not know their exact position in the world.
/// </summary>
[Serializable]
public class ClientSection : Section
{
    private static long runCount;
    private static long runTime;
    [NonSerialized] private bool hasMesh;
    [NonSerialized] private SectionRenderer? renderer;

    /// <summary>
    ///     Create a new client section.
    /// </summary>
    /// <param name="world">The world containing the client section.</param>
    public ClientSection(World world) : base(world) {}

    /// <inheritdoc />
    protected override void Setup()
    {
        renderer = new SectionRenderer();

        hasMesh = false;
        disposed = false;
    }

    /// <summary>
    ///     Create a mesh for this section and activate it.
    /// </summary>
    /// <param name="position">The position of the section.</param>
    public void CreateAndSetMesh(SectionPosition position)
    {
        SectionMeshData meshData = CreateMeshData(position);
        SetMeshData(meshData);
    }

    /// <summary>
    ///     Create mesh data for this section.
    /// </summary>
    /// <param name="position">The position of the section.</param>
    /// <returns>The created mesh data.</returns>
    public SectionMeshData CreateMeshData(SectionPosition position)
    {
        Stopwatch stopwatch = new();

        stopwatch.Start();

        SectionMeshData result = CreateMeshData_Untimed(position);

        stopwatch.Stop();

        runCount++;
        runTime += stopwatch.ElapsedMilliseconds;

        if (runCount % 1000 == 0) System.Console.WriteLine($"Average run time: {runTime / (float) runCount}ms");

        return result;
    }

    /// <summary>
    ///     Create mesh data for this section.
    /// </summary>
    /// <param name="position">The position of the section.</param>
    /// <returns>The created mesh data.</returns>
    [SuppressMessage(
        "Blocker Code Smell",
        "S2437:Silly bit operations should not be performed",
        Justification = "Improves readability.")]
    private SectionMeshData CreateMeshData_Untimed(SectionPosition position)
    {
        // Set the neutral tint colors.
        TintColor blockTint = TintColor.Green;
        TintColor fluidTint = TintColor.Blue;

        // Get the sections next to this section.
        ClientSection?[] neighbors = GetNeighborSections(position);

        BlockMeshFaceHolder[] blockMeshFaceHolders = CreateBlockMeshFaceHolders();

        PooledList<float> complexVertexPositions = new(capacity: 64);
        PooledList<int> complexVertexData = new(capacity: 32);
        PooledList<uint> complexIndices = new(capacity: 16);

        uint complexVertexCount = 0;

        VaryingHeightMeshFaceHolder[] varyingHeightMeshFaceHolders = CreateVaryingHeightMeshFaceHolders();

        VaryingHeightMeshFaceHolder[] opaqueFluidMeshFaceHolders = CreateVaryingHeightMeshFaceHolders();
        VaryingHeightMeshFaceHolder[] transparentFluidMeshFaceHolders = CreateVaryingHeightMeshFaceHolders();

        PooledList<int> crossPlantVertexData = new(capacity: 16);

        PooledList<int> cropPlantVertexData = new(capacity: 16);

        // Loop through the section
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

            var pos = new Vector3i(x, y, z);
            bool isFull = level == FluidLevel.Eight;

            switch (currentBlock.TargetBuffer)
            {
                case TargetBuffer.Simple:
                {
                    // Check all six sides of this block

                    MeshSimpleSide(BlockSide.Front);
                    MeshSimpleSide(BlockSide.Back);
                    MeshSimpleSide(BlockSide.Left);
                    MeshSimpleSide(BlockSide.Right);
                    MeshSimpleSide(BlockSide.Bottom);
                    MeshSimpleSide(BlockSide.Top);

                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    void MeshSimpleSide(BlockSide side)
                    {
                        ClientSection? neighbor = neighbors[(int) side];
                        Block? blockToCheck;

                        Vector3i checkPos = side.Offset(pos);

                        if (IsPositionOutOfSection(checkPos))
                        {
                            checkPos = checkPos.Mod(Size);

                            blockToCheck = neighbor?.GetBlock(checkPos);
                        }
                        else
                        {
                            blockToCheck = GetBlock(checkPos);
                        }

                        if (blockToCheck == null) return;

                        if (!blockToCheck.IsFull
                            || !blockToCheck.IsOpaque && (currentBlock.IsOpaque ||
                                                          currentBlock.RenderFaceAtNonOpaques ||
                                                          blockToCheck.RenderFaceAtNonOpaques))
                        {
                            BlockMeshData mesh = currentBlock.GetMesh(
                                BlockMeshInfo.Simple(side, data, currentFluid));

                            side.Corners(out int[] a, out int[] b, out int[] c, out int[] d);
                            int[][] uvs = BlockModels.GetBlockUVs(mesh.IsTextureRotated);

                            // int: uv-- ---- ---- -xxx xxyy yyyz zzzz (uv: texture coords; xyz: position)
                            int upperDataA = (uvs[0][0] << 31) | (uvs[0][1] << 30) | ((a[0] + x) << 10) |
                                             ((a[1] + y) << 5) | (a[2] + z);

                            int upperDataB = (uvs[1][0] << 31) | (uvs[1][1] << 30) | ((b[0] + x) << 10) |
                                             ((b[1] + y) << 5) | (b[2] + z);

                            int upperDataC = (uvs[2][0] << 31) | (uvs[2][1] << 30) | ((c[0] + x) << 10) |
                                             ((c[1] + y) << 5) | (c[2] + z);

                            int upperDataD = (uvs[3][0] << 31) | (uvs[3][1] << 30) | ((d[0] + x) << 10) |
                                             ((d[1] + y) << 5) | (d[2] + z);

                            // int: tttt tttt t--n nn-a ---i iiii iiii iiii (t: tint; n: normal; a: animated; i: texture index)
                            int lowerData = (mesh.Tint.GetBits(blockTint) << 23) | ((int) side << 18) |
                                            mesh.GetAnimationBit(shift: 16) | mesh.TextureIndex;

                            blockMeshFaceHolders[(int) side].AddFace(
                                pos,
                                lowerData,
                                (upperDataA, upperDataB, upperDataC, upperDataD),
                                mesh.IsTextureRotated);
                        }
                    }

                    break;
                }
                case TargetBuffer.Complex:
                {
                    BlockMeshData mesh = currentBlock.GetMesh(BlockMeshInfo.Complex(data, currentFluid));
                    float[] vertices = mesh.GetVertices();
                    int[] textureIndices = mesh.GetTextureIndices();
                    uint[] indices = mesh.GetIndices();

                    complexIndices.AddRange(indices);

                    for (var i = 0; i < mesh.VertexCount; i++)
                    {
                        complexVertexPositions.Add(vertices[i * 8 + 0] + x);
                        complexVertexPositions.Add(vertices[i * 8 + 1] + y);
                        complexVertexPositions.Add(vertices[i * 8 + 2] + z);

                        // int: nnnn nooo oopp ppp- ---- --uu uuuv vvvv (nop: normal; uv: texture coords)
                        int upperData =
                            ((vertices[i * 8 + 5] < 0f
                                ? 0b1_0000 | (int) (vertices[i * 8 + 5] * -15f)
                                : (int) (vertices[i * 8 + 5] * 15f)) << 27) |
                            ((vertices[i * 8 + 6] < 0f
                                ? 0b1_0000 | (int) (vertices[i * 8 + 6] * -15f)
                                : (int) (vertices[i * 8 + 6] * 15f)) << 22) |
                            ((vertices[i * 8 + 7] < 0f
                                ? 0b1_0000 | (int) (vertices[i * 8 + 7] * -15f)
                                : (int) (vertices[i * 8 + 7] * 15f)) << 17) |
                            ((int) (vertices[i * 8 + 3] * 16f) << 5) |
                            (int) (vertices[i * 8 + 4] * 16f);

                        complexVertexData.Add(upperData);

                        // int: tttt tttt t--- ---a ---i iiii iiii iiii(t: tint; a: animated; i: texture index)
                        int lowerData = (mesh.Tint.GetBits(blockTint) << 23) | mesh.GetAnimationBit(i, shift: 16) |
                                        textureIndices[i];

                        complexVertexData.Add(lowerData);
                    }

                    for (int i = complexIndices.Count - indices.Length; i < complexIndices.Count; i++)
                        complexIndices[i] += complexVertexCount;

                    complexVertexCount += mesh.VertexCount;

                    break;
                }
                case TargetBuffer.VaryingHeight:
                {
                    MeshVaryingHeightSide(BlockSide.Front);
                    MeshVaryingHeightSide(BlockSide.Back);
                    MeshVaryingHeightSide(BlockSide.Left);
                    MeshVaryingHeightSide(BlockSide.Right);
                    MeshVaryingHeightSide(BlockSide.Bottom);
                    MeshVaryingHeightSide(BlockSide.Top);

                    [MethodImpl(MethodImplOptions.AggressiveInlining)]
                    void MeshVaryingHeightSide(BlockSide side)
                    {
                        ClientSection? neighbor = neighbors[(int) side];
                        Block? blockToCheck;
                        uint blockToCheckData = default;

                        Vector3i checkPos = side.Offset(pos);

                        if (IsPositionOutOfSection(checkPos))
                        {
                            checkPos = checkPos.Mod(Size);

                            blockToCheck = neighbor?.GetBlock(checkPos, out blockToCheckData);
                        }
                        else
                        {
                            blockToCheck = GetBlock(checkPos, out blockToCheckData);
                        }

                        if (blockToCheck != null && (!blockToCheck.IsFull || !blockToCheck.IsOpaque))
                        {
                            bool isModified = side != BlockSide.Bottom &&
                                              ((IHeightVariable) currentBlock).GetHeight(data) !=
                                              IHeightVariable.MaximumHeight;

                            BlockMeshData mesh = currentBlock.GetMesh(
                                BlockMeshInfo.Simple(side, data, currentFluid));

                            side.Corners(out int[] a, out int[] b, out int[] c, out int[] d);

                            if (isModified)
                            {
                                // Mesh similar to fluids.

                                int height = ((IHeightVariable) currentBlock).GetHeight(data);

                                if (side != BlockSide.Top && blockToCheck is IHeightVariable toCheck &&
                                    toCheck.GetHeight(blockToCheckData) == height) return;

                                // int: uv-- ---- ---- ---- -xxx xxey yyyz zzzz (uv: texture coords; hl: texture repetition; xyz: position; e: lower/upper end)
                                int upperDataA = (0 << 31) | (0 << 30) | ((x + a[0]) << 10) | (a[1] << 9) |
                                                 (y << 5) | (z + a[2]);

                                int upperDataB = (0 << 31) | (1 << 30) | ((x + b[0]) << 10) | (b[1] << 9) |
                                                 (y << 5) | (z + b[2]);

                                int upperDataC = (1 << 31) | (1 << 30) | ((x + c[0]) << 10) | (c[1] << 9) |
                                                 (y << 5) | (z + c[2]);

                                int upperDataD = (1 << 31) | (0 << 30) | ((x + d[0]) << 10) | (d[1] << 9) |
                                                 (y << 5) | (z + d[2]);

                                // int: tttt tttt tnnn hhhh ---i iiii iiii iiii (t: tint; n: normal; h: height; i: texture index)
                                int lowerData = (mesh.Tint.GetBits(blockTint) << 23) | ((int) side << 20) |
                                                (height << 16) | mesh.TextureIndex;

                                varyingHeightMeshFaceHolders[(int) side].AddFace(
                                    pos,
                                    lowerData,
                                    (upperDataA, upperDataB, upperDataC, upperDataD),
                                    isSingleSided: true,
                                    isFull: false);
                            }
                            else
                            {
                                // Mesh into the simple buffer.

                                // int: uv-- ---- ---- ---- -xxx xxyy yyyz zzzz (uv: texture coords; xyz: position)
                                int upperDataA = (0 << 31) | (0 << 30) | ((a[0] + x) << 10) |
                                                 ((a[1] + y) << 5) | (a[2] + z);

                                int upperDataB = (0 << 31) | (1 << 30) | ((b[0] + x) << 10) |
                                                 ((b[1] + y) << 5) | (b[2] + z);

                                int upperDataC = (1 << 31) | (1 << 30) | ((c[0] + x) << 10) |
                                                 ((c[1] + y) << 5) | (c[2] + z);

                                int upperDataD = (1 << 31) | (0 << 30) | ((d[0] + x) << 10) |
                                                 ((d[1] + y) << 5) | (d[2] + z);

                                // int: tttt tttt t--n nn-_ ---i iiii iiii iiii (t: tint; n: normal; i: texture index, _: used for simple blocks but not here)
                                int lowerData = (mesh.Tint.GetBits(blockTint) << 23) | ((int) side << 18) |
                                                mesh.TextureIndex;

                                blockMeshFaceHolders[(int) side].AddFace(
                                    pos,
                                    lowerData,
                                    (upperDataA, upperDataB, upperDataC, upperDataD),
                                    isRotated: false);
                            }
                        }
                    }

                    break;
                }
                case TargetBuffer.CrossPlant:
                {
                    BlockMeshData mesh =
                        currentBlock.GetMesh(BlockMeshInfo.CrossPlant(data, currentFluid));

                    // int: ---- ---- ---- ---- -xxx xxyy yyyz zzzz (xyz: position)
                    int upperData = (x << 10) | (y << 5) | z;

                    // int: tttt tttt tulh ---- ---i iiii iiii iiii (t: tint; u: has upper; l: lowered; h: height; i: texture index)
                    int lowerData = (mesh.Tint.GetBits(blockTint) << 23) | ((mesh.HasUpper ? 1 : 0) << 22) |
                                    ((mesh.IsLowered ? 1 : 0) << 21) | ((mesh.IsUpper ? 1 : 0) << 20) |
                                    mesh.TextureIndex;

                    crossPlantVertexData.Add(upperData);
                    crossPlantVertexData.Add(lowerData);

                    break;
                }
                case TargetBuffer.CropPlant:
                {
                    BlockMeshData mesh = currentBlock.GetMesh(BlockMeshInfo.CropPlant(data, currentFluid));

                    // int: o--- ssss ---- ---- -xxx xxyy yyyz zzzz (o: orientation; s: shift, xyz: position)
                    int upperData = (x << 10) | (y << 5) | z;

                    // int: tttt tttt tulh ---c ---i iiii iiii iiii (t: tint; u: has upper; l: lowered; h: height; c: crop type; i: texture index)
                    int lowerData = (mesh.Tint.GetBits(blockTint) << 23) | ((mesh.HasUpper ? 1 : 0) << 22) |
                                    ((mesh.IsLowered ? 1 : 0) << 21) | ((mesh.IsUpper ? 1 : 0) << 20) |
                                    ((mesh.IsDoubleCropPlant ? 1 : 0) << 16) | mesh.TextureIndex;

                    if (!mesh.IsDoubleCropPlant)
                    {
                        cropPlantVertexData.Add((4 << 24) | upperData);
                        cropPlantVertexData.Add(lowerData);

                        cropPlantVertexData.Add((8 << 24) | upperData);
                        cropPlantVertexData.Add(lowerData);

                        cropPlantVertexData.Add((12 << 24) | upperData);
                        cropPlantVertexData.Add(lowerData);

                        const int o = 1 << 31;

                        cropPlantVertexData.Add(o | (4 << 24) | upperData);
                        cropPlantVertexData.Add(lowerData);

                        cropPlantVertexData.Add(o | (8 << 24) | upperData);
                        cropPlantVertexData.Add(lowerData);

                        cropPlantVertexData.Add(o | (12 << 24) | upperData);
                        cropPlantVertexData.Add(lowerData);
                    }
                    else
                    {
                        cropPlantVertexData.Add((4 << 24) | upperData);
                        cropPlantVertexData.Add(lowerData);

                        cropPlantVertexData.Add((12 << 24) | upperData);
                        cropPlantVertexData.Add(lowerData);

                        const int o = 1 << 31;

                        cropPlantVertexData.Add(o | (4 << 24) | upperData);
                        cropPlantVertexData.Add(lowerData);

                        cropPlantVertexData.Add(o | (12 << 24) | upperData);
                        cropPlantVertexData.Add(lowerData);
                    }

                    break;
                }
            }

            if (currentFluid.RenderType != RenderType.NotRendered &&
                (currentBlock is IFillable { RenderFluid: true } ||
                 currentBlock is not IFillable && !currentBlock.IsSolidAndFull))
            {
                VaryingHeightMeshFaceHolder[] fluidMeshFaceHolders =
                    currentFluid.RenderType == RenderType.Opaque
                        ? opaqueFluidMeshFaceHolders
                        : transparentFluidMeshFaceHolders;

                MeshFluidSide(BlockSide.Front);
                MeshFluidSide(BlockSide.Back);
                MeshFluidSide(BlockSide.Left);
                MeshFluidSide(BlockSide.Right);
                MeshFluidSide(BlockSide.Bottom);
                MeshFluidSide(BlockSide.Top);

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                void MeshFluidSide(BlockSide side)
                {
                    ClientSection? neighbor = neighbors[(int) side];

                    Fluid? fluidToCheck;
                    Block? blockToCheck;

                    Vector3i checkPos = side.Offset(pos);

                    int sideHeight = -1;
                    bool atVerticalEnd = side is BlockSide.Top or BlockSide.Bottom;

                    if (IsPositionOutOfSection(checkPos))
                    {
                        checkPos = checkPos.Mod(Size);

                        fluidToCheck = neighbor?.GetFluid(checkPos, out sideHeight);
                        blockToCheck = neighbor?.GetBlock(checkPos);
                    }
                    else
                    {
                        fluidToCheck = GetFluid(checkPos, out sideHeight);
                        blockToCheck = GetBlock(checkPos);
                    }

                    bool isNeighborFluidMeshed =
                        blockToCheck is IFillable { RenderFluid: true };

                    if (fluidToCheck != currentFluid || !isNeighborFluidMeshed) sideHeight = -1;

                    bool flowsTowardsFace = side == BlockSide.Top
                        ? currentFluid.Direction == VerticalFlow.Upwards
                        : currentFluid.Direction == VerticalFlow.Downwards;

                    bool meshAtNormal = (int) level > sideHeight && blockToCheck?.IsOpaque != true;

                    bool meshAtEnd =
                        flowsTowardsFace && sideHeight != 7 && blockToCheck?.IsOpaque != true
                        || !flowsTowardsFace && (level != FluidLevel.Eight ||
                                                 fluidToCheck != currentFluid &&
                                                 blockToCheck?.IsOpaque != true);

                    if (atVerticalEnd ? !meshAtEnd : !meshAtNormal) return;

                    FluidMeshData mesh =
                        currentFluid.GetMesh(FluidMeshInfo.Fluid(level, side, isStatic));

                    bool singleSided = blockToCheck?.IsOpaque == false &&
                                       blockToCheck.IsSolidAndFull;

                    side.Corners(out int[] a, out int[] b, out int[] c, out int[] d);

                    // int: uv-- ---- ---- ---- -xxx xxey yyyz zzzz (uv: texture coords; xyz: position; e: lower/upper end)
                    int upperDataA = (0 << 31) | (0 << 30) | ((x + a[0]) << 10) | (a[1] << 9) | (y << 5) |
                                     (z + a[2]);

                    int upperDataB = (0 << 31) | (1 << 30) | ((x + b[0]) << 10) | (b[1] << 9) | (y << 5) |
                                     (z + b[2]);

                    int upperDataC = (1 << 31) | (1 << 30) | ((x + c[0]) << 10) | (c[1] << 9) | (y << 5) |
                                     (z + c[2]);

                    int upperDataD = (1 << 31) | (0 << 30) | ((x + d[0]) << 10) | (d[1] << 9) | (y << 5) |
                                     (z + d[2]);

                    // int: tttt tttt t--- -nnn hhhh dlll siii iiii (t: tint; n: normal; h: side height; d: direction; l: level; s: isStatic; i: texture index)
                    int lowerData = (mesh.Tint.GetBits(fluidTint) << 23) | ((int) side << 16) |
                                    ((sideHeight + 1) << 12) |
                                    (currentFluid.Direction.GetBit() << 11) | ((int) level << 8) |
                                    (isStatic ? 1 << 7 : 0 << 7) |
                                    ((((mesh.TextureIndex - 1) >> 4) + 1) & 0b0111_1111);

                    fluidMeshFaceHolders[(int) side].AddFace(
                        pos,
                        lowerData,
                        (upperDataA, upperDataB, upperDataC, upperDataD),
                        singleSided,
                        isFull);
                }
            }
        }

        // Complex mesh data is already built at this point.

        // Build the simple mesh data.
        PooledList<int> simpleVertexData = new(capacity: 2048);
        GenerateMesh(blockMeshFaceHolders, simpleVertexData);

        // Build the varying height mesh data.
        PooledList<int> varyingHeightVertexData = new(capacity: 8);
        PooledList<uint> varyingHeightIndices = new(capacity: 8);

        uint varyingHeightVertexCount = 0;

        GenerateMesh(
            varyingHeightMeshFaceHolders,
            ref varyingHeightVertexCount,
            varyingHeightVertexData,
            varyingHeightIndices);

        // Build the fluid mesh data.
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

        // Finish up.
        SectionMeshData meshData = new(
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

        hasMesh = meshData.IsFilled;

        // Cleanup.
        ReturnToPool(blockMeshFaceHolders);
        ReturnToPool(varyingHeightMeshFaceHolders);
        ReturnToPool(opaqueFluidMeshFaceHolders);
        ReturnToPool(transparentFluidMeshFaceHolders);

        return meshData;
    }

    private ClientSection?[] GetNeighborSections(SectionPosition position)
    {
        var neighbors = new ClientSection?[6];

        foreach (BlockSide side in BlockSide.All.Sides())
            neighbors[(int) side] =
                World.GetSection(side.Offset(position)) as ClientSection;

        return neighbors;
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

    private static bool IsPositionOutOfSection(Vector3i position)
    {
        return position.X is < 0 or >= Size || position.Y is < 0 or >= Size ||
               position.Z is < 0 or >= Size;
    }

    /// <summary>
    ///     Set the mesh data for this section. The mesh must be generated from this section.
    /// </summary>
    /// <param name="meshData">The mesh data to use and activate.</param>
    public void SetMeshData(SectionMeshData meshData)
    {
        Debug.Assert(renderer != null);
        Debug.Assert(hasMesh == meshData.IsFilled);

        renderer.SetData(meshData);
    }

    /// <summary>
    ///     Render this section.
    /// </summary>
    /// <param name="stage">The current render stage.</param>
    /// <param name="position">The position of this section in world coordinates.</param>
    public void Render(int stage, Vector3 position)
    {
        if (hasMesh) renderer?.DrawStage(stage, position);
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
