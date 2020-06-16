// <copyright file="CompactedMeshFaceHolder.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using System;
using System.Buffers;
using VoxelGame.Logic;

namespace VoxelGame.Collections
{
    /// <summary>
    /// A specialized class used to compact faces when meshing.
    /// </summary>
    public class CompactedMeshFaceHolder
    {
        private readonly BlockSide side;
        private readonly MeshFace[][] lastFaces;

        private int count;

        public CompactedMeshFaceHolder(BlockSide side)
        {
            this.side = side;

            // Initialize layers.
            lastFaces = ArrayPool<MeshFace[]>.Shared.Rent(Section.SectionSize);

            // Initialize rows.
            for (int i = 0; i < Section.SectionSize; i++)
            {
                lastFaces[i] = ArrayPool<MeshFace>.Shared.Rent(Section.SectionSize);

                for (int j = 0; j < Section.SectionSize; j++)
                {
                    lastFaces[i][j] = null;
                }
            }
        }

        public void AddFace(int layer, int row, int position, int vertA, int vertB, int vertC, int vertD, int vertData)
        {
            // Build current face.
            MeshFace currentFace = new MeshFace
            {
                vert_0_0 = vertA,
                vert_0_1 = vertB,
                vert_1_1 = vertC,
                vert_1_0 = vertD,

                vertData = vertData,

                isRotated = (int)((uint)vertC >> 30) != 0b11,

                position = position
            };

            // Check if an already existing face can be extended.
            if (lastFaces[layer][row]?.IsExtendable(currentFace) ?? false)
            {
                currentFace = lastFaces[layer][row];

                switch (side)
                {
                    case BlockSide.Front:
                    case BlockSide.Back:
                    case BlockSide.Bottom:
                        currentFace.vert_0_1 = vertB;
                        currentFace.vert_1_1 = vertC;
                        break;

                    case BlockSide.Left:
                        currentFace.vert_1_1 = vertC;
                        currentFace.vert_1_0 = vertD;
                        break;

                    case BlockSide.Right:
                        currentFace.vert_0_0 = vertA;
                        currentFace.vert_0_1 = vertB;
                        break;

                    case BlockSide.Top:
                        currentFace.vert_0_0 = vertA;
                        currentFace.vert_1_0 = vertD;
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

            MeshFace combinationRowFace = lastFaces[layer][row - 1];
            MeshFace lastCombinationRowFace = null;

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
                        lastFaces[layer][row - 1] = combinationRowFace.previousFace;
                    }
                    else
                    {
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
            if (meshData == null)
            {
                throw new ArgumentNullException(nameof(meshData));
            }

            if (count == 0)
            {
                return;
            }

            meshData.Capacity += count;

            for (int l = 0; l < Section.SectionSize; l++)
            {
                for (int r = 0; r < Section.SectionSize; r++)
                {
                    MeshFace currentFace = lastFaces[l][r];

                    while (currentFace != null)
                    {
                        if (side == BlockSide.Left || side == BlockSide.Right)
                        {
                            currentFace.isRotated = !currentFace.isRotated;
                        }

                        int vertTexRepetition = (!currentFace.isRotated) ? ((currentFace.height << 25) | (currentFace.length << 20)) : ((currentFace.length << 25) | (currentFace.height << 20));

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

                        currentFace = currentFace.previousFace;
                    }
                }
            }
        }

        public void ReturnToPool()
        {
            for (int i = 0; i < Section.SectionSize; i++)
            {
                ArrayPool<MeshFace>.Shared.Return(lastFaces[i]);
            }

            ArrayPool<MeshFace[]>.Shared.Return(lastFaces);
        }

        private class MeshFace
        {
            public MeshFace previousFace;

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
        }
    }
}