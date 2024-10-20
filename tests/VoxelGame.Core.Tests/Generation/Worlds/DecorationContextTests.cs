// <copyright file="DecorationContextTests.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using OpenTK.Mathematics;
using VoxelGame.Core.Collections;
using VoxelGame.Core.Generation.Worlds;
using VoxelGame.Core.Logic.Chunks;
using VoxelGame.Core.Logic.Sections;
using Xunit;

namespace VoxelGame.Core.Tests.Generation.Worlds;

[TestSubject(typeof(IDecorationContext))]
[Collection("Logger")]
public class DecorationContextTests : ContextTestBase
{
    [Fact]
    public void IDecorationContext_ShouldDecoratePassedChunks()
    {
        ChunkContext context = new(null!, CreateChunk, _ => null, _ => null, _ => {});
        MockDecorationContext mockDecorationContext = new();
        IDecorationContext decorationContext = mockDecorationContext;

        Neighborhood<Chunk?> chunks = new();

        foreach ((Int32 x, Int32 y, Int32 z) index in Neighborhood.Indices)
        {
            chunks[index] = CreateInitializedChunk(context, new ChunkPosition(index.x, index.y, index.z));

            decorationContext.DecorateCenter(chunks[index]!);

            Assert.True(chunks[index]!.IsGenerated);
        }

        decorationContext.Decorate(chunks);

        // 27 chunks with 8 sections in the center, and 8 corners with 4x4x4 section without their tips:
        Assert.Equal(27 * 8 + 8 * (4 * 4 * 4 - 8), mockDecorationContext.NumberOfDecoratedSections);

        Assert.True(chunks.Center!.IsFullyDecorated);
    }

    private sealed class MockDecorationContext : IDecorationContext
    {
        private readonly HashSet<SectionPosition> decoratedSections = [];

        public Int32 NumberOfDecoratedSections => decoratedSections.Count;

        public IWorldGenerator Generator => throw new NotSupportedException();

        public void DecorateSection(Neighborhood<Section> sections)
        {
            Boolean newlyAdded = decoratedSections.Add(sections.Center.Position);
            Assert.True(newlyAdded);

            foreach (Vector3i index in Neighborhood.Indices)
            {
                Vector3i offset = index - Neighborhood.Center;
                Assert.Equal(offset, sections.Center.Position.OffsetTo(sections[index].Position));
            }
        }

 #pragma warning disable CA1822
        public void Dispose()
 #pragma warning restore CA1822
        {
            // Nothing to dispose.
        }
    }
}
