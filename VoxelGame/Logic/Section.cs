// <copyright file="Section.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
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

                                while (simpleFrontFaces[z].Count <= x)
                                {
                                    simpleFrontFaces[z].Add(new PooledList<SimpleFace>());
                                }

                                if (simpleFrontFaces[z][x].Count == 0 || !simpleFrontFaces[z][x][simpleFrontFaces[z][x].Count - 1].TryAdd(true, y, upperDataA, upperDataB, upperDataC, upperDataD, lowerData, out SimpleFace extendedFace))
                                {
                                    extendedFace = new SimpleFace(true, y, upperDataA, upperDataB, upperDataC, upperDataD, false, lowerData);

                                    if (x > 0)
                                    {
                                        bool couldCombine = false;

                                        for (int g = 0; g < simpleFrontFaces[z][x - 1].Count; g++)
                                        {
                                            if (simpleFrontFaces[z][x - 1][g].TryCombine(true, extendedFace, false, out SimpleFace up, out SimpleFace down))
                                            {
                                                couldCombine = true;

                                                simpleFrontFaces[z][x - 1][g] = up;
                                                simpleFrontFaces[z][x].Add(down);

                                                break;
                                            }
                                        }

                                        if (!couldCombine)
                                        {
                                            simpleFrontFaces[z][x].Add(new SimpleFace(true, y, upperDataA, upperDataB, upperDataC, upperDataD, false, lowerData));
                                        }
                                    }
                                    else
                                    {
                                        simpleFrontFaces[z][x].Add(new SimpleFace(true, y, upperDataA, upperDataB, upperDataC, upperDataD, false, lowerData));
                                    }
                                }
                                else
                                {
                                    if (x > 0)
                                    {
                                        bool couldCombine = false;

                                        for (int g = 0; g < simpleFrontFaces[z][x - 1].Count; g++)
                                        {
                                            if (simpleFrontFaces[z][x - 1][g].TryCombine(true, extendedFace, false, out SimpleFace up, out SimpleFace down))
                                            {
                                                couldCombine = true;

                                                simpleFrontFaces[z][x - 1][g] = up;
                                                simpleFrontFaces[z][x][simpleFrontFaces[z][x].Count - 1] = down;

                                                break;
                                            }
                                        }

                                        if (!couldCombine)
                                        {
                                            if (simpleFrontFaces[z][x][simpleFrontFaces[z][x].Count - 1].height == 0)
                                            {
                                                simpleFrontFaces[z][x][simpleFrontFaces[z][x].Count - 1] = extendedFace;
                                            }
                                            else
                                            {
                                                simpleFrontFaces[z][x].Add(new SimpleFace(true, y, upperDataA, upperDataB, upperDataC, upperDataD, false, lowerData));
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (simpleFrontFaces[z][x][simpleFrontFaces[z][x].Count - 1].height == 0)
                                        {
                                            simpleFrontFaces[z][x][simpleFrontFaces[z][x].Count - 1] = extendedFace;
                                        }
                                        else
                                        {
                                            simpleFrontFaces[z][x].Add(new SimpleFace(true, y, upperDataA, upperDataB, upperDataC, upperDataD, false, lowerData));
                                        }
                                    }
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

                                while (simpleBackFaces[z].Count <= x)
                                {
                                    simpleBackFaces[z].Add(new PooledList<SimpleFace>());
                                }

                                if (simpleBackFaces[z][x].Count == 0 || !simpleBackFaces[z][x][simpleBackFaces[z][x].Count - 1].TryAdd(true, y, upperDataA, upperDataB, upperDataC, upperDataD, lowerData, out SimpleFace extendedFace))
                                {
                                    extendedFace = new SimpleFace(true, y, upperDataA, upperDataB, upperDataC, upperDataD, false, lowerData);

                                    if (x > 0)
                                    {
                                        bool couldCombine = false;

                                        for (int g = 0; g < simpleBackFaces[z][x - 1].Count; g++)
                                        {
                                            if (simpleBackFaces[z][x - 1][g].TryCombine(true, extendedFace, true, out SimpleFace up, out SimpleFace down))
                                            {
                                                couldCombine = true;

                                                simpleBackFaces[z][x - 1][g] = up;
                                                simpleBackFaces[z][x].Add(down);

                                                break;
                                            }
                                        }

                                        if (!couldCombine)
                                        {
                                            simpleBackFaces[z][x].Add(new SimpleFace(true, y, upperDataA, upperDataB, upperDataC, upperDataD, false, lowerData));
                                        }
                                    }
                                    else
                                    {
                                        simpleBackFaces[z][x].Add(new SimpleFace(true, y, upperDataA, upperDataB, upperDataC, upperDataD, false, lowerData));
                                    }
                                }
                                else
                                {
                                    if (x > 0)
                                    {
                                        bool couldCombine = false;

                                        for (int g = 0; g < simpleBackFaces[z][x - 1].Count; g++)
                                        {
                                            if (simpleBackFaces[z][x - 1][g].TryCombine(true, extendedFace, true, out SimpleFace up, out SimpleFace down))
                                            {
                                                couldCombine = true;

                                                simpleBackFaces[z][x - 1][g] = up;
                                                simpleBackFaces[z][x][simpleBackFaces[z][x].Count - 1] = down;

                                                break;
                                            }
                                        }

                                        if (!couldCombine)
                                        {
                                            if (simpleBackFaces[z][x][simpleBackFaces[z][x].Count - 1].height == 0)
                                            {
                                                simpleBackFaces[z][x][simpleBackFaces[z][x].Count - 1] = extendedFace;
                                            }
                                            else
                                            {
                                                simpleBackFaces[z][x].Add(new SimpleFace(true, y, upperDataA, upperDataB, upperDataC, upperDataD, false, lowerData));
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (simpleBackFaces[z][x][simpleBackFaces[z][x].Count - 1].height == 0)
                                        {
                                            simpleBackFaces[z][x][simpleBackFaces[z][x].Count - 1] = extendedFace;
                                        }
                                        else
                                        {
                                            simpleBackFaces[z][x].Add(new SimpleFace(true, y, upperDataA, upperDataB, upperDataC, upperDataD, false, lowerData));
                                        }
                                    }
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

                                if (simpleLeftFaces[x][y].Count == 0 || !simpleLeftFaces[x][y][simpleLeftFaces[x][y].Count - 1].TryAdd(false, z, upperDataA, upperDataB, upperDataC, upperDataD, lowerData, out SimpleFace extendedFace))
                                {
                                    extendedFace = new SimpleFace(false, z, upperDataA, upperDataB, upperDataC, upperDataD, false, lowerData);

                                    if (y > 0)
                                    {
                                        bool couldCombine = false;

                                        for (int g = 0; g < simpleLeftFaces[x][y - 1].Count; g++)
                                        {
                                            if (simpleLeftFaces[x][y - 1][g].TryCombine(false, extendedFace, true, out SimpleFace up, out SimpleFace down))
                                            {
                                                couldCombine = true;

                                                simpleLeftFaces[x][y - 1][g] = up;
                                                simpleLeftFaces[x][y].Add(down);

                                                break;
                                            }
                                        }

                                        if (!couldCombine)
                                        {
                                            simpleLeftFaces[x][y].Add(new SimpleFace(false, z, upperDataA, upperDataB, upperDataC, upperDataD, false, lowerData));
                                        }
                                    }
                                    else
                                    {
                                        simpleLeftFaces[x][y].Add(new SimpleFace(false, z, upperDataA, upperDataB, upperDataC, upperDataD, false, lowerData));
                                    }
                                }
                                else
                                {
                                    if (y > 0)
                                    {
                                        bool couldCombine = false;

                                        for (int g = 0; g < simpleLeftFaces[x][y - 1].Count; g++)
                                        {
                                            if (simpleLeftFaces[x][y - 1][g].TryCombine(false, extendedFace, true, out SimpleFace up, out SimpleFace down))
                                            {
                                                couldCombine = true;

                                                simpleLeftFaces[x][y - 1][g] = up;
                                                simpleLeftFaces[x][y][simpleLeftFaces[x][y].Count - 1] = down;

                                                break;
                                            }
                                        }

                                        if (!couldCombine)
                                        {
                                            if (simpleLeftFaces[x][y][simpleLeftFaces[x][y].Count - 1].height == 0)
                                            {
                                                simpleLeftFaces[x][y][simpleLeftFaces[x][y].Count - 1] = extendedFace;
                                            }
                                            else
                                            {
                                                simpleLeftFaces[x][y].Add(new SimpleFace(false, z, upperDataA, upperDataB, upperDataC, upperDataD, false, lowerData));
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (simpleLeftFaces[x][y][simpleLeftFaces[x][y].Count - 1].height == 0)
                                        {
                                            simpleLeftFaces[x][y][simpleLeftFaces[x][y].Count - 1] = extendedFace;
                                        }
                                        else
                                        {
                                            simpleLeftFaces[x][y].Add(new SimpleFace(false, z, upperDataA, upperDataB, upperDataC, upperDataD, false, lowerData));
                                        }
                                    }
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

                                if (simpleRightFaces[x][y].Count == 0 || !simpleRightFaces[x][y][simpleRightFaces[x][y].Count - 1].TryAdd(false, z, upperDataA, upperDataB, upperDataC, upperDataD, lowerData, out SimpleFace extendedFace))
                                {
                                    extendedFace = new SimpleFace(false, z, upperDataA, upperDataB, upperDataC, upperDataD, true, lowerData);

                                    if (y > 0)
                                    {
                                        bool couldCombine = false;

                                        for (int g = 0; g < simpleRightFaces[x][y - 1].Count; g++)
                                        {
                                            if (simpleRightFaces[x][y - 1][g].TryCombine(false, extendedFace, false, out SimpleFace up, out SimpleFace down))
                                            {
                                                couldCombine = true;

                                                simpleRightFaces[x][y - 1][g] = up;
                                                simpleRightFaces[x][y].Add(down);

                                                break;
                                            }
                                        }

                                        if (!couldCombine)
                                        {
                                            simpleRightFaces[x][y].Add(new SimpleFace(false, z, upperDataA, upperDataB, upperDataC, upperDataD, true, lowerData));
                                        }
                                    }
                                    else
                                    {
                                        simpleRightFaces[x][y].Add(new SimpleFace(false, z, upperDataA, upperDataB, upperDataC, upperDataD, true, lowerData));
                                    }
                                }
                                else
                                {
                                    if (y > 0)
                                    {
                                        bool couldCombine = false;

                                        for (int g = 0; g < simpleRightFaces[x][y - 1].Count; g++)
                                        {
                                            if (simpleRightFaces[x][y - 1][g].TryCombine(false, extendedFace, false, out SimpleFace up, out SimpleFace down))
                                            {
                                                couldCombine = true;

                                                simpleRightFaces[x][y - 1][g] = up;
                                                simpleRightFaces[x][y][simpleRightFaces[x][y].Count - 1] = down;

                                                break;
                                            }
                                        }

                                        if (!couldCombine)
                                        {
                                            if (simpleRightFaces[x][y][simpleRightFaces[x][y].Count - 1].height == 0)
                                            {
                                                simpleRightFaces[x][y][simpleRightFaces[x][y].Count - 1] = extendedFace;
                                            }
                                            else
                                            {
                                                simpleRightFaces[x][y].Add(new SimpleFace(false, z, upperDataA, upperDataB, upperDataC, upperDataD, true, lowerData));
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (simpleRightFaces[x][y][simpleRightFaces[x][y].Count - 1].height == 0)
                                        {
                                            simpleRightFaces[x][y][simpleRightFaces[x][y].Count - 1] = extendedFace;
                                        }
                                        else
                                        {
                                            simpleRightFaces[x][y].Add(new SimpleFace(false, z, upperDataA, upperDataB, upperDataC, upperDataD, true, lowerData));
                                        }
                                    }
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

                                while (simpleBottomFaces[y].Count <= x)
                                {
                                    simpleBottomFaces[y].Add(new PooledList<SimpleFace>());
                                }

                                if (simpleBottomFaces[y][x].Count == 0 || !simpleBottomFaces[y][x][simpleBottomFaces[y][x].Count - 1].TryAdd(true, z, upperDataA, upperDataB, upperDataC, upperDataD, lowerData, out SimpleFace extendedFace))
                                {
                                    extendedFace = new SimpleFace(true, z, upperDataA, upperDataB, upperDataC, upperDataD, false, lowerData);

                                    if (x > 0)
                                    {
                                        bool couldCombine = false;

                                        for (int g = 0; g < simpleBottomFaces[y][x - 1].Count; g++)
                                        {
                                            if (simpleBottomFaces[y][x - 1][g].TryCombine(true, extendedFace, false, out SimpleFace up, out SimpleFace down))
                                            {
                                                couldCombine = true;

                                                simpleBottomFaces[y][x - 1][g] = up;
                                                simpleBottomFaces[y][x].Add(down);

                                                break;
                                            }
                                        }

                                        if (!couldCombine)
                                        {
                                            simpleBottomFaces[y][x].Add(new SimpleFace(true, z, upperDataA, upperDataB, upperDataC, upperDataD, false, lowerData));
                                        }
                                    }
                                    else
                                    {
                                        simpleBottomFaces[y][x].Add(new SimpleFace(true, z, upperDataA, upperDataB, upperDataC, upperDataD, false, lowerData));
                                    }
                                }
                                else
                                {
                                    if (x > 0)
                                    {
                                        bool couldCombine = false;

                                        for (int g = 0; g < simpleBottomFaces[y][x - 1].Count; g++)
                                        {
                                            if (simpleBottomFaces[y][x - 1][g].TryCombine(true, extendedFace, false, out SimpleFace up, out SimpleFace down))
                                            {
                                                couldCombine = true;

                                                simpleBottomFaces[y][x - 1][g] = up;
                                                simpleBottomFaces[y][x][simpleBottomFaces[y][x].Count - 1] = down;

                                                break;
                                            }
                                        }

                                        if (!couldCombine)
                                        {
                                            if (simpleBottomFaces[y][x][simpleBottomFaces[y][x].Count - 1].height == 0)
                                            {
                                                simpleBottomFaces[y][x][simpleBottomFaces[y][x].Count - 1] = extendedFace;
                                            }
                                            else
                                            {
                                                simpleBottomFaces[y][x].Add(new SimpleFace(true, z, upperDataA, upperDataB, upperDataC, upperDataD, false, lowerData));
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (simpleBottomFaces[y][x][simpleBottomFaces[y][x].Count - 1].height == 0)
                                        {
                                            simpleBottomFaces[y][x][simpleBottomFaces[y][x].Count - 1] = extendedFace;
                                        }
                                        else
                                        {
                                            simpleBottomFaces[y][x].Add(new SimpleFace(true, z, upperDataA, upperDataB, upperDataC, upperDataD, false, lowerData));
                                        }
                                    }
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

                                while (simpleTopFaces[y].Count <= x)
                                {
                                    simpleTopFaces[y].Add(new PooledList<SimpleFace>());
                                }

                                if (simpleTopFaces[y][x].Count == 0 || !simpleTopFaces[y][x][simpleTopFaces[y][x].Count - 1].TryAdd(true, z, upperDataA, upperDataB, upperDataC, upperDataD, lowerData, out SimpleFace extendedFace))
                                {
                                    extendedFace = new SimpleFace(true, z, upperDataA, upperDataB, upperDataC, upperDataD, true, lowerData);

                                    if (x > 0)
                                    {
                                        bool couldCombine = false;

                                        for (int g = 0; g < simpleTopFaces[y][x - 1].Count; g++)
                                        {
                                            if (simpleTopFaces[y][x - 1][g].TryCombine(true, extendedFace, false, out SimpleFace up, out SimpleFace down))
                                            {
                                                couldCombine = true;

                                                simpleTopFaces[y][x - 1][g] = up;
                                                simpleTopFaces[y][x].Add(down);

                                                break;
                                            }
                                        }

                                        if (!couldCombine)
                                        {
                                            simpleTopFaces[y][x].Add(new SimpleFace(true, z, upperDataA, upperDataB, upperDataC, upperDataD, true, lowerData));
                                        }
                                    }
                                    else
                                    {
                                        simpleTopFaces[y][x].Add(new SimpleFace(true, z, upperDataA, upperDataB, upperDataC, upperDataD, true, lowerData));
                                    }
                                }
                                else
                                {
                                    if (x > 0)
                                    {
                                        bool couldCombine = false;

                                        for (int g = 0; g < simpleTopFaces[y][x - 1].Count; g++)
                                        {
                                            if (simpleTopFaces[y][x - 1][g].TryCombine(true, extendedFace, false, out SimpleFace up, out SimpleFace down))
                                            {
                                                couldCombine = true;

                                                simpleTopFaces[y][x - 1][g] = up;
                                                simpleTopFaces[y][x][simpleTopFaces[y][x].Count - 1] = down;

                                                break;
                                            }
                                        }

                                        if (!couldCombine)
                                        {
                                            if (simpleTopFaces[y][x][simpleTopFaces[y][x].Count - 1].height == 0)
                                            {
                                                simpleTopFaces[y][x][simpleTopFaces[y][x].Count - 1] = extendedFace;
                                            }
                                            else
                                            {
                                                simpleTopFaces[y][x].Add(new SimpleFace(true, z, upperDataA, upperDataB, upperDataC, upperDataD, true, lowerData));
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (simpleTopFaces[y][x][simpleTopFaces[y][x].Count - 1].height == 0)
                                        {
                                            simpleTopFaces[y][x][simpleTopFaces[y][x].Count - 1] = extendedFace;
                                        }
                                        else
                                        {
                                            simpleTopFaces[y][x].Add(new SimpleFace(true, z, upperDataA, upperDataB, upperDataC, upperDataD, true, lowerData));
                                        }
                                    }
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
                            simpleFrontFaces[i][j][k].AddMeshTo(true, ref simpleVertexData);
                        }

                        if (i < simpleBackFaces.Count && j < simpleBackFaces[i].Count && k < simpleBackFaces[i][j].Count)
                        {
                            simpleBackFaces[i][j][k].AddMeshTo(true, ref simpleVertexData);
                        }

                        if (i < simpleLeftFaces.Count && j < simpleLeftFaces[i].Count && k < simpleLeftFaces[i][j].Count)
                        {
                            simpleLeftFaces[i][j][k].AddMeshTo(false, ref simpleVertexData);
                        }

                        if (i < simpleRightFaces.Count && j < simpleRightFaces[i].Count && k < simpleRightFaces[i][j].Count)
                        {
                            simpleRightFaces[i][j][k].AddMeshTo(false, ref simpleVertexData);
                        }

                        if (i < simpleBottomFaces.Count && j < simpleBottomFaces[i].Count && k < simpleBottomFaces[i][j].Count)
                        {
                            simpleBottomFaces[i][j][k].AddMeshTo(true, ref simpleVertexData);
                        }

                        if (i < simpleTopFaces.Count && j < simpleTopFaces[i].Count && k < simpleTopFaces[i][j].Count)
                        {
                            simpleTopFaces[i][j][k].AddMeshTo(false, ref simpleVertexData);
                        }
                    }
                }
            }

            isEmpty = complexVertexPositions.Count == 0 && simpleVertexData.Count == 0;

            meshData = new SectionMeshData(ref simpleVertexData, ref complexVertexPositions, ref complexVertexData, ref complexIndices);
        }

        private struct SimpleFace
        {
            private bool isMeshed;

            private readonly bool inverse;

            public readonly int first;
            public int lenght;
            public int height;

            private readonly int lowerVertexData;
            private readonly bool isRotated;

            private int varyingStationaryA;
            private int varyingStationaryB;
            private int varyingExtendableA;
            private int varyingExtendableB;

            public SimpleFace(bool isSpinned, int first, int upperDataA, int upperDataB, int upperDataC, int upperDataD, bool inverse, int lowerVertexData)
            {
                isMeshed = false;

                this.first = first;
                lenght = 0;
                height = 0;

                this.lowerVertexData = lowerVertexData;

                int uv = (int)((uint)upperDataC >> 30);
                isRotated = uv != 0b11;
                isRotated = (isSpinned) ? !isRotated : isRotated;

                if (!isSpinned)
                {
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
                else
                {
                    if (!inverse)
                    {
                        varyingStationaryA = upperDataB;
                        varyingStationaryB = upperDataA;
                        varyingExtendableA = upperDataD;
                        varyingExtendableB = upperDataC;
                    }
                    else
                    {
                        varyingStationaryA = upperDataA;
                        varyingStationaryB = upperDataB;
                        varyingExtendableA = upperDataC;
                        varyingExtendableB = upperDataD;
                    }

                    this.inverse = inverse;
                }
            }

            public bool TryAdd(bool isSpinned, int next, int upperDataA, int upperDataB, int upperDataC, int upperDataD, int lowerVertexData, out SimpleFace newStruct)
            {
                int uv = (int)((uint)upperDataC >> 30);
                bool isRotated = uv != 0b11;
                isRotated = (isSpinned) ? !isRotated : isRotated;

                if (next == first + lenght + 1 && isRotated == this.isRotated && lowerVertexData == this.lowerVertexData)
                {
                    if (!isSpinned)
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
                    }
                    else
                    {
                        if (!inverse)
                        {
                            varyingStationaryA = upperDataB;
                            varyingExtendableB = upperDataC;
                        }
                        else
                        {
                            varyingStationaryA = upperDataA;
                            varyingExtendableB = upperDataD;
                        }
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

            public bool TryCombine(bool isSpinned, SimpleFace below, bool isFlipped, out SimpleFace newUpperFace, out SimpleFace newLowerFace)
            {
                if (below.isRotated == this.isRotated && below.lowerVertexData == this.lowerVertexData && below.first == this.first && below.lenght == this.lenght)
                {
                    newUpperFace = new SimpleFace { isMeshed = true };
                    newLowerFace = new SimpleFace(isSpinned, isFlipped, below, this);

                    return true;
                }
                else
                {
                    newUpperFace = new SimpleFace();
                    newLowerFace = new SimpleFace();

                    return false;
                }
            }

            private SimpleFace(bool isSpinned, bool isFlipped, SimpleFace old, SimpleFace upper)
            {
                isMeshed = false;

                inverse = old.inverse;

                first = old.first;
                lenght = old.lenght;
                height = upper.height + 1;

                lowerVertexData = old.lowerVertexData;
                isRotated = old.isRotated;

                if (!isSpinned)
                {
                    if (!isFlipped)
                    {
                        varyingStationaryA = old.varyingStationaryA;
                        varyingStationaryB = upper.varyingStationaryB;
                        varyingExtendableA = upper.varyingExtendableA;
                        varyingExtendableB = old.varyingExtendableB;
                    }
                    else
                    {
                        varyingStationaryA = upper.varyingStationaryA;
                        varyingStationaryB = old.varyingStationaryB;
                        varyingExtendableA = old.varyingExtendableA;
                        varyingExtendableB = upper.varyingExtendableB;
                    }
                }
                else
                {
                    if (!isFlipped)
                    {
                        varyingStationaryA = upper.varyingStationaryA;
                        varyingStationaryB = upper.varyingStationaryB;
                        varyingExtendableA = old.varyingExtendableA;
                        varyingExtendableB = old.varyingExtendableB;
                    }
                    else
                    {
                        varyingStationaryA = old.varyingStationaryA;
                        varyingStationaryB = old.varyingStationaryB;
                        varyingExtendableA = upper.varyingExtendableA;
                        varyingExtendableB = upper.varyingExtendableB;
                    }
                }
            }

            public void AddMeshTo(bool turn, ref PooledList<int> meshData)
            {
                if (isMeshed)
                {
                    return;
                }

                if (isRotated)
                {
                    int temp = lenght;
                    lenght = height;
                    height = temp;
                }

                if (!turn)                
                {
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
                else
                {
                    meshData.Add((lenght << 25) | (height << 20) | varyingStationaryA);
                    meshData.Add(lowerVertexData);

                    meshData.Add((lenght << 25) | (height << 20) | varyingStationaryB);
                    meshData.Add(lowerVertexData);

                    meshData.Add((lenght << 25) | (height << 20) | varyingExtendableA);
                    meshData.Add(lowerVertexData);

                    meshData.Add((lenght << 25) | (height << 20) | varyingStationaryA);
                    meshData.Add(lowerVertexData);

                    meshData.Add((lenght << 25) | (height << 20) | varyingExtendableA);
                    meshData.Add(lowerVertexData);

                    meshData.Add((lenght << 25) | (height << 20) | varyingExtendableB);
                    meshData.Add(lowerVertexData);
                }
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