// <copyright file="ChunkDecorationTest.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using OpenTK.Mathematics;
using VoxelGame.Core.Collections;
using VoxelGame.Core.Generation;
using VoxelGame.Core.Logic.Chunks;
using VoxelGame.Core.Logic.Elements;
using VoxelGame.Core.Logic.Sections;
using Xunit;

namespace VoxelGame.Core.Tests.Logic.Chunks;

[TestSubject(typeof(ChunkDecoration))]
[Collection("Logger")]
public class ChunkDecorationTest
{
    [Fact]
    public void TestDecorationOfChunks()
    {
        TestGenerator generator = new();
        ChunkContext context = new(null!, generator, _ => null, _ => null, _ => {});

        Neighborhood<Chunk> chunks = new();

        foreach ((Int32 x, Int32 y, Int32 z) index in Neighborhood.Indices)
        {
            chunks[index] = CreateChunk(new ChunkPosition(index.x, index.y, index.z));

            ChunkDecoration.DecorateCenter(chunks[index]);
        }

        ChunkDecoration.Decorate(chunks);

        // 27 chunks with 8 sections in the center, and 8 corners with 4x4x4 section without their tips:
        Assert.Equal(27 * 8 + 8 * (4 * 4 * 4 - 8), generator.NumberOfDecoratedSections);

        Assert.True(chunks.Center.IsFullyDecorated);

        Chunk CreateChunk(ChunkPosition position)
        {
            Chunk chunk = new(context, blocks => new Section(blocks));
            chunk.Initialize(null!, position);

            return chunk;
        }
    }

    private class TestGenerator : IWorldGenerator
    {
        private readonly HashSet<SectionPosition> decoratedSections = [];

        public Int32 NumberOfDecoratedSections => decoratedSections.Count;

        public void DecorateSection(SectionPosition position, Neighborhood<Section> sections)
        {
            Boolean newlyAdded = decoratedSections.Add(position);
            Assert.True(newlyAdded);

            Assert.Equal(position, sections.Center.Position);

            foreach (Vector3i index in Neighborhood.Indices)
            {
                Vector3i offset = index - Neighborhood.Center;
                Assert.Equal(offset, sections.Center.Position.OffsetTo(sections[index].Position));
            }
        }

        #region NOT SUPPORTED

        public IMap Map => throw new NotSupportedException();

        public IEnumerable<Content> GenerateColumn(Int32 x, Int32 z, (Int32 start, Int32 end) heightRange)
        {
            throw new NotSupportedException();
        }

        public void EmitViews(DirectoryInfo path)
        {
            throw new NotSupportedException();
        }

        public void GenerateStructures(Section section, SectionPosition position)
        {
            throw new NotSupportedException();
        }

        public IEnumerable<Vector3i> SearchNamedGeneratedElements(Vector3i start, String name, UInt32 maxDistance)
        {
            throw new NotSupportedException();
        }

        #endregion NOT SUPPORTED
    }
}
