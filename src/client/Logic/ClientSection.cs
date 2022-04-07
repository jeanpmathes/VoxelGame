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
    /// <param name="sectionX">The x position, in section coordinates.</param>
    /// <param name="sectionY">The y position, in section coordinates.</param>
    /// <param name="sectionZ">The z position, in section coordinates.</param>
    public void CreateAndSetMesh(int sectionX, int sectionY, int sectionZ)
    {
        SectionMeshData meshData = CreateMeshData(sectionX, sectionY, sectionZ);
        SetMeshData(meshData);
    }

    /// <summary>
    ///     Create mesh data for this section.
    /// </summary>
    /// <param name="sectionX">The x position, in section coordinates.</param>
    /// <param name="sectionY">The y position, in section coordinates.</param>
    /// <param name="sectionZ">The z position, in section coordinates.</param>
    /// <returns>The created mesh data.</returns>
    [SuppressMessage(
        "Blocker Code Smell",
        "S2437:Silly bit operations should not be performed",
        Justification = "Improves readability.")]
    public SectionMeshData CreateMeshData(int sectionX, int sectionY, int sectionZ)
    {
        // Set the neutral tint colors.
        TintColor blockTint = TintColor.Green;
        TintColor liquidTint = TintColor.Blue;

        Vector3i sectionPosition = (sectionX, sectionY, sectionZ);

        // Get the sections next to this section.
        ClientSection?[] neighbors = GetNeighborSections(sectionPosition);

        BlockMeshFaceHolder[] blockMeshFaceHolders = CreateBlockMeshFaceHolders();

        PooledList<float> complexVertexPositions = new(capacity: 64);
        PooledList<int> complexVertexData = new(capacity: 32);
        PooledList<uint> complexIndices = new(capacity: 16);

        uint complexVertexCount = 0;

        VaryingHeightMeshFaceHolder[] varyingHeightMeshFaceHolders = CreateVaryingHeightMeshFaceHolders();

        VaryingHeightMeshFaceHolder[] opaqueLiquidMeshFaceHolders = CreateVaryingHeightMeshFaceHolders();
        VaryingHeightMeshFaceHolder[] transparentLiquidMeshFaceHolders = CreateVaryingHeightMeshFaceHolders();

        PooledList<int> crossPlantVertexData = new(capacity: 16);

        PooledList<int> cropPlantVertexData = new(capacity: 16);

        // Loop through the section
        for (var x = 0; x < SectionSize; x++)
        for (var y = 0; y < SectionSize; y++)
        for (var z = 0; z < SectionSize; z++)
        {
            uint val = blocks[(x << SectionSizeExp2) + (y << SectionSizeExp) + z];

            Decode(
                val,
                out Block currentBlock,
                out uint data,
                out Liquid currentLiquid,
                out LiquidLevel level,
                out bool isStatic);

            var pos = new Vector3i(x, y, z);
            bool isFull = level == LiquidLevel.Eight;

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
                            checkPos = checkPos.Mod(SectionSize);

                            bool atVerticalEnd = side is BlockSide.Top or BlockSide.Bottom;

                            blockToCheck = neighbor?.GetBlock(checkPos) ??
                                           (atVerticalEnd ? Block.Air : null);
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
                                BlockMeshInfo.Simple(side, data, currentLiquid));

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
                    BlockMeshData mesh = currentBlock.GetMesh(BlockMeshInfo.Complex(data, currentLiquid));
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
                            checkPos = checkPos.Mod(SectionSize);

                            bool atVerticalEnd = side is BlockSide.Top or BlockSide.Bottom;

                            blockToCheck = neighbor?.GetBlock(checkPos, out blockToCheckData) ??
                                           (atVerticalEnd ? Block.Air : null);
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
                                BlockMeshInfo.Simple(side, data, currentLiquid));

                            side.Corners(out int[] a, out int[] b, out int[] c, out int[] d);

                            if (isModified)
                            {
                                // Mesh similar to liquids.

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
                        currentBlock.GetMesh(BlockMeshInfo.CrossPlant(data, currentLiquid));

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
                    BlockMeshData mesh = currentBlock.GetMesh(BlockMeshInfo.CropPlant(data, currentLiquid));

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

            if (currentLiquid.RenderType != RenderType.NotRendered &&
                (currentBlock is IFillable { RenderLiquid: true } ||
                 currentBlock is not IFillable && !currentBlock.IsSolidAndFull))
            {
                VaryingHeightMeshFaceHolder[] liquidMeshFaceHolders =
                    currentLiquid.RenderType == RenderType.Opaque
                        ? opaqueLiquidMeshFaceHolders
                        : transparentLiquidMeshFaceHolders;

                MeshLiquidSide(BlockSide.Front);
                MeshLiquidSide(BlockSide.Back);
                MeshLiquidSide(BlockSide.Left);
                MeshLiquidSide(BlockSide.Right);
                MeshLiquidSide(BlockSide.Bottom);
                MeshLiquidSide(BlockSide.Top);

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                void MeshLiquidSide(BlockSide side)
                {
                    ClientSection? neighbor = neighbors[(int) side];

                    Liquid? liquidToCheck;
                    Block? blockToCheck;

                    Vector3i checkPos = side.Offset(pos);

                    int sideHeight = -1;
                    bool atVerticalEnd = side is BlockSide.Top or BlockSide.Bottom;

                    if (IsPositionOutOfSection(checkPos))
                    {
                        checkPos = checkPos.Mod(SectionSize);

                        liquidToCheck = neighbor?.GetLiquid(checkPos, out sideHeight) ??
                                        (atVerticalEnd ? Liquid.None : null);

                        blockToCheck = neighbor?.GetBlock(checkPos) ?? (atVerticalEnd ? Block.Air : null);
                    }
                    else
                    {
                        liquidToCheck = GetLiquid(checkPos, out sideHeight);
                        blockToCheck = GetBlock(checkPos);
                    }

                    bool isNeighborLiquidMeshed =
                        blockToCheck is IFillable { RenderLiquid: true };

                    if (liquidToCheck != currentLiquid || !isNeighborLiquidMeshed) sideHeight = -1;

                    bool flowsTowardsFace = side == BlockSide.Top
                        ? currentLiquid.Direction == VerticalFlow.Upwards
                        : currentLiquid.Direction == VerticalFlow.Downwards;

                    bool meshAtNormal = (int) level > sideHeight && blockToCheck?.IsOpaque != true;

                    bool meshAtEnd =
                        flowsTowardsFace && sideHeight != 7 && blockToCheck?.IsOpaque != true
                        || !flowsTowardsFace && (level != LiquidLevel.Eight ||
                                                 liquidToCheck != currentLiquid &&
                                                 blockToCheck?.IsOpaque != true);

                    if (atVerticalEnd ? !meshAtEnd : !meshAtNormal) return;

                    LiquidMeshData mesh =
                        currentLiquid.GetMesh(LiquidMeshInfo.Liquid(level, side, isStatic));

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
                    int lowerData = (mesh.Tint.GetBits(liquidTint) << 23) | ((int) side << 16) |
                                    ((sideHeight + 1) << 12) |
                                    (currentLiquid.Direction.GetBit() << 11) | ((int) level << 8) |
                                    (isStatic ? 1 << 7 : 0 << 7) |
                                    ((((mesh.TextureIndex - 1) >> 4) + 1) & 0b0111_1111);

                    liquidMeshFaceHolders[(int) side].AddFace(
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

        // Build the liquid mesh data.
        PooledList<int> opaqueLiquidVertexData = new(capacity: 8);
        PooledList<uint> opaqueLiquidIndices = new(capacity: 8);
        uint opaqueLiquidVertexCount = 0;

        GenerateMesh(
            opaqueLiquidMeshFaceHolders,
            ref opaqueLiquidVertexCount,
            opaqueLiquidVertexData,
            opaqueLiquidIndices);

        PooledList<int> transparentLiquidVertexData = new(capacity: 8);
        PooledList<uint> transparentLiquidIndices = new(capacity: 8);
        uint transparentLiquidVertexCount = 0;

        GenerateMesh(
            transparentLiquidMeshFaceHolders,
            ref transparentLiquidVertexCount,
            transparentLiquidVertexData,
            transparentLiquidIndices);

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
            opaqueLiquidVertexData,
            opaqueLiquidIndices,
            transparentLiquidVertexData,
            transparentLiquidIndices);

        hasMesh = meshData.IsFilled;

        // Cleanup.
        ReturnToPool(blockMeshFaceHolders);
        ReturnToPool(varyingHeightMeshFaceHolders);
        ReturnToPool(opaqueLiquidMeshFaceHolders);
        ReturnToPool(transparentLiquidMeshFaceHolders);

        return meshData;
    }

    private ClientSection?[] GetNeighborSections(Vector3i sectionPosition)
    {
        var neighbors = new ClientSection?[6];

        neighbors[(int) BlockSide.Front] =
            World.GetSection(BlockSide.Front.Offset(sectionPosition)) as ClientSection;

        neighbors[(int) BlockSide.Back] = World.GetSection(BlockSide.Back.Offset(sectionPosition)) as ClientSection;
        neighbors[(int) BlockSide.Left] = World.GetSection(BlockSide.Left.Offset(sectionPosition)) as ClientSection;

        neighbors[(int) BlockSide.Right] =
            World.GetSection(BlockSide.Right.Offset(sectionPosition)) as ClientSection;

        neighbors[(int) BlockSide.Bottom] =
            World.GetSection(BlockSide.Bottom.Offset(sectionPosition)) as ClientSection;

        neighbors[(int) BlockSide.Top] = World.GetSection(BlockSide.Top.Offset(sectionPosition)) as ClientSection;

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
        return position.X is < 0 or >= SectionSize || position.Y is < 0 or >= SectionSize ||
               position.Z is < 0 or >= SectionSize;
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
