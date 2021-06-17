// <copyright file="BlockModels.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

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
                -0.5f,  0.5f, 0.0f, 0f, 1f,
                0.5f,  0.5f, 0.0f, 1f, 1f,
                0.5f, -0.5f, 0.0f, 1f, 0f
            };

            indices = new uint[]
            {
                0, 2, 1,
                0, 3, 2
            };
        }
    }
}