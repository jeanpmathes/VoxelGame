// <copyright file="GenerationContextTests.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using OpenTK.Mathematics;
using VoxelGame.Core.Generation.Worlds;
using VoxelGame.Core.Logic.Chunks;
using VoxelGame.Core.Logic.Definitions.Fluids;
using VoxelGame.Core.Logic.Elements;
using VoxelGame.Core.Logic.Sections;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Tests.Logic.Chunks;
using VoxelGame.Core.Visuals;
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
        private readonly HashSet<Vector3i> generatedBlocks = [];
        private readonly HashSet<SectionPosition> generatedSections = [];

        private readonly Content content = new(new MockBlock(), new MockFluid());

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

    private sealed class MockBlock() : Block("Mock Block", nameof(MockBlock), new BlockFlags(), BoundingVolume.Block);

    private sealed class MockFluid() : BasicFluid("Mock Fluid",
        nameof(MockFluid),
        density: 1.0,
        viscosity: 1,
        hasNeutralTint: false,
        TextureLayout.Uniform(TID.MissingTexture),
        TextureLayout.Uniform(TID.MissingTexture));
}
