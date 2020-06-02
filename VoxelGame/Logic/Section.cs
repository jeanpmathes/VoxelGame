// <copyright file="Section.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using OpenTK;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using VoxelGame.Rendering;
using VoxelGame.WorldGeneration;

namespace VoxelGame.Logic
{
    [Serializable]
    public class Section : IDisposable
    {
        public const int SectionSize = 32;
        public const int TickBatchSize = 4;

        public const int BlockMask = 0b0000_0000_0000_0000_0000_0111_1111_1111;
        public const int DataMask = 0b0000_0000_0000_0000_1111_1000_0000_0000;

        private readonly ushort[] blocks;

        [NonSerialized] private bool isEmpty;
        [NonSerialized] private SectionRenderer renderer;

        public Section()
        {
            blocks = new ushort[SectionSize * SectionSize * SectionSize];

            Setup();
        }

        /// <summary>
        /// Sets up all non serialized members.
        /// </summary>
        public void Setup()
        {
            renderer = new SectionRenderer();
        }

        public void Generate(IWorldGenerator generator, int xOffset, int yOffset, int zOffset)
        {
            if (generator == null)
            {
                throw new ArgumentNullException(paramName: nameof(generator));
            }

            for (int x = 0; x < SectionSize; x++)
            {
                for (int y = 0; y < SectionSize; y++)
                {
                    for (int z = 0; z < SectionSize; z++)
                    {
                        blocks[(x << 10) + (y << 5) + z] = generator.GenerateBlock(x + xOffset, y + yOffset, z + zOffset).Id;
                    }
                }
            }
        }

        public void CreateMesh(int sectionX, int sectionY, int sectionZ)
        {
            CreateMeshData(sectionX, sectionY, sectionZ, out float[] complexVertexPositions, out int[] complexVertexData, out uint[] complexIndices, out int[] simpleVertexData, out uint[] simpleIndices);
            SetMeshData(ref complexVertexPositions, ref complexVertexData, ref complexIndices, ref simpleVertexData, ref simpleIndices);
        }

        private static long TotalTime = 0;
        private static float TotalRuns = 0;

        public void CreateMeshData(int sectionX, int sectionY, int sectionZ, out float[] complexVertexPositions, out int[] complexVertexData, out uint[] complexIndices, out int[] simpleVertexData, out uint[] simpleIndices)
        {
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

            // Set the neutral tint color
            TintColor neutral = new TintColor(0f, 1f, 0f);

            // Get the sections next to this section
            Section frontNeighbour = Game.World.GetSection(sectionX, sectionY, sectionZ + 1);
            Section backNeighbour = Game.World.GetSection(sectionX, sectionY, sectionZ - 1);
            Section leftNeighbour = Game.World.GetSection(sectionX - 1, sectionY, sectionZ);
            Section rightNeighbour = Game.World.GetSection(sectionX + 1, sectionY, sectionZ);
            Section bottomNeighbour = Game.World.GetSection(sectionX, sectionY - 1, sectionZ);
            Section topNeighbour = Game.World.GetSection(sectionX, sectionY + 1, sectionZ);

            // Create the mesh data
            List<int> simpleVertexDataBuilder = new List<int>(2048);
            List<uint> simpleIndicesBuilder = new List<uint>(1024);

            List<float> complexVertexPositionsBuilder = new List<float>(64);
            List<int> complexVertexDataBuilder = new List<int>(32);
            List<uint> complexIndicesBuilder = new List<uint>(16);

            uint simpleVertCount = 0;
            uint complexVertCount = 0;

            for (int x = 0; x < SectionSize; x++)
            {
                for (int y = 0; y < SectionSize; y++)
                {
                    for (int z = 0; z < SectionSize; z++)
                    {
                        ushort currentBlockData = blocks[(x << 10) + (y << 5) + z];

                        Block currentBlock = Block.TranslateID((ushort)(currentBlockData & BlockMask));
                        byte currentData = (byte)((currentBlockData & DataMask) >> 11);

                        if (currentBlock.TargetBuffer == TargetBuffer.Simple)
                        {
                            Block blockToCheck;

                            // Check all six sides of this block

                            // Front
                            if (z + 1 >= SectionSize && frontNeighbour != null)
                            {
                                blockToCheck = frontNeighbour.GetBlock(x, y, 0);
                            }
                            else if (z + 1 >= SectionSize)
                            {
                                blockToCheck = null;
                            }
                            else
                            {
                                blockToCheck = GetBlock(x, y, z + 1);
                            }

                            if (blockToCheck != null && (!blockToCheck.IsFull || (!blockToCheck.IsOpaque && currentBlock.IsOpaque) || (!blockToCheck.IsOpaque && (currentBlock.RenderFaceAtNonOpaques || blockToCheck.RenderFaceAtNonOpaques))))
                            {
                                uint verts = currentBlock.GetMesh(BlockSide.Front, currentData, out float[] vertices, out int[] textureIndices, out uint[] indices, out TintColor tint);

                                simpleIndicesBuilder.AddRange(indices);

                                for (int i = 0; i < verts; i++)
                                {
                                    // int: ---- ---- ---n nnxx xxxx yyyy yyzz zzzz (n: normal; xyz: position)
                                    int upperData = ((int)BlockSide.Front << 18) | (((int)vertices[(i * 8) + 0] + x) << 12) | (((int)vertices[(i * 8) + 1] + y) << 6) | ((int)vertices[(i * 8) + 2] + z);
                                    simpleVertexDataBuilder.Add(upperData);

                                    // int: tttt tttt t--- -uv- ---- iiii iiii iiii (t: tint; o: orientation; i: texture index)
                                    int lowerData =  (((tint.IsNeutral) ? neutral.ToBits : tint.ToBits) << 23) | (((int)vertices[(i * 8) + 3]) << 18) | (((int)vertices[(i * 8) + 4]) << 17) | textureIndices[i];
                                    simpleVertexDataBuilder.Add(lowerData);
                                }

                                for (int i = simpleIndicesBuilder.Count - indices.Length; i < simpleIndicesBuilder.Count; i++)
                                {
                                    simpleIndicesBuilder[i] += simpleVertCount;
                                }

                                simpleVertCount += verts;
                            }

                            // Back
                            if (z - 1 < 0 && backNeighbour != null)
                            {
                                blockToCheck = backNeighbour.GetBlock(x, y, SectionSize - 1);
                            }
                            else if (z - 1 < 0)
                            {
                                blockToCheck = null;
                            }
                            else
                            {
                                blockToCheck = GetBlock(x, y, z - 1);
                            }

                            if (blockToCheck != null && (!blockToCheck.IsFull || (!blockToCheck.IsOpaque && currentBlock.IsOpaque) || (!blockToCheck.IsOpaque && (currentBlock.RenderFaceAtNonOpaques || blockToCheck.RenderFaceAtNonOpaques))))
                            {
                                uint verts = currentBlock.GetMesh(BlockSide.Back, currentData, out float[] vertices, out int[] textureIndices, out uint[] indices, out TintColor tint);

                                simpleIndicesBuilder.AddRange(indices);

                                for (int i = 0; i < verts; i++)
                                {
                                    // int: ---- ---- ---n nnxx xxxx yyyy yyzz zzzz (n: normal; xyz: position)
                                    int upperData = ((int)BlockSide.Back << 18) | (((int)vertices[(i * 8) + 0] + x) << 12) | (((int)vertices[(i * 8) + 1] + y) << 6) | ((int)vertices[(i * 8) + 2] + z);
                                    simpleVertexDataBuilder.Add(upperData);

                                    // int: tttt tttt t--- -uv- ---- iiii iiii iiii (t: tint; o: orientation; i: texture index)
                                    int lowerData = (((tint.IsNeutral) ? neutral.ToBits : tint.ToBits) << 23) | (((int)vertices[(i * 8) + 3]) << 18) | (((int)vertices[(i * 8) + 4]) << 17) | textureIndices[i];
                                    simpleVertexDataBuilder.Add(lowerData);
                                }

                                for (int i = simpleIndicesBuilder.Count - indices.Length; i < simpleIndicesBuilder.Count; i++)
                                {
                                    simpleIndicesBuilder[i] += simpleVertCount;
                                }

                                simpleVertCount += verts;
                            }

                            // Left
                            if (x - 1 < 0 && leftNeighbour != null)
                            {
                                blockToCheck = leftNeighbour.GetBlock(SectionSize - 1, y, z);
                            }
                            else if (x - 1 < 0)
                            {
                                blockToCheck = null;
                            }
                            else
                            {
                                blockToCheck = GetBlock(x - 1, y, z);
                            }

                            if (blockToCheck != null && (!blockToCheck.IsFull || (!blockToCheck.IsOpaque && currentBlock.IsOpaque) || (!blockToCheck.IsOpaque && (currentBlock.RenderFaceAtNonOpaques || blockToCheck.RenderFaceAtNonOpaques))))
                            {
                                uint verts = currentBlock.GetMesh(BlockSide.Left, currentData, out float[] vertices, out int[] textureIndices, out uint[] indices, out TintColor tint);

                                simpleIndicesBuilder.AddRange(indices);

                                for (int i = 0; i < verts; i++)
                                {
                                    // int: ---- ---- ---n nnxx xxxx yyyy yyzz zzzz (n: normal; xyz: position)
                                    int upperData = ((int)BlockSide.Left << 18) | (((int)vertices[(i * 8) + 0] + x) << 12) | (((int)vertices[(i * 8) + 1] + y) << 6) | ((int)vertices[(i * 8) + 2] + z);
                                    simpleVertexDataBuilder.Add(upperData);

                                    // int: tttt tttt t--- -uv- ---- iiii iiii iiii (t: tint; o: orientation; i: texture index)
                                    int lowerData = (((tint.IsNeutral) ? neutral.ToBits : tint.ToBits) << 23) | (((int)vertices[(i * 8) + 3]) << 18) | (((int)vertices[(i * 8) + 4]) << 17) | textureIndices[i];
                                    simpleVertexDataBuilder.Add(lowerData);
                                }

                                for (int i = simpleIndicesBuilder.Count - indices.Length; i < simpleIndicesBuilder.Count; i++)
                                {
                                    simpleIndicesBuilder[i] += simpleVertCount;
                                }

                                simpleVertCount += verts;
                            }

                            // Right
                            if (x + 1 >= SectionSize && rightNeighbour != null)
                            {
                                blockToCheck = rightNeighbour.GetBlock(0, y, z);
                            }
                            else if (x + 1 >= SectionSize)
                            {
                                blockToCheck = null;
                            }
                            else
                            {
                                blockToCheck = GetBlock(x + 1, y, z);
                            }

                            if (blockToCheck != null && (!blockToCheck.IsFull || (!blockToCheck.IsOpaque && currentBlock.IsOpaque) || (!blockToCheck.IsOpaque && (currentBlock.RenderFaceAtNonOpaques || blockToCheck.RenderFaceAtNonOpaques))))
                            {
                                uint verts = currentBlock.GetMesh(BlockSide.Right, currentData, out float[] vertices, out int[] textureIndices, out uint[] indices, out TintColor tint);

                                simpleIndicesBuilder.AddRange(indices);

                                for (int i = 0; i < verts; i++)
                                {
                                    // int: ---- ---- ---n nnxx xxxx yyyy yyzz zzzz (n: normal; xyz: position)
                                    int upperData = ((int)BlockSide.Right << 18) | (((int)vertices[(i * 8) + 0] + x) << 12) | (((int)vertices[(i * 8) + 1] + y) << 6) | ((int)vertices[(i * 8) + 2] + z);
                                    simpleVertexDataBuilder.Add(upperData);

                                    // int: tttt tttt t--- -uv- ---- iiii iiii iiii (t: tint; o: orientation; i: texture index)
                                    int lowerData = (((tint.IsNeutral) ? neutral.ToBits : tint.ToBits) << 23) | (((int)vertices[(i * 8) + 3]) << 18) | (((int)vertices[(i * 8) + 4]) << 17) | textureIndices[i];
                                    simpleVertexDataBuilder.Add(lowerData);
                                }

                                for (int i = simpleIndicesBuilder.Count - indices.Length; i < simpleIndicesBuilder.Count; i++)
                                {
                                    simpleIndicesBuilder[i] += simpleVertCount;
                                }

                                simpleVertCount += verts;
                            }

                            // Bottom
                            if (y - 1 < 0 && bottomNeighbour != null)
                            {
                                blockToCheck = bottomNeighbour.GetBlock(x, SectionSize - 1, z);
                            }
                            else if (y - 1 < 0)
                            {
                                blockToCheck = null;
                            }
                            else
                            {
                                blockToCheck = GetBlock(x, y - 1, z);
                            }

                            if (blockToCheck?.IsFull != true || (!blockToCheck.IsOpaque && currentBlock.IsOpaque) || (!blockToCheck.IsOpaque && (currentBlock.RenderFaceAtNonOpaques || blockToCheck.RenderFaceAtNonOpaques)))
                            {
                                uint verts = currentBlock.GetMesh(BlockSide.Bottom, currentData, out float[] vertices, out int[] textureIndices, out uint[] indices, out TintColor tint);

                                simpleIndicesBuilder.AddRange(indices);

                                for (int i = 0; i < verts; i++)
                                {
                                    // int: ---- ---- ---n nnxx xxxx yyyy yyzz zzzz (n: normal; xyz: position)
                                    int upperData = ((int)BlockSide.Bottom << 18) | (((int)vertices[(i * 8) + 0] + x) << 12) | (((int)vertices[(i * 8) + 1] + y) << 6) | ((int)vertices[(i * 8) + 2] + z);
                                    simpleVertexDataBuilder.Add(upperData);

                                    // int: tttt tttt t--- -uv- ---- iiii iiii iiii (t: tint; o: orientation; i: texture index)
                                    int lowerData = ((tint.IsNeutral ? neutral.ToBits : tint.ToBits) << 23) | (((int)vertices[(i * 8) + 3]) << 18) | (((int)vertices[(i * 8) + 4]) << 17) | textureIndices[i];
                                    simpleVertexDataBuilder.Add(lowerData);
                                }

                                for (int i = simpleIndicesBuilder.Count - indices.Length; i < simpleIndicesBuilder.Count; i++)
                                {
                                    simpleIndicesBuilder[i] += simpleVertCount;
                                }

                                simpleVertCount += verts;
                            }

                            // Top
                            if (y + 1 >= SectionSize && topNeighbour != null)
                            {
                                blockToCheck = topNeighbour.GetBlock(x, 0, z);
                            }
                            else if (y + 1 >= SectionSize)
                            {
                                blockToCheck = null;
                            }
                            else
                            {
                                blockToCheck = GetBlock(x, y + 1, z);
                            }

                            if (blockToCheck?.IsFull != true || (!blockToCheck.IsOpaque && currentBlock.IsOpaque) || (!blockToCheck.IsOpaque && (currentBlock.RenderFaceAtNonOpaques || blockToCheck.RenderFaceAtNonOpaques)))
                            {
                                uint verts = currentBlock.GetMesh(BlockSide.Top, currentData, out float[] vertices, out int[] textureIndices, out uint[] indices, out TintColor tint);

                                simpleIndicesBuilder.AddRange(indices);

                                for (int i = 0; i < verts; i++)
                                {
                                    // int: ---- ---- ---n nnxx xxxx yyyy yyzz zzzz (n: normal; xyz: position)
                                    int upperData = ((int)BlockSide.Top << 18) | (((int)vertices[(i * 8) + 0] + x) << 12) | (((int)vertices[(i * 8) + 1] + y) << 6) | ((int)vertices[(i * 8) + 2] + z);
                                    simpleVertexDataBuilder.Add(upperData);

                                    // int: tttt tttt t--- -uv- ---- iiii iiii iiii (t: tint; o: orientation; i: texture index)
                                    int lowerData = (((tint.IsNeutral) ? neutral.ToBits : tint.ToBits) << 23) | (((int)vertices[(i * 8) + 3]) << 18) | (((int)vertices[(i * 8) + 4]) << 17) | textureIndices[i];
                                    simpleVertexDataBuilder.Add(lowerData);
                                }

                                for (int i = simpleIndicesBuilder.Count - indices.Length; i < simpleIndicesBuilder.Count; i++)
                                {
                                    simpleIndicesBuilder[i] += simpleVertCount;
                                }

                                simpleVertCount += verts;
                            }
                        }
                        else if (currentBlock.TargetBuffer == TargetBuffer.Complex)
                        {
                            uint verts = currentBlock.GetMesh(BlockSide.All, currentData, out float[] vertices, out int[] textureIndices, out uint[] indices, out TintColor tint);

                            complexIndicesBuilder.AddRange(indices);

                            for (int i = 0; i < verts; i++)
                            {
                                complexVertexPositionsBuilder.Add(vertices[(i * 8) + 0] + x);
                                complexVertexPositionsBuilder.Add(vertices[(i * 8) + 1] + y);
                                complexVertexPositionsBuilder.Add(vertices[(i * 8) + 2] + z);

                                // int: nnnn nooo oopp ppp- ---- --uu uuuv vvvv (nop: normal; uv: texture coords)
                                int upperData =
                                    (((vertices[(i * 8) + 5] < 0f) ? (0b1_0000 | (int)(vertices[(i * 8) + 5] * -15f)) : (int)(vertices[(i * 8) + 5] * 15f)) << 27) |
                                    (((vertices[(i * 8) + 6] < 0f) ? (0b1_0000 | (int)(vertices[(i * 8) + 6] * -15f)) : (int)(vertices[(i * 8) + 6] * 15f)) << 22) |
                                    (((vertices[(i * 8) + 7] < 0f) ? (0b1_0000 | (int)(vertices[(i * 8) + 7] * -15f)) : (int)(vertices[(i * 8) + 7] * 15f)) << 17) |
                                    ((int)(vertices[(i * 8) + 3] * 16f) << 5) |
                                    ((int)(vertices[(i * 8) + 4] * 16f));

                                complexVertexDataBuilder.Add(upperData);

                                // int: tttt tttt t--- ---- ---- iiii iiii iiii (t: tint; i: texture index)
                                int lowerData = (((tint.IsNeutral) ? neutral.ToBits : tint.ToBits) << 23) | textureIndices[i];
                                complexVertexDataBuilder.Add(lowerData);
                            }

                            for (int i = complexIndicesBuilder.Count - indices.Length; i < complexIndicesBuilder.Count; i++)
                            {
                                complexIndicesBuilder[i] += complexVertCount;
                            }

                            complexVertCount += verts;
                        }
                    }
                }
            }

            isEmpty = complexVertexPositionsBuilder.Count == 0 && simpleVertexDataBuilder.Count == 0;

            complexVertexPositions = complexVertexPositionsBuilder.ToArray();
            complexVertexData = complexVertexDataBuilder.ToArray();
            complexIndices = complexIndicesBuilder.ToArray();

            simpleVertexData = simpleVertexDataBuilder.ToArray();
            simpleIndices = simpleIndicesBuilder.ToArray();

            stopwatch.Stop();
            TotalTime += stopwatch.ElapsedMilliseconds;
            TotalRuns++;

            if (TotalRuns % 50 == 0)
            {
                Console.WriteLine($"RUN {TotalRuns} WITH AVRG TIME OF {TotalTime / TotalRuns}");
            }
        }

        public void SetMeshData(ref float[] complexVertexPositions, ref int[] complexVertexData, ref uint[] complexIndices, ref int[] simpleVertexData, ref uint[] simpleIndices)
        {
            renderer.SetData(ref complexVertexPositions, ref complexVertexData, ref complexIndices, ref simpleVertexData, ref simpleIndices);
        }

        public void Render(Vector3 position)
        {
            if (!isEmpty)
            {
                renderer.Draw(position);
            }
        }

        public void Tick(int sectionX, int sectionY, int sectionZ)
        {
            for (int i = 0; i < TickBatchSize; i++)
            {
                int index = Game.Random.Next(0, SectionSize * SectionSize * SectionSize);
                ushort val = blocks[index];

                int z = index & 31;
                index = (index - z) >> 5;
                int y = index & 31;
                index = (index - y) >> 5;
                int x = index;

                Block.TranslateID((ushort)(val & BlockMask))?.RandomUpdate(x + (sectionX * SectionSize), y + (sectionY * SectionSize), z + (sectionZ * SectionSize), (byte)((val & DataMask) >> 11));
            }
        }

        /// <summary>
        /// Gets or sets the block at a section position.
        /// </summary>
        /// <param name="x">The x position of the block in this section.</param>
        /// <param name="y">The y position of the block in this section.</param>
        /// <param name="z">The z position of the block in this section.</param>
        /// <returns>The block at the given position.</returns>
        public ushort this[int x, int y, int z]
        {
            get
            {
                return blocks[(x << 10) + (y << 5) + z];
            }

            set
            {
                blocks[(x << 10) + (y << 5) + z] = value;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Block GetBlock(int x, int y, int z)
        {
            return Block.TranslateID((ushort)(this[x, y, z] & BlockMask));
        }

        #region IDisposable Support

        [NonSerialized] private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    renderer?.Dispose();
                }

                disposed = true;
            }
        }

        ~Section()
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