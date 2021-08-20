// <copyright file="ClientSection.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

#define BENCHMARK_SECTION_MESHING

using Microsoft.Extensions.Logging;
using OpenToolkit.Mathematics;
using System;
using System.Diagnostics;
using VoxelGame.Client.Collections;
using VoxelGame.Client.Rendering;
using VoxelGame.Core.Collections;
using VoxelGame.Core.Logic;
using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Visuals;
using VoxelGame.Logging;

namespace VoxelGame.Client.Logic
{
    [Serializable]
    public class ClientSection : Core.Logic.Section
    {
#if BENCHMARK_SECTION_MESHING

        private static readonly ILogger Logger = LoggingHelper.CreateLogger<ClientSection>();

        private static long _totalMeshingTime;
        private static long _meshingRuns;

#endif

        [NonSerialized] private bool hasMesh;
        [NonSerialized] private SectionRenderer? renderer;

        public ClientSection(World world) : base(world)
        {
        }

        protected override void Setup()
        {
            renderer = new SectionRenderer();

            hasMesh = false;
            disposed = false;
        }

        public void CreateAndSetMesh(int sectionX, int sectionY, int sectionZ)
        {
            CreateMeshData(sectionX, sectionY, sectionZ, out SectionMeshData meshData);
            SetMeshData(meshData);
        }

        public void CreateMeshData(int sectionX, int sectionY, int sectionZ, out SectionMeshData meshData)
        {
#if BENCHMARK_SECTION_MESHING

            System.Diagnostics.Stopwatch stopwatch = Stopwatch.StartNew();

#endif

            // Set the neutral tint colors.
            TintColor blockTint = TintColor.Green;
            TintColor liquidTint = TintColor.Blue;

            Vector3i sectionPosition = (sectionX, sectionY, sectionZ);

            // Get the sections next to this section.
            ClientSection?[] neighbors = GetNeighborSections(sectionPosition);

            BlockMeshFaceHolder[] blockMeshFaceHolders = CreateBlockMeshFaceHolders();

            PooledList<float> complexVertexPositions = new PooledList<float>(64);
            PooledList<int> complexVertexData = new PooledList<int>(32);
            PooledList<uint> complexIndices = new PooledList<uint>(16);

            uint complexVertexCount = 0;

            VaryingHeightMeshFaceHolder[] varyingHeightMeshFaceHolders = CreateVaryingHeightMeshFaceHolders();

            VaryingHeightMeshFaceHolder[] opaqueLiquidMeshFaceHolders = CreateVaryingHeightMeshFaceHolders();
            VaryingHeightMeshFaceHolder[] transparentLiquidMeshFaceHolders = CreateVaryingHeightMeshFaceHolders();

            PooledList<int> crossPlantVertexData = new PooledList<int>(16);

            PooledList<int> cropPlantVertexData = new PooledList<int>(16);

            // Loop through the section
            for (var x = 0; x < SectionSize; x++)
            {
                for (var y = 0; y < SectionSize; y++)
                {
                    for (var z = 0; z < SectionSize; z++)
                    {
                        uint val = blocks[(x << SectionSizeExp2) + (y << SectionSizeExp) + z];
                        Section.Decode(val, out Block currentBlock, out uint data, out Liquid currentLiquid, out LiquidLevel level, out bool isStatic);

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

                                    void MeshSimpleSide(BlockSide side)
                                    {
                                        ClientSection? neighbor = neighbors[(int) side];
                                        Block? blockToCheck;

                                        Vector3i checkPos = side.Offset(pos);

                                        if (IsPositionOutOfSection(checkPos))
                                        {
                                            checkPos = checkPos.Mod(SectionSize);

                                            bool atEnd = side == BlockSide.Top || side == BlockSide.Bottom;
                                            blockToCheck = neighbor?.GetBlock(checkPos) ?? (atEnd ? Block.Air : null);
                                        }
                                        else
                                        {
                                            blockToCheck = GetBlock(checkPos);
                                        }

                                        if (blockToCheck != null && (!blockToCheck.IsFull || (!blockToCheck.IsOpaque && currentBlock.IsOpaque) || (!blockToCheck.IsOpaque && (currentBlock.RenderFaceAtNonOpaques || blockToCheck.RenderFaceAtNonOpaques))))
                                        {
                                            BlockMeshData mesh = currentBlock.GetMesh(BlockMeshInfo.Simple(side, data, currentLiquid));

                                            side.Corners(out int[] a, out int[] b, out int[] c, out int[] d);
                                            int[][] uvs = BlockModels.GetBlockUVs(mesh.IsTextureRotated);

                                            // int: uv-- ---- ---- -xxx xxyy yyyz zzzz (uv: texture coords; xyz: position)
                                            int upperDataA = (uvs[0][0] << 31) | (uvs[0][1] << 30) | ((a[0] + x) << 10) | ((a[1] + y) << 5) | (a[2] + z);
                                            int upperDataB = (uvs[1][0] << 31) | (uvs[1][1] << 30) | ((b[0] + x) << 10) | ((b[1] + y) << 5) | (b[2] + z);
                                            int upperDataC = (uvs[2][0] << 31) | (uvs[2][1] << 30) | ((c[0] + x) << 10) | ((c[1] + y) << 5) | (c[2] + z);
                                            int upperDataD = (uvs[3][0] << 31) | (uvs[3][1] << 30) | ((d[0] + x) << 10) | ((d[1] + y) << 5) | (d[2] + z);

                                            // int: tttt tttt t--n nn-a ---i iiii iiii iiii (t: tint; n: normal; a: animated; i: texture index)
                                            int lowerData = (mesh.Tint.GetBits(blockTint) << 23) | ((int) side << 18) | mesh.GetAnimationBit(16) | mesh.TextureIndex;

                                            blockMeshFaceHolders[(int) side].AddFace(pos, lowerData, (upperDataA, upperDataB, upperDataC, upperDataD), mesh.IsTextureRotated);
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
                                        complexVertexPositions.Add(vertices[(i * 8) + 0] + x);
                                        complexVertexPositions.Add(vertices[(i * 8) + 1] + y);
                                        complexVertexPositions.Add(vertices[(i * 8) + 2] + z);

                                        // int: nnnn nooo oopp ppp- ---- --uu uuuv vvvv (nop: normal; uv: texture coords)
                                        int upperData =
                                            (((vertices[(i * 8) + 5] < 0f) ? (0b1_0000 | (int) (vertices[(i * 8) + 5] * -15f)) : (int) (vertices[(i * 8) + 5] * 15f)) << 27) |
                                            (((vertices[(i * 8) + 6] < 0f) ? (0b1_0000 | (int) (vertices[(i * 8) + 6] * -15f)) : (int) (vertices[(i * 8) + 6] * 15f)) << 22) |
                                            (((vertices[(i * 8) + 7] < 0f) ? (0b1_0000 | (int) (vertices[(i * 8) + 7] * -15f)) : (int) (vertices[(i * 8) + 7] * 15f)) << 17) |
                                            ((int) (vertices[(i * 8) + 3] * 16f) << 5) |
                                            ((int) (vertices[(i * 8) + 4] * 16f));

                                        complexVertexData.Add(upperData);

                                        // int: tttt tttt t--- ---a ---i iiii iiii iiii(t: tint; a: animated; i: texture index)
                                        int lowerData = (mesh.Tint.GetBits(blockTint) << 23) | mesh.GetAnimationBit(i, 16) | textureIndices[i];
                                        complexVertexData.Add(lowerData);
                                    }

                                    for (int i = complexIndices.Count - indices.Length; i < complexIndices.Count; i++)
                                    {
                                        complexIndices[i] += complexVertexCount;
                                    }

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

                                    void MeshVaryingHeightSide(BlockSide side)
                                    {
                                        ClientSection? neighbor = neighbors[(int) side];
                                        Block? blockToCheck;
                                        uint blockToCheckData = default;

                                        Vector3i checkPos = side.Offset(pos);

                                        if (IsPositionOutOfSection(checkPos))
                                        {
                                            checkPos = checkPos.Mod(SectionSize);

                                            bool atEnd = side == BlockSide.Top || side == BlockSide.Bottom;
                                            blockToCheck = neighbor?.GetBlock(checkPos, out blockToCheckData) ?? (atEnd ? Block.Air : null);
                                        }
                                        else
                                        {
                                            blockToCheck = GetBlock(checkPos, out blockToCheckData);
                                        }

                                        if (blockToCheck != null && (!blockToCheck.IsFull || !blockToCheck.IsOpaque))
                                        {
                                            bool isModified = side != BlockSide.Bottom &&
                                                              ((IHeightVariable) currentBlock).GetHeight(data) != IHeightVariable.MaximumHeight;

                                            BlockMeshData mesh = currentBlock.GetMesh(BlockMeshInfo.Simple(side, data, currentLiquid));
                                            side.Corners(out int[] a, out int[] b, out int[] c, out int[] d);

                                            if (isModified)
                                            {
                                                // Mesh similar to liquids.

                                                int height = ((IHeightVariable) currentBlock).GetHeight(data);
                                                if (side != BlockSide.Top && blockToCheck is IHeightVariable toCheck && toCheck.GetHeight(blockToCheckData) == height) return;

                                                // int: uv-- ---- ---- ---- -xxx xxey yyyz zzzz (uv: texture coords; hl: texture repetition; xyz: position; e: lower/upper end)
                                                int upperDataA = (0 << 31) | (0 << 30) | (x + a[0] << 10) | (a[1] << 9) | (y << 5) | (z + a[2]);
                                                int upperDataB = (0 << 31) | (1 << 30) | (x + b[0] << 10) | (b[1] << 9) | (y << 5) | (z + b[2]);
                                                int upperDataC = (1 << 31) | (1 << 30) | (x + c[0] << 10) | (c[1] << 9) | (y << 5) | (z + c[2]);
                                                int upperDataD = (1 << 31) | (0 << 30) | (x + d[0] << 10) | (d[1] << 9) | (y << 5) | (z + d[2]);

                                                // int: tttt tttt tnnn hhhh ---i iiii iiii iiii (t: tint; n: normal; h: height; i: texture index)
                                                int lowerData = (mesh.Tint.GetBits(blockTint) << 23) | ((int) side << 20) | (height << 16) | mesh.TextureIndex;

                                                varyingHeightMeshFaceHolders[(int) side].AddFace(pos, lowerData, (upperDataA, upperDataB, upperDataC, upperDataD), true, false);
                                            }
                                            else
                                            {
                                                // Mesh into the simple buffer.

                                                // int: uv-- ---- ---- ---- -xxx xxyy yyyz zzzz (uv: texture coords; xyz: position)
                                                int upperDataA = (0 << 31) | (0 << 30) | ((a[0] + x) << 10) | ((a[1] + y) << 5) | (a[2] + z);
                                                int upperDataB = (0 << 31) | (1 << 30) | ((b[0] + x) << 10) | ((b[1] + y) << 5) | (b[2] + z);
                                                int upperDataC = (1 << 31) | (1 << 30) | ((c[0] + x) << 10) | ((c[1] + y) << 5) | (c[2] + z);
                                                int upperDataD = (1 << 31) | (0 << 30) | ((d[0] + x) << 10) | ((d[1] + y) << 5) | (d[2] + z);

                                                // int: tttt tttt t--n nn-_ ---i iiii iiii iiii (t: tint; n: normal; i: texture index, _: used for simple blocks but not here)
                                                int lowerData = (mesh.Tint.GetBits(blockTint) << 23) | ((int) side << 18) | mesh.TextureIndex;

                                                blockMeshFaceHolders[(int) side].AddFace(pos, lowerData, (upperDataA, upperDataB, upperDataC, upperDataD), false);
                                            }
                                        }
                                    }

                                    break;
                                }
                            case TargetBuffer.CrossPlant:
                                {
                                    BlockMeshData mesh = currentBlock.GetMesh(BlockMeshInfo.CrossPlant(data, currentLiquid));

                                    // int: uv-o ---- ---- ---- -xxx xxyy yyyz zzzz (uv: texture coords; xyz: position;)
                                    int upperDataA = (0 << 31) | (0 << 30) | (x + 0 << 10) | (y + 0 << 5);
                                    int upperDataB = (0 << 31) | (1 << 30) | (x + 0 << 10) | (y + 1 << 5);
                                    int upperDataC = (1 << 31) | (1 << 30) | (x + 1 << 10) | (y + 1 << 5);
                                    int upperDataD = (1 << 31) | (0 << 30) | (x + 1 << 10) | (y + 0 << 5);

                                    // int: tttt tttt tulh ---- ---i iiii iiii iiii (t: tint; u: has upper; l: lowered; h: height; i: texture index)
                                    int lowerData = (mesh.Tint.GetBits(blockTint) << 23) | ((mesh.HasUpper ? 1 : 0) << 22) | ((mesh.IsLowered ? 1 : 0) << 21) | ((mesh.IsUpper ? 1 : 0) << 20) | mesh.TextureIndex;

                                    // Z position.
                                    int lowZ = z;
                                    int highZ = z + 1;

                                    AddFace(0, highZ, lowZ);
                                    AddFace(1 << 28, lowZ, highZ);

                                    void AddFace(int orientation, int zA, int zB)
                                    {
                                        crossPlantVertexData.AddRange(new[]
                                        {
                                            upperDataA | orientation | zA, lowerData,
                                            upperDataC | orientation | zB, lowerData,
                                            upperDataB | orientation | zA, lowerData,
                                            upperDataA | orientation | zA, lowerData,
                                            upperDataD | orientation | zB, lowerData,
                                            upperDataC | orientation | zB, lowerData
                                        });
                                    }

                                    break;
                                }
                            case TargetBuffer.CropPlant:
                                {
                                    BlockMeshData mesh = currentBlock.GetMesh(BlockMeshInfo.CropPlant(data, currentLiquid));

                                    // int: uv-- -oss ---- ---- -xxx xxyy yyyz zzzz (uv: texture coords; o: orientation; s: shift, xyz: position)
                                    int upperDataA = (0 << 31) | (0 << 30) | (y + 0 << 5);
                                    int upperDataB = (0 << 31) | (1 << 30) | (y + 1 << 5);
                                    int upperDataC = (1 << 31) | (1 << 30) | (y + 1 << 5);
                                    int upperDataD = (1 << 31) | (0 << 30) | (y + 0 << 5);

                                    // int: tttt tttt tulh ---c ---i iiii iiii iiii (t: tint; u: has upper; l: lowered; h: height; c: crop type; i: texture index)
                                    int lowerData = (mesh.Tint.GetBits(blockTint) << 23) | ((mesh.HasUpper ? 1 : 0) << 22) | ((mesh.IsLowered ? 1 : 0) << 21) | ((mesh.IsUpper ? 1 : 0) << 20) | ((mesh.IsDoubleCropPlant ? 1 : 0) << 16) | mesh.TextureIndex;

                                    int firstAlongX = (x << 10) | (z + 0);
                                    int secondAlongX = (x << 10) | (z + 1);

                                    int firstAlongZ = (x + 0 << 10) | z;
                                    int secondAlongZ = (x + 1 << 10) | z;

                                    AddFace(0 << 26, 0 << 24, firstAlongX, secondAlongX);
                                    AddFace(1 << 26, 0 << 24, firstAlongZ, secondAlongZ);

                                    AddFace(0 << 26, 1 << 24, firstAlongX, secondAlongX);
                                    AddFace(1 << 26, 1 << 24, firstAlongZ, secondAlongZ);

                                    if (!mesh.IsDoubleCropPlant)
                                    {
                                        AddFace(0 << 26, 2 << 24, firstAlongX, secondAlongX);
                                        AddFace(1 << 26, 2 << 24, firstAlongZ, secondAlongZ);
                                    }

                                    void AddFace(int orientation, int shift, int first, int second)
                                    {
                                        cropPlantVertexData.AddRange(new[]
                                        {
                                            upperDataA | orientation | shift | first, lowerData,
                                            upperDataC | orientation | shift | second, lowerData,
                                            upperDataB | orientation | shift | first, lowerData,
                                            upperDataA | orientation | shift | first, lowerData,
                                            upperDataD | orientation | shift | second, lowerData,
                                            upperDataC | orientation | shift | second, lowerData
                                        });
                                    }

                                    break;
                                }
                        }

                        if (currentLiquid.RenderType != RenderType.NotRendered && ((currentBlock is IFillable fillable && fillable.RenderLiquid) || (currentBlock is not IFillable && !currentBlock.IsSolidAndFull)))
                        {
                            VaryingHeightMeshFaceHolder[] liquidMeshFaceHolders = currentLiquid.RenderType == RenderType.Opaque ?
                                opaqueLiquidMeshFaceHolders : transparentLiquidMeshFaceHolders;

                            MeshLiquidSide(BlockSide.Front);
                            MeshLiquidSide(BlockSide.Back);
                            MeshLiquidSide(BlockSide.Left);
                            MeshLiquidSide(BlockSide.Right);
                            MeshLiquidSide(BlockSide.Bottom);
                            MeshLiquidSide(BlockSide.Top);

                            void MeshLiquidSide(BlockSide side)
                            {
                                ClientSection? neighbor = neighbors[(int) side];

                                Liquid? liquidToCheck;
                                Block? blockToCheck;

                                Vector3i checkPos = side.Offset(pos);

                                int sideHeight = -1;
                                bool atEnd = side == BlockSide.Top || side == BlockSide.Bottom;

                                if (IsPositionOutOfSection(checkPos))
                                {
                                    checkPos = checkPos.Mod(SectionSize);

                                    liquidToCheck = neighbor?.GetLiquid(checkPos, out sideHeight) ?? (atEnd ? Liquid.None : null);
                                    blockToCheck = neighbor?.GetBlock(checkPos) ?? (atEnd ? Block.Air : null);
                                }
                                else
                                {
                                    liquidToCheck = GetLiquid(checkPos, out sideHeight);
                                    blockToCheck = GetBlock(checkPos);
                                }

                                bool isNeighborLiquidMeshed = blockToCheck is IFillable frontFillable && frontFillable.RenderLiquid;

                                if (liquidToCheck != currentLiquid || !isNeighborLiquidMeshed) sideHeight = -1;

                                bool flowsTowardsFace = side == BlockSide.Top
                                    ? currentLiquid.Direction < 0
                                    : currentLiquid.Direction > 0;

                                bool meshAtNormal = (int) level > sideHeight && blockToCheck?.IsOpaque != true;
                                bool meshAtEnd = ((flowsTowardsFace && sideHeight != 7 && blockToCheck?.IsOpaque != true)
                                                  || (!flowsTowardsFace && (level != LiquidLevel.Eight || (liquidToCheck != currentLiquid && blockToCheck?.IsOpaque != true))));

                                if (atEnd ? meshAtEnd : meshAtNormal)
                                {
                                    LiquidMeshData mesh = currentLiquid.GetMesh(new LiquidMeshInfo(level, side, isStatic));

                                    bool singleSided = (blockToCheck?.IsOpaque == false &&
                                                        blockToCheck?.IsSolidAndFull == true);

                                    side.Corners(out int[] a, out int[] b, out int[] c, out int[] d);

                                    // int: uv-- ---- ---- ---- -xxx xxey yyyz zzzz (uv: texture coords; xyz: position; e: lower/upper end)
                                    int upperDataA = (0 << 31) | (0 << 30) | (x + a[0] << 10) | (a[1] << 9) | (y << 5) | (z + a[2]);
                                    int upperDataB = (0 << 31) | (1 << 30) | (x + b[0] << 10) | (b[1] << 9) | (y << 5) | (z + b[2]);
                                    int upperDataC = (1 << 31) | (1 << 30) | (x + c[0] << 10) | (c[1] << 9) | (y << 5) | (z + c[2]);
                                    int upperDataD = (1 << 31) | (0 << 30) | (x + d[0] << 10) | (d[1] << 9) | (y << 5) | (z + d[2]);

                                    // int: tttt tttt t--- -nnn hhhh dlll siii iiii (t: tint; n: normal; h: side height; d: direction; l: level; s: isStatic; i: texture index)
                                    int lowerData = (mesh.Tint.GetBits(liquidTint) << 23) | ((int) side << 16) | ((sideHeight + 1) << 12) | ((currentLiquid.Direction > 0 ? 0 : 1) << 11) | ((int) level << 8) | (isStatic ? (1 << 7) : (0 << 7)) | ((((mesh.TextureIndex - 1) >> 4) + 1) & 0b0111_1111);

                                    liquidMeshFaceHolders[(int) side].AddFace(pos, lowerData, (upperDataA, upperDataB, upperDataC, upperDataD), singleSided, isFull);
                                }
                            }
                        }
                    }
                }
            }

            // Complex mesh data is already built at this point.

            // Build the simple mesh data.
            PooledList<int> simpleVertexData = new PooledList<int>(2048);
            GenerateMesh(blockMeshFaceHolders, simpleVertexData);

            // Build the varying height mesh data.
            PooledList<int> varyingHeightVertexData = new PooledList<int>(8);
            PooledList<uint> varyingHeightIndices = new PooledList<uint>(8);

            uint varyingHeightVertexCount = 0;

            GenerateMesh(varyingHeightMeshFaceHolders, ref varyingHeightVertexCount, varyingHeightVertexData, varyingHeightIndices);

            // Build the liquid mesh data.
            PooledList<int> opaqueLiquidVertexData = new PooledList<int>(8);
            PooledList<uint> opaqueLiquidIndices = new PooledList<uint>(8);
            uint opaqueLiquidVertexCount = 0;

            GenerateMesh(opaqueLiquidMeshFaceHolders, ref opaqueLiquidVertexCount, opaqueLiquidVertexData, opaqueLiquidIndices);

            PooledList<int> transparentLiquidVertexData = new PooledList<int>(8);
            PooledList<uint> transparentLiquidIndices = new PooledList<uint>(8);
            uint transparentLiquidVertexCount = 0;

            GenerateMesh(transparentLiquidMeshFaceHolders, ref transparentLiquidVertexCount, transparentLiquidVertexData, transparentLiquidIndices);

            // Finish up.
            meshData = new SectionMeshData(
                simpleVertexData,
                complexVertexPositions, complexVertexData, complexIndices,
                varyingHeightVertexData, varyingHeightIndices,
                crossPlantVertexData,
                cropPlantVertexData,
                opaqueLiquidVertexData, opaqueLiquidIndices,
                transparentLiquidVertexData, transparentLiquidIndices);

            hasMesh = meshData.IsFilled;

            // Cleanup.
            ReturnToPool(blockMeshFaceHolders);
            ReturnToPool(varyingHeightMeshFaceHolders);
            ReturnToPool(opaqueLiquidMeshFaceHolders);
            ReturnToPool(transparentLiquidMeshFaceHolders);

#if BENCHMARK_SECTION_MESHING

            stopwatch.Stop();
            if (hasMesh) IncreaseTotalRuntime(stopwatch.ElapsedMilliseconds);

#endif
        }

#if BENCHMARK_SECTION_MESHING

        private static void IncreaseTotalRuntime(long ms)
        {
            long totalRuntime = System.Threading.Interlocked.Add(ref _totalMeshingTime, ms);
            long runs = System.Threading.Interlocked.Increment(ref _meshingRuns);

            if (runs % 100 != 0) return;

            double averageRuntime = totalRuntime / (double) runs;

            string msg = $"Average section meshing time: {averageRuntime}ms";
            Console.WriteLine(msg);
        }

#endif

        private ClientSection?[] GetNeighborSections(Vector3i sectionPosition)
        {
            ClientSection?[] neighbors = new ClientSection?[6];

            neighbors[(int) BlockSide.Front] = World.GetSection(BlockSide.Front.Offset(sectionPosition)) as ClientSection;
            neighbors[(int) BlockSide.Back] = World.GetSection(BlockSide.Back.Offset(sectionPosition)) as ClientSection;
            neighbors[(int) BlockSide.Left] = World.GetSection(BlockSide.Left.Offset(sectionPosition)) as ClientSection;
            neighbors[(int) BlockSide.Right] = World.GetSection(BlockSide.Right.Offset(sectionPosition)) as ClientSection;
            neighbors[(int) BlockSide.Bottom] = World.GetSection(BlockSide.Bottom.Offset(sectionPosition)) as ClientSection;
            neighbors[(int) BlockSide.Top] = World.GetSection(BlockSide.Top.Offset(sectionPosition)) as ClientSection;

            return neighbors;
        }

        private static BlockMeshFaceHolder[] CreateBlockMeshFaceHolders()
        {
            BlockMeshFaceHolder[] holders = new BlockMeshFaceHolder[6];

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
            VaryingHeightMeshFaceHolder[] holders = new VaryingHeightMeshFaceHolder[6];

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
            foreach (BlockMeshFaceHolder holder in holders)
            {
                holder.GenerateMesh(data);
            }
        }

        private static void GenerateMesh(VaryingHeightMeshFaceHolder[] holders, ref uint vertexCount, PooledList<int> vertexData, PooledList<uint> indexData)
        {
            foreach (VaryingHeightMeshFaceHolder holder in holders)
            {
                holder.GenerateMesh(ref vertexCount, vertexData, indexData);
            }
        }

        private static void ReturnToPool(BlockMeshFaceHolder[] holders)
        {
            foreach (BlockMeshFaceHolder holder in holders)
            {
                holder.ReturnToPool();
            }
        }

        private static void ReturnToPool(VaryingHeightMeshFaceHolder[] holders)
        {
            foreach (VaryingHeightMeshFaceHolder holder in holders)
            {
                holder.ReturnToPool();
            }
        }

        private static bool IsPositionOutOfSection(Vector3i position)
        {
            return position.X < 0 || position.X >= SectionSize ||
                   position.Y < 0 || position.Y >= SectionSize ||
                   position.Z < 0 || position.Z >= SectionSize;
        }

        public void SetMeshData(SectionMeshData meshData)
        {
            Debug.Assert(renderer != null);
            Debug.Assert(hasMesh == meshData.IsFilled);

            renderer.SetData(meshData);
        }

        public void Render(int stage, Vector3 position)
        {
            if (hasMesh)
            {
                renderer?.DrawStage(stage, position);
            }
        }

        #region IDisposable Support

        [NonSerialized] private bool disposed;

        protected override void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    renderer?.Dispose();
                }

                disposed = true;
            }
        }

        #endregion IDisposable Support
    }
}