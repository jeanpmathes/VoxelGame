// <copyright file="GenerationContextTests.cs" company="VoxelGame">
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
using VoxelGame.Core.Generation.Worlds;
using VoxelGame.Core.Logic.Chunks;
using VoxelGame.Core.Logic.Sections;
using VoxelGame.Core.Logic.Voxels;
using VoxelGame.Core.Tests.Logic.Chunks;
using VoxelGame.Core.Tests.Logic.Elements;
using Xunit;

namespace VoxelGame.Core.Tests.Generation.Worlds;

[TestSubject(typeof(IGenerationContext))]
[Collection(LoggerCollection.Name)]
public class GenerationContextTests : ContextTestBase
{
    [Fact]
    public void IGenerationContext_ShouldDecoratePassedChunks()
    {
        using ChunkContext context = MockChunkContext.Create(CreateChunk);
        MockGenerationContext mockGenerationContext = new();
        IGenerationContext generationContext = mockGenerationContext;

        Chunk chunk = CreateInitializedChunk(context, new ChunkPosition(x: 0, y: 0, z: 0));

        generationContext.Generate(chunk);

        Assert.Equal(Chunk.BlockSize * Chunk.BlockSize * Chunk.BlockSize, mockGenerationContext.NumberOfGeneratedBlocks);
        Assert.Equal(Chunk.SectionCount, mockGenerationContext.NumberOfGeneratedSections);
    }

    private sealed class MockGenerationContext : IGenerationContext
    {
        private readonly Content content = Content.CreateGenerated(new MockBlock(), new MockFluid());
        private readonly HashSet<Vector3i> generatedBlocks = [];
        private readonly HashSet<SectionPosition> generatedSections = [];

        public Int32 NumberOfGeneratedBlocks => generatedBlocks.Count;
        public Int32 NumberOfGeneratedSections => generatedSections.Count;

        public IEnumerable<Content> GenerateColumn(Int32 x, Int32 z, (Int32 start, Int32 end) heightRange)
        {
            for (Int32 y = heightRange.start; y < heightRange.end; y++)
            {
                Vector3i position = new(x, y, z);

                Boolean newlyAdded = generatedBlocks.Add(position);
                Assert.True(newlyAdded);

                yield return content;
            }
        }

        public void GenerateStructures(Section section)
        {
            Boolean newlyAdded = generatedSections.Add(section.Position);
            Assert.True(newlyAdded);
        }

        public IWorldGenerator Generator => throw new NotSupportedException();

        public void Dispose()
        {
            // Nothing to dispose.
        }
    }
}
