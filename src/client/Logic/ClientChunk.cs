﻿// <copyright file="ClientChunk.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OpenTK.Mathematics;
using VoxelGame.Client.Rendering;
using VoxelGame.Core.Logic;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Client.Logic;

/// <summary>
///     A chunk of the world, specifically for the client.
/// </summary>
[Serializable]
public class ClientChunk : Chunk
{
    private const int MaxMeshDataStep = 8;

    [NonSerialized] private bool hasMeshData;
    [NonSerialized] private int meshDataIndex;

    /// <summary>
    ///     Create a new client chunk.
    /// </summary>
    /// <param name="world">The world that contains the chunk.</param>
    /// <param name="x">The chunk x position, in chunk coordinates.</param>
    /// <param name="y">The chunk y position, in chunk coordinates.</param>
    /// <param name="z">The chunk z position, in chunk coordinates.</param>
    public ClientChunk(World world, int x, int y, int z) : base(world, x, y, z) {}

    /// <inheritdoc />
    protected override Section CreateSection()
    {
        return new ClientSection(World);
    }

    /// <summary>
    ///     Create a mesh for this chunk and activate it.
    /// </summary>
    public void CreateAndSetMesh()
    {
        for (var s = 0; s < SectionCount; s++)
        {
            (int x, int y, int z) = IndexToLocalPosition(s);
            ((ClientSection) sections[s]).CreateAndSetMesh(x + X * Size, y + Y * Size, z + Z * Size);
        }

        hasMeshData = true;
        meshDataIndex = 0;
    }

    /// <summary>
    ///     Create a mesh for a section of this chunk and activate it.
    /// </summary>
    /// <param name="x">The x position of the section relative in this chunk.</param>
    /// <param name="y">The y position of the section relative in this chunk.</param>
    /// <param name="z">The z position of the section relative in this chunk.</param>
    public void CreateAndSetMesh(int x, int y, int z)
    {
        ((ClientSection) GetSection(x, y, z)).CreateAndSetMesh(x + X * Size, y + Y * Size, z + Z * Size);
    }

    /// <summary>
    ///     Start a task that will create mesh data for this chunk.
    /// </summary>
    /// <returns>The meshing task.</returns>
    public Task<SectionMeshData[]> CreateMeshDataAsync()
    {
        return Task.Run(CreateMeshData);
    }

    private SectionMeshData[] CreateMeshData()
    {
        var sectionMeshes = new SectionMeshData[SectionCount];

        for (var s = 0; s < SectionCount; s++)
        {
            (int x, int y, int z) = IndexToLocalPosition(s);
            sectionMeshes[s] = ((ClientSection) sections[s]).CreateMeshData(x + X * Size, y + Y * Size, z + Z * Size);
        }

        meshDataIndex = 0;

        return sectionMeshes;
    }

    /// <summary>
    ///     Reset the mesh data set-step.
    /// </summary>
    public void ResetMeshDataSetSteps()
    {
        hasMeshData = false;
        meshDataIndex = 0;
    }

    /// <summary>
    ///     Do a mesh data set-step. This will apply a part of the mesh data and activate the part.
    /// </summary>
    /// <param name="sectionMeshes">The mesh data to apply.</param>
    /// <returns>True if this step was the final step.</returns>
    public bool DoMeshDataSetStep(SectionMeshData[] sectionMeshes)
    {
        hasMeshData = false;

        for (var count = 0; count < MaxMeshDataStep; count++)
        {
            ((ClientSection) sections[meshDataIndex]).SetMeshData(sectionMeshes[meshDataIndex]);

            // The index has reached the end, all sections have received their mesh data.
            if (meshDataIndex == SectionCount - 1)
            {
                hasMeshData = true;
                meshDataIndex = 0;

                return true;
            }

            meshDataIndex++;
        }

        return false;
    }

    /// <summary>
    ///     Adds all sections inside of the frustum to the render list.
    /// </summary>
    /// <param name="frustum">The view frustum to use for culling.</param>
    /// <param name="renderList">The list to add the chunks and positions too.</param>
    public void AddCulledToRenderList(Frustum frustum,
        ICollection<(ClientSection section, Vector3 position)> renderList)
    {
        if (!hasMeshData || !frustum.IsBoxInFrustum(VMath.CreateBox3(ChunkPoint, ChunkExtents))) return;

        for (var x = 0; x < Size; x++)
        for (var y = 0; y < Size; y++)
        for (var z = 0; z < Size; z++)
        {
            Vector3 position = new(
                (x + X * Size) * Section.Size,
                (y + Y * Size) * Section.Size,
                (z + Z * Size) * Section.Size);

            if (frustum.IsBoxInFrustum(
                    VMath.CreateBox3(position + Section.Extents, Section.Extents)))
            {
                renderList.Add(((ClientSection) sections[LocalPositionToIndex(x, y, z)], position));
            }
        }
    }

    #region IDisposable Support

    [NonSerialized] private bool disposed; // To detect redundant calls

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        if (!disposed)
        {
            if (disposing)
                for (var s = 0; s < SectionCount; s++)
                    sections[s].Dispose();

            disposed = true;
        }
    }

    #endregion IDisposable Support
}
