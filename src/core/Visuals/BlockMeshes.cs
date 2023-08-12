// <copyright file="BlockMeshes.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System.Collections.Generic;
using OpenTK.Mathematics;
using VoxelGame.Core.Logic;
using VoxelGame.Core.Visuals.Meshables;

namespace VoxelGame.Core.Visuals;

/// <summary>
///     Utility methods to get different commonly used meshes.
/// </summary>
public static class BlockMeshes
{
    private static readonly int[][] defaultBlockUVs =
        {new[] {0, 0}, new[] {0, 1}, new[] {1, 1}, new[] {1, 0}};

    private static readonly int[][] rotatedBlockUVs =
        {new[] {0, 1}, new[] {1, 1}, new[] {1, 0}, new[] {0, 0}};

    private static BlockMesh.Quad[] CreateDoubleSidedQuads(BlockMesh.Quad[] quads)
    {
        List<BlockMesh.Quad> newQuads = new();

        foreach (BlockMesh.Quad quad in quads)
        {
            newQuads.Add(quad);

            newQuads.Add(new BlockMesh.Quad
            {
                A = quad.D,
                B = quad.C,
                C = quad.B,
                D = quad.A
            });
        }

        return newQuads.ToArray();
    }

    /// <summary>
    ///     Create a cross mesh.
    /// </summary>
    /// <param name="textureIndex">The texture index to use.</param>
    /// <returns>The created mesh.</returns>
    public static BlockMesh CreateCrossMesh(int textureIndex)
    {
        BlockMesh.Quad[] quads =
        {
            new()
            {
                A = new Vector3(x: 0.145f, y: 0f, z: 0.855f),
                B = new Vector3(x: 0.145f, y: 1f, z: 0.855f),
                C = new Vector3(x: 0.855f, y: 1f, z: 0.145f),
                D = new Vector3(x: 0.855f, y: 0f, z: 0.145f)
            },
            new()
            {
                A = new Vector3(x: 0.145f, y: 0f, z: 0.145f),
                B = new Vector3(x: 0.145f, y: 1f, z: 0.145f),
                C = new Vector3(x: 0.855f, y: 1f, z: 0.855f),
                D = new Vector3(x: 0.855f, y: 0f, z: 0.855f)
            }
        };

        quads = CreateDoubleSidedQuads(quads);

        for (var quad = 0; quad < quads.Length; quad++)
        {
            Meshing.SetTextureIndex(ref quads[quad].data, textureIndex);
            Meshing.SetFullUVs(ref quads[quad].data, quad % 2 != 0);
        }

        return new BlockMesh(quads);
    }

    /// <summary>
    ///     Create a flat mesh.
    /// </summary>
    /// <param name="side">The side the mesh is attached to.</param>
    /// <param name="offset">The offset from the block side.</param>
    /// <param name="textureIndex">The texture index to use.</param>
    /// <returns>The created mesh.</returns>
    public static BlockMesh CreateFlatModel(BlockSide side, float offset, int textureIndex)
    {
        side.Corners(out int[] a, out int[] b, out int[] c, out int[] d);

        var n = side.Direction().ToVector3();
        Vector3 vOffset = n * offset * -1;

        Vector3 v1 = (a[0], a[1], a[2]) + vOffset;
        Vector3 v2 = (b[0], b[1], b[2]) + vOffset;
        Vector3 v3 = (c[0], c[1], c[2]) + vOffset;
        Vector3 v4 = (d[0], d[1], d[2]) + vOffset;

        BlockMesh.Quad[] quads =
        {
            new()
            {
                A = v1,
                B = v2,
                C = v3,
                D = v4
            }
        };

        quads = CreateDoubleSidedQuads(quads);

        for (var quad = 0; quad < quads.Length; quad++)
        {
            Meshing.SetTextureIndex(ref quads[quad].data, textureIndex);
            Meshing.SetFullUVs(ref quads[quad].data, quad % 2 != 0);
        }

        return new BlockMesh(quads);
    }

    /// <summary>
    ///     Create a plane model.
    /// </summary>
    /// <returns>The model data.</returns>
    public static (float[] vertices, uint[] indices) CreatePlaneModel() // todo: move to Draw2D or a screen util class
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
    ///     Get block uvs.
    /// </summary>
    /// <param name="isRotated">Whether the block is rotated.</param>
    /// <returns>The block uvs.</returns>
    public static int[][] GetBlockUVs(bool isRotated)
    {
        return isRotated ? rotatedBlockUVs : defaultBlockUVs;
    }
}
