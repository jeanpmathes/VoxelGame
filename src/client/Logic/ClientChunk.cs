// <copyright file="ClientChunk.cs" company="VoxelGame">
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

namespace VoxelGame.Client.Logic
{
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
        /// <param name="z">The chunk z position, in chunk coordinates.</param>
        public ClientChunk(World world, int x, int z) : base(world, x, z) {}

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
            for (var y = 0; y < HeightInSections; y++) ((ClientSection) sections[y]).CreateAndSetMesh(X, y, Z);

            hasMeshData = true;
            meshDataIndex = 0;
        }

        /// <summary>
        ///     Create a mesh for a section of this chunk and activate it.
        /// </summary>
        /// <param name="y"></param>
        public void CreateAndSetMesh(int y)
        {
            ((ClientSection) sections[y]).CreateAndSetMesh(X, y, Z);
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
            var sectionMeshes = new SectionMeshData[HeightInSections];

            for (var y = 0; y < HeightInSections; y++)
                sectionMeshes[y] = ((ClientSection) sections[y]).CreateMeshData(X, y, Z);

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
                if (meshDataIndex == HeightInSections - 1)
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
            if (!hasMeshData || !frustum.BoxInFrustum(new BoundingBox(ChunkPoint, ChunkExtents))) return;

            var start = 0;
            int end = HeightInSections - 1;

            for (int y = start; y < HeightInSections; y++)
                if (frustum.BoxInFrustum(
                        new BoundingBox(
                            new Vector3(X * Section.SectionSize, y * Section.SectionSize, Z * Section.SectionSize) +
                            Section.Extents,
                            Section.Extents)))
                {
                    start = y;

                    break;
                }

            for (int y = end; y >= 0; y--)
                if (frustum.BoxInFrustum(
                        new BoundingBox(
                            new Vector3(X * Section.SectionSize, y * Section.SectionSize, Z * Section.SectionSize) +
                            Section.Extents,
                            Section.Extents)))
                {
                    end = y;

                    break;
                }

            for (int y = start; y <= end; y++)
                renderList.Add(
                    ((ClientSection) sections[y],
                        new Vector3(X * Section.SectionSize, y * Section.SectionSize, Z * Section.SectionSize)));
        }

        #region IDisposable Support

        [NonSerialized] private bool disposed; // To detect redundant calls

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                    for (var y = 0; y < HeightInSections; y++)
                        sections[y].Dispose();

                disposed = true;
            }
        }

        #endregion IDisposable Support
    }
}
