// <copyright file="ClientSection.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using System;
using OpenToolkit.Mathematics;
using VoxelGame.Client.Rendering;
using VoxelGame.Core;
using VoxelGame.Client.Collections;
using VoxelGame.Core.Logic;
using VoxelGame.Core.Visuals;
using VoxelGame.Core.Collections;

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
            renderer = new SectionRenderer();

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

            // Get the sections next to this section.
            ClientSection? frontNeighbour = Game.World.GetSection(sectionX, sectionY, sectionZ + 1) as ClientSection;
            ClientSection? backNeighbour = Game.World.GetSection(sectionX, sectionY, sectionZ - 1) as ClientSection;
            ClientSection? leftNeighbour = Game.World.GetSection(sectionX - 1, sectionY, sectionZ) as ClientSection;
            ClientSection? rightNeighbour = Game.World.GetSection(sectionX + 1, sectionY, sectionZ) as ClientSection;
            ClientSection? bottomNeighbour = Game.World.GetSection(sectionX, sectionY - 1, sectionZ) as ClientSection;
            ClientSection? topNeighbour = Game.World.GetSection(sectionX, sectionY + 1, sectionZ) as ClientSection;

            BlockMeshFaceHolder simpleFrontFaceHolder = new BlockMeshFaceHolder(BlockSide.Front);
            BlockMeshFaceHolder simpleBackFaceHolder = new BlockMeshFaceHolder(BlockSide.Back);
            BlockMeshFaceHolder simpleLeftFaceHolder = new BlockMeshFaceHolder(BlockSide.Left);
            BlockMeshFaceHolder simpleRightFaceHolder = new BlockMeshFaceHolder(BlockSide.Right);
            BlockMeshFaceHolder simpleBottomFaceHolder = new BlockMeshFaceHolder(BlockSide.Bottom);
            BlockMeshFaceHolder simpleTopFaceHolder = new BlockMeshFaceHolder(BlockSide.Top);

            PooledList<float> complexVertexPositions = new PooledList<float>(64);
            PooledList<int> complexVertexData = new PooledList<int>(32);
            PooledList<uint> complexIndices = new PooledList<uint>(16);

            uint complexVertCount = 0;

            LiquidMeshFaceHolder liquidFrontFaceHolder = new LiquidMeshFaceHolder(BlockSide.Front);
            LiquidMeshFaceHolder liquidBackFaceHolder = new LiquidMeshFaceHolder(BlockSide.Back);
            LiquidMeshFaceHolder liquidLeftFaceHolder = new LiquidMeshFaceHolder(BlockSide.Left);
            LiquidMeshFaceHolder liquidRightFaceHolder = new LiquidMeshFaceHolder(BlockSide.Right);
            LiquidMeshFaceHolder liquidBottomFaceHolder = new LiquidMeshFaceHolder(BlockSide.Bottom);
            LiquidMeshFaceHolder liquidTopFaceHolder = new LiquidMeshFaceHolder(BlockSide.Top);

            // Loop through the section
            for (int x = 0; x < SectionSize; x++)
            {
                for (int y = 0; y < SectionSize; y++)
                {
                    for (int z = 0; z < SectionSize; z++)
                    {
                        uint val = blocks[(x << 10) + (y << 5) + z];

                        Block currentBlock = Block.TranslateID(val & BLOCKMASK);
                        uint data = (val & DATAMASK) >> DATASHIFT;

                        Liquid currentLiquid = Liquid.TranslateID((val & Section.LIQUIDMASK) >> Section.LIQUIDSHIFT);
                        LiquidLevel level = (LiquidLevel)((val & Section.LEVELMASK) >> Section.LEVELSHIFT);
                        bool isStatic = (val & Section.STATICMASK) != 0;
                        bool isFull = level == LiquidLevel.Eight;

                        if (currentBlock.TargetBuffer == TargetBuffer.Simple)
                        {
                            Block? blockToCheck;

                            // Check all six sides of this block

                            // Front
                            if (z + 1 >= SectionSize && frontNeighbour != null)
                            {
                                blockToCheck = frontNeighbour.GetBlock(x, y, 0);
                            }
                            else if (z + 1 >= SectionSize)
                            {
                                blockToCheck = null;
                            }
                            else
                            {
                                blockToCheck = GetBlock(x, y, z + 1);
                            }

                            if (blockToCheck != null && (!blockToCheck.IsFull || (!blockToCheck.IsOpaque && currentBlock.IsOpaque) || (!blockToCheck.IsOpaque && (currentBlock.RenderFaceAtNonOpaques || blockToCheck.RenderFaceAtNonOpaques))))
                            {
                                currentBlock.GetMesh(BlockSide.Front, data, out float[] vertices, out int[] textureIndices, out _, out TintColor tint, out bool isAnimated);

                                // int: uv-- ---- ---- --xx xxxx yyyy yyzz zzzz (uv: texture coords; xyz: position)
                                int upperDataA = (((int)vertices[(0 * 8) + 3]) << 31) | (((int)vertices[(0 * 8) + 4]) << 30) | (((int)vertices[(0 * 8) + 0] + x) << 12) | (((int)vertices[(0 * 8) + 1] + y) << 6) | ((int)vertices[(0 * 8) + 2] + z);
                                int upperDataB = (((int)vertices[(1 * 8) + 3]) << 31) | (((int)vertices[(1 * 8) + 4]) << 30) | (((int)vertices[(1 * 8) + 0] + x) << 12) | (((int)vertices[(1 * 8) + 1] + y) << 6) | ((int)vertices[(1 * 8) + 2] + z);
                                int upperDataC = (((int)vertices[(2 * 8) + 3]) << 31) | (((int)vertices[(2 * 8) + 4]) << 30) | (((int)vertices[(2 * 8) + 0] + x) << 12) | (((int)vertices[(2 * 8) + 1] + y) << 6) | ((int)vertices[(2 * 8) + 2] + z);
                                int upperDataD = (((int)vertices[(3 * 8) + 3]) << 31) | (((int)vertices[(3 * 8) + 4]) << 30) | (((int)vertices[(3 * 8) + 0] + x) << 12) | (((int)vertices[(3 * 8) + 1] + y) << 6) | ((int)vertices[(3 * 8) + 2] + z);

                                // int: tttt tttt t--n nn-a ---i iiii iiii iiii (t: tint; n: normal; a: animated; i: texture index)
                                int lowerData = (((tint.IsNeutral) ? blockTint.ToBits : tint.ToBits) << 23) | ((int)BlockSide.Front << 18) | (isAnimated && textureIndices[0] != 0 ? (1 << 16) : 0) | textureIndices[0];

                                simpleFrontFaceHolder.AddFace(z, x, y, lowerData, (upperDataA, upperDataB, upperDataC, upperDataD));
                            }

                            // Back
                            if (z - 1 < 0 && backNeighbour != null)
                            {
                                blockToCheck = backNeighbour.GetBlock(x, y, SectionSize - 1);
                            }
                            else if (z - 1 < 0)
                            {
                                blockToCheck = null;
                            }
                            else
                            {
                                blockToCheck = GetBlock(x, y, z - 1);
                            }

                            if (blockToCheck != null && (!blockToCheck.IsFull || (!blockToCheck.IsOpaque && currentBlock.IsOpaque) || (!blockToCheck.IsOpaque && (currentBlock.RenderFaceAtNonOpaques || blockToCheck.RenderFaceAtNonOpaques))))
                            {
                                currentBlock.GetMesh(BlockSide.Back, data, out float[] vertices, out int[] textureIndices, out _, out TintColor tint, out bool isAnimated);

                                // int: uv-- ---- ---- --xx xxxx yyyy yyzz zzzz (uv: texture coords; xyz: position)
                                int upperDataA = (((int)vertices[(0 * 8) + 3]) << 31) | (((int)vertices[(0 * 8) + 4]) << 30) | (((int)vertices[(0 * 8) + 0] + x) << 12) | (((int)vertices[(0 * 8) + 1] + y) << 6) | ((int)vertices[(0 * 8) + 2] + z);
                                int upperDataB = (((int)vertices[(1 * 8) + 3]) << 31) | (((int)vertices[(1 * 8) + 4]) << 30) | (((int)vertices[(1 * 8) + 0] + x) << 12) | (((int)vertices[(1 * 8) + 1] + y) << 6) | ((int)vertices[(1 * 8) + 2] + z);
                                int upperDataC = (((int)vertices[(2 * 8) + 3]) << 31) | (((int)vertices[(2 * 8) + 4]) << 30) | (((int)vertices[(2 * 8) + 0] + x) << 12) | (((int)vertices[(2 * 8) + 1] + y) << 6) | ((int)vertices[(2 * 8) + 2] + z);
                                int upperDataD = (((int)vertices[(3 * 8) + 3]) << 31) | (((int)vertices[(3 * 8) + 4]) << 30) | (((int)vertices[(3 * 8) + 0] + x) << 12) | (((int)vertices[(3 * 8) + 1] + y) << 6) | ((int)vertices[(3 * 8) + 2] + z);

                                // int: tttt tttt t--n nn-a ---i iiii iiii iiii (t: tint; n: normal; a: animated; i: texture index)
                                int lowerData = (((tint.IsNeutral) ? blockTint.ToBits : tint.ToBits) << 23) | ((int)BlockSide.Back << 18) | (isAnimated && textureIndices[0] != 0 ? (1 << 16) : 0) | textureIndices[0];

                                simpleBackFaceHolder.AddFace(z, x, y, lowerData, (upperDataA, upperDataB, upperDataC, upperDataD));
                            }

                            // Left
                            if (x - 1 < 0 && leftNeighbour != null)
                            {
                                blockToCheck = leftNeighbour.GetBlock(SectionSize - 1, y, z);
                            }
                            else if (x - 1 < 0)
                            {
                                blockToCheck = null;
                            }
                            else
                            {
                                blockToCheck = GetBlock(x - 1, y, z);
                            }

                            if (blockToCheck != null && (!blockToCheck.IsFull || (!blockToCheck.IsOpaque && currentBlock.IsOpaque) || (!blockToCheck.IsOpaque && (currentBlock.RenderFaceAtNonOpaques || blockToCheck.RenderFaceAtNonOpaques))))
                            {
                                currentBlock.GetMesh(BlockSide.Left, data, out float[] vertices, out int[] textureIndices, out _, out TintColor tint, out bool isAnimated);

                                // int: uv-- ---- ---- --xx xxxx yyyy yyzz zzzz (uv: texture coords; xyz: position)
                                int upperDataA = (((int)vertices[(0 * 8) + 3]) << 31) | (((int)vertices[(0 * 8) + 4]) << 30) | (((int)vertices[(0 * 8) + 0] + x) << 12) | (((int)vertices[(0 * 8) + 1] + y) << 6) | ((int)vertices[(0 * 8) + 2] + z);
                                int upperDataB = (((int)vertices[(1 * 8) + 3]) << 31) | (((int)vertices[(1 * 8) + 4]) << 30) | (((int)vertices[(1 * 8) + 0] + x) << 12) | (((int)vertices[(1 * 8) + 1] + y) << 6) | ((int)vertices[(1 * 8) + 2] + z);
                                int upperDataC = (((int)vertices[(2 * 8) + 3]) << 31) | (((int)vertices[(2 * 8) + 4]) << 30) | (((int)vertices[(2 * 8) + 0] + x) << 12) | (((int)vertices[(2 * 8) + 1] + y) << 6) | ((int)vertices[(2 * 8) + 2] + z);
                                int upperDataD = (((int)vertices[(3 * 8) + 3]) << 31) | (((int)vertices[(3 * 8) + 4]) << 30) | (((int)vertices[(3 * 8) + 0] + x) << 12) | (((int)vertices[(3 * 8) + 1] + y) << 6) | ((int)vertices[(3 * 8) + 2] + z);

                                // int: tttt tttt t--n nn-a ---i iiii iiii iiii (t: tint; n: normal; a: animated; i: texture index)
                                int lowerData = (((tint.IsNeutral) ? blockTint.ToBits : tint.ToBits) << 23) | ((int)BlockSide.Left << 18) | (isAnimated && textureIndices[0] != 0 ? (1 << 16) : 0) | textureIndices[0];

                                simpleLeftFaceHolder.AddFace(x, y, z, lowerData, (upperDataA, upperDataB, upperDataC, upperDataD));
                            }

                            // Right
                            if (x + 1 >= SectionSize && rightNeighbour != null)
                            {
                                blockToCheck = rightNeighbour.GetBlock(0, y, z);
                            }
                            else if (x + 1 >= SectionSize)
                            {
                                blockToCheck = null;
                            }
                            else
                            {
                                blockToCheck = GetBlock(x + 1, y, z);
                            }

                            if (blockToCheck != null && (!blockToCheck.IsFull || (!blockToCheck.IsOpaque && currentBlock.IsOpaque) || (!blockToCheck.IsOpaque && (currentBlock.RenderFaceAtNonOpaques || blockToCheck.RenderFaceAtNonOpaques))))
                            {
                                currentBlock.GetMesh(BlockSide.Right, data, out float[] vertices, out int[] textureIndices, out _, out TintColor tint, out bool isAnimated);

                                // int: uv-- ---- ---- --xx xxxx yyyy yyzz zzzz (uv: texture coords; xyz: position)
                                int upperDataA = (((int)vertices[(0 * 8) + 3]) << 31) | (((int)vertices[(0 * 8) + 4]) << 30) | (((int)vertices[(0 * 8) + 0] + x) << 12) | (((int)vertices[(0 * 8) + 1] + y) << 6) | ((int)vertices[(0 * 8) + 2] + z);
                                int upperDataB = (((int)vertices[(1 * 8) + 3]) << 31) | (((int)vertices[(1 * 8) + 4]) << 30) | (((int)vertices[(1 * 8) + 0] + x) << 12) | (((int)vertices[(1 * 8) + 1] + y) << 6) | ((int)vertices[(1 * 8) + 2] + z);
                                int upperDataC = (((int)vertices[(2 * 8) + 3]) << 31) | (((int)vertices[(2 * 8) + 4]) << 30) | (((int)vertices[(2 * 8) + 0] + x) << 12) | (((int)vertices[(2 * 8) + 1] + y) << 6) | ((int)vertices[(2 * 8) + 2] + z);
                                int upperDataD = (((int)vertices[(3 * 8) + 3]) << 31) | (((int)vertices[(3 * 8) + 4]) << 30) | (((int)vertices[(3 * 8) + 0] + x) << 12) | (((int)vertices[(3 * 8) + 1] + y) << 6) | ((int)vertices[(3 * 8) + 2] + z);

                                // int: tttt tttt t--n nn-a ---i iiii iiii iiii (t: tint; n: normal; a: animated; i: texture index)
                                int lowerData = (((tint.IsNeutral) ? blockTint.ToBits : tint.ToBits) << 23) | ((int)BlockSide.Right << 18) | (isAnimated && textureIndices[0] != 0 ? (1 << 16) : 0) | textureIndices[0];

                                simpleRightFaceHolder.AddFace(x, y, z, lowerData, (upperDataA, upperDataB, upperDataC, upperDataD));
                            }

                            // Bottom
                            if (y - 1 < 0 && bottomNeighbour != null)
                            {
                                blockToCheck = bottomNeighbour.GetBlock(x, SectionSize - 1, z);
                            }
                            else if (y - 1 < 0)
                            {
                                blockToCheck = null;
                            }
                            else
                            {
                                blockToCheck = GetBlock(x, y - 1, z);
                            }

                            if (blockToCheck?.IsFull != true || (!blockToCheck.IsOpaque && currentBlock.IsOpaque) || (!blockToCheck.IsOpaque && (currentBlock.RenderFaceAtNonOpaques || blockToCheck.RenderFaceAtNonOpaques)))
                            {
                                currentBlock.GetMesh(BlockSide.Bottom, data, out float[] vertices, out int[] textureIndices, out _, out TintColor tint, out bool isAnimated);

                                // int: uv-- ---- ---- --xx xxxx yyyy yyzz zzzz (uv: texture coords; xyz: position)
                                int upperDataA = (((int)vertices[(0 * 8) + 3]) << 31) | (((int)vertices[(0 * 8) + 4]) << 30) | (((int)vertices[(0 * 8) + 0] + x) << 12) | (((int)vertices[(0 * 8) + 1] + y) << 6) | ((int)vertices[(0 * 8) + 2] + z);
                                int upperDataB = (((int)vertices[(1 * 8) + 3]) << 31) | (((int)vertices[(1 * 8) + 4]) << 30) | (((int)vertices[(1 * 8) + 0] + x) << 12) | (((int)vertices[(1 * 8) + 1] + y) << 6) | ((int)vertices[(1 * 8) + 2] + z);
                                int upperDataC = (((int)vertices[(2 * 8) + 3]) << 31) | (((int)vertices[(2 * 8) + 4]) << 30) | (((int)vertices[(2 * 8) + 0] + x) << 12) | (((int)vertices[(2 * 8) + 1] + y) << 6) | ((int)vertices[(2 * 8) + 2] + z);
                                int upperDataD = (((int)vertices[(3 * 8) + 3]) << 31) | (((int)vertices[(3 * 8) + 4]) << 30) | (((int)vertices[(3 * 8) + 0] + x) << 12) | (((int)vertices[(3 * 8) + 1] + y) << 6) | ((int)vertices[(3 * 8) + 2] + z);

                                // int: tttt tttt t--n nn-a ---i iiii iiii iiii (t: tint; n: normal; a: animated; i: texture index)
                                int lowerData = (((tint.IsNeutral) ? blockTint.ToBits : tint.ToBits) << 23) | ((int)BlockSide.Bottom << 18) | (isAnimated && textureIndices[0] != 0 ? (1 << 16) : 0) | textureIndices[0];

                                simpleBottomFaceHolder.AddFace(y, x, z, lowerData, (upperDataA, upperDataB, upperDataC, upperDataD));
                            }

                            // Top
                            if (y + 1 >= SectionSize && topNeighbour != null)
                            {
                                blockToCheck = topNeighbour.GetBlock(x, 0, z);
                            }
                            else if (y + 1 >= SectionSize)
                            {
                                blockToCheck = null;
                            }
                            else
                            {
                                blockToCheck = GetBlock(x, y + 1, z);
                            }

                            if (blockToCheck?.IsFull != true || (!blockToCheck.IsOpaque && currentBlock.IsOpaque) || (!blockToCheck.IsOpaque && (currentBlock.RenderFaceAtNonOpaques || blockToCheck.RenderFaceAtNonOpaques)))
                            {
                                currentBlock.GetMesh(BlockSide.Top, data, out float[] vertices, out int[] textureIndices, out _, out TintColor tint, out bool isAnimated);

                                // int: uv-- ---- ---- --xx xxxx yyyy yyzz zzzz (uv: texture coords; xyz: position)
                                int upperDataA = (((int)vertices[(0 * 8) + 3]) << 31) | (((int)vertices[(0 * 8) + 4]) << 30) | (((int)vertices[(0 * 8) + 0] + x) << 12) | (((int)vertices[(0 * 8) + 1] + y) << 6) | ((int)vertices[(0 * 8) + 2] + z);
                                int upperDataB = (((int)vertices[(1 * 8) + 3]) << 31) | (((int)vertices[(1 * 8) + 4]) << 30) | (((int)vertices[(1 * 8) + 0] + x) << 12) | (((int)vertices[(1 * 8) + 1] + y) << 6) | ((int)vertices[(1 * 8) + 2] + z);
                                int upperDataC = (((int)vertices[(2 * 8) + 3]) << 31) | (((int)vertices[(2 * 8) + 4]) << 30) | (((int)vertices[(2 * 8) + 0] + x) << 12) | (((int)vertices[(2 * 8) + 1] + y) << 6) | ((int)vertices[(2 * 8) + 2] + z);
                                int upperDataD = (((int)vertices[(3 * 8) + 3]) << 31) | (((int)vertices[(3 * 8) + 4]) << 30) | (((int)vertices[(3 * 8) + 0] + x) << 12) | (((int)vertices[(3 * 8) + 1] + y) << 6) | ((int)vertices[(3 * 8) + 2] + z);

                                // int: tttt tttt t--n nn-a ---i iiii iiii iiii (t: tint; n: normal; a: animated; i: texture index)
                                int lowerData = (((tint.IsNeutral) ? blockTint.ToBits : tint.ToBits) << 23) | ((int)BlockSide.Top << 18) | (isAnimated && textureIndices[0] != 0 ? (1 << 16) : 0) | textureIndices[0];

                                simpleTopFaceHolder.AddFace(y, x, z, lowerData, (upperDataA, upperDataB, upperDataC, upperDataD));
                            }
                        }
                        else if (currentBlock.TargetBuffer == TargetBuffer.Complex)
                        {
                            uint verts = currentBlock.GetMesh(BlockSide.All, data, out float[] vertices, out int[] textureIndices, out uint[] indices, out TintColor tint, out bool isAnimated);

                            complexIndices.AddRange(indices);

                            for (int i = 0; i < verts; i++)
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
                                int lowerData = (((tint.IsNeutral) ? blockTint.ToBits : tint.ToBits) << 23) | (isAnimated && textureIndices[i] != 0 ? (1 << 16) : 0) | textureIndices[i];
                                complexVertexData.Add(lowerData);
                            }

                            for (int i = complexIndices.Count - indices.Length; i < complexIndices.Count; i++)
                            {
                                complexIndices[i] += complexVertCount;
                            }

                            complexVertCount += verts;
                        }

                        if (currentLiquid.RenderType != RenderType.NotRendered && !currentBlock.IsSolidAndFull)
                        {
                            Liquid? liquidToCheck;
                            Block? blockToCheck;
                            int sideHeight = -1;

                            // Front.
                            if (z + 1 >= SectionSize && frontNeighbour != null)
                            {
                                liquidToCheck = frontNeighbour.GetLiquid(x, y, 0, out sideHeight);
                                blockToCheck = frontNeighbour.GetBlock(x, y, 0);
                            }
                            else if (z + 1 >= SectionSize)
                            {
                                liquidToCheck = null;
                                blockToCheck = null;
                            }
                            else
                            {
                                liquidToCheck = GetLiquid(x, y, z + 1, out sideHeight);
                                blockToCheck = GetBlock(x, y, z + 1);
                            }

                            if (liquidToCheck != currentLiquid) sideHeight = -1;

                            if ((int)level > sideHeight && blockToCheck?.IsOpaque != true)
                            {
                                currentLiquid.GetMesh(level, BlockSide.Front, isStatic, out int textureIndex, out TintColor tint);

                                bool singleSided = (blockToCheck?.IsOpaque == false && blockToCheck?.IsSolidAndFull == true) || (liquidToCheck != currentLiquid && (liquidToCheck?.RenderType ?? RenderType.NotRendered) != RenderType.NotRendered);

                                // int: uv-- ---- ---- --xx xxxx eyyy yyzz zzzz (uv: texture coords; xyz: position; e: lower/upper end)
                                int upperDataA = (0 << 31) | (0 << 30) | (x + 0 << 12) | (0 << 11) | (y << 6) | (z + 1);
                                int upperDataB = (0 << 31) | (1 << 30) | (x + 0 << 12) | (1 << 11) | (y << 6) | (z + 1);
                                int upperDataC = (1 << 31) | (1 << 30) | (x + 1 << 12) | (1 << 11) | (y << 6) | (z + 1);
                                int upperDataD = (1 << 31) | (0 << 30) | (x + 1 << 12) | (0 << 11) | (y << 6) | (z + 1);

                                // int: tttt tttt t--- -nnn hhhh dlll siii iiii (t: tint; n: normal; h: side height; d: direction; l: level; s: isStatic; i: texture index)
                                int lowerData = (((tint.IsNeutral) ? liquidTint.ToBits : tint.ToBits) << 23) | ((int)BlockSide.Front << 16) | ((sideHeight + 1) << 12) | ((currentLiquid.Direction > 0 ? 0 : 1) << 11) | ((int)level << 8) | (isStatic ? (1 << 7) : (0 << 7)) | ((((textureIndex - 1) >> 4) + 1) & 0b0111_1111);

                                liquidFrontFaceHolder.AddFace(z, x, y, lowerData, (upperDataA, upperDataB, upperDataC, upperDataD), singleSided, isFull);
                            }

                            // Back.
                            if (z - 1 < 0 && backNeighbour != null)
                            {
                                liquidToCheck = backNeighbour.GetLiquid(x, y, SectionSize - 1, out sideHeight);
                                blockToCheck = backNeighbour.GetBlock(x, y, SectionSize - 1);
                            }
                            else if (z - 1 < 0)
                            {
                                liquidToCheck = null;
                                blockToCheck = null;
                            }
                            else
                            {
                                liquidToCheck = GetLiquid(x, y, z - 1, out sideHeight);
                                blockToCheck = GetBlock(x, y, z - 1);
                            }

                            if (liquidToCheck != currentLiquid) sideHeight = -1;

                            if ((int)level > sideHeight && blockToCheck?.IsOpaque != true)
                            {
                                currentLiquid.GetMesh(level, BlockSide.Back, isStatic, out int textureIndex, out TintColor tint);

                                bool singleSided = (blockToCheck?.IsOpaque == false && blockToCheck?.IsSolidAndFull == true) || (liquidToCheck != currentLiquid && (liquidToCheck?.RenderType ?? RenderType.NotRendered) != RenderType.NotRendered);

                                // int: uv-- ---- ---- --xx xxxx eyyy yyzz zzzz (uv: texture coords; xyz: position; e: lower/upper end)
                                int upperDataA = (0 << 31) | (0 << 30) | (x + 1 << 12) | (0 << 11) | (y << 6) | (z + 0);
                                int upperDataB = (0 << 31) | (1 << 30) | (x + 1 << 12) | (1 << 11) | (y << 6) | (z + 0);
                                int upperDataC = (1 << 31) | (1 << 30) | (x + 0 << 12) | (1 << 11) | (y << 6) | (z + 0);
                                int upperDataD = (1 << 31) | (0 << 30) | (x + 0 << 12) | (0 << 11) | (y << 6) | (z + 0);

                                // int: tttt tttt t--- -nnn hhhh dlll siii iiii (t: tint; n: normal; h: side height; d: direction; l: level; s: isStatic; i: texture index)
                                int lowerData = (((tint.IsNeutral) ? liquidTint.ToBits : tint.ToBits) << 23) | ((int)BlockSide.Back << 16) | ((sideHeight + 1) << 12) | ((currentLiquid.Direction > 0 ? 0 : 1) << 11) | ((int)level << 8) | (isStatic ? (1 << 7) : (0 << 7)) | ((((textureIndex - 1) >> 4) + 1) & 0b0111_1111);

                                liquidBackFaceHolder.AddFace(z, x, y, lowerData, (upperDataA, upperDataB, upperDataC, upperDataD), singleSided, isFull);
                            }

                            // Left.
                            if (x - 1 < 0 && leftNeighbour != null)
                            {
                                liquidToCheck = leftNeighbour.GetLiquid(SectionSize - 1, y, z, out sideHeight);
                                blockToCheck = leftNeighbour.GetBlock(SectionSize - 1, y, z);
                            }
                            else if (x - 1 < 0)
                            {
                                liquidToCheck = null;
                                blockToCheck = null;
                            }
                            else
                            {
                                liquidToCheck = GetLiquid(x - 1, y, z, out sideHeight);
                                blockToCheck = GetBlock(x - 1, y, z);
                            }

                            if (liquidToCheck != currentLiquid) sideHeight = -1;

                            if ((int)level > sideHeight && blockToCheck?.IsOpaque != true)
                            {
                                currentLiquid.GetMesh(level, BlockSide.Left, isStatic, out int textureIndex, out TintColor tint);

                                bool singleSided = (blockToCheck?.IsOpaque == false && blockToCheck?.IsSolidAndFull == true) || (liquidToCheck != currentLiquid && (liquidToCheck?.RenderType ?? RenderType.NotRendered) != RenderType.NotRendered);

                                // int: uv-- ---- ---- --xx xxxx eyyy yyzz zzzz (uv: texture coords; xyz: position; e: lower/upper end)
                                int upperDataA = (0 << 31) | (0 << 30) | (x + 0 << 12) | (0 << 11) | (y << 6) | (z + 0);
                                int upperDataB = (0 << 31) | (1 << 30) | (x + 0 << 12) | (1 << 11) | (y << 6) | (z + 0);
                                int upperDataC = (1 << 31) | (1 << 30) | (x + 0 << 12) | (1 << 11) | (y << 6) | (z + 1);
                                int upperDataD = (1 << 31) | (0 << 30) | (x + 0 << 12) | (0 << 11) | (y << 6) | (z + 1);

                                // int: tttt tttt t--- -nnn hhhh dlll siii iiii (t: tint; n: normal; h: side height; d: direction; l: level; s: isStatic; i: texture index)
                                int lowerData = (((tint.IsNeutral) ? liquidTint.ToBits : tint.ToBits) << 23) | ((int)BlockSide.Left << 16) | ((sideHeight + 1) << 12) | ((currentLiquid.Direction > 0 ? 0 : 1) << 11) | ((int)level << 8) | (isStatic ? (1 << 7) : (0 << 7)) | ((((textureIndex - 1) >> 4) + 1) & 0b0111_1111);

                                liquidLeftFaceHolder.AddFace(x, y, z, lowerData, (upperDataA, upperDataB, upperDataC, upperDataD), singleSided, isFull);
                            }

                            // Right.
                            if (x + 1 >= SectionSize && rightNeighbour != null)
                            {
                                liquidToCheck = rightNeighbour.GetLiquid(0, y, z, out sideHeight);
                                blockToCheck = rightNeighbour.GetBlock(0, y, z);
                            }
                            else if (x + 1 >= SectionSize)
                            {
                                liquidToCheck = null;
                                blockToCheck = null;
                            }
                            else
                            {
                                liquidToCheck = GetLiquid(x + 1, y, z, out sideHeight);
                                blockToCheck = GetBlock(x + 1, y, z);
                            }

                            if (liquidToCheck != currentLiquid) sideHeight = -1;

                            if ((int)level > sideHeight && blockToCheck?.IsOpaque != true)
                            {
                                currentLiquid.GetMesh(level, BlockSide.Right, isStatic, out int textureIndex, out TintColor tint);

                                bool singleSided = (blockToCheck?.IsOpaque == false && blockToCheck?.IsSolidAndFull == true) || (liquidToCheck != currentLiquid && (liquidToCheck?.RenderType ?? RenderType.NotRendered) != RenderType.NotRendered);

                                // int: uv-- ---- ---- --xx xxxx eyyy yyzz zzzz (uv: texture coords; xyz: position; e: lower/upper end)
                                int upperDataA = (0 << 31) | (0 << 30) | (x + 1 << 12) | (0 << 11) | (y << 6) | (z + 1);
                                int upperDataB = (0 << 31) | (1 << 30) | (x + 1 << 12) | (1 << 11) | (y << 6) | (z + 1);
                                int upperDataC = (1 << 31) | (1 << 30) | (x + 1 << 12) | (1 << 11) | (y << 6) | (z + 0);
                                int upperDataD = (1 << 31) | (0 << 30) | (x + 1 << 12) | (0 << 11) | (y << 6) | (z + 0);

                                // int: tttt tttt t--- -nnn hhhh dlll siii iiii (t: tint; n: normal; h: side height; d: direction; l: level; s: isStatic; i: texture index)
                                int lowerData = (((tint.IsNeutral) ? liquidTint.ToBits : tint.ToBits) << 23) | ((int)BlockSide.Right << 16) | ((sideHeight + 1) << 12) | ((currentLiquid.Direction > 0 ? 0 : 1) << 11) | ((int)level << 8) | (isStatic ? (1 << 7) : (0 << 7)) | ((((textureIndex - 1) >> 4) + 1) & 0b0111_1111);

                                liquidRightFaceHolder.AddFace(x, y, z, lowerData, (upperDataA, upperDataB, upperDataC, upperDataD), singleSided, isFull);
                            }

                            // Bottom.
                            if (y - 1 < 0 && bottomNeighbour != null)
                            {
                                liquidToCheck = bottomNeighbour.GetLiquid(x, SectionSize - 1, z, out sideHeight);
                                blockToCheck = bottomNeighbour.GetBlock(x, SectionSize - 1, z);
                            }
                            else if (y - 1 < 0)
                            {
                                liquidToCheck = null;
                                blockToCheck = null;
                            }
                            else
                            {
                                liquidToCheck = GetLiquid(x, y - 1, z, out sideHeight);
                                blockToCheck = GetBlock(x, y - 1, z);
                            }

                            if (liquidToCheck != currentLiquid) sideHeight = -1;

                            if ((currentLiquid.Direction > 0 && sideHeight != 7 && blockToCheck?.IsOpaque != true) || (currentLiquid.Direction < 0 && (level != LiquidLevel.Eight || (liquidToCheck != currentLiquid && blockToCheck?.IsOpaque != true))))
                            {
                                currentLiquid.GetMesh(level, BlockSide.Bottom, isStatic, out int textureIndex, out TintColor tint);

                                bool singleSided = ((currentLiquid.Direction > 0 || level == LiquidLevel.Eight) && blockToCheck?.IsOpaque == false && blockToCheck?.IsSolidAndFull == true) || (liquidToCheck != currentLiquid && (liquidToCheck?.RenderType ?? RenderType.NotRendered) != RenderType.NotRendered);

                                // int: uv-- ---- ---- --xx xxxx eyyy yyzz zzzz (uv: texture coords; xyz: position; e: lower/upper end)
                                int upperDataA = (0 << 31) | (0 << 30) | (x + 0 << 12) | (0 << 11) | (y << 6) | (z + 0);
                                int upperDataB = (0 << 31) | (1 << 30) | (x + 0 << 12) | (0 << 11) | (y << 6) | (z + 1);
                                int upperDataC = (1 << 31) | (1 << 30) | (x + 1 << 12) | (0 << 11) | (y << 6) | (z + 1);
                                int upperDataD = (1 << 31) | (0 << 30) | (x + 1 << 12) | (0 << 11) | (y << 6) | (z + 0);

                                // int: tttt tttt t--- -nnn hhhh dlll siii iiii (t: tint; n: normal; h: side height; d: direction; l: level; s: isStatic; i: texture index)
                                int lowerData = (((tint.IsNeutral) ? liquidTint.ToBits : tint.ToBits) << 23) | ((int)BlockSide.Bottom << 16) | ((sideHeight + 1) << 12) | ((currentLiquid.Direction > 0 ? 0 : 1) << 11) | ((int)level << 8) | (isStatic ? (1 << 7) : (0 << 7)) | ((((textureIndex - 1) >> 4) + 1) & 0b0111_1111);

                                liquidBottomFaceHolder.AddFace(y, x, z, lowerData, (upperDataA, upperDataB, upperDataC, upperDataD), singleSided, isFull);
                            }

                            // Top.
                            if (y + 1 >= SectionSize && topNeighbour != null)
                            {
                                liquidToCheck = topNeighbour.GetLiquid(x, 0, z, out sideHeight);
                                blockToCheck = topNeighbour.GetBlock(x, 0, z);
                            }
                            else if (y + 1 >= SectionSize)
                            {
                                liquidToCheck = null;
                                blockToCheck = null;
                            }
                            else
                            {
                                liquidToCheck = GetLiquid(x, y + 1, z, out sideHeight);
                                blockToCheck = GetBlock(x, y + 1, z);
                            }

                            if (liquidToCheck != currentLiquid) sideHeight = -1;

                            if ((currentLiquid.Direction < 0 && sideHeight != 7 && blockToCheck?.IsOpaque != true) || (currentLiquid.Direction > 0 && (level != LiquidLevel.Eight || (liquidToCheck != currentLiquid && blockToCheck?.IsOpaque != true))))
                            {
                                currentLiquid.GetMesh(level, BlockSide.Top, isStatic, out int textureIndex, out TintColor tint);

                                bool singleSided = ((currentLiquid.Direction < 0 || level == LiquidLevel.Eight) && blockToCheck?.IsOpaque == false && blockToCheck?.IsSolidAndFull == true) || (liquidToCheck != currentLiquid && (liquidToCheck?.RenderType ?? RenderType.NotRendered) != RenderType.NotRendered);

                                // int: uv-- ---- ---- --xx xxxx eyyy yyzz zzzz (uv: texture coords; xyz: position; e: lower/upper end)
                                int upperDataA = (0 << 31) | (0 << 30) | (x + 0 << 12) | (1 << 11) | (y << 6) | (z + 1);
                                int upperDataB = (0 << 31) | (1 << 30) | (x + 0 << 12) | (1 << 11) | (y << 6) | (z + 0);
                                int upperDataC = (1 << 31) | (1 << 30) | (x + 1 << 12) | (1 << 11) | (y << 6) | (z + 0);
                                int upperDataD = (1 << 31) | (0 << 30) | (x + 1 << 12) | (1 << 11) | (y << 6) | (z + 1);

                                // int: tttt tttt t--- -nnn hhhh dlll siii iiii (t: tint; n: normal; h: side height; d: direction; l: level; s: isStatic; i: texture index)
                                int lowerData = (((tint.IsNeutral) ? liquidTint.ToBits : tint.ToBits) << 23) | ((int)BlockSide.Top << 16) | ((sideHeight + 1) << 12) | ((currentLiquid.Direction > 0 ? 0 : 1) << 11) | ((int)level << 8) | (isStatic ? (1 << 7) : (0 << 7)) | ((((textureIndex - 1) >> 4) + 1) & 0b0111_1111);

                                liquidTopFaceHolder.AddFace(y, x, z, lowerData, (upperDataA, upperDataB, upperDataC, upperDataD), singleSided, isFull);
                            }
                        }
                    }
                }
            }

            // Build the simple mesh data.
            PooledList<int> simpleVertexData = new PooledList<int>(4096);

            simpleFrontFaceHolder.GenerateMesh(ref simpleVertexData);
            simpleBackFaceHolder.GenerateMesh(ref simpleVertexData);
            simpleLeftFaceHolder.GenerateMesh(ref simpleVertexData);
            simpleRightFaceHolder.GenerateMesh(ref simpleVertexData);
            simpleBottomFaceHolder.GenerateMesh(ref simpleVertexData);
            simpleTopFaceHolder.GenerateMesh(ref simpleVertexData);

            // Build the liquid mesh data.
            PooledList<int> liquidVertexData = new PooledList<int>();
            PooledList<uint> liquidIndices = new PooledList<uint>();
            uint liquidVertCount = 0;

            liquidFrontFaceHolder.GenerateMesh(ref liquidVertexData, ref liquidVertCount, ref liquidIndices);
            liquidBackFaceHolder.GenerateMesh(ref liquidVertexData, ref liquidVertCount, ref liquidIndices);
            liquidLeftFaceHolder.GenerateMesh(ref liquidVertexData, ref liquidVertCount, ref liquidIndices);
            liquidRightFaceHolder.GenerateMesh(ref liquidVertexData, ref liquidVertCount, ref liquidIndices);
            liquidBottomFaceHolder.GenerateMesh(ref liquidVertexData, ref liquidVertCount, ref liquidIndices);
            liquidTopFaceHolder.GenerateMesh(ref liquidVertexData, ref liquidVertCount, ref liquidIndices);

            hasMesh = complexVertexPositions.Count != 0 || simpleVertexData.Count != 0 || liquidVertexData.Count != 0;

            meshData = new SectionMeshData(ref simpleVertexData, ref complexVertexPositions, ref complexVertexData, ref complexIndices, ref liquidVertexData, ref liquidIndices);

            simpleFrontFaceHolder.ReturnToPool();
            simpleBackFaceHolder.ReturnToPool();
            simpleLeftFaceHolder.ReturnToPool();
            simpleRightFaceHolder.ReturnToPool();
            simpleBottomFaceHolder.ReturnToPool();
            simpleTopFaceHolder.ReturnToPool();

            liquidFrontFaceHolder.ReturnToPool();
            liquidBackFaceHolder.ReturnToPool();
            liquidLeftFaceHolder.ReturnToPool();
            liquidRightFaceHolder.ReturnToPool();
            liquidBottomFaceHolder.ReturnToPool();
            liquidTopFaceHolder.ReturnToPool();
        }

        public void SetMeshData(ref SectionMeshData meshData)
        {
            renderer?.SetData(ref meshData);
        }

        public void Render(Vector3 position)
        {
            if (hasMesh)
            {
                renderer?.Draw(position);
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