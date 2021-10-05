// <copyright file="ClientChunk.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OpenToolkit.Mathematics;
using VoxelGame.Client.Rendering;
using VoxelGame.Core.Logic;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Updates;

namespace VoxelGame.Client.Logic
{
    [Serializable]
    public class ClientChunk : Chunk
    {
        private const int MaxMeshDataStep = 8;

        [NonSerialized] private bool hasMeshData;
        [NonSerialized] private int meshDataIndex;

        public ClientChunk(World world, int x, int z, UpdateCounter updateCounter) : base(world, x, z, updateCounter) {}

        protected override Section CreateSection()
        {
            return new ClientSection(World);
        }

        public void CreateAndSetMesh()
        {
            for (var y = 0; y < VerticalSectionCount; y++) ((ClientSection) sections[y]).CreateAndSetMesh(X, y, Z);

            hasMeshData = true;
            meshDataIndex = 0;
        }

        public void CreateAndSetMesh(int y)
        {
            ((ClientSection) sections[y]).CreateAndSetMesh(X, y, Z);
        }

        public Task<SectionMeshData[]> CreateMeshDataTask()
        {
            return Task.Run(CreateMeshData);
        }

        private SectionMeshData[] CreateMeshData()
        {
            SectionMeshData[] sectionMeshes = new SectionMeshData[VerticalSectionCount];

            for (var y = 0; y < VerticalSectionCount; y++)
                ((ClientSection) sections[y]).CreateMeshData(X, y, Z, out sectionMeshes[y]);

            meshDataIndex = 0;

            return sectionMeshes;
        }

        public void ResetMeshDataSetSteps()
        {
            hasMeshData = false;
            meshDataIndex = 0;
        }

        public bool DoMeshDataSetStep(SectionMeshData[] sectionMeshes)
        {
            hasMeshData = false;

            for (var count = 0; count < MaxMeshDataStep; count++)
            {
                ((ClientSection) sections[meshDataIndex]).SetMeshData(sectionMeshes[meshDataIndex]);

                // The index has reached the end, all sections have received their mesh data.
                if (meshDataIndex == VerticalSectionCount - 1)
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
        public void AddCulledToRenderList(Frustum frustum, List<(ClientSection section, Vector3 position)> renderList)
        {
            if (hasMeshData && frustum.BoxInFrustum(new BoundingBox(ChunkPoint, ChunkExtents)))
            {
                int start = 0, end = VerticalSectionCount - 1;

                for (int y = start; y < VerticalSectionCount; y++)
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
        }

        #region IDisposable Support

        [NonSerialized] private bool disposed; // To detect redundant calls

        protected override void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                    for (var y = 0; y < VerticalSectionCount; y++)
                        sections[y].Dispose();

                disposed = true;
            }
        }

        #endregion IDisposable Support
    }
}