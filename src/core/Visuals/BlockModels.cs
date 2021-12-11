// <copyright file="BlockModels.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using System.Linq;
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

            uint[] indices = GenerateDoubleSidedIndexDataArray(faces: 2);
            int[] textureIndices = GenerateTextureDataArray(textureIndex, length: 8);

            return (vertices, indices, textureIndices);
        }

        public static (float[] vertices, uint[] indices) CreateCrossPlantModel(Quality quality)
        {
            return quality switch
            {
                Quality.Low => CreateCrossPlantModel(
                    horizontalSteps: 1,
                    verticalSteps: 1,
                    ((0.145f, 0.855f), (0.855f, 0.145f)),
                    ((0.145f, 0.145f), (0.855f, 0.855f))),
                Quality.Medium => CreateCrossPlantModel(
                    horizontalSteps: 2,
                    verticalSteps: 1,
                    ((0.145f, 0.855f), (0.855f, 0.145f)),
                    ((0.145f, 0.145f), (0.855f, 0.855f))),
                Quality.High => CreateCrossPlantModel(
                    horizontalSteps: 2,
                    verticalSteps: 2,
                    ((0.145f, 0.855f), (0.855f, 0.145f)),
                    ((0.145f, 0.145f), (0.855f, 0.855f))),
                Quality.Ultra => CreateCrossPlantModel(
                    horizontalSteps: 2,
                    verticalSteps: 2,
                    ((0.145f, 0.855f), (0.855f, 0.145f)),
                    ((0.145f, 0.145f), (0.855f, 0.855f)),
                    ((0.0f, 0.5f), (1.0f, 0.5f)),
                    ((0.5f, 0.0f), (0.5f, 1.0f))),
                _ => throw new NotImplementedException()
            };
        }

        private static (float[] vertices, uint[] indices) CreateCrossPlantModel(
            int horizontalSteps, int verticalSteps,
            params ((float x, float z) a, (float x, float z) b)[] parts)
        {
            int faceCount = parts.Length * horizontalSteps * verticalSteps;
            float[][] faces = new float[faceCount][];

            float hStep = 1f / horizontalSteps;
            float vStep = 1f / verticalSteps;
            var face = 0;

            foreach (((float x, float z) a, (float x, float z) b) in parts)
                for (var h = 0; h < horizontalSteps; h++)
                for (var v = 0; v < verticalSteps; v++)
                {
                    float x1 = h * hStep;
                    float x2 = (h + 1) * hStep;

                    float y1 = v * vStep;
                    float y2 = (v + 1) * vStep;

                    (float x, float z) begin = Lerp(a, b, x1);
                    (float x, float z) finis = Lerp(a, b, x2);

                    faces[face] = new[]
                    {
                        begin.x, y1, begin.z, x1, y1,
                        begin.x, y2, begin.z, x1, y2,
                        finis.x, y2, finis.z, x2, y2,
                        finis.x, y1, finis.z, x2, y1
                    };

                    face++;
                }

            float[] vertices = faces.SelectMany(f => f).ToArray();
            uint[] indices = GenerateDoubleSidedIndexDataArray(faceCount);

            return (vertices, indices);

            (float x, float z) Lerp((float x, float z) a, (float x, float z) b, float t)
            {
                return (a.x + (b.x - a.x) * t, a.z + (b.z - a.z) * t);
            }
        }

        public static (float[] vertices, uint[] indices) CreateCropPlantModel(Quality quality)
        {
            return quality switch
            {
                Quality.Low => CreateLowCropPlantModel(),
                Quality.Medium => CreateMediumCropPlantModel(),
                _ => CreateLowCropPlantModel()
            };
        }

        private static (float[] vertices, uint[] indices) CreateLowCropPlantModel()
        {
            float[] vertices =
            {
                0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f,
                0f, 1f, 0f, 0f, 1f, 0f, 0f, 1f,
                1f, 1f, 0f, 0f, 1f, 1f, 1f, 1f,
                1f, 0f, 0f, 0f, 0f, 1f, 1f, 0f
            };

            uint[] indices = GenerateDoubleSidedIndexDataArray(faces: 1);

            return (vertices, indices);
        }

        private static (float[] vertices, uint[] indices) CreateMediumCropPlantModel()
        {
            const int faceCount = 4;
            float[][] faces = new float[faceCount][];

            for (var f = 0; f < faceCount; f++)
            {
                const float xzStep = 1f / 16f * (16f / faceCount);
                float a = f * xzStep;
                float b = (f + 1) * xzStep;

                const float uvStep = 1f / faceCount;
                float p = f * uvStep;
                float q = (f + 1) * uvStep;

                faces[f] = new[]
                {
                    a, 0f, 0f, 0f, 0f, a, p, 0f,
                    a, 1f, 0f, 0f, 1f, a, p, 1f,
                    b, 1f, 0f, 0f, 1f, b, q, 1f,
                    b, 0f, 0f, 0f, 0f, b, q, 0f
                };
            }

            float[] vertices = faces.SelectMany(f => f).ToArray();
            uint[] indices = GenerateDoubleSidedIndexDataArray(faceCount);

            return (vertices, indices);
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

        public static uint[] GenerateDoubleSidedIndexDataArray(int faces)
        {
            uint[] indices = new uint[faces * 12];

            for (var f = 0; f < faces; f++)
            {
                var offset = (uint) (f * 4);

                indices[f * 12 + 0] = 0 + offset;
                indices[f * 12 + 1] = 2 + offset;
                indices[f * 12 + 2] = 1 + offset;
                indices[f * 12 + 3] = 0 + offset;
                indices[f * 12 + 4] = 3 + offset;
                indices[f * 12 + 5] = 2 + offset;

                indices[f * 12 + 6] = 0 + offset;
                indices[f * 12 + 7] = 1 + offset;
                indices[f * 12 + 8] = 2 + offset;
                indices[f * 12 + 9] = 0 + offset;
                indices[f * 12 + 10] = 2 + offset;
                indices[f * 12 + 11] = 3 + offset;
            }

            return indices;
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
