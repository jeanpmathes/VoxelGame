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
        public static float[][] CubeVertices()
        {
            return new[]
            {
                new[] // Front face
                {
                    0f, 0f, 1f, 0f, 0f, 0f, 0f, 1f,
                    0f, 1f, 1f, 0f, 1f, 0f, 0f, 1f,
                    1f, 1f, 1f, 1f, 1f, 0f, 0f, 1f,
                    1f, 0f, 1f, 1f, 0f, 0f, 0f, 1f
                },
                new[] // Back face
                {
                    1f, 0f, 0f, 0f, 0f, 0f, 0f, -1f,
                    1f, 1f, 0f, 0f, 1f, 0f, 0f, -1f,
                    0f, 1f, 0f, 1f, 1f, 0f, 0f, -1f,
                    0f, 0f, 0f, 1f, 0f, 0f, 0f, -1f
                },
                new[] // Left face
                {
                    0f, 0f, 0f, 0f, 0f, -1f, 0f, 0f,
                    0f, 1f, 0f, 0f, 1f, -1f, 0f, 0f,
                    0f, 1f, 1f, 1f, 1f, -1f, 0f, 0f,
                    0f, 0f, 1f, 1f, 0f, -1f, 0f, 0f
                },
                new[] // Right face
                {
                    1f, 0f, 1f, 0f, 0f, 1f, 0f, 0f,
                    1f, 1f, 1f, 0f, 1f, 1f, 0f, 0f,
                    1f, 1f, 0f, 1f, 1f, 1f, 0f, 0f,
                    1f, 0f, 0f, 1f, 0f, 1f, 0f, 0f
                },
                new[] // Bottom face
                {
                    0f, 0f, 0f, 0f, 0f, 0f, -1f, 0f,
                    0f, 0f, 1f, 0f, 1f, 0f, -1f, 0f,
                    1f, 0f, 1f, 1f, 1f, 0f, -1f, 0f,
                    1f, 0f, 0f, 1f, 0f, 0f, -1f, 0f
                },
                new[] // Top face
                {
                    0f, 1f, 1f, 0f, 0f, 0f, 1f, 0f,
                    0f, 1f, 0f, 0f, 1f, 0f, 1f, 0f,
                    1f, 1f, 0f, 1f, 1f, 0f, 1f, 0f,
                    1f, 1f, 1f, 1f, 0f, 0f, 1f, 0f
                }
            };
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

                indices[(f * 6) + 0] = 0 + offset;
                indices[(f * 6) + 1] = 2 + offset;
                indices[(f * 6) + 2] = 1 + offset;
                indices[(f * 6) + 3] = 0 + offset;
                indices[(f * 6) + 4] = 3 + offset;
                indices[(f * 6) + 5] = 2 + offset;
            }

            return indices;
        }

        private static readonly int[][] defaultBlockUVs = {new[] {0, 0}, new[] {0, 1}, new[] {1, 1}, new[] {1, 0}};
        private static readonly int[][] rotatedBlockUVs = {new[] {0, 1}, new[] {1, 1}, new[] {1, 0}, new[] {0, 0}};

        public static int[][] GetBlockUVs(bool isRotated)
        {
            return isRotated ? rotatedBlockUVs : defaultBlockUVs;
        }
    }
}