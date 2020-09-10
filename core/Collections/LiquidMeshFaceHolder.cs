// <copyright file="LiquidMeshFaceHolder.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using System.Buffers;
using System.Collections.Concurrent;
using VoxelGame.Core.Logic;

namespace VoxelGame.Core.Collections
{
    /// <summary>
    /// A specialized class used to compact liquid faces while meshing.
    /// </summary>
    public class LiquidMeshFaceHolder
    {
        private static readonly ArrayPool<MeshFace[]> layerPool = ArrayPool<MeshFace[]>.Create(Section.SectionSize, 64);
        private static readonly ArrayPool<MeshFace> rowPool = ArrayPool<MeshFace>.Create(Section.SectionSize, 256);

        private readonly BlockSide side;
        private readonly MeshFace?[][] lastFaces;

        private int count;

        public LiquidMeshFaceHolder(BlockSide side)
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

        public void AddFace(int layer, int row, int position, int vertData, (int vertA, int vertB, int vertC, int vertD) vertices, bool isSingleSided, bool isFull)
        {
            // Build current face.
            MeshFace currentFace = MeshFace.Get(vertices.vertA, vertices.vertB, vertices.vertC, vertices.vertD, vertData, position, isSingleSided);

            // Front and Back faces cannot be extended (along the y axis) when the liquid is not all full level.
            bool levelPermitsExtending = !((side == BlockSide.Front || side == BlockSide.Back) && !isFull);

            // Check if an already existing face can be extended.
            if (levelPermitsExtending && (lastFaces[layer][row]?.IsExtendable(currentFace) ?? false))
            {
                currentFace.Return();
                currentFace = lastFaces[layer][row]!;

                switch (side)
                {
                    case BlockSide.Front:
                    case BlockSide.Back:
                    case BlockSide.Bottom:
                        currentFace.vertB = vertices.vertB;
                        currentFace.vertC = vertices.vertC;
                        break;

                    case BlockSide.Left:
                        currentFace.vertC = vertices.vertC;
                        currentFace.vertD = vertices.vertD;
                        break;

                    case BlockSide.Right:
                        currentFace.vertA = vertices.vertA;
                        currentFace.vertB = vertices.vertB;
                        break;

                    case BlockSide.Top:
                        currentFace.vertA = vertices.vertA;
                        currentFace.vertD = vertices.vertD;
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

            // Left and right faces cannot be combined (along the y axis) when the liquid is not all full level.
            if ((side == BlockSide.Left || side == BlockSide.Right) && !isFull) return;

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
                            currentFace.vertA = combinationRowFace.vertA;
                            currentFace.vertB = combinationRowFace.vertB;
                            break;

                        case BlockSide.Back:
                            currentFace.vertC = combinationRowFace.vertC;
                            currentFace.vertD = combinationRowFace.vertD;
                            break;

                        case BlockSide.Left:
                        case BlockSide.Right:
                            currentFace.vertA = combinationRowFace.vertA;
                            currentFace.vertD = combinationRowFace.vertD;
                            break;
                    }

                    currentFace.height = combinationRowFace.height + 1;

                    if (lastCombinationRowFace == null)
                    {
                        lastFaces[layer][row - 1] = combinationRowFace.previousFace;
                        combinationRowFace.Return();
                    }
                    else
                    {
                        lastCombinationRowFace.previousFace = combinationRowFace.previousFace;
                        combinationRowFace.Return();
                    }

                    count--;

                    break;
                }

                lastCombinationRowFace = combinationRowFace;
                combinationRowFace = combinationRowFace.previousFace;
            }
        }

        private static readonly uint[] indices = new uint[]
        {
            0, 2, 1,
            0, 3, 2,
            0, 1, 2,
            0, 2, 3
        };

        public void GenerateMesh(ref PooledList<int> meshData, ref uint vertexCount, ref PooledList<uint> meshIndices)
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
                        int vertexTexRepetition = BuildVertexTextureRepetition(currentFace.height, currentFace.length);

                        meshData.Add(vertexTexRepetition | currentFace.vertA);
                        meshData.Add(currentFace.vertData);

                        meshData.Add(vertexTexRepetition | currentFace.vertB);
                        meshData.Add(currentFace.vertData);

                        meshData.Add(vertexTexRepetition | currentFace.vertC);
                        meshData.Add(currentFace.vertData);

                        meshData.Add(vertexTexRepetition | currentFace.vertD);
                        meshData.Add(currentFace.vertData);

                        int newIndices = currentFace.isSingleSided ? 6 : 12;
                        meshIndices.AddRange(indices, newIndices);

                        for (int i = 0; i < newIndices; i++)
                        {
                            meshIndices[meshIndices.Count - newIndices + i] += vertexCount;
                        }

                        vertexCount += 4;

                        MeshFace? next = currentFace.previousFace;
                        currentFace.Return();
                        currentFace = next;
                    }
                }
            }
        }

        private int BuildVertexTextureRepetition(int height, int length)
        {
            return !(side == BlockSide.Left || side == BlockSide.Right) ? ((height << 25) | (length << 20)) : ((length << 25) | (height << 20));
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

            public int vertA;
            public int vertB;
            public int vertC;
            public int vertD;

            public int vertData;

            public int position;
            public int length;
            public int height;

            public bool isSingleSided;

            private MeshFace() { }

            public bool IsExtendable(MeshFace extension)
            {
                return this.position + this.length + 1 == extension.position &&
                    this.height == extension.height &&
                    this.vertData == extension.vertData &&
                    this.isSingleSided == extension.isSingleSided;
            }

            public bool IsCombineable(MeshFace addition)
            {
                return this.position == addition.position &&
                    this.length == addition.length &&
                    this.vertData == addition.vertData &&
                    this.isSingleSided == addition.isSingleSided;
            }

            #region POOLING

            private readonly static ConcurrentBag<MeshFace> objects = new ConcurrentBag<MeshFace>();

            public static MeshFace Get(int vert_0_0, int vert_0_1, int vert_1_1, int vert_1_0, int vertData, int position, bool isSingleSided)
            {
                MeshFace instance = objects.TryTake(out instance!) ? instance : new MeshFace();

                instance.previousFace = null;

                instance.vertA = vert_0_0;
                instance.vertB = vert_0_1;
                instance.vertC = vert_1_1;
                instance.vertD = vert_1_0;

                instance.vertData = vertData;

                instance.position = position;
                instance.length = 0;
                instance.height = 0;

                instance.isSingleSided = isSingleSided;

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
