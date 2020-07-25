// <copyright file="Section.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using OpenToolkit.Mathematics;
using System;
using System.Runtime.CompilerServices;
using VoxelGame.Collections;
using VoxelGame.Rendering;
using VoxelGame.Visuals;

namespace VoxelGame.Logic
{
    [Serializable]
    public class Section : IDisposable
    {
        public const int SectionSize = 32;
        public const int TickBatchSize = 4;

        public const int BlockMask = 0b0000_0000_0000_0000_0000_0111_1111_1111;
        public const int DataMask = 0b0000_0000_0000_0000_1111_1000_0000_0000;

        private readonly ushort[] blocks;

        [NonSerialized] private bool isEmpty;
        [NonSerialized] private SectionRenderer? renderer;

        public Section()
        {
            blocks = new ushort[SectionSize * SectionSize * SectionSize];

            Setup();
        }

        /// <summary>
        /// Sets up all non serialized members.
        /// </summary>
        public void Setup()
        {
            renderer = new SectionRenderer();
        }

        public static Vector3 Extents { get => new Vector3(SectionSize / 2f, SectionSize / 2f, SectionSize / 2f); }

        public void CreateAndSetMesh(int sectionX, int sectionY, int sectionZ)
        {
            CreateMeshData(sectionX, sectionY, sectionZ, out SectionMeshData meshData);
            SetMeshData(ref meshData);
        }

        public void CreateMeshData(int sectionX, int sectionY, int sectionZ, out SectionMeshData meshData)
        {
            // Set the neutral tint color
            TintColor neutral = new TintColor(0f, 1f, 0f);

            // Get the sections next to this section
            Section? frontNeighbour = Game.World.GetSection(sectionX, sectionY, sectionZ + 1);
            Section? backNeighbour = Game.World.GetSection(sectionX, sectionY, sectionZ - 1);
            Section? leftNeighbour = Game.World.GetSection(sectionX - 1, sectionY, sectionZ);
            Section? rightNeighbour = Game.World.GetSection(sectionX + 1, sectionY, sectionZ);
            Section? bottomNeighbour = Game.World.GetSection(sectionX, sectionY - 1, sectionZ);
            Section? topNeighbour = Game.World.GetSection(sectionX, sectionY + 1, sectionZ);

            CompactedMeshFaceHolder simpleFrontFaceHolder = new CompactedMeshFaceHolder(BlockSide.Front);
            CompactedMeshFaceHolder simpleBackFaceHolder = new CompactedMeshFaceHolder(BlockSide.Back);
            CompactedMeshFaceHolder simpleLeftFaceHolder = new CompactedMeshFaceHolder(BlockSide.Left);
            CompactedMeshFaceHolder simpleRightFaceHolder = new CompactedMeshFaceHolder(BlockSide.Right);
            CompactedMeshFaceHolder simpleBottomFaceHolder = new CompactedMeshFaceHolder(BlockSide.Bottom);
            CompactedMeshFaceHolder simpleTopFaceHolder = new CompactedMeshFaceHolder(BlockSide.Top);

            PooledList<float> complexVertexPositions = new PooledList<float>(64);
            PooledList<int> complexVertexData = new PooledList<int>(32);
            PooledList<uint> complexIndices = new PooledList<uint>(16);

            uint complexVertCount = 0;

            // Loop through the section
            for (int x = 0; x < SectionSize; x++)
            {
                for (int y = 0; y < SectionSize; y++)
                {
                    for (int z = 0; z < SectionSize; z++)
                    {
                        ushort currentBlockData = blocks[(x << 10) + (y << 5) + z];

                        Block currentBlock = Block.TranslateID((ushort)(currentBlockData & BlockMask));
                        byte currentData = (byte)((currentBlockData & DataMask) >> 11);

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
                                currentBlock.GetMesh(BlockSide.Front, currentData, out float[] vertices, out int[] textureIndices, out _, out TintColor tint, out bool isAnimated);

                                // int: uv-- ---- ---- --xx xxxx yyyy yyzz zzzz (uv: texture coords; xyz: position)
                                int upperDataA = (((int)vertices[(0 * 8) + 3]) << 31) | (((int)vertices[(0 * 8) + 4]) << 30) | (((int)vertices[(0 * 8) + 0] + x) << 12) | (((int)vertices[(0 * 8) + 1] + y) << 6) | ((int)vertices[(0 * 8) + 2] + z);
                                int upperDataB = (((int)vertices[(1 * 8) + 3]) << 31) | (((int)vertices[(1 * 8) + 4]) << 30) | (((int)vertices[(1 * 8) + 0] + x) << 12) | (((int)vertices[(1 * 8) + 1] + y) << 6) | ((int)vertices[(1 * 8) + 2] + z);
                                int upperDataC = (((int)vertices[(2 * 8) + 3]) << 31) | (((int)vertices[(2 * 8) + 4]) << 30) | (((int)vertices[(2 * 8) + 0] + x) << 12) | (((int)vertices[(2 * 8) + 1] + y) << 6) | ((int)vertices[(2 * 8) + 2] + z);
                                int upperDataD = (((int)vertices[(3 * 8) + 3]) << 31) | (((int)vertices[(3 * 8) + 4]) << 30) | (((int)vertices[(3 * 8) + 0] + x) << 12) | (((int)vertices[(3 * 8) + 1] + y) << 6) | ((int)vertices[(3 * 8) + 2] + z);

                                // int: tttt tttt t--n nn-a ---- iiii iiii iiii (t: tint; n: normal; a: animated; i: texture index)
                                int lowerData = (((tint.IsNeutral) ? neutral.ToBits : tint.ToBits) << 23) | ((int)BlockSide.Front << 18) | (isAnimated && textureIndices[0] != 0 ? (1 << 16) : 0) | textureIndices[0];

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
                                currentBlock.GetMesh(BlockSide.Back, currentData, out float[] vertices, out int[] textureIndices, out _, out TintColor tint, out bool isAnimated);

                                // int: uv-- ---- ---- --xx xxxx yyyy yyzz zzzz (uv: texture coords; xyz: position)
                                int upperDataA = (((int)vertices[(0 * 8) + 3]) << 31) | (((int)vertices[(0 * 8) + 4]) << 30) | (((int)vertices[(0 * 8) + 0] + x) << 12) | (((int)vertices[(0 * 8) + 1] + y) << 6) | ((int)vertices[(0 * 8) + 2] + z);
                                int upperDataB = (((int)vertices[(1 * 8) + 3]) << 31) | (((int)vertices[(1 * 8) + 4]) << 30) | (((int)vertices[(1 * 8) + 0] + x) << 12) | (((int)vertices[(1 * 8) + 1] + y) << 6) | ((int)vertices[(1 * 8) + 2] + z);
                                int upperDataC = (((int)vertices[(2 * 8) + 3]) << 31) | (((int)vertices[(2 * 8) + 4]) << 30) | (((int)vertices[(2 * 8) + 0] + x) << 12) | (((int)vertices[(2 * 8) + 1] + y) << 6) | ((int)vertices[(2 * 8) + 2] + z);
                                int upperDataD = (((int)vertices[(3 * 8) + 3]) << 31) | (((int)vertices[(3 * 8) + 4]) << 30) | (((int)vertices[(3 * 8) + 0] + x) << 12) | (((int)vertices[(3 * 8) + 1] + y) << 6) | ((int)vertices[(3 * 8) + 2] + z);

                                // int: tttt tttt t--n nn-a ---- iiii iiii iiii (t: tint; n: normal; a: animated; i: texture index)
                                int lowerData = (((tint.IsNeutral) ? neutral.ToBits : tint.ToBits) << 23) | ((int)BlockSide.Back << 18) | (isAnimated && textureIndices[0] != 0 ? (1 << 16) : 0) | textureIndices[0];

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
                                currentBlock.GetMesh(BlockSide.Left, currentData, out float[] vertices, out int[] textureIndices, out _, out TintColor tint, out bool isAnimated);

                                // int: uv-- ---- ---- --xx xxxx yyyy yyzz zzzz (uv: texture coords; xyz: position)
                                int upperDataA = (((int)vertices[(0 * 8) + 3]) << 31) | (((int)vertices[(0 * 8) + 4]) << 30) | (((int)vertices[(0 * 8) + 0] + x) << 12) | (((int)vertices[(0 * 8) + 1] + y) << 6) | ((int)vertices[(0 * 8) + 2] + z);
                                int upperDataB = (((int)vertices[(1 * 8) + 3]) << 31) | (((int)vertices[(1 * 8) + 4]) << 30) | (((int)vertices[(1 * 8) + 0] + x) << 12) | (((int)vertices[(1 * 8) + 1] + y) << 6) | ((int)vertices[(1 * 8) + 2] + z);
                                int upperDataC = (((int)vertices[(2 * 8) + 3]) << 31) | (((int)vertices[(2 * 8) + 4]) << 30) | (((int)vertices[(2 * 8) + 0] + x) << 12) | (((int)vertices[(2 * 8) + 1] + y) << 6) | ((int)vertices[(2 * 8) + 2] + z);
                                int upperDataD = (((int)vertices[(3 * 8) + 3]) << 31) | (((int)vertices[(3 * 8) + 4]) << 30) | (((int)vertices[(3 * 8) + 0] + x) << 12) | (((int)vertices[(3 * 8) + 1] + y) << 6) | ((int)vertices[(3 * 8) + 2] + z);

                                // int: tttt tttt t--n nn-a ---- iiii iiii iiii (t: tint; n: normal; a: animated; i: texture index)
                                int lowerData = (((tint.IsNeutral) ? neutral.ToBits : tint.ToBits) << 23) | ((int)BlockSide.Left << 18) | (isAnimated && textureIndices[0] != 0 ? (1 << 16) : 0) | textureIndices[0];

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
                                currentBlock.GetMesh(BlockSide.Right, currentData, out float[] vertices, out int[] textureIndices, out _, out TintColor tint, out bool isAnimated);

                                // int: uv-- ---- ---- --xx xxxx yyyy yyzz zzzz (uv: texture coords; xyz: position)
                                int upperDataA = (((int)vertices[(0 * 8) + 3]) << 31) | (((int)vertices[(0 * 8) + 4]) << 30) | (((int)vertices[(0 * 8) + 0] + x) << 12) | (((int)vertices[(0 * 8) + 1] + y) << 6) | ((int)vertices[(0 * 8) + 2] + z);
                                int upperDataB = (((int)vertices[(1 * 8) + 3]) << 31) | (((int)vertices[(1 * 8) + 4]) << 30) | (((int)vertices[(1 * 8) + 0] + x) << 12) | (((int)vertices[(1 * 8) + 1] + y) << 6) | ((int)vertices[(1 * 8) + 2] + z);
                                int upperDataC = (((int)vertices[(2 * 8) + 3]) << 31) | (((int)vertices[(2 * 8) + 4]) << 30) | (((int)vertices[(2 * 8) + 0] + x) << 12) | (((int)vertices[(2 * 8) + 1] + y) << 6) | ((int)vertices[(2 * 8) + 2] + z);
                                int upperDataD = (((int)vertices[(3 * 8) + 3]) << 31) | (((int)vertices[(3 * 8) + 4]) << 30) | (((int)vertices[(3 * 8) + 0] + x) << 12) | (((int)vertices[(3 * 8) + 1] + y) << 6) | ((int)vertices[(3 * 8) + 2] + z);

                                // int: tttt tttt t--n nn-a ---- iiii iiii iiii (t: tint; n: normal; a: animated; i: texture index)
                                int lowerData = (((tint.IsNeutral) ? neutral.ToBits : tint.ToBits) << 23) | ((int)BlockSide.Right << 18) | (isAnimated && textureIndices[0] != 0 ? (1 << 16) : 0) | textureIndices[0];

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
                                currentBlock.GetMesh(BlockSide.Bottom, currentData, out float[] vertices, out int[] textureIndices, out _, out TintColor tint, out bool isAnimated);

                                // int: uv-- ---- ---- --xx xxxx yyyy yyzz zzzz (uv: texture coords; xyz: position)
                                int upperDataA = (((int)vertices[(0 * 8) + 3]) << 31) | (((int)vertices[(0 * 8) + 4]) << 30) | (((int)vertices[(0 * 8) + 0] + x) << 12) | (((int)vertices[(0 * 8) + 1] + y) << 6) | ((int)vertices[(0 * 8) + 2] + z);
                                int upperDataB = (((int)vertices[(1 * 8) + 3]) << 31) | (((int)vertices[(1 * 8) + 4]) << 30) | (((int)vertices[(1 * 8) + 0] + x) << 12) | (((int)vertices[(1 * 8) + 1] + y) << 6) | ((int)vertices[(1 * 8) + 2] + z);
                                int upperDataC = (((int)vertices[(2 * 8) + 3]) << 31) | (((int)vertices[(2 * 8) + 4]) << 30) | (((int)vertices[(2 * 8) + 0] + x) << 12) | (((int)vertices[(2 * 8) + 1] + y) << 6) | ((int)vertices[(2 * 8) + 2] + z);
                                int upperDataD = (((int)vertices[(3 * 8) + 3]) << 31) | (((int)vertices[(3 * 8) + 4]) << 30) | (((int)vertices[(3 * 8) + 0] + x) << 12) | (((int)vertices[(3 * 8) + 1] + y) << 6) | ((int)vertices[(3 * 8) + 2] + z);

                                // int: tttt tttt t--n nn-a ---- iiii iiii iiii (t: tint; n: normal; a: animated; i: texture index)
                                int lowerData = (((tint.IsNeutral) ? neutral.ToBits : tint.ToBits) << 23) | ((int)BlockSide.Bottom << 18) | (isAnimated && textureIndices[0] != 0 ? (1 << 16) : 0) | textureIndices[0];

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
                                currentBlock.GetMesh(BlockSide.Top, currentData, out float[] vertices, out int[] textureIndices, out _, out TintColor tint, out bool isAnimated);

                                // int: uv-- ---- ---- --xx xxxx yyyy yyzz zzzz (uv: texture coords; xyz: position)
                                int upperDataA = (((int)vertices[(0 * 8) + 3]) << 31) | (((int)vertices[(0 * 8) + 4]) << 30) | (((int)vertices[(0 * 8) + 0] + x) << 12) | (((int)vertices[(0 * 8) + 1] + y) << 6) | ((int)vertices[(0 * 8) + 2] + z);
                                int upperDataB = (((int)vertices[(1 * 8) + 3]) << 31) | (((int)vertices[(1 * 8) + 4]) << 30) | (((int)vertices[(1 * 8) + 0] + x) << 12) | (((int)vertices[(1 * 8) + 1] + y) << 6) | ((int)vertices[(1 * 8) + 2] + z);
                                int upperDataC = (((int)vertices[(2 * 8) + 3]) << 31) | (((int)vertices[(2 * 8) + 4]) << 30) | (((int)vertices[(2 * 8) + 0] + x) << 12) | (((int)vertices[(2 * 8) + 1] + y) << 6) | ((int)vertices[(2 * 8) + 2] + z);
                                int upperDataD = (((int)vertices[(3 * 8) + 3]) << 31) | (((int)vertices[(3 * 8) + 4]) << 30) | (((int)vertices[(3 * 8) + 0] + x) << 12) | (((int)vertices[(3 * 8) + 1] + y) << 6) | ((int)vertices[(3 * 8) + 2] + z);

                                // int: tttt tttt t--n nn-a ---- iiii iiii iiii (t: tint; n: normal; a: animated; i: texture index)
                                int lowerData = (((tint.IsNeutral) ? neutral.ToBits : tint.ToBits) << 23) | ((int)BlockSide.Top << 18) | (isAnimated && textureIndices[0] != 0 ? (1 << 16) : 0) | textureIndices[0];

                                simpleTopFaceHolder.AddFace(y, x, z, lowerData, (upperDataA, upperDataB, upperDataC, upperDataD));
                            }
                        }
                        else if (currentBlock.TargetBuffer == TargetBuffer.Complex)
                        {
                            uint verts = currentBlock.GetMesh(BlockSide.All, currentData, out float[] vertices, out int[] textureIndices, out uint[] indices, out TintColor tint, out bool isAnimated);

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

                                // int: tttt tttt t--- ---a ---- iiii iiii iiii(t: tint; a: animated; i: texture index)
                                int lowerData = (((tint.IsNeutral) ? neutral.ToBits : tint.ToBits) << 23) | (isAnimated && textureIndices[i] != 0 ? (1 << 16) : 0) | textureIndices[i];
                                complexVertexData.Add(lowerData);
                            }

                            for (int i = complexIndices.Count - indices.Length; i < complexIndices.Count; i++)
                            {
                                complexIndices[i] += complexVertCount;
                            }

                            complexVertCount += verts;
                        }
                    }
                }
            }

            // Build the simple mesh data
            PooledList<int> simpleVertexData = new PooledList<int>(4096);

            simpleFrontFaceHolder.GenerateMesh(ref simpleVertexData);
            simpleBackFaceHolder.GenerateMesh(ref simpleVertexData);
            simpleLeftFaceHolder.GenerateMesh(ref simpleVertexData);
            simpleRightFaceHolder.GenerateMesh(ref simpleVertexData);
            simpleBottomFaceHolder.GenerateMesh(ref simpleVertexData);
            simpleTopFaceHolder.GenerateMesh(ref simpleVertexData);

            isEmpty = complexVertexPositions.Count == 0 && simpleVertexData.Count == 0;

            meshData = new SectionMeshData(ref simpleVertexData, ref complexVertexPositions, ref complexVertexData, ref complexIndices);

            simpleFrontFaceHolder.ReturnToPool();
            simpleBackFaceHolder.ReturnToPool();
            simpleLeftFaceHolder.ReturnToPool();
            simpleRightFaceHolder.ReturnToPool();
            simpleBottomFaceHolder.ReturnToPool();
            simpleTopFaceHolder.ReturnToPool();
        }

        public void SetMeshData(ref SectionMeshData meshData)
        {
            renderer?.SetData(ref meshData);
        }

        public void Render(Vector3 position)
        {
            if (!isEmpty)
            {
                renderer?.Draw(position);
            }
        }

        public void Tick(int sectionX, int sectionY, int sectionZ)
        {
            for (int i = 0; i < TickBatchSize; i++)
            {
                int index = Game.Random.Next(0, SectionSize * SectionSize * SectionSize);
                ushort val = blocks[index];

                int z = index & 31;
                index = (index - z) >> 5;
                int y = index & 31;
                index = (index - y) >> 5;
                int x = index;

                Block.TranslateID((ushort)(val & BlockMask))?.RandomUpdate(x + (sectionX * SectionSize), y + (sectionY * SectionSize), z + (sectionZ * SectionSize), (byte)((val & DataMask) >> 11));
            }
        }

        /// <summary>
        /// Gets or sets the block at a section position.
        /// </summary>
        /// <param name="x">The x position of the block in this section.</param>
        /// <param name="y">The y position of the block in this section.</param>
        /// <param name="z">The z position of the block in this section.</param>
        /// <returns>The block at the given position.</returns>
        public ushort this[int x, int y, int z]
        {
            get
            {
                return blocks[(x << 10) + (y << 5) + z];
            }

            set
            {
                blocks[(x << 10) + (y << 5) + z] = value;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Block GetBlock(int x, int y, int z)
        {
            return Block.TranslateID((ushort)(this[x, y, z] & BlockMask));
        }

        #region IDisposable Support

        [NonSerialized] private bool disposed = false;

        protected virtual void Dispose(bool disposing)
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

        ~Section()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable Support
    }
}