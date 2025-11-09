// <copyright file="Meshes.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using OpenTK.Mathematics;
using VoxelGame.Core.Logic.Voxels;
using VoxelGame.Core.Utilities.Resources;
using VoxelGame.Core.Visuals.Meshables;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Core.Visuals;

/// <summary>
///     Utility methods to get different commonly used meshes.
/// </summary>
public static class Meshes
{
    private static readonly Int32[][] defaultBlockUVs =
        [[0, 0], [0, 1], [1, 1], [1, 0]];

    private static readonly Int32[][] rotatedBlockUVs =
        [[0, 1], [1, 1], [1, 0], [0, 0]];

    private static Mesh.Quad[] CreateDoubleSidedQuads(Mesh.Quad[] quads)
    {
        List<Mesh.Quad> newQuads = [];

        foreach (Mesh.Quad quad in quads)
        {
            newQuads.Add(quad);

            newQuads.Add(new Mesh.Quad
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
    public static Mesh CreateCrossMesh(Int32 textureIndex)
    {
        Mesh.Quad[] quads =
        [
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
        ];

        quads = CreateDoubleSidedQuads(quads);

        for (var quad = 0; quad < quads.Length; quad++)
        {
            Meshing.SetTextureIndex(ref quads[quad].data, textureIndex);
            Meshing.SetFullUVs(ref quads[quad].data, quad % 2 != 0);
        }

        return new Mesh(quads);
    }

    /// <summary>
    ///     Create a flat mesh.
    /// </summary>
    /// <param name="side">The side the mesh is attached to, meaning the side of the other block, not within this block.</param>
    /// <param name="offset">The offset from the block side.</param>
    /// <param name="textureIndex">The texture index to use.</param>
    /// <returns>The created mesh.</returns>
    public static Mesh CreateFlatMesh(Side side, Single offset, Int32 textureIndex)
    {
        side.Corners(out Int32[] a, out Int32[] b, out Int32[] c, out Int32[] d);

        var normal = side.Direction().ToVector3();
        Vector3 vOffset = normal * offset * -1;

        Vector3 v1 = (a[0], a[1], a[2]) + vOffset;
        Vector3 v2 = (b[0], b[1], b[2]) + vOffset;
        Vector3 v3 = (c[0], c[1], c[2]) + vOffset;
        Vector3 v4 = (d[0], d[1], d[2]) + vOffset;

        Mesh.Quad[] quads =
        [
            new()
            {
                A = v1,
                B = v2,
                C = v3,
                D = v4
            }
        ];

        quads = CreateDoubleSidedQuads(quads);

        for (var quad = 0; quad < quads.Length; quad++)
        {
            Meshing.SetTextureIndex(ref quads[quad].data, textureIndex);
            Meshing.SetFullUVs(ref quads[quad].data, quad % 2 != 0);
        }

        return new Mesh(quads);
    }

    /// <summary>
    ///     Create a flat mesh on all given sides.
    /// </summary>
    /// <param name="sides">The sides the mesh is attached to.</param>
    /// <param name="offset">The offset from the block side.</param>
    /// <param name="textureIndex">The texture index to use.</param>
    /// <returns>The created mesh.</returns>
    public static Mesh CreateFlatMesh(Sides sides, Single offset, Int32 textureIndex)
    {
        List<Mesh> meshes = [];

        foreach (Side side in Side.All.Sides())
        {
            if (!sides.HasFlag(side.ToFlag())) continue;

            meshes.Add(CreateFlatMesh(side, offset, textureIndex));
        }

        return Mesh.Combine(meshes.ToArray());
    }

    /// <summary>
    ///     Create a cross plant mesh for a given quality level.
    /// </summary>
    /// <param name="quality">The quality level.</param>
    /// <param name="textureIndex">The texture index to use.</param>
    /// <param name="lowered">Whether the plant is lowered by one 16th of a block.</param>
    /// <returns>The model data.</returns>
    public static Mesh CreateCrossPlantMesh(Quality quality, Int32 textureIndex, Boolean lowered)
    {
        Vector3 offset = lowered ? new Vector3(x: 0, -1 * (1 / 16f), z: 0) : Vector3.Zero;

        return quality switch
        {
            Quality.Low => CreateCrossMesh(textureIndex).WithOffset(offset),
            Quality.Medium => CreateCrossMesh(textureIndex).WithOffset(offset),
            Quality.High => CreateCrossMesh(textureIndex).WithOffset(offset).SubdivideV(),
            Quality.Ultra => CreateCrossMesh(textureIndex).WithOffset(offset).SubdivideU().SubdivideV(),
            _ => throw Exceptions.UnsupportedEnumValue(quality)
        };
    }

    /// <summary>
    ///     Create a crop plant model for a given quality level.
    /// </summary>
    /// <param name="quality">The quality level.</param>
    /// <param name="createMiddlePiece">
    ///     Whether to create a middle piece, in effect switching between two parallel planes or
    ///     three planes.
    /// </param>
    /// <param name="textureIndex">The texture index to use.</param>
    /// <param name="lowered">Whether the plant is lowered.</param>
    /// <returns>The model data.</returns>
    public static Mesh CreateCropPlantMesh(Quality quality, Boolean createMiddlePiece, Int32 textureIndex, Boolean lowered)
    {
        Vector3 offset = lowered ? new Vector3(x: 0, -1 * (1 / 16f), z: 0) : Vector3.Zero;

        return quality switch
        {
            Quality.Low => CreateCropPlantMesh(textureIndex, createMiddlePiece).WithOffset(offset),
            Quality.Medium => CreateCropPlantMesh(textureIndex, createMiddlePiece).WithOffset(offset),
            Quality.High => CreateCropPlantMesh(textureIndex, createMiddlePiece).WithOffset(offset).SubdivideV(),
            Quality.Ultra => CreateCropPlantMesh(textureIndex, createMiddlePiece).WithOffset(offset).SubdivideU().SubdivideV(),
            _ => throw Exceptions.UnsupportedEnumValue(quality)
        };
    }

    private static Mesh CreateCropPlantMesh(Int32 textureIndex, Boolean addMiddlePiece)
    {
        Mesh.Quad[] quads = CreateCropPlantQuads(addMiddlePiece);

        quads = CreateDoubleSidedQuads(quads);

        for (var quad = 0; quad < quads.Length; quad++)
        {
            Meshing.SetTextureIndex(ref quads[quad].data, textureIndex);
            Meshing.SetFullUVs(ref quads[quad].data, quad % 2 != 0);
        }

        return new Mesh(quads);
    }

    private static Mesh.Quad[] CreateCropPlantQuads(Boolean addMiddlePiece)
    {
        List<Mesh.Quad> list =
        [
            new()
            {
                A = new Vector3(x: 0.25f, y: 0f, z: 0.0f),
                B = new Vector3(x: 0.25f, y: 1f, z: 0.0f),
                C = new Vector3(x: 0.25f, y: 1f, z: 1.0f),
                D = new Vector3(x: 0.25f, y: 0f, z: 1.0f)
            },

            new()
            {
                A = new Vector3(x: 0.0f, y: 0f, z: 0.25f),
                B = new Vector3(x: 0.0f, y: 1f, z: 0.25f),
                C = new Vector3(x: 1.0f, y: 1f, z: 0.25f),
                D = new Vector3(x: 1.0f, y: 0f, z: 0.25f)
            },

            new()
            {
                A = new Vector3(x: 0.75f, y: 0f, z: 0.0f),
                B = new Vector3(x: 0.75f, y: 1f, z: 0.0f),
                C = new Vector3(x: 0.75f, y: 1f, z: 1.0f),
                D = new Vector3(x: 0.75f, y: 0f, z: 1.0f)
            },

            new()
            {
                A = new Vector3(x: 0.0f, y: 0f, z: 0.75f),
                B = new Vector3(x: 0.0f, y: 1f, z: 0.75f),
                C = new Vector3(x: 1.0f, y: 1f, z: 0.75f),
                D = new Vector3(x: 1.0f, y: 0f, z: 0.75f)
            }
        ];

        if (!addMiddlePiece) return list.ToArray();

        list.Add(new Mesh.Quad
        {
            A = new Vector3(x: 0.5f, y: 0f, z: 0.0f),
            B = new Vector3(x: 0.5f, y: 1f, z: 0.0f),
            C = new Vector3(x: 0.5f, y: 1f, z: 1.0f),
            D = new Vector3(x: 0.5f, y: 0f, z: 1.0f)
        });

        list.Add(new Mesh.Quad
        {
            A = new Vector3(x: 0.0f, y: 0f, z: 0.5f),
            B = new Vector3(x: 0.0f, y: 1f, z: 0.5f),
            C = new Vector3(x: 1.0f, y: 1f, z: 0.5f),
            D = new Vector3(x: 1.0f, y: 0f, z: 0.5f)
        });

        return list.ToArray();
    }

    /// <summary>
    ///     Get block uvs.
    /// </summary>
    /// <param name="isRotated">Whether the block is rotated.</param>
    /// <returns>The block uvs.</returns>
    public static Int32[][] GetBlockUVs(Boolean isRotated)
    {
        return isRotated ? rotatedBlockUVs : defaultBlockUVs;
    }

    /// <summary>
    ///     Create a fallback mesh. It does not depend on any loaded textures and can be used as a placeholder.
    /// </summary>
    /// <returns>The created mesh.</returns>
    public static Mesh CreateFallback()
    {
        return Model.CreateFallback().CreateMesh(FallbackTextureIndexProvider.Instance);
    }

    private sealed class FallbackTextureIndexProvider : ITextureIndexProvider
    {
        public static ITextureIndexProvider Instance { get; } = new FallbackTextureIndexProvider();

        public IResourceContext? Context { get; set; }
        
        public void SetUp() {}

        public Int32 GetTextureIndex(TID identifier)
        {
            return ITextureIndexProvider.MissingTextureIndex;
        }
    }
}
