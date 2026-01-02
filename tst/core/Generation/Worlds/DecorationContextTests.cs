// <copyright file="DecorationContextTests.cs" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2026 Jean Patrick Mathes
//      
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
//     
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//     
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <https://www.gnu.org/licenses/>.
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
using VoxelGame.Core.Tests.Logic.Chunks;
using Xunit;

namespace VoxelGame.Core.Tests.Generation.Worlds;

[TestSubject(typeof(IDecorationContext))]
[Collection(LoggerCollection.Name)]
public class DecorationContextTests : ContextTestBase
{
    [Fact]
    public void IDecorationContext_ShouldDecoratePassedChunks()
    {
        using ChunkContext context = MockChunkContext.Create(CreateChunk);
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
