// <copyright file="BlockModels.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;

namespace VoxelGame.Core.Visuals
{
    public static class BlockModels
    {
        private static readonly int[][] defaultBlockUVs = {new[] {0, 0}, new[] {0, 1}, new[] {1, 1}, new[] {1, 0}};
        private static readonly int[][] rotatedBlockUVs = {new[] {0, 1}, new[] {1, 1}, new[] {1, 0}, new[] {0, 0}};

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

        public static void CreatePlaneModel(out float[] vertices, out uint[] indices)
        {
            vertices = new[]
            {
                -0.5f, -0.5f, 0.0f, 0f, 0f,
                -0.5f, 0.5f, 0.0f, 0f, 1f,
                0.5f, 0.5f, 0.0f, 1f, 1f,
                0.5f, -0.5f, 0.0f, 1f, 0f
            };

            indices = new uint[]
            {
                0, 2, 1,
                0, 3, 2
            };
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
