// <copyright file="BlockModels.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using OpenToolkit.Mathematics;
using VoxelGame.Core.Logic;

namespace VoxelGame.Core.Visuals
{
    public static class BlockModels
    {
        private static readonly int[][] defaultBlockUVs =
            { new[] { 0, 0 }, new[] { 0, 1 }, new[] { 1, 1 }, new[] { 1, 0 } };

        private static readonly int[][] rotatedBlockUVs =
            { new[] { 0, 1 }, new[] { 1, 1 }, new[] { 1, 0 }, new[] { 0, 0 } };

        public static (float[] vertices, uint[] indices, int[] textureIndices) CreateCrossModel(int textureIndex)
        {
            float[] vertices =
            {
                // Two sides: /
                0.145f, 0f, 0.855f, 0f, 0f, 0f, 0f, 0f,
                0.145f, 1f, 0.855f, 0f, 1f, 0f, 0f, 0f,
                0.855f, 1f, 0.145f, 1f, 1f, 0f, 0f, 0f,
                0.855f, 0f, 0.145f, 1f, 0f, 0f, 0f, 0f,

                // Two sides: \
                0.145f, 0f, 0.145f, 0f, 0f, 0f, 0f, 0f,
                0.145f, 1f, 0.145f, 0f, 1f, 0f, 0f, 0f,
                0.855f, 1f, 0.855f, 1f, 1f, 0f, 0f, 0f,
                0.855f, 0f, 0.855f, 1f, 0f, 0f, 0f, 0f
            };

            uint[] indices =
            {
                // Direction: /
                0, 2, 1,
                0, 3, 2,

                0, 1, 2,
                0, 2, 3,

                // Direction: \
                4, 6, 5,
                4, 7, 6,

                4, 5, 6,
                4, 6, 7
            };

            int[] textureIndices = GenerateTextureDataArray(textureIndex, length: 8);

            return (vertices, indices, textureIndices);
        }

        public static float[] CreateFlatModel(BlockSide side, float offset)
        {
            side.Corners(out int[] a, out int[] b, out int[] c, out int[] d);

            var n = side.Direction().ToVector3();
            Vector3 vOffset = n * offset * -1;

            Vector3 v1 = (a[0], a[1], a[2]) + vOffset;
            Vector3 v2 = (b[0], b[1], b[2]) + vOffset;
            Vector3 v3 = (c[0], c[1], c[2]) + vOffset;
            Vector3 v4 = (d[0], d[1], d[2]) + vOffset;

            return new[]
            {
                v1.X, v1.Y, v1.Z, 1f, 0f, n.X, n.Y, n.Z,
                v2.X, v2.Y, v2.Z, 1f, 1f, n.X, n.Y, n.Z,
                v3.X, v3.Y, v3.Z, 0f, 1f, n.X, n.Y, n.Z,
                v4.X, v4.Y, v4.Z, 0f, 0f, n.X, n.Y, n.Z,

                v4.X, v4.Y, v4.Z, 0f, 0f, -n.X, -n.Y, -n.Z,
                v3.X, v3.Y, v3.Z, 0f, 1f, -n.X, -n.Y, -n.Z,
                v2.X, v2.Y, v2.Z, 1f, 1f, -n.X, -n.Y, -n.Z,
                v1.X, v1.Y, v1.Z, 1f, 0f, -n.X, -n.Y, -n.Z
            };
        }

        public static (float[] vertices, uint[] indices) CreatePlaneModel()
        {
            float[] vertices =
            {
                -0.5f, -0.5f, 0.0f, 0f, 0f,
                -0.5f, 0.5f, 0.0f, 0f, 1f,
                0.5f, 0.5f, 0.0f, 1f, 1f,
                0.5f, -0.5f, 0.0f, 1f, 0f
            };

            uint[] indices =
            {
                0, 2, 1,
                0, 3, 2
            };

            return (vertices, indices);
        }

        public static int[] GenerateTextureDataArray(int tex, int length)
        {
            int[] data = new int[length];
            Array.Fill(data, tex);

            return data;
        }

        public static uint[] GenerateIndexDataArray(int faces)
        {
            uint[] indices = new uint[faces * 6];

            for (var f = 0; f < faces; f++)
            {
                var offset = (uint) (f * 4);

                indices[f * 6 + 0] = 0 + offset;
                indices[f * 6 + 1] = 2 + offset;
                indices[f * 6 + 2] = 1 + offset;
                indices[f * 6 + 3] = 0 + offset;
                indices[f * 6 + 4] = 3 + offset;
                indices[f * 6 + 5] = 2 + offset;
            }

            return indices;
        }

        public static int[][] GetBlockUVs(bool isRotated)
        {
            return isRotated ? rotatedBlockUVs : defaultBlockUVs;
        }
    }
}