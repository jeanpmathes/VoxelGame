﻿// <copyright file="GeneratorTest.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using VoxelGame.Core.Collections;
using VoxelGame.Core.Generation;
using VoxelGame.Core.Generation.Default;
using VoxelGame.Core.Logic.Chunks;
using VoxelGame.Core.Logic.Definitions.Structures;
using VoxelGame.Core.Logic.Elements;
using VoxelGame.Core.Logic.Sections;
using VoxelGame.Core.Profiling;
using VoxelGame.Core.Tests.Utilities;
using VoxelGame.Core.Tests.Visuals;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Visuals;
using Xunit;
using Xunit.Abstractions;

namespace VoxelGame.Core.Tests.Generation;

[Collection("Logger")]
public class GeneratorTest(ITestOutputHelper output)
{
    [Fact]
    public void TestDefaultGenerator()
    {
        TestGenerator<Generator>();
    }

    [Fact]
    public void TestWaterGenerator()
    {
        TestGenerator<Core.Generation.Water.Generator>();
    }

    private void TestGenerator<TGenerator>() where TGenerator : IWorldGenerator
    {
        ILoadingContext loadingContext = new MockLoadingContext();

        BlockModel.EnableLoading(loadingContext);
        StaticStructure.SetLoadingContext(loadingContext);

        MockTextureBundle textures = new();
        BlockModel.SetBlockTextureIndexProvider(textures);
        Blocks.Load(textures, new VisualConfiguration(), loadingContext);
        Fluids.Load(textures, textures, loadingContext);

        TGenerator.Initialize(loadingContext);

        IWorldGenerator generator;
        ChunkContext context;

        using (Timer.Start(duration => output.WriteLine($"Creation: {duration}")))
        {
            generator = TGenerator.Create(new MockWorldGeneratorContext());
            context = new ChunkContext(null!, generator, _ => null, _ => null, _ => {});
        }

        Neighborhood<Chunk?> chunks = new();

        using (Timer.Start(duration => output.WriteLine($"Generation: {duration}")))
        {
            foreach ((Int32 x, Int32 y, Int32 z) index in Neighborhood.Indices)
                chunks[index] = CreateChunk(new ChunkPosition(index.x, index.y, index.z));
        }

        using (Timer.Start(duration => output.WriteLine($"Decoration: {duration}")))
        {
            using IDecorationContext decorationContext = generator.CreateDecorationContext(chunks.Center!.Position, extents: 1);

            chunks.Center!.Decorate(chunks, decorationContext);
        }

        Assert.True(chunks.Center!.IsFullyDecorated);

        generator.Dispose();

        Chunk CreateChunk(ChunkPosition position)
        {
            Chunk chunk = new(context, blocks => new Section(blocks));
            chunk.Initialize(null!, position);

            using IGenerationContext generationContext = generator.CreateGenerationContext(position);
            using IDecorationContext decorationContext = generator.CreateDecorationContext(position, extents: 0);

            chunk.Generate(generationContext, decorationContext);

            return chunk;
        }
    }
}
