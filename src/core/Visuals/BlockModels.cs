// <copyright file="BlockModels.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using System.Linq;
using OpenTK.Mathematics;
using VoxelGame.Core.Logic;

namespace VoxelGame.Core.Visuals;

/// <summary>
///     Utility methods to get different commonly used models.
/// </summary>
public static class BlockModels
{
    private static readonly int[][] defaultBlockUVs =
        { new[] { 0, 0 }, new[] { 0, 1 }, new[] { 1, 1 }, new[] { 1, 0 } };

    private static readonly int[][] rotatedBlockUVs =
        { new[] { 0, 1 }, new[] { 1, 1 }, new[] { 1, 0 }, new[] { 0, 0 } };

    /// <summary>
    ///     Create a cross block model.
    /// </summary>
    /// <param name="textureIndex">The texture index to use.</param>
    /// <returns>The created model.</returns>
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

    /// <summary>
    ///     Create a cross plant model for a given quality level.
    /// </summary>
    /// <param name="quality">The quality level.</param>
    /// <returns>The model data.</returns>
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
            _ => throw new NotSupportedException()
        };
    }

    private static (float[] vertices, uint[] indices) CreateCrossPlantModel(
        int horizontalSteps, int verticalSteps,
        params ((float x, float z) a, (float x, float z) b)[] parts)
    {
        int faceCount = parts.Length * horizontalSteps * verticalSteps;
        var faces = new float[faceCount][];

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

    /// <summary>
    ///     Create a crop plant model for a given quality level.
    /// </summary>
    /// <param name="quality">The quality level.</param>
    /// <returns>The model data.</returns>
    public static (float[] vertices, uint[] indices) CreateCropPlantModel(Quality quality)
    {
        return quality switch
        {
            Quality.Low => CreateCropPlantModel(horizontalSteps: 1, verticalSteps: 1),
            Quality.Medium => CreateCropPlantModel(horizontalSteps: 4, verticalSteps: 1),
            Quality.High => CreateCropPlantModel(horizontalSteps: 4, verticalSteps: 2),
            Quality.Ultra => CreateCropPlantModel(horizontalSteps: 4, verticalSteps: 2),
            _ => throw new NotSupportedException()
        };
    }

    private static (float[] vertices, uint[] indices) CreateCropPlantModel(
        int horizontalSteps, int verticalSteps)
    {
        int faceCount = horizontalSteps * verticalSteps;
        var faces = new float[faceCount][];

        float hStep = 1f / horizontalSteps;
        float vStep = 1f / verticalSteps;
        var face = 0;

        for (var h = 0; h < horizontalSteps; h++)
        for (var v = 0; v < verticalSteps; v++)
        {
            float z1;
            float z2;

            float x1 = z1 = h * hStep;
            float x2 = z2 = (h + 1) * hStep;

            float y1 = v * vStep;
            float y2 = (v + 1) * vStep;

            faces[face] = new[]
            {
                x1, y1, 0f, 0f, y1, z1, x1, y1,
                x1, y2, 0f, 0f, y2, z1, x1, y2,
                x2, y2, 0f, 0f, y2, z2, x2, y2,
                x2, y1, 0f, 0f, y1, z2, x2, y1
            };

            face++;
        }

        float[] vertices = faces.SelectMany(f => f).ToArray();
        uint[] indices = GenerateDoubleSidedIndexDataArray(faceCount);

        return (vertices, indices);
    }

    /// <summary>
    ///     Create a flat model.
    /// </summary>
    /// <param name="side">The side the model is attached to.</param>
    /// <param name="offset">The offset from the block side.</param>
    /// <returns>The created vertices.</returns>
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

    /// <summary>
    ///     Create a plane model.
    /// </summary>
    /// <returns>The model data.</returns>
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

    /// <summary>
    ///     Generate a texture data array.
    /// </summary>
    /// <param name="tex">The texture index.</param>
    /// <param name="length">The length of the array.</param>
    /// <returns>An array of the given length, filled with the texture index.</returns>
    public static int[] GenerateTextureDataArray(int tex, int length)
    {
        var data = new int[length];
        Array.Fill(data, tex);

        return data;
    }

    /// <summary>
    /// </summary>
    /// <param name="faces"></param>
    /// <returns></returns>
    public static uint[] GenerateDoubleSidedIndexDataArray(int faces)
    {
        var indices = new uint[faces * 12];

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

    /// <summary>
    ///     Generate a index data array for a number of faces that do not share vertices.
    /// </summary>
    /// <param name="faces">The number of faces.</param>
    /// <returns>The index arrays.</returns>
    public static uint[] GenerateIndexDataArray(int faces)
    {
        var indices = new uint[faces * 6];

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

    /// <summary>
    ///     Get block uvs.
    /// </summary>
    /// <param name="isRotated">Whether the block is rotated.</param>
    /// <returns>The block uvs.</returns>
    public static int[][] GetBlockUVs(bool isRotated)
    {
        return isRotated ? rotatedBlockUVs : defaultBlockUVs;
    }
}
