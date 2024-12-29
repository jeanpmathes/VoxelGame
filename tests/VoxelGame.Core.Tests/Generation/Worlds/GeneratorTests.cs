// <copyright file="GeneratorTest.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using VoxelGame.Core.Collections;
using VoxelGame.Core.Generation.Worlds;
using VoxelGame.Core.Generation.Worlds.Default;
using VoxelGame.Core.Logic.Chunks;
using VoxelGame.Core.Logic.Sections;
using VoxelGame.Core.Profiling;
using VoxelGame.Core.Tests.Logic.Chunks;
using VoxelGame.Toolkit.Memory;
using Xunit;
using Xunit.Abstractions;

namespace VoxelGame.Core.Tests.Generation.Worlds;

[Collection(ResourceCollection.Name)]
public sealed class GeneratorTests(ITestOutputHelper output) : IDisposable
{
    private readonly NativeAllocator allocator = new();

    public void Dispose()
    {
        allocator.Dispose();
    }

    [Fact]
    public void Default_Generator_ShouldFullyGeneratePassedChunks()
    {
        TestGenerator<Generator>();
    }

    [Fact]
    public void Water_Generator_ShouldFullyGeneratePassedChunks()
    {
        TestGenerator<Core.Generation.Worlds.Water.Generator>();
    }

    private void TestGenerator<TGenerator>() where TGenerator : IWorldGenerator
    {
        IWorldGenerator? generator;
        ChunkContext context;

        using (Timer.Start(duration => output.WriteLine($"Creation: {duration}")))
        {
            generator = TGenerator.Create(new MockWorldGeneratorContext());
            context = MockChunkContext.Create(CreateChunk);
        }

        Assert.NotNull(generator);

        Neighborhood<Chunk?> chunks = new();

        using (Timer.Start(duration => output.WriteLine($"Generation: {duration}")))
        {
            foreach ((Int32 x, Int32 y, Int32 z) index in Neighborhood.Indices)
                chunks[index] = GenerateChunk(new ChunkPosition(index.x, index.y, index.z));
        }

        using (Timer.Start(duration => output.WriteLine($"Decoration: {duration}")))
        {
            using IDecorationContext decorationContext = generator.CreateDecorationContext(chunks.Center!.Position, extents: 1);

            chunks.Center!.Decorate(chunks, decorationContext);
        }

        Assert.True(chunks.Center!.IsFullyDecorated);

        generator.Dispose();
        context.Dispose();

        Chunk GenerateChunk(ChunkPosition position)
        {
            Chunk chunk = CreateChunk(allocator.Allocate<UInt32>(Chunk.BlockCount).Segment, context);
            chunk.Initialize(null!, position);

            using IGenerationContext generationContext = generator.CreateGenerationContext(position);
            using IDecorationContext decorationContext = generator.CreateDecorationContext(position, extents: 0);

            chunk.Generate(generationContext, decorationContext);

            return chunk;
        }

        Chunk CreateChunk(NativeSegment<UInt32> segment, ChunkContext ctx)
        {
            return new Chunk(ctx, segment, blocks => new Section(blocks));
        }
    }
}
