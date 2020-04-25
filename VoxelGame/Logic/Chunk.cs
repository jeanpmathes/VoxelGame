// <copyright file="Chunk.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using OpenTK;
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using VoxelGame.WorldGeneration;

namespace VoxelGame.Logic
{
    [Serializable]
    public class Chunk : IDisposable
    {
        public const int ChunkHeight = 32;

        private const int maxMeshDataStep = 8;

        /// <summary>
        /// The X position of this chunk in chunk units
        /// </summary>
        public int X { get; }

        /// <summary>
        /// The Y position of this chunk in chunk units
        /// </summary>
        public int Z { get; }

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
        public static Chunk Load(string path, int x, int z)
        {
            Chunk chunk;

            using (Stream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                IFormatter formatter = new BinaryFormatter();
                chunk = (Chunk) formatter.Deserialize(stream);
            }

            // Checking the chunk
            if (chunk.X == x && chunk.Z == z)
            {
                return chunk;
            }
            else
            {
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
        public static Task<Chunk> LoadAsync(string path, int x, int z)
        {        
            return Task.Run(() =>
            {
                return Load(path, x, z);
            });
        }

        /// <summary>
        /// Saves this chunk in the directory specified by the path.
        /// </summary>
        /// <param name="path">The path of the directory where this chunk should be saved.</param>
        public void Save(string path)
        {
            using (Stream stream = new FileStream(path + $@"\x{X}z{Z}.chunk", FileMode.Create, FileAccess.Write, FileShare.Read))
            {
                IFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, this);
            }
        }

        /// <summary>
        /// Runs a task which saves this chunk in the directory specified by the path.
        /// </summary>
        /// <param name="path">The path of the directory where this chunk should be saved.</param>
        /// <returns>A task.</returns>
        public Task SaveAsync(string path)
        {
            return Task.Run(() =>
            {
                Save(path);
            });
        }

        public void Generate(IWorldGenerator generator)
        {
            if (generator == null)
            {
                throw new ArgumentNullException(paramName: nameof(generator));
            }

            for (int x = 0; x < Section.SectionSize; x++)
            {
                for (int z = 0; z < Section.SectionSize; z++)
                {
                    int y = 0;

                    foreach (Block block in generator.GenerateColumn(x + (X * Section.SectionSize), z + (Z * Section.SectionSize)))
                    {
                        sections[y >> 5][x, y & (Section.SectionSize - 1), z] = block;

                        y++;
                    }
                }
            }
        }

        public Task GenerateAsync(IWorldGenerator generator)
        {
            return Task.Run(() => Generate(generator));
        }

        public void CreateMesh()
        {
            for (int y = 0; y < ChunkHeight; y++)
            {
                sections[y].CreateMesh(X, y, Z);
            }

            hasMeshData = true;
            meshDataIndex = 0;
        }

        public Task<(float[][] verticesData, uint[][] indicesData)> CreateMeshDataAsync()
        {
            return Task.Run(CreateMeshData);
        }

        private (float[][] verticesData, uint[][] indicesData) CreateMeshData()
        {
            float[][] verticesData = new float[ChunkHeight][];
            uint[][] indicesData = new uint[ChunkHeight][];

            for (int y = 0; y < ChunkHeight; y++)
            {
                sections[y].CreateMeshData(X, y, Z, out verticesData[y], out indicesData[y]);
            }

            meshDataIndex = 0;

            return (verticesData, indicesData);
        }

        public void SetMeshData(float[][] verticesData, uint[][] indicesData)
        {
            if (verticesData == null)
            {
                throw new ArgumentNullException(nameof(verticesData));
            }

            if (indicesData == null)
            {
                throw new ArgumentNullException(nameof(indicesData));
            }

            for (int y = 0; y < ChunkHeight; y++)
            {
                sections[y].SetMeshData(ref verticesData[y], ref indicesData[y]);
            }

            hasMeshData = true;
            meshDataIndex = 0;
        }

        public bool SetMeshDataStep(float[][] verticesData, uint[][] indicesData)
        {
            if (verticesData == null)
            {
                throw new ArgumentNullException(nameof(verticesData));
            }

            if (indicesData == null)
            {
                throw new ArgumentNullException(nameof(indicesData));
            }

            for (int i = 0; i < maxMeshDataStep; i++)
            {
                sections[meshDataIndex].SetMeshData(ref verticesData[meshDataIndex], ref indicesData[meshDataIndex]);

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

        public void CreateMesh(int y)
        {
            sections[y].CreateMesh(X, y, Z);
        }

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

        public Section GetSection(int y)
        {
            return sections[y];
        }

        public override string ToString()
        {
            return $"Chunk ({X}|{Z})";
        }

        public override bool Equals(object obj)
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
            unchecked
            {
                int hash = 23;
                hash = hash * 31 + X;
                hash = hash * 31 + Z;

                return hash;
            }
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
        #endregion
    }
}