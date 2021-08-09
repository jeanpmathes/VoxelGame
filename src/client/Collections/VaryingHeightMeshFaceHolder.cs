// <copyright file="LiquidMeshFaceHolder.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using System.Buffers;
using System.Collections.Concurrent;
using OpenToolkit.Mathematics;
using VoxelGame.Core.Collections;
using VoxelGame.Core.Logic;

namespace VoxelGame.Client.Collections
{
    /// <summary>
    /// A specialized class used to compact varying height block faces and liquid faces while meshing.
    /// </summary>
    public class VaryingHeightMeshFaceHolder : MeshFaceHolder
    {
        private static readonly ArrayPool<MeshFace[]> layerPool = ArrayPool<MeshFace[]>.Create(Section.SectionSize, 64);
        private static readonly ArrayPool<MeshFace> rowPool = ArrayPool<MeshFace>.Create(Section.SectionSize, 256);

        private readonly MeshFace?[][] lastFaces;

        private int count;

        public VaryingHeightMeshFaceHolder(BlockSide side) : base(side)
        {
            // Initialize layers.
            lastFaces = layerPool.Rent(Section.SectionSize);

            // Initialize rows.
            for (var i = 0; i < Section.SectionSize; i++)
            {
                lastFaces[i] = rowPool.Rent(Section.SectionSize);

                for (var j = 0; j < Section.SectionSize; j++)
                {
                    lastFaces[i][j] = null;
                }
            }
        }

        public void AddFace(Vector3i pos, int vertexData, (int vertA, int vertB, int vertC, int vertD) vertices, bool isSingleSided, bool isFull)
        {
            ExtractIndices(pos, out int layer, out int row, out int position);

            // Build current face.
            MeshFace currentFace = MeshFace.Get(vertices.vertA, vertices.vertB, vertices.vertC, vertices.vertD, vertexData, position, isSingleSided);

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
                        currentFace.vertexB = vertices.vertB;
                        currentFace.vertexC = vertices.vertC;
                        break;

                    case BlockSide.Left:
                        currentFace.vertexC = vertices.vertC;
                        currentFace.vertexD = vertices.vertD;
                        break;

                    case BlockSide.Right:
                        currentFace.vertexA = vertices.vertA;
                        currentFace.vertexB = vertices.vertB;
                        break;

                    case BlockSide.Top:
                        currentFace.vertexA = vertices.vertA;
                        currentFace.vertexD = vertices.vertD;
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
                            currentFace.vertexA = combinationRowFace.vertexA;
                            currentFace.vertexB = combinationRowFace.vertexB;
                            break;

                        case BlockSide.Back:
                            currentFace.vertexC = combinationRowFace.vertexC;
                            currentFace.vertexD = combinationRowFace.vertexD;
                            break;

                        case BlockSide.Left:
                        case BlockSide.Right:
                            currentFace.vertexA = combinationRowFace.vertexA;
                            currentFace.vertexD = combinationRowFace.vertexD;
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

            for (var l = 0; l < Section.SectionSize; l++)
            {
                for (var r = 0; r < Section.SectionSize; r++)
                {
                    MeshFace? currentFace = lastFaces[l][r];

                    while (currentFace != null)
                    {
                        int vertexTexRepetition = BuildVertexTextureRepetition(currentFace.height, currentFace.length);

                        meshData.Add(vertexTexRepetition | currentFace.vertexA);
                        meshData.Add(currentFace.vertexData);

                        meshData.Add(vertexTexRepetition | currentFace.vertexB);
                        meshData.Add(currentFace.vertexData);

                        meshData.Add(vertexTexRepetition | currentFace.vertexC);
                        meshData.Add(currentFace.vertexData);

                        meshData.Add(vertexTexRepetition | currentFace.vertexD);
                        meshData.Add(currentFace.vertexData);

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
            const int heightShift = 24;
            const int lengthShift = 20;

            return !(side == BlockSide.Left || side == BlockSide.Right)
                ? ((height << heightShift) | (length << lengthShift))
                : ((length << heightShift) | (height << lengthShift));
        }

        public void ReturnToPool()
        {
            for (var i = 0; i < Section.SectionSize; i++)
            {
                rowPool.Return(lastFaces[i]!);
            }

            layerPool.Return(lastFaces!);
        }

        private class MeshFace
        {
            public MeshFace? previousFace;

            public int vertexA;
            public int vertexB;
            public int vertexC;
            public int vertexD;

            public int vertexData;

            public int position;
            public int length;
            public int height;

            public bool isSingleSided;

            private MeshFace()
            {
            }

            public bool IsExtendable(MeshFace extension)
            {
                return this.position + this.length + 1 == extension.position &&
                    this.height == extension.height &&
                    this.vertexData == extension.vertexData &&
                    this.isSingleSided == extension.isSingleSided;
            }

            public bool IsCombineable(MeshFace addition)
            {
                return this.position == addition.position &&
                    this.length == addition.length &&
                    this.vertexData == addition.vertexData &&
                    this.isSingleSided == addition.isSingleSided;
            }

            #region POOLING

            private static readonly ConcurrentBag<MeshFace> objects = new ConcurrentBag<MeshFace>();

            public static MeshFace Get(int vert_0_0, int vert01, int vert11, int vert10, int vertData, int position, bool isSingleSided)
            {
                MeshFace instance = objects.TryTake(out instance!) ? instance : new MeshFace();

                instance.previousFace = null;

                instance.vertexA = vert_0_0;
                instance.vertexB = vert01;
                instance.vertexC = vert11;
                instance.vertexD = vert10;

                instance.vertexData = vertData;

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