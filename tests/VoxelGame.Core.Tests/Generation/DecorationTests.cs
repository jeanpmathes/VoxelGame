// <copyright file="ChunkDecorationTest.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using JetBrains.Annotations;
using OpenTK.Mathematics;
using VoxelGame.Core.Collections;
using VoxelGame.Core.Generation;
using VoxelGame.Core.Generation.Default;
using VoxelGame.Core.Logic.Chunks;
using VoxelGame.Core.Logic.Definitions.Structures;
using VoxelGame.Core.Logic.Elements;
using VoxelGame.Core.Logic.Sections;
using VoxelGame.Core.Profiling;
using VoxelGame.Core.Serialization;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Visuals;
using Xunit;
using Xunit.Abstractions;

namespace VoxelGame.Core.Tests.Generation;

[TestSubject(typeof(IDecorationContext))]
[Collection("Logger")]
public class DecorationTests(ITestOutputHelper output)
{
    [Fact]
    public void Benchmark()
    {
        // todo: when performance is done, make this into an actual testcase in separate class, add case for Water gen to and make both the same code using generics and interface
        // todo: remove all output writing

        ILoadingContext loadingContext = new MockLoadingContext();

        BlockModel.EnableLoading(loadingContext);
        StaticStructure.SetLoadingContext(loadingContext);

        MockTextureBundle textures = new();
        BlockModel.SetBlockTextureIndexProvider(textures);
        Blocks.Load(textures, new VisualConfiguration(), loadingContext);
        Fluids.Load(textures, textures, loadingContext);

        Generator.Initialize(loadingContext);

        Stopwatch timer = new();
        timer.Start();

        Generator generator = new(new MockWorldGeneratorContext());
        ChunkContext context = new(null!, generator, _ => null, _ => null, _ => {});

        timer.Stop();
        output.WriteLine($"Generator creation took {timer.ElapsedMilliseconds}ms");

        timer.Restart();

        Neighborhood<Chunk?> chunks = new();

        foreach ((Int32 x, Int32 y, Int32 z) index in Neighborhood.Indices) chunks[index] = CreateChunk(new ChunkPosition(index.x, index.y, index.z));

        timer.Stop();
        output.WriteLine($"Chunk generation took {timer.ElapsedMilliseconds}ms");

        timer.Restart();

        using (IDecorationContext decorationContext = generator.CreateDecorationContext())
        {
            chunks.Center!.Decorate(chunks, decorationContext);
        }

        timer.Stop();
        output.WriteLine($"Chunk decoration took {timer.ElapsedMilliseconds}ms");

        Chunk CreateChunk(ChunkPosition position)
        {
            Chunk chunk = new(context, blocks => new Section(blocks));
            chunk.Initialize(null!, position);

            using IGenerationContext generationContext = generator.CreateGenerationContext(position);
            using IDecorationContext decorationContext = generator.CreateDecorationContext();

            chunk.Generate(generationContext, decorationContext);

            return chunk;
        }
    }

    // todo: try looking for the wrong section access bug

    [Fact]
    public void TestDecorationOfChunks()
    {
        ChunkContext context = new(null!, null!, _ => null, _ => null, _ => {}); // todo: fix this ugly mess
        MockDecorationContext mockDecorationContext = new();
        IDecorationContext decorationContext = mockDecorationContext;

        Neighborhood<Chunk?> chunks = new();

        foreach ((Int32 x, Int32 y, Int32 z) index in Neighborhood.Indices)
        {
            chunks[index] = CreateChunk(new ChunkPosition(index.x, index.y, index.z));

            decorationContext.DecorateCenter(chunks[index]!);
        }

        decorationContext.Decorate(chunks);

        // 27 chunks with 8 sections in the center, and 8 corners with 4x4x4 section without their tips:
        Assert.Equal(27 * 8 + 8 * (4 * 4 * 4 - 8), mockDecorationContext.NumberOfDecoratedSections);

        Assert.True(chunks.Center!.IsFullyDecorated);

        Chunk CreateChunk(ChunkPosition position)
        {
            Chunk chunk = new(context, blocks => new Section(blocks));
            chunk.Initialize(null!, position);

            return chunk;
        }
    }

    private class MockLoadingContext : ILoadingContext
    {
        public IDisposable BeginStep(String name)
        {
            return new Disposer();
        }

        public void ReportSuccess(String type, String resource) {}

        public void ReportFailure(String type, String resource, Exception exception, Boolean abort = false) {}

        public void ReportFailure(String type, String resource, String message, Boolean abort = false) {}

        public void ReportWarning(String type, String resource, Exception exception) {}

        public void ReportWarning(String type, String resource, String message) {}
    }

    private class MockTextureBundle : ITextureIndexProvider, IDominantColorProvider
    {
        public Color GetDominantColor(Int32 index)
        {
            return Color.Black;
        }

        public Int32 GetTextureIndex(String name)
        {
            return 0;
        }
    }

    private class MockWorldGeneratorContext : IWorldGeneratorContext
    {
        private readonly Dictionary<String, Object> blobs = new();

        public (Int32 upper, Int32 lower) Seed => (0, 0);
        public Timer? Timer => null;

        public T? ReadBlob<T>(String name) where T : class, IEntity, new()
        {
            return blobs.GetValueOrDefault(name) as T;
        }

        public void WriteBlob<T>(String name, T entity) where T : class, IEntity, new()
        {
            blobs[name] = entity;
        }
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
