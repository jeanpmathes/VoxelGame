// <copyright file="CompactedMeshFaceHolder.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using System;
using System.Buffers;
using System.Collections.Concurrent;
using VoxelGame.Logic;

namespace VoxelGame.Collections
{
    /// <summary>
    /// A specialized class used to compact faces when meshing.
    /// </summary>
    public class CompactedMeshFaceHolder
    {
        private static readonly ArrayPool<MeshFace[]> layerPool = ArrayPool<MeshFace[]>.Create(Section.SectionSize, 64);
        private static readonly ArrayPool<MeshFace> rowPool = ArrayPool<MeshFace>.Create(Section.SectionSize, 256);

        private readonly BlockSide side;
        private readonly MeshFace?[][] lastFaces;

        private int count;

        public CompactedMeshFaceHolder(BlockSide side)
        {
            this.side = side;

            // Initialize layers.
            lastFaces = layerPool.Rent(Section.SectionSize);

            // Initialize rows.
            for (int i = 0; i < Section.SectionSize; i++)
            {
                lastFaces[i] = rowPool.Rent(Section.SectionSize);

                for (int j = 0; j < Section.SectionSize; j++)
                {
                    lastFaces[i][j] = null;
                }
            }
        }

        public void AddFace(int layer, int row, int position, int vertData, (int vertA, int vertB, int vertC, int vertD) vertices)
        {
            // Build current face.
            MeshFace currentFace = MeshFace.Get(vertices.vertA, vertices.vertB, vertices.vertC, vertices.vertD, vertData, (int)((uint)vertices.vertC >> 30) != 0b11, position);

            // Check if an already existing face can be extended.
            if (lastFaces[layer][row]?.IsExtendable(currentFace) ?? false)
            {
                currentFace.Return();
                currentFace = lastFaces[layer][row]!;

                switch (side)
                {
                    case BlockSide.Front:
                    case BlockSide.Back:
                    case BlockSide.Bottom:
                        currentFace.vert_0_1 = vertices.vertB;
                        currentFace.vert_1_1 = vertices.vertC;
                        break;

                    case BlockSide.Left:
                        currentFace.vert_1_1 = vertices.vertC;
                        currentFace.vert_1_0 = vertices.vertD;
                        break;

                    case BlockSide.Right:
                        currentFace.vert_0_0 = vertices.vertA;
                        currentFace.vert_0_1 = vertices.vertB;
                        break;

                    case BlockSide.Top:
                        currentFace.vert_0_0 = vertices.vertA;
                        currentFace.vert_1_0 = vertices.vertD;
                        break;
                }

                currentFace.length++;
            }
            else
            {
                currentFace.previousFace = lastFaces[layer][row];
                lastFaces[layer][row] = currentFace;

                count++;
            }

            if (row == 0)
            {
                return;
            }

            MeshFace? combinationRowFace = lastFaces[layer][row - 1];
            MeshFace? lastCombinationRowFace = null;

            // Check if the current face can be combined with a face in the previous row.
            while (combinationRowFace != null)
            {
                if (combinationRowFace.IsCombineable(currentFace))
                {
                    switch (side)
                    {
                        case BlockSide.Front:
                        case BlockSide.Bottom:
                        case BlockSide.Top:
                            currentFace.vert_0_0 = combinationRowFace.vert_0_0;
                            currentFace.vert_0_1 = combinationRowFace.vert_0_1;
                            break;

                        case BlockSide.Back:
                            currentFace.vert_1_1 = combinationRowFace.vert_1_1;
                            currentFace.vert_1_0 = combinationRowFace.vert_1_0;
                            break;

                        case BlockSide.Left:
                        case BlockSide.Right:
                            currentFace.vert_0_0 = combinationRowFace.vert_0_0;
                            currentFace.vert_1_0 = combinationRowFace.vert_1_0;
                            break;
                    }

                    currentFace.height = combinationRowFace.height + 1;

                    if (lastCombinationRowFace == null)
                    {
                        lastFaces[layer][row - 1]?.Return();
                        lastFaces[layer][row - 1] = combinationRowFace.previousFace;
                    }
                    else
                    {
                        lastCombinationRowFace.previousFace?.Return();
                        lastCombinationRowFace.previousFace = combinationRowFace.previousFace;
                    }

                    count--;

                    break;
                }

                lastCombinationRowFace = combinationRowFace;
                combinationRowFace = combinationRowFace.previousFace;
            }
        }

        public void GenerateMesh(ref PooledList<int> meshData)
        {
            if (count == 0)
            {
                return;
            }

            meshData.Capacity += count;

            for (int l = 0; l < Section.SectionSize; l++)
            {
                for (int r = 0; r < Section.SectionSize; r++)
                {
                    MeshFace? currentFace = lastFaces[l][r];

                    while (currentFace != null)
                    {
                        if (side == BlockSide.Left || side == BlockSide.Right)
                        {
                            currentFace.isRotated = !currentFace.isRotated;
                        }

                        int vertTexRepetition = BuildVertexTexRepetitionMask(currentFace.isRotated, currentFace.height, currentFace.length);

                        meshData.Add(vertTexRepetition | currentFace.vert_0_0);
                        meshData.Add(currentFace.vertData);

                        meshData.Add(vertTexRepetition | currentFace.vert_1_1);
                        meshData.Add(currentFace.vertData);

                        meshData.Add(vertTexRepetition | currentFace.vert_0_1);
                        meshData.Add(currentFace.vertData);

                        meshData.Add(vertTexRepetition | currentFace.vert_0_0);
                        meshData.Add(currentFace.vertData);

                        meshData.Add(vertTexRepetition | currentFace.vert_1_0);
                        meshData.Add(currentFace.vertData);

                        meshData.Add(vertTexRepetition | currentFace.vert_1_1);
                        meshData.Add(currentFace.vertData);

                        currentFace.Return();
                        currentFace = currentFace.previousFace;
                    }
                }
            }
        }

        private static int BuildVertexTexRepetitionMask(bool isRotated, int height, int length)
        {
            return !isRotated ? ((height << 25) | (length << 20)) : ((length << 25) | (height << 20));
        }

        public void ReturnToPool()
        {
            for (int i = 0; i < Section.SectionSize; i++)
            {
                rowPool.Return(lastFaces[i]!);
            }

            layerPool.Return(lastFaces!);
        }

        private class MeshFace
        {
            public MeshFace? previousFace;

            public int vert_0_0;
            public int vert_0_1;
            public int vert_1_1;
            public int vert_1_0;

            public int vertData;

            public bool isRotated;

            public int position;
            public int length;
            public int height;

            public bool IsExtendable(MeshFace extension)
            {
                return this.position + this.length + 1 == extension.position &&
                    this.height == extension.height &&
                    this.isRotated == extension.isRotated &&
                    this.vertData == extension.vertData;
            }

            public bool IsCombineable(MeshFace addition)
            {
                return this.position == addition.position &&
                    this.length == addition.length &&
                    this.isRotated == addition.isRotated &&
                    this.vertData == addition.vertData;
            }

            #region POOLING

            private readonly static ConcurrentBag<MeshFace> objects = new ConcurrentBag<MeshFace>();

            public static MeshFace Get(int vert_0_0, int vert_0_1, int vert_1_1, int vert_1_0, int vertData, bool isRotated, int position)
            {
                MeshFace instance = objects.TryTake(out instance!) ? instance : new MeshFace();

                instance.previousFace = null;

                instance.vert_0_0 = vert_0_0;
                instance.vert_0_1 = vert_0_1;
                instance.vert_1_1 = vert_1_1;
                instance.vert_1_0 = vert_1_0;

                instance.vertData = vertData;

                instance.isRotated = isRotated;

                instance.position = position;
                instance.length = 0;
                instance.height = 0;

                return instance;
            }

            public void Return()
            {
                objects.Add(this);
            }

            #endregion POOLING
        }
    }
}