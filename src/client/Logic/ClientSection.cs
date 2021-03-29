// <copyright file="ClientSection.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using OpenToolkit.Mathematics;
using System;
using VoxelGame.Client.Collections;
using VoxelGame.Client.Rendering;
using VoxelGame.Core;
using VoxelGame.Core.Collections;
using VoxelGame.Core.Logic;
using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Client.Logic
{
    [Serializable]
    public class ClientSection : Core.Logic.Section
    {
        [NonSerialized] private bool hasMesh;
        [NonSerialized] private SectionRenderer? renderer;

        /// <summary>
        /// Sets up all non serialized members.
        /// </summary>
        public override void Setup()
        {
            renderer = GLManager.SectionRendererFactory.CreateSectionRenderer();

            hasMesh = false;
            disposed = false;
        }

        public void CreateAndSetMesh(int sectionX, int sectionY, int sectionZ)
        {
            CreateMeshData(sectionX, sectionY, sectionZ, out SectionMeshData meshData);
            SetMeshData(ref meshData);
        }

        public void CreateMeshData(int sectionX, int sectionY, int sectionZ, out SectionMeshData meshData)
        {
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

            // Loop through the section
            for (var x = 0; x < SectionSize; x++)
            {
                for (var y = 0; y < SectionSize; y++)
                {
                    for (var z = 0; z < SectionSize; z++)
                    {
                        Vector3i pos = (x, y, z);
                        uint val = blocks[(x << 10) + (y << 5) + z];

                        Section.Decode(val, out Block currentBlock, out uint data, out Liquid currentLiquid, out LiquidLevel level, out bool isStatic);

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
                                        ClientSection? neighbor = neighbors[(int)side];
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
                                            float[] vertices = mesh.GetVertices();

                                            // int: uv-- ---- ---- --xx xxxx yyyy yyzz zzzz (uv: texture coords; xyz: position)
                                            int upperDataA = (((int)vertices[(0 * 8) + 3]) << 31) | (((int)vertices[(0 * 8) + 4]) << 30) | (((int)vertices[(0 * 8) + 0] + x) << 12) | (((int)vertices[(0 * 8) + 1] + y) << 6) | ((int)vertices[(0 * 8) + 2] + z);
                                            int upperDataB = (((int)vertices[(1 * 8) + 3]) << 31) | (((int)vertices[(1 * 8) + 4]) << 30) | (((int)vertices[(1 * 8) + 0] + x) << 12) | (((int)vertices[(1 * 8) + 1] + y) << 6) | ((int)vertices[(1 * 8) + 2] + z);
                                            int upperDataC = (((int)vertices[(2 * 8) + 3]) << 31) | (((int)vertices[(2 * 8) + 4]) << 30) | (((int)vertices[(2 * 8) + 0] + x) << 12) | (((int)vertices[(2 * 8) + 1] + y) << 6) | ((int)vertices[(2 * 8) + 2] + z);
                                            int upperDataD = (((int)vertices[(3 * 8) + 3]) << 31) | (((int)vertices[(3 * 8) + 4]) << 30) | (((int)vertices[(3 * 8) + 0] + x) << 12) | (((int)vertices[(3 * 8) + 1] + y) << 6) | ((int)vertices[(3 * 8) + 2] + z);

                                            // int: tttt tttt t--n nn-a ---i iiii iiii iiii (t: tint; n: normal; a: animated; i: texture index)
                                            int lowerData = (mesh.Tint.GetBits(blockTint) << 23) | ((int)side << 18) | mesh.GetAnimationBit(16) | mesh.TextureIndex;

                                            blockMeshFaceHolders[(int)side].AddFace(pos, lowerData, (upperDataA, upperDataB, upperDataC, upperDataD));
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
                                            (((vertices[(i * 8) + 5] < 0f) ? (0b1_0000 | (int)(vertices[(i * 8) + 5] * -15f)) : (int)(vertices[(i * 8) + 5] * 15f)) << 27) |
                                            (((vertices[(i * 8) + 6] < 0f) ? (0b1_0000 | (int)(vertices[(i * 8) + 6] * -15f)) : (int)(vertices[(i * 8) + 6] * 15f)) << 22) |
                                            (((vertices[(i * 8) + 7] < 0f) ? (0b1_0000 | (int)(vertices[(i * 8) + 7] * -15f)) : (int)(vertices[(i * 8) + 7] * 15f)) << 17) |
                                            ((int)(vertices[(i * 8) + 3] * 16f) << 5) |
                                            ((int)(vertices[(i * 8) + 4] * 16f));

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
                                        ClientSection? neighbor = neighbors[(int)side];
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
                                                              ((IHeightVariable)currentBlock).GetHeight(data) != IHeightVariable.MaximumHeight;

                                            BlockMeshData mesh = currentBlock.GetMesh(BlockMeshInfo.Simple(side, data, currentLiquid));
                                            side.Corners(out int[] a, out int[] b, out int[] c, out int[] d);

                                            if (isModified)
                                            {
                                                // Mesh similar to liquids.

                                                int height = ((IHeightVariable)currentBlock).GetHeight(data);
                                                if (blockToCheck is IHeightVariable toCheck && toCheck.GetHeight(blockToCheckData) == height) return;

                                                // int: uvll lllh hhhh --xx xxxx eyyy yyzz zzzz (uv: texture coords; hl: texture repetition; xyz: position; e: lower/upper end)
                                                int upperDataA = (0 << 31) | (0 << 30) | (x + a[0] << 12) | (a[1] << 11) | (y << 6) | (z + a[2]);
                                                int upperDataB = (0 << 31) | (1 << 30) | (x + b[0] << 12) | (b[1] << 11) | (y << 6) | (z + b[2]);
                                                int upperDataC = (1 << 31) | (1 << 30) | (x + c[0] << 12) | (c[1] << 11) | (y << 6) | (z + c[2]);
                                                int upperDataD = (1 << 31) | (0 << 30) | (x + d[0] << 12) | (d[1] << 11) | (y << 6) | (z + d[2]);

                                                // int: tttt tttt tnnn hhhh ---i iiii iiii iiii (t: tint; n: normal; h: height; i: texture index)
                                                int lowerData = (mesh.Tint.GetBits(blockTint) << 23) | ((int)side << 20) | (height << 16) | mesh.TextureIndex;

                                                varyingHeightMeshFaceHolders[(int)side].AddFace(pos, lowerData, (upperDataA, upperDataB, upperDataC, upperDataD), true, false);
                                            }
                                            else
                                            {
                                                // Mesh into the simple buffer.

                                                // int: uv-- ---- ---- --xx xxxx yyyy yyzz zzzz (uv: texture coords; xyz: position)
                                                int upperDataA = (0 << 31) | (0 << 30) | ((a[0] + x) << 12) | ((a[1] + y) << 6) | (a[2] + z);
                                                int upperDataB = (0 << 31) | (1 << 30) | ((b[0] + x) << 12) | ((b[1] + y) << 6) | (b[2] + z);
                                                int upperDataC = (1 << 31) | (1 << 30) | ((c[0] + x) << 12) | ((c[1] + y) << 6) | (c[2] + z);
                                                int upperDataD = (1 << 31) | (0 << 30) | ((d[0] + x) << 12) | ((d[1] + y) << 6) | (d[2] + z);

                                                // int: tttt tttt t--n nn-_ ---i iiii iiii iiii (t: tint; n: normal; i: texture index, _: used for simple blocks but not here)
                                                int lowerData = (mesh.Tint.GetBits(blockTint) << 23) | ((int)side << 18) | mesh.TextureIndex;

                                                blockMeshFaceHolders[(int)side].AddFace(pos, lowerData, (upperDataA, upperDataB, upperDataC, upperDataD));
                                            }
                                        }
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
                                ClientSection? neighbor = neighbors[(int)side];

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

                                bool meshAtNormal = (int)level > sideHeight && blockToCheck?.IsOpaque != true;
                                bool meshAtEnd = ((flowsTowardsFace && sideHeight != 7 && blockToCheck?.IsOpaque != true)
                                                  || (!flowsTowardsFace && (level != LiquidLevel.Eight || (liquidToCheck != currentLiquid && blockToCheck?.IsOpaque != true))));

                                if (atEnd ? meshAtEnd : meshAtNormal)
                                {
                                    LiquidMeshData mesh = currentLiquid.GetMesh(new LiquidMeshInfo(level, side, isStatic));

                                    bool singleSided = (blockToCheck?.IsOpaque == false &&
                                                        blockToCheck?.IsSolidAndFull == true);

                                    side.Corners(out int[] a, out int[] b, out int[] c, out int[] d);

                                    // int: uv-- ---- ---- --xx xxxx eyyy yyzz zzzz (uv: texture coords; xyz: position; e: lower/upper end)
                                    int upperDataA = (0 << 31) | (0 << 30) | (x + a[0] << 12) | (a[1] << 11) | (y << 6) | (z + a[2]);
                                    int upperDataB = (0 << 31) | (1 << 30) | (x + b[0] << 12) | (b[1] << 11) | (y << 6) | (z + b[2]);
                                    int upperDataC = (1 << 31) | (1 << 30) | (x + c[0] << 12) | (c[1] << 11) | (y << 6) | (z + c[2]);
                                    int upperDataD = (1 << 31) | (0 << 30) | (x + d[0] << 12) | (d[1] << 11) | (y << 6) | (z + d[2]);

                                    // int: tttt tttt t--- -nnn hhhh dlll siii iiii (t: tint; n: normal; h: side height; d: direction; l: level; s: isStatic; i: texture index)
                                    int lowerData = (mesh.Tint.GetBits(liquidTint) << 23) | ((int)side << 16) | ((sideHeight + 1) << 12) | ((currentLiquid.Direction > 0 ? 0 : 1) << 11) | ((int)level << 8) | (isStatic ? (1 << 7) : (0 << 7)) | ((((mesh.TextureIndex - 1) >> 4) + 1) & 0b0111_1111);

                                    liquidMeshFaceHolders[(int)side].AddFace(pos, lowerData, (upperDataA, upperDataB, upperDataC, upperDataD), singleSided, isFull);
                                }
                            }
                        }
                    }
                }
            }

            // Complex mesh data is already built at this point.

            // Build the simple mesh data.
            PooledList<int> simpleVertexData = new PooledList<int>(4096);
            GenerateMesh(blockMeshFaceHolders, ref simpleVertexData);

            // Build the varying height mesh data.
            PooledList<int> varyingHeightVertexData = new PooledList<int>();
            PooledList<uint> varyingHeightIndices = new PooledList<uint>();
            uint varyingHeightVertexCount = 0;

            GenerateMesh(varyingHeightMeshFaceHolders, ref varyingHeightVertexData, ref varyingHeightVertexCount, ref varyingHeightIndices);

            // Build the liquid mesh data.
            PooledList<int> opaqueLiquidVertexData = new PooledList<int>();
            PooledList<uint> opaqueLiquidIndices = new PooledList<uint>();
            uint opaqueLiquidVertexCount = 0;

            GenerateMesh(opaqueLiquidMeshFaceHolders, ref opaqueLiquidVertexData, ref opaqueLiquidVertexCount, ref opaqueLiquidIndices);

            PooledList<int> transparentLiquidVertexData = new PooledList<int>();
            PooledList<uint> transparentLiquidIndices = new PooledList<uint>();
            uint transparentLiquidVertexCount = 0;

            GenerateMesh(transparentLiquidMeshFaceHolders, ref transparentLiquidVertexData, ref transparentLiquidVertexCount, ref transparentLiquidIndices);

            // Finish up.
            hasMesh = complexVertexPositions.Count != 0 || simpleVertexData.Count != 0 || varyingHeightVertexData.Count != 0 || opaqueLiquidVertexData.Count != 0 || transparentLiquidVertexData.Count != 0;

            meshData = new SectionMeshData(ref simpleVertexData,
                ref complexVertexPositions, ref complexVertexData, ref complexIndices,
                ref varyingHeightVertexData, ref varyingHeightIndices,
                ref opaqueLiquidVertexData, ref opaqueLiquidIndices,
                ref transparentLiquidVertexData, ref transparentLiquidIndices);

            // Cleanup.
            ReturnToPool(blockMeshFaceHolders);
            ReturnToPool(varyingHeightMeshFaceHolders);
            ReturnToPool(opaqueLiquidMeshFaceHolders);
            ReturnToPool(transparentLiquidMeshFaceHolders);
        }

        private static ClientSection?[] GetNeighborSections(Vector3i sectionPosition)
        {
            ClientSection?[] neighbors = new ClientSection?[6];

            neighbors[(int)BlockSide.Front] = Game.World.GetSection(BlockSide.Front.Offset(sectionPosition)) as ClientSection;
            neighbors[(int)BlockSide.Back] = Game.World.GetSection(BlockSide.Back.Offset(sectionPosition)) as ClientSection;
            neighbors[(int)BlockSide.Left] = Game.World.GetSection(BlockSide.Left.Offset(sectionPosition)) as ClientSection;
            neighbors[(int)BlockSide.Right] = Game.World.GetSection(BlockSide.Right.Offset(sectionPosition)) as ClientSection;
            neighbors[(int)BlockSide.Bottom] = Game.World.GetSection(BlockSide.Bottom.Offset(sectionPosition)) as ClientSection;
            neighbors[(int)BlockSide.Top] = Game.World.GetSection(BlockSide.Top.Offset(sectionPosition)) as ClientSection;

            return neighbors;
        }

        private static BlockMeshFaceHolder[] CreateBlockMeshFaceHolders()
        {
            BlockMeshFaceHolder[] holders = new BlockMeshFaceHolder[6];

            holders[(int)BlockSide.Front] = new BlockMeshFaceHolder(BlockSide.Front);
            holders[(int)BlockSide.Back] = new BlockMeshFaceHolder(BlockSide.Back);
            holders[(int)BlockSide.Left] = new BlockMeshFaceHolder(BlockSide.Left);
            holders[(int)BlockSide.Right] = new BlockMeshFaceHolder(BlockSide.Right);
            holders[(int)BlockSide.Bottom] = new BlockMeshFaceHolder(BlockSide.Bottom);
            holders[(int)BlockSide.Top] = new BlockMeshFaceHolder(BlockSide.Top);

            return holders;
        }

        private static VaryingHeightMeshFaceHolder[] CreateVaryingHeightMeshFaceHolders()
        {
            VaryingHeightMeshFaceHolder[] holders = new VaryingHeightMeshFaceHolder[6];

            holders[(int)BlockSide.Front] = new VaryingHeightMeshFaceHolder(BlockSide.Front);
            holders[(int)BlockSide.Back] = new VaryingHeightMeshFaceHolder(BlockSide.Back);
            holders[(int)BlockSide.Left] = new VaryingHeightMeshFaceHolder(BlockSide.Left);
            holders[(int)BlockSide.Right] = new VaryingHeightMeshFaceHolder(BlockSide.Right);
            holders[(int)BlockSide.Bottom] = new VaryingHeightMeshFaceHolder(BlockSide.Bottom);
            holders[(int)BlockSide.Top] = new VaryingHeightMeshFaceHolder(BlockSide.Top);

            return holders;
        }

        private static void GenerateMesh(BlockMeshFaceHolder[] holders, ref PooledList<int> data)
        {
            foreach (BlockMeshFaceHolder holder in holders)
            {
                holder.GenerateMesh(ref data);
            }
        }

        private static void GenerateMesh(VaryingHeightMeshFaceHolder[] holders, ref PooledList<int> vertexData, ref uint vertexCount, ref PooledList<uint> indexData)
        {
            foreach (VaryingHeightMeshFaceHolder holder in holders)
            {
                holder.GenerateMesh(ref vertexData, ref vertexCount, ref indexData);
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
            return (position.X < 0 || position.X >= SectionSize)
                   || (position.Y < 0 || position.Y >= SectionSize)
                   || (position.Z < 0 || position.Z >= SectionSize);
        }

        public void SetMeshData(ref SectionMeshData meshData)
        {
            renderer?.SetData(ref meshData);
        }

        public void PrepareRender(int stage)
        {
            renderer?.PrepareStage(stage);
        }

        public void FinishRender(int stage)
        {
            renderer?.FinishStage(stage);
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