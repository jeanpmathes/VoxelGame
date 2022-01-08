// <copyright file="CompactedMeshFaceHolder.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System.Buffers;
using OpenToolkit.Mathematics;
using VoxelGame.Core.Collections;
using VoxelGame.Core.Logic;

namespace VoxelGame.Client.Collections
{
    /// <summary>
    ///     A specialized class used to compact block faces when meshing.
    /// </summary>
    public class BlockMeshFaceHolder : MeshFaceHolder
    {
        private readonly MeshFace?[][] lastFaces;

        private int count;

        /// <summary>
        ///     Create a new instance of the <see cref="BlockMeshFaceHolder" /> class for a given block side.
        /// </summary>
        /// <param name="side">The block side that the faces will correspond too.</param>
        public BlockMeshFaceHolder(BlockSide side) : base(side)
        {
            // Initialize layers.
            lastFaces = ArrayPool<MeshFace[]>.Shared.Rent(Section.SectionSize);

            // Initialize rows.
            for (var i = 0; i < Section.SectionSize; i++)
            {
                lastFaces[i] = ArrayPool<MeshFace>.Shared.Rent(Section.SectionSize);

                for (var j = 0; j < Section.SectionSize; j++) lastFaces[i][j] = null;
            }
        }

        /// <summary>
        ///     Add a new face to the holder.
        /// </summary>
        /// <param name="pos">The position of the face, relative to the section origin.</param>
        /// <param name="vertexData">The binary encoded vertex data to use for every vertex.</param>
        /// <param name="vertices">The binary encoded vertices of the face.</param>
        /// <param name="isRotated">True if the face is rotated.</param>
        public void AddFace(Vector3i pos, int vertexData, (int vertA, int vertB, int vertC, int vertD) vertices,
            bool isRotated)
        {
            ExtractIndices(pos, out int layer, out int row, out int position);

            // Build current face.
            MeshFace currentFace = MeshFace.Get(
                vertices.vertA,
                vertices.vertB,
                vertices.vertC,
                vertices.vertD,
                vertexData,
                isRotated,
                position);

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
                        currentFace.vertex01 = vertices.vertB;
                        currentFace.vertex11 = vertices.vertC;

                        break;

                    case BlockSide.Left:
                        currentFace.vertex11 = vertices.vertC;
                        currentFace.vertex10 = vertices.vertD;

                        break;

                    case BlockSide.Right:
                        currentFace.vertex00 = vertices.vertA;
                        currentFace.vertex01 = vertices.vertB;

                        break;

                    case BlockSide.Top:
                        currentFace.vertex00 = vertices.vertA;
                        currentFace.vertex10 = vertices.vertD;

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

            if (row == 0) return;

            MeshFace? combinationRowFace = lastFaces[layer][row - 1];
            MeshFace? lastCombinationRowFace = null;

            // Check if the current face can be combined with a face in the previous row.
            while (combinationRowFace != null)
            {
                if (combinationRowFace.IsCombinable(currentFace))
                {
                    switch (side)
                    {
                        case BlockSide.Front:
                        case BlockSide.Bottom:
                        case BlockSide.Top:
                            currentFace.vertex00 = combinationRowFace.vertex00;
                            currentFace.vertex01 = combinationRowFace.vertex01;

                            break;

                        case BlockSide.Back:
                            currentFace.vertex11 = combinationRowFace.vertex11;
                            currentFace.vertex10 = combinationRowFace.vertex10;

                            break;

                        case BlockSide.Left:
                        case BlockSide.Right:
                            currentFace.vertex00 = combinationRowFace.vertex00;
                            currentFace.vertex10 = combinationRowFace.vertex10;

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

        /// <summary>
        ///     Generate the mesh using all faces held by this holder.
        /// </summary>
        /// <param name="meshData">The list where the meshData will be added to.</param>
        public void GenerateMesh(PooledList<int> meshData)
        {
            if (count == 0) return;

            meshData.Capacity += count;

            for (var l = 0; l < Section.SectionSize; l++)
            for (var r = 0; r < Section.SectionSize; r++)
            {
                MeshFace? currentFace = lastFaces[l][r];

                while (currentFace != null)
                {
                    if (side is BlockSide.Left or BlockSide.Right)
                        currentFace.isRotated = !currentFace.isRotated;

                    int vertTexRepetition = BuildVertexTexRepetitionMask(
                        currentFace.isRotated,
                        currentFace.height,
                        currentFace.length);

                    meshData.Add(vertTexRepetition | currentFace.vertex00);
                    meshData.Add(currentFace.vertData);

                    meshData.Add(vertTexRepetition | currentFace.vertex11);
                    meshData.Add(currentFace.vertData);

                    meshData.Add(vertTexRepetition | currentFace.vertex01);
                    meshData.Add(currentFace.vertData);

                    meshData.Add(vertTexRepetition | currentFace.vertex00);
                    meshData.Add(currentFace.vertData);

                    meshData.Add(vertTexRepetition | currentFace.vertex10);
                    meshData.Add(currentFace.vertData);

                    meshData.Add(vertTexRepetition | currentFace.vertex11);
                    meshData.Add(currentFace.vertData);

                    MeshFace? next = currentFace.previousFace;
                    currentFace.Return();
                    currentFace = next;
                }
            }
        }

        private static int BuildVertexTexRepetitionMask(bool isRotated, int height, int length)
        {
            const int heightShift = 24;
            const int lengthShift = 20;

            return !isRotated
                ? (height << heightShift) | (length << lengthShift)
                : (length << heightShift) | (height << lengthShift);
        }

        /// <summary>
        ///     Return all pooled resources used.
        /// </summary>
        public void ReturnToPool()
        {
            for (var i = 0; i < Section.SectionSize; i++) ArrayPool<MeshFace>.Shared.Return(lastFaces[i]!);

            ArrayPool<MeshFace[]>.Shared.Return(lastFaces!);
        }

#pragma warning disable CA1812

        private class MeshFace
        {
            public int height;

            public bool isRotated;
            public int length;

            private int position;
            public MeshFace? previousFace;

            public int vertData;

            public int vertex00;
            public int vertex01;
            public int vertex10;
            public int vertex11;

            public bool IsExtendable(MeshFace extension)
            {
                return position + length + 1 == extension.position &&
                       height == extension.height &&
                       isRotated == extension.isRotated &&
                       vertData == extension.vertData;
            }

            public bool IsCombinable(MeshFace addition)
            {
                return position == addition.position &&
                       length == addition.length &&
                       isRotated == addition.isRotated &&
                       vertData == addition.vertData;
            }

            #region POOLING

            public static MeshFace Get(int vert00, int vert01, int vert11, int vert10, int vertData, bool isRotated,
                int position)
            {
                MeshFace instance = ObjectPool<MeshFace>.Shared.Get();

                instance.previousFace = null;

                instance.vertex00 = vert00;
                instance.vertex01 = vert01;
                instance.vertex11 = vert11;
                instance.vertex10 = vert10;

                instance.vertData = vertData;

                instance.isRotated = isRotated;

                instance.position = position;
                instance.length = 0;
                instance.height = 0;

                return instance;
            }

            public void Return()
            {
                ObjectPool<MeshFace>.Shared.Return(this);
            }

            #endregion POOLING
        }

#pragma warning restore CA1812
    }
}