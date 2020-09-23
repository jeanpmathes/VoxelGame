// <copyright file="ClientChunk.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using OpenToolkit.Mathematics;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VoxelGame.Client.Rendering;
using VoxelGame.Core.Logic;
using VoxelGame.Core.Physics;

namespace VoxelGame.Client.Logic
{
    [Serializable]
    public class ClientChunk : Core.Logic.Chunk
    {
        private const int maxMeshDataStep = 4;

        [NonSerialized] private bool hasMeshData;
        [NonSerialized] private int meshDataIndex;

        public ClientChunk(int x, int z) : base(x, z)
        {
        }

        protected override Section CreateSection()
        {
            return new ClientSection();
        }

        public void CreateAndSetMesh()
        {
            for (int y = 0; y < ChunkHeight; y++)
            {
                ((ClientSection)sections[y]).CreateAndSetMesh(X, y, Z);
            }

            hasMeshData = true;
            meshDataIndex = 0;
        }

        public void CreateAndSetMesh(int y)
        {
            ((ClientSection)sections[y]).CreateAndSetMesh(X, y, Z);
        }

        public Task<SectionMeshData[]> CreateMeshDataTask()
        {
            return Task.Run(CreateMeshData);
        }

        private SectionMeshData[] CreateMeshData()
        {
            SectionMeshData[] sectionMeshes = new SectionMeshData[ChunkHeight];

            for (int y = 0; y < ChunkHeight; y++)
            {
                ((ClientSection)sections[y]).CreateMeshData(X, y, Z, out sectionMeshes[y]);
            }

            meshDataIndex = 0;

            return sectionMeshes;
        }

        public void SetMeshData(SectionMeshData[] sectionMeshes)
        {
            for (int y = 0; y < ChunkHeight; y++)
            {
                ((ClientSection)sections[y]).SetMeshData(ref sectionMeshes[y]);
            }

            hasMeshData = true;
            meshDataIndex = 0;
        }

        public bool SetMeshDataStep(SectionMeshData[] sectionMeshes)
        {
            hasMeshData = false;

            for (int count = 0; count < maxMeshDataStep; count++)
            {
                ((ClientSection)sections[meshDataIndex]).SetMeshData(ref sectionMeshes[meshDataIndex]);

                // The index has reached the end, all sections have received their mesh data.
                if (meshDataIndex == ChunkHeight - 1)
                {
                    hasMeshData = true;
                    meshDataIndex = 0;

                    return true;
                }
                else
                {
                    meshDataIndex++;
                }
            }

            return false;
        }

        /// <summary>
        /// Adds all sections inside of the frustum to the render list.
        /// </summary>
        /// <param name="frustum"></param>
        /// <param name="renderList"></param>
        public void AddCulledToRenderList(Frustum frustum, ref List<(ClientSection section, Vector3 position)> renderList)
        {
            if (hasMeshData && frustum.BoxInFrustrum(new BoundingBox(ChunkPoint, ChunkExtents)))
            {
                int start = 0, end = Section.SectionSize - 1;

                for (int y = start; y < ChunkHeight; y++)
                {
                    if (frustum.BoxInFrustrum(new BoundingBox(new Vector3(X * Section.SectionSize, y * Section.SectionSize, Z * Section.SectionSize) + Section.Extents, Section.Extents)))
                    {
                        start = y;

                        break;
                    }
                }

                for (int y = end; y >= 0; y--)
                {
                    if (frustum.BoxInFrustrum(new BoundingBox(new Vector3(X * Section.SectionSize, y * Section.SectionSize, Z * Section.SectionSize) + Section.Extents, Section.Extents)))
                    {
                        end = y;

                        break;
                    }
                }

                for (int y = start; y <= end; y++)
                {
                    renderList.Add(((ClientSection)sections[y], new Vector3(X * Section.SectionSize, y * Section.SectionSize, Z * Section.SectionSize)));
                }
            }
        }

        #region IDisposable Support

        [NonSerialized] private bool disposed; // To detect redundant calls

        protected override void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    for (int y = 0; y < ChunkHeight; y++)
                    {
                        sections[y].Dispose();
                    }
                }

                disposed = true;
            }
        }

        #endregion IDisposable Support
    }
}