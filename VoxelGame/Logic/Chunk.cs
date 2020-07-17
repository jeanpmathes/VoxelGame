// <copyright file="Chunk.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using OpenToolkit.Mathematics;
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VoxelGame.Rendering;
using VoxelGame.WorldGeneration;
using VoxelGame.Physics;

namespace VoxelGame.Logic
{
    [Serializable]
    public class Chunk : IDisposable
    {
        private static readonly ILogger logger = Program.CreateLogger<Chunk>();

        public const int ChunkHeight = 32;

        private const int maxMeshDataStep = 4;

        /// <summary>
        /// The X position of this chunk in chunk units
        /// </summary>
        public int X { get; }

        /// <summary>
        /// The Y position of this chunk in chunk units
        /// </summary>
        public int Z { get; }

        /// <summary>
        /// Gets the position of the chunk as a point located in the center of the chunk.
        /// </summary>
        public Vector3 ChunkPoint { get => new Vector3((X * Section.SectionSize) + (Section.SectionSize / 2f), ChunkHeight * Section.SectionSize / 2f, (Z * Section.SectionSize) + (Section.SectionSize / 2f)); }

        public static Vector3 ChunkExtents { get => new Vector3(Section.SectionSize / 2f, ChunkHeight * Section.SectionSize / 2f, Section.SectionSize / 2f); }

        private readonly Section[] sections = new Section[ChunkHeight];

        [NonSerialized] private bool hasMeshData = false;
        [NonSerialized] private int meshDataIndex = 0;

        public Chunk(int x, int z)
        {
            X = x;
            Z = z;

            for (int y = 0; y < ChunkHeight; y++)
            {
                sections[y] = new Section();
            }
        }

        /// <summary>
        /// Calls setup on all sections. This is required after loading.
        /// </summary>
        public void Setup()
        {
            for (int y = 0; y < ChunkHeight; y++)
            {
                sections[y].Setup();
            }
        }

        /// <summary>
        /// Loads a chunk from a file specified by the path. If the loaded chunk does not fit the x and z parameters, null is returned.
        /// </summary>
        /// <param name="path">The path to the chunk file to load and check. The path itself is not checked.</param>
        /// <param name="x">The x coordinate of the chunk.</param>
        /// <param name="z">The z coordinate of the chunk.</param>
        /// <returns>The loaded chunk if its coordinates fit the requirements; null if they don't.</returns>
        public static Chunk? Load(string path, int x, int z)
        {
            logger.LogDebug("Loading chunk for position: ({x}|{z})", x, z);

            Chunk chunk;

            using (Stream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                IFormatter formatter = new BinaryFormatter();
                chunk = (Chunk)formatter.Deserialize(stream);
            }

            // Checking the chunk
            if (chunk.X == x && chunk.Z == z)
            {
                return chunk;
            }
            else
            {
                logger.LogWarning("The file for the chunk at ({x}|{z}) was not valid as the position did not match.", x, z);

                return null;
            }
        }

        /// <summary>
        /// Runs a task that loads a chunk from a file specified by the path. If the loaded chunk does not fit the x and z parameters, null is returned.
        /// </summary>
        /// <param name="path">The path to the chunk file to load and check. The path itself is not checked.</param>
        /// <param name="x">The x coordinate of the chunk.</param>
        /// <param name="z">The z coordinate of the chunk.</param>
        /// <returns>A task containing the loaded chunk if its coordinates fit the requirements; null if they don't.</returns>
        public static Task<Chunk?> LoadTask(string path, int x, int z)
        {
            return Task.Run(() => Load(path, x, z));
        }

        /// <summary>
        /// Saves this chunk in the directory specified by the path.
        /// </summary>
        /// <param name="path">The path of the directory where this chunk should be saved.</param>
        public void Save(string path)
        {
            string chunkFile = path + $@"\x{X}z{Z}.chunk";

            logger.LogDebug("Saving the chunk ({x}|{z}) to: {path}", X, Z, chunkFile);

            using Stream stream = new FileStream(chunkFile, FileMode.Create, FileAccess.Write, FileShare.Read);
            IFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, this);
        }

        /// <summary>
        /// Runs a task which saves this chunk in the directory specified by the path.
        /// </summary>
        /// <param name="path">The path of the directory where this chunk should be saved.</param>
        /// <returns>A task.</returns>
        public Task SaveTask(string path)
        {
            return Task.Run(() => Save(path));
        }

        public void Generate(IWorldGenerator generator)
        {
            logger.LogDebug("Generating the chunk ({x}|{z}) using the '{name}' generator.", X, Z, generator);

            for (int x = 0; x < Section.SectionSize; x++)
            {
                for (int z = 0; z < Section.SectionSize; z++)
                {
                    int y = 0;

                    foreach (Block block in generator.GenerateColumn(x + (X * Section.SectionSize), z + (Z * Section.SectionSize)))
                    {
                        sections[y >> 5][x, y & (Section.SectionSize - 1), z] = block.Id;

                        y++;
                    }
                }
            }
        }

        public Task GenerateTask(IWorldGenerator generator)
        {
            return Task.Run(() => Generate(generator));
        }

        public void CreateAndSetMesh()
        {
            for (int y = 0; y < ChunkHeight; y++)
            {
                sections[y].CreateAndSetMesh(X, y, Z);
            }

            hasMeshData = true;
            meshDataIndex = 0;
        }

        public void CreateAndSetMesh(int y)
        {
            sections[y].CreateAndSetMesh(X, y, Z);
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
                sections[y].CreateMeshData(X, y, Z, out sectionMeshes[y]);
            }

            meshDataIndex = 0;

            return sectionMeshes;
        }

        public void SetMeshData(SectionMeshData[] sectionMeshes)
        {
            for (int y = 0; y < ChunkHeight; y++)
            {
                sections[y].SetMeshData(ref sectionMeshes[y]);
            }

            hasMeshData = true;
            meshDataIndex = 0;
        }

        public bool SetMeshDataStep(SectionMeshData[] sectionMeshes)
        {
            for (int i = 0; i < maxMeshDataStep; i++)
            {
                sections[meshDataIndex].SetMeshData(ref sectionMeshes[meshDataIndex]);

                // The index has reached the end, all sections have received their mesh data
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
        /// Renders all sections of this chunk.
        /// </summary>
        [Obsolete("Use RenderCulled instead.")]
        public void Render()
        {
            if (hasMeshData)
            {
                for (int y = 0; y < ChunkHeight; y++)
                {
                    sections[y].Render(new Vector3(X * Section.SectionSize, y * Section.SectionSize, Z * Section.SectionSize));
                }
            }
        }

        /// <summary>
        /// Renders only the sections that are inside the given <see cref="Frustum"/>.
        /// </summary>
        public void RenderCulled(Frustum frustum)
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
                    sections[y].Render(new Vector3(X * Section.SectionSize, y * Section.SectionSize, Z * Section.SectionSize));
                }
            }
        }

        public void Tick()
        {
            for (int y = 0; y < ChunkHeight; y++)
            {
                sections[y].Tick(X, y, Z);
            }
        }

        public Section GetSection(int y)
        {
            return sections[y];
        }

        public override string ToString()
        {
            return $"Chunk ({X}|{Z})";
        }

        public override bool Equals(object? obj)
        {
            if (obj != null && obj is Chunk other)
            {
                return other.X == this.X && other.Z == this.Z;
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Z);
        }

        #region IDisposable Support

        [NonSerialized] private bool disposed = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
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

        ~Chunk()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable Support
    }
}