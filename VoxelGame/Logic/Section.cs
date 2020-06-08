// <copyright file="Section.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using OpenTK;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using VoxelGame.Collections;
using VoxelGame.Rendering;
using VoxelGame.WorldGeneration;

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
        [NonSerialized] private SectionRenderer renderer;

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

        public void Generate(IWorldGenerator generator, int xOffset, int yOffset, int zOffset)
        {
            if (generator == null)
            {
                throw new ArgumentNullException(paramName: nameof(generator));
            }

            for (int x = 0; x < SectionSize; x++)
            {
                for (int y = 0; y < SectionSize; y++)
                {
                    for (int z = 0; z < SectionSize; z++)
                    {
                        blocks[(x << 10) + (y << 5) + z] = generator.GenerateBlock(x + xOffset, y + yOffset, z + zOffset).Id;
                    }
                }
            }
        }

        public void CreateMesh(int sectionX, int sectionY, int sectionZ)
        {
            CreateMeshData(sectionX, sectionY, sectionZ, out SectionMeshData meshData);
            SetMeshData(ref meshData);
        }

        public void CreateMeshData(int sectionX, int sectionY, int sectionZ, out SectionMeshData meshData)
        {
            // Set the neutral tint color
            TintColor neutral = new TintColor(0f, 1f, 0f);

            // Get the sections next to this section
            Section frontNeighbour = Game.World.GetSection(sectionX, sectionY, sectionZ + 1);
            Section backNeighbour = Game.World.GetSection(sectionX, sectionY, sectionZ - 1);
            Section leftNeighbour = Game.World.GetSection(sectionX - 1, sectionY, sectionZ);
            Section rightNeighbour = Game.World.GetSection(sectionX + 1, sectionY, sectionZ);
            Section bottomNeighbour = Game.World.GetSection(sectionX, sectionY - 1, sectionZ);
            Section topNeighbour = Game.World.GetSection(sectionX, sectionY + 1, sectionZ);

            var simpleFrontFaces = new PooledList<PooledList<PooledList<SimpleFace>>>();
            var simpleBackFaces = new PooledList<PooledList<PooledList<SimpleFace>>>();
            var simpleLeftFaces = new PooledList<PooledList<PooledList<SimpleFace>>>();
            var simpleRightFaces = new PooledList<PooledList<PooledList<SimpleFace>>>();
            var simpleBottomFaces = new PooledList<PooledList<PooledList<SimpleFace>>>();
            var simpleTopFaces = new PooledList<PooledList<PooledList<SimpleFace>>>();

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
                            Block blockToCheck;

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
                                currentBlock.GetMesh(BlockSide.Front, currentData, out float[] vertices, out int[] textureIndices, out _, out TintColor tint);

                                // int: uv-- ---- ---- --xx xxxx yyyy yyzz zzzz (uv: texture coords; xyz: position)
                                int upperDataA = (((int)vertices[(0 * 8) + 3]) << 31) | (((int)vertices[(0 * 8) + 4]) << 30) | (((int)vertices[(0 * 8) + 0] + x) << 12) | (((int)vertices[(0 * 8) + 1] + y) << 6) | ((int)vertices[(0 * 8) + 2] + z);
                                int upperDataB = (((int)vertices[(1 * 8) + 3]) << 31) | (((int)vertices[(1 * 8) + 4]) << 30) | (((int)vertices[(1 * 8) + 0] + x) << 12) | (((int)vertices[(1 * 8) + 1] + y) << 6) | ((int)vertices[(1 * 8) + 2] + z);
                                int upperDataC = (((int)vertices[(2 * 8) + 3]) << 31) | (((int)vertices[(2 * 8) + 4]) << 30) | (((int)vertices[(2 * 8) + 0] + x) << 12) | (((int)vertices[(2 * 8) + 1] + y) << 6) | ((int)vertices[(2 * 8) + 2] + z);
                                int upperDataD = (((int)vertices[(3 * 8) + 3]) << 31) | (((int)vertices[(3 * 8) + 4]) << 30) | (((int)vertices[(3 * 8) + 0] + x) << 12) | (((int)vertices[(3 * 8) + 1] + y) << 6) | ((int)vertices[(3 * 8) + 2] + z);

                                // int: tttt tttt t--n nn-- ---- iiii iiii iiii (t: tint; n: normal; i: texture index)
                                int lowerData = (((tint.IsNeutral) ? neutral.ToBits : tint.ToBits) << 23) | ((int)BlockSide.Front << 18) | textureIndices[0];

                                while (simpleFrontFaces.Count <= z)
                                {
                                    simpleFrontFaces.Add(new PooledList<PooledList<SimpleFace>>());
                                }

                                while (simpleFrontFaces[z].Count <= y)
                                {
                                    simpleFrontFaces[z].Add(new PooledList<SimpleFace>());
                                }

                                if (simpleFrontFaces[z][y].Count == 0 || !simpleFrontFaces[z][y][simpleFrontFaces[z][y].Count - 1].TryAdd(x, upperDataA, upperDataB, upperDataC, upperDataD, lowerData, out SimpleFace extendedFace))
                                {
                                    simpleFrontFaces[z][y].Add(new SimpleFace(x, upperDataA, upperDataB, upperDataC, upperDataD, false, lowerData));
                                }
                                else
                                {
                                    simpleFrontFaces[z][y][simpleFrontFaces[z][y].Count - 1] = extendedFace;
                                }
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
                                currentBlock.GetMesh(BlockSide.Back, currentData, out float[] vertices, out int[] textureIndices, out _, out TintColor tint);

                                // int: uv-- ---- ---- --xx xxxx yyyy yyzz zzzz (uv: texture coords; xyz: position)
                                int upperDataA = (((int)vertices[(0 * 8) + 3]) << 31) | (((int)vertices[(0 * 8) + 4]) << 30) | (((int)vertices[(0 * 8) + 0] + x) << 12) | (((int)vertices[(0 * 8) + 1] + y) << 6) | ((int)vertices[(0 * 8) + 2] + z);
                                int upperDataB = (((int)vertices[(1 * 8) + 3]) << 31) | (((int)vertices[(1 * 8) + 4]) << 30) | (((int)vertices[(1 * 8) + 0] + x) << 12) | (((int)vertices[(1 * 8) + 1] + y) << 6) | ((int)vertices[(1 * 8) + 2] + z);
                                int upperDataC = (((int)vertices[(2 * 8) + 3]) << 31) | (((int)vertices[(2 * 8) + 4]) << 30) | (((int)vertices[(2 * 8) + 0] + x) << 12) | (((int)vertices[(2 * 8) + 1] + y) << 6) | ((int)vertices[(2 * 8) + 2] + z);
                                int upperDataD = (((int)vertices[(3 * 8) + 3]) << 31) | (((int)vertices[(3 * 8) + 4]) << 30) | (((int)vertices[(3 * 8) + 0] + x) << 12) | (((int)vertices[(3 * 8) + 1] + y) << 6) | ((int)vertices[(3 * 8) + 2] + z);

                                // int: tttt tttt t--n nn-- ---- iiii iiii iiii (t: tint; n: normal; i: texture index)
                                int lowerData = (((tint.IsNeutral) ? neutral.ToBits : tint.ToBits) << 23) | ((int)BlockSide.Back << 18) | textureIndices[0];

                                while (simpleBackFaces.Count <= z)
                                {
                                    simpleBackFaces.Add(new PooledList<PooledList<SimpleFace>>());
                                }

                                while (simpleBackFaces[z].Count <= y)
                                {
                                    simpleBackFaces[z].Add(new PooledList<SimpleFace>());
                                }

                                if (simpleBackFaces[z][y].Count == 0 || !simpleBackFaces[z][y][simpleBackFaces[z][y].Count - 1].TryAdd(x, upperDataA, upperDataB, upperDataC, upperDataD, lowerData, out SimpleFace extendedFace))
                                {
                                    simpleBackFaces[z][y].Add(new SimpleFace(x, upperDataA, upperDataB, upperDataC, upperDataD, true, lowerData));
                                }
                                else
                                {
                                    simpleBackFaces[z][y][simpleBackFaces[z][y].Count - 1] = extendedFace;
                                }
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
                                currentBlock.GetMesh(BlockSide.Left, currentData, out float[] vertices, out int[] textureIndices, out _, out TintColor tint);

                                // int: uv-- ---- ---- --xx xxxx yyyy yyzz zzzz (uv: texture coords; xyz: position)
                                int upperDataA = (((int)vertices[(0 * 8) + 3]) << 31) | (((int)vertices[(0 * 8) + 4]) << 30) | (((int)vertices[(0 * 8) + 0] + x) << 12) | (((int)vertices[(0 * 8) + 1] + y) << 6) | ((int)vertices[(0 * 8) + 2] + z);
                                int upperDataB = (((int)vertices[(1 * 8) + 3]) << 31) | (((int)vertices[(1 * 8) + 4]) << 30) | (((int)vertices[(1 * 8) + 0] + x) << 12) | (((int)vertices[(1 * 8) + 1] + y) << 6) | ((int)vertices[(1 * 8) + 2] + z);
                                int upperDataC = (((int)vertices[(2 * 8) + 3]) << 31) | (((int)vertices[(2 * 8) + 4]) << 30) | (((int)vertices[(2 * 8) + 0] + x) << 12) | (((int)vertices[(2 * 8) + 1] + y) << 6) | ((int)vertices[(2 * 8) + 2] + z);
                                int upperDataD = (((int)vertices[(3 * 8) + 3]) << 31) | (((int)vertices[(3 * 8) + 4]) << 30) | (((int)vertices[(3 * 8) + 0] + x) << 12) | (((int)vertices[(3 * 8) + 1] + y) << 6) | ((int)vertices[(3 * 8) + 2] + z);

                                // int: tttt tttt t--n nn-- ---- iiii iiii iiii (t: tint; n: normal; i: texture index)
                                int lowerData = (((tint.IsNeutral) ? neutral.ToBits : tint.ToBits) << 23) | ((int)BlockSide.Left << 18) | textureIndices[0];

                                while (simpleLeftFaces.Count <= x)
                                {
                                    simpleLeftFaces.Add(new PooledList<PooledList<SimpleFace>>());
                                }

                                while (simpleLeftFaces[x].Count <= y)
                                {
                                    simpleLeftFaces[x].Add(new PooledList<SimpleFace>());
                                }

                                if (simpleLeftFaces[x][y].Count == 0 || !simpleLeftFaces[x][y][simpleLeftFaces[x][y].Count - 1].TryAdd(z, upperDataA, upperDataB, upperDataC, upperDataD, lowerData, out SimpleFace extendedFace))
                                {
                                    simpleLeftFaces[x][y].Add(new SimpleFace(z, upperDataA, upperDataB, upperDataC, upperDataD, false, lowerData));
                                }
                                else
                                {
                                    simpleLeftFaces[x][y][simpleLeftFaces[x][y].Count - 1] = extendedFace;
                                }
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
                                currentBlock.GetMesh(BlockSide.Right, currentData, out float[] vertices, out int[] textureIndices, out _, out TintColor tint);

                                // int: uv-- ---- ---- --xx xxxx yyyy yyzz zzzz (uv: texture coords; xyz: position)
                                int upperDataA = (((int)vertices[(0 * 8) + 3]) << 31) | (((int)vertices[(0 * 8) + 4]) << 30) | (((int)vertices[(0 * 8) + 0] + x) << 12) | (((int)vertices[(0 * 8) + 1] + y) << 6) | ((int)vertices[(0 * 8) + 2] + z);
                                int upperDataB = (((int)vertices[(1 * 8) + 3]) << 31) | (((int)vertices[(1 * 8) + 4]) << 30) | (((int)vertices[(1 * 8) + 0] + x) << 12) | (((int)vertices[(1 * 8) + 1] + y) << 6) | ((int)vertices[(1 * 8) + 2] + z);
                                int upperDataC = (((int)vertices[(2 * 8) + 3]) << 31) | (((int)vertices[(2 * 8) + 4]) << 30) | (((int)vertices[(2 * 8) + 0] + x) << 12) | (((int)vertices[(2 * 8) + 1] + y) << 6) | ((int)vertices[(2 * 8) + 2] + z);
                                int upperDataD = (((int)vertices[(3 * 8) + 3]) << 31) | (((int)vertices[(3 * 8) + 4]) << 30) | (((int)vertices[(3 * 8) + 0] + x) << 12) | (((int)vertices[(3 * 8) + 1] + y) << 6) | ((int)vertices[(3 * 8) + 2] + z);

                                // int: tttt tttt t--n nn-- ---- iiii iiii iiii (t: tint; n: normal; i: texture index)
                                int lowerData = (((tint.IsNeutral) ? neutral.ToBits : tint.ToBits) << 23) | ((int)BlockSide.Right << 18) | textureIndices[0];

                                while (simpleRightFaces.Count <= x)
                                {
                                    simpleRightFaces.Add(new PooledList<PooledList<SimpleFace>>());
                                }

                                while (simpleRightFaces[x].Count <= y)
                                {
                                    simpleRightFaces[x].Add(new PooledList<SimpleFace>());
                                }

                                if (simpleRightFaces[x][y].Count == 0 || !simpleRightFaces[x][y][simpleRightFaces[x][y].Count - 1].TryAdd(z, upperDataA, upperDataB, upperDataC, upperDataD, lowerData, out SimpleFace extendedFace))
                                {
                                    simpleRightFaces[x][y].Add(new SimpleFace(z, upperDataA, upperDataB, upperDataC, upperDataD, true, lowerData));
                                }
                                else
                                {
                                    simpleRightFaces[x][y][simpleRightFaces[x][y].Count - 1] = extendedFace;
                                }
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
                                currentBlock.GetMesh(BlockSide.Bottom, currentData, out float[] vertices, out int[] textureIndices, out _, out TintColor tint);

                                // int: uv-- ---- ---- --xx xxxx yyyy yyzz zzzz (uv: texture coords; xyz: position)
                                int upperDataA = (((int)vertices[(0 * 8) + 3]) << 31) | (((int)vertices[(0 * 8) + 4]) << 30) | (((int)vertices[(0 * 8) + 0] + x) << 12) | (((int)vertices[(0 * 8) + 1] + y) << 6) | ((int)vertices[(0 * 8) + 2] + z);
                                int upperDataB = (((int)vertices[(1 * 8) + 3]) << 31) | (((int)vertices[(1 * 8) + 4]) << 30) | (((int)vertices[(1 * 8) + 0] + x) << 12) | (((int)vertices[(1 * 8) + 1] + y) << 6) | ((int)vertices[(1 * 8) + 2] + z);
                                int upperDataC = (((int)vertices[(2 * 8) + 3]) << 31) | (((int)vertices[(2 * 8) + 4]) << 30) | (((int)vertices[(2 * 8) + 0] + x) << 12) | (((int)vertices[(2 * 8) + 1] + y) << 6) | ((int)vertices[(2 * 8) + 2] + z);
                                int upperDataD = (((int)vertices[(3 * 8) + 3]) << 31) | (((int)vertices[(3 * 8) + 4]) << 30) | (((int)vertices[(3 * 8) + 0] + x) << 12) | (((int)vertices[(3 * 8) + 1] + y) << 6) | ((int)vertices[(3 * 8) + 2] + z);

                                // int: tttt tttt t--n nn-- ---- iiii iiii iiii (t: tint; n: normal; i: texture index)
                                int lowerData = (((tint.IsNeutral) ? neutral.ToBits : tint.ToBits) << 23) | ((int)BlockSide.Bottom << 18) | textureIndices[0];

                                while (simpleBottomFaces.Count <= y)
                                {
                                    simpleBottomFaces.Add(new PooledList<PooledList<SimpleFace>>());
                                }

                                while (simpleBottomFaces[y].Count <= z)
                                {
                                    simpleBottomFaces[y].Add(new PooledList<SimpleFace>());
                                }

                                if (simpleBottomFaces[y][z].Count == 0 || !simpleBottomFaces[y][z][simpleBottomFaces[y][z].Count - 1].TryAdd(x, upperDataA, upperDataB, upperDataC, upperDataD, lowerData, out SimpleFace extendedFace))
                                {
                                    simpleBottomFaces[y][z].Add(new SimpleFace(x, upperDataA, upperDataB, upperDataC, upperDataD, false, lowerData));
                                }
                                else
                                {
                                    simpleBottomFaces[y][z][simpleBottomFaces[y][z].Count - 1] = extendedFace;
                                }
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
                                currentBlock.GetMesh(BlockSide.Top, currentData, out float[] vertices, out int[] textureIndices, out _, out TintColor tint);

                                // int: uv-- ---- ---- --xx xxxx yyyy yyzz zzzz (uv: texture coords; xyz: position)
                                int upperDataA = (((int)vertices[(0 * 8) + 3]) << 31) | (((int)vertices[(0 * 8) + 4]) << 30) | (((int)vertices[(0 * 8) + 0] + x) << 12) | (((int)vertices[(0 * 8) + 1] + y) << 6) | ((int)vertices[(0 * 8) + 2] + z);
                                int upperDataB = (((int)vertices[(1 * 8) + 3]) << 31) | (((int)vertices[(1 * 8) + 4]) << 30) | (((int)vertices[(1 * 8) + 0] + x) << 12) | (((int)vertices[(1 * 8) + 1] + y) << 6) | ((int)vertices[(1 * 8) + 2] + z);
                                int upperDataC = (((int)vertices[(2 * 8) + 3]) << 31) | (((int)vertices[(2 * 8) + 4]) << 30) | (((int)vertices[(2 * 8) + 0] + x) << 12) | (((int)vertices[(2 * 8) + 1] + y) << 6) | ((int)vertices[(2 * 8) + 2] + z);
                                int upperDataD = (((int)vertices[(3 * 8) + 3]) << 31) | (((int)vertices[(3 * 8) + 4]) << 30) | (((int)vertices[(3 * 8) + 0] + x) << 12) | (((int)vertices[(3 * 8) + 1] + y) << 6) | ((int)vertices[(3 * 8) + 2] + z);

                                // int: tttt tttt t--n nn-- ---- iiii iiii iiii (t: tint; n: normal; i: texture index)
                                int lowerData = (((tint.IsNeutral) ? neutral.ToBits : tint.ToBits) << 23) | ((int)BlockSide.Top << 18) | textureIndices[0];

                                while (simpleTopFaces.Count <= y)
                                {
                                    simpleTopFaces.Add(new PooledList<PooledList<SimpleFace>>());
                                }

                                while (simpleTopFaces[y].Count <= z)
                                {
                                    simpleTopFaces[y].Add(new PooledList<SimpleFace>());
                                }

                                if (simpleTopFaces[y][z].Count == 0 || !simpleTopFaces[y][z][simpleTopFaces[y][z].Count - 1].TryAdd(x, upperDataA, upperDataB, upperDataC, upperDataD, lowerData, out SimpleFace extendedFace))
                                {
                                    simpleTopFaces[y][z].Add(new SimpleFace(x, upperDataA, upperDataB, upperDataC, upperDataD, false, lowerData));
                                }
                                else
                                {
                                    simpleTopFaces[y][z][simpleTopFaces[y][z].Count - 1] = extendedFace;
                                }
                            }
                        }
                        else if (currentBlock.TargetBuffer == TargetBuffer.Complex)
                        {
                            uint verts = currentBlock.GetMesh(BlockSide.All, currentData, out float[] vertices, out int[] textureIndices, out uint[] indices, out TintColor tint);

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

                                // int: tttt tttt t--- ---- ---- iiii iiii iiii (t: tint; i: texture index)
                                int lowerData = (((tint.IsNeutral) ? neutral.ToBits : tint.ToBits) << 23) | textureIndices[i];
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
            PooledList<int> simpleVertexData = new PooledList<int>();

            for (int i = 0; i < SectionSize; i++)
            {
                for (int j = 0; j < SectionSize; j++)
                {
                    for (int k = 0; k < SectionSize; k++)
                    {
                        if (i < simpleFrontFaces.Count && j < simpleFrontFaces[i].Count && k < simpleFrontFaces[i][j].Count)
                        {
                            simpleFrontFaces[i][j][k].AddMeshTo(ref simpleVertexData);
                        }

                        if (i < simpleBackFaces.Count && j < simpleBackFaces[i].Count && k < simpleBackFaces[i][j].Count)
                        {
                            simpleBackFaces[i][j][k].AddMeshTo(ref simpleVertexData);
                        }

                        if (i < simpleLeftFaces.Count && j < simpleLeftFaces[i].Count && k < simpleLeftFaces[i][j].Count)
                        {
                            simpleLeftFaces[i][j][k].AddMeshTo(ref simpleVertexData);
                        }

                        if (i < simpleRightFaces.Count && j < simpleRightFaces[i].Count && k < simpleRightFaces[i][j].Count)
                        {
                            simpleRightFaces[i][j][k].AddMeshTo(ref simpleVertexData);
                        }

                        if (i < simpleBottomFaces.Count && j < simpleBottomFaces[i].Count && k < simpleBottomFaces[i][j].Count)
                        {
                            simpleBottomFaces[i][j][k].AddMeshTo(ref simpleVertexData);
                        }

                        if (i < simpleTopFaces.Count && j < simpleTopFaces[i].Count && k < simpleTopFaces[i][j].Count)
                        {
                            simpleTopFaces[i][j][k].AddMeshTo(ref simpleVertexData);
                        }
                    }
                }
            }

            isEmpty = complexVertexPositions.Count == 0 && simpleVertexData.Count == 0;

            meshData = new SectionMeshData(ref simpleVertexData, ref complexVertexPositions, ref complexVertexData, ref complexIndices);
        }

        private struct SimpleFace
        {
            private readonly bool inverse;

            private readonly int first;
            private int lenght;
            private int height;

            private readonly int lowerVertexData;
            private readonly bool isRotated;

            private readonly int varyingStationaryA;
            private readonly int varyingStationaryB;
            private int varyingExtendableA;
            private int varyingExtendableB;

            public SimpleFace(int first, int upperDataA, int upperDataB, int upperDataC, int upperDataD, bool inverse, int lowerVertexData)
            {
                this.first = first;
                lenght = 0;
                height = 0;

                this.lowerVertexData = lowerVertexData;

                int uv = (int)((uint)upperDataC >> 30);
                isRotated = uv != 0b11;

                if (!inverse)
                {
                    varyingStationaryA = upperDataA;
                    varyingStationaryB = upperDataB;
                    varyingExtendableA = upperDataC;
                    varyingExtendableB = upperDataD;

                    this.inverse = false;
                }
                else
                {
                    varyingStationaryA = upperDataC;
                    varyingStationaryB = upperDataD;
                    varyingExtendableA = upperDataA;
                    varyingExtendableB = upperDataB;

                    this.inverse = true;
                }
            }

            public bool TryAdd(int next, int upperDataA, int upperDataB, int upperDataC, int upperDataD, int lowerVertexData, out SimpleFace newStruct)
            {
                int uv = (int)((uint)upperDataC >> 30);
                bool isRotated = uv != 0b11;
                if (next == first + lenght + 1 && isRotated == this.isRotated && lowerVertexData == this.lowerVertexData)
                {
                    if (!inverse)
                    {
                        varyingExtendableA = upperDataC;
                        varyingExtendableB = upperDataD;
                    }
                    else
                    {
                        varyingExtendableA = upperDataA;
                        varyingExtendableB = upperDataB;
                    }

                    lenght++;

                    newStruct = this;

                    return true;
                }
                else
                {
                    newStruct = this;

                    return false;
                }
            }

            public void AddMeshTo(ref PooledList<int> meshData)
            {
                if (isRotated)
                {
                    int temp = lenght;
                    lenght = height;
                    height = temp;
                }

                meshData.Add((lenght << 25) | (height << 20) | varyingStationaryA);
                meshData.Add(lowerVertexData);

                meshData.Add((lenght << 25) | (height << 20) | varyingExtendableA);
                meshData.Add(lowerVertexData);

                meshData.Add((lenght << 25) | (height << 20) | varyingStationaryB);
                meshData.Add(lowerVertexData);

                meshData.Add((lenght << 25) | (height << 20) | varyingStationaryA);
                meshData.Add(lowerVertexData);

                meshData.Add((lenght << 25) | (height << 20) | varyingExtendableB);
                meshData.Add(lowerVertexData);

                meshData.Add((lenght << 25) | (height << 20) | varyingExtendableA);
                meshData.Add(lowerVertexData);
            }
        }

        public void SetMeshData(ref SectionMeshData meshData)
        {
            renderer.SetData(ref meshData);
        }

        public void Render(Vector3 position)
        {
            if (!isEmpty)
            {
                renderer.Draw(position);
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