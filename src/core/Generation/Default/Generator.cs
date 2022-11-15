// <copyright file="Generator.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using OpenTK.Mathematics;
using VoxelGame.Core.Collections;
using VoxelGame.Core.Logic;
using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Utilities;
using VoxelGame.Logging;

namespace VoxelGame.Core.Generation.Default;

/// <summary>
///     The default world generator.
/// </summary>
public class Generator : IWorldGenerator
{
    private const int SeaLevel = 0;

    private const string MapBlobName = "default_map";
    private static readonly ILogger logger = LoggingHelper.CreateLogger<Generator>();

    private readonly FastNoiseLite decorationNoise;

    private readonly Map map;

    private readonly Palette palette = new();

    private readonly int seed;

    private readonly World world;

    /// <summary>
    ///     Creates a new default world generator.
    /// </summary>
    /// <param name="world">The world to generate.</param>
    public Generator(World world)
    {
        this.world = world;
        seed = world.Seed;

        Biomes biomes = Biomes.Load();
        biomes.Setup(seed, palette);

        map = new Map(BiomeDistribution.CreateDefault(biomes));

        Initialize();
        Store();

        decorationNoise = new FastNoiseLite(seed);
        decorationNoise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
        decorationNoise.SetFrequency(frequency: 0.5f);

        logger.LogInformation(Events.WorldGeneration, "Created '{Name}' world generator", nameof(Default));
    }

    /// <inheritdoc />
    public IEnumerable<Content> GenerateColumn(int x, int z, (int start, int end) heightRange)
    {
        Map.Sample sample = map.GetSample((x, z));

        double offset = GetOffset((x, z), sample);
        double height = sample.Height * Default.Map.MaxHeight;

        var rawHeight = (int) height;
        var modifiedHeight = (int) (height + offset);
        int effectiveOffset = rawHeight - modifiedHeight;

        Context context = new()
        {
            Map = map,
            Sample = sample,
            WorldHeight = modifiedHeight,
            Dampening = CreateFilledDampening(effectiveOffset, sample),
            IceWidth = GetIceWidth(sample)
        };

        for (int y = heightRange.start; y < heightRange.end; y++) yield return GenerateContent((x, y, z), context);
    }

    /// <inheritdoc />
    public void DecorateSection(SectionPosition position, Array3D<Section> sections)
    {
        Debug.Assert(sections.Length == 3);

        List<Biome> biomes = GetSectionBiomes(position);

        HashSet<Decoration> decorations = new();
        Dictionary<Decoration, HashSet<Biome>> decorationBiomes = new();

        foreach (Biome biome in biomes)
        foreach (Decoration decoration in biome.Decorations)
        {
            decorations.Add(decoration);
            decorationBiomes.GetOrAdd(decoration).Add(biome);
        }

        Debug.Assert(decorations.GroupBy(d => d.Name).All(g => g.Count() <= 1), "Duplicate decoration names or cloned decorations.");

        Array3D<float> noise = GenerateDecorationNoise(position);

        foreach (Decoration decoration in decorations.OrderByDescending(d => d.Size).ThenBy(d => d.Name))
        {
            Decoration.Context context = new()
            {
                Position = position,
                Sections = sections,
                Biomes = decorationBiomes[decoration],
                Noise = noise,
                Map = map
            };

            decoration.Place(context);
        }
    }

    /// <inheritdoc />
    public void EmitViews(string path)
    {
        map.EmitViews(path);
    }

    /// <inheritdoc />
    public IMap Map => map;

    private Array3D<float> GenerateDecorationNoise(SectionPosition position)
    {
        var noise = new Array3D<float>(Section.Size);

        for (var x = 0; x < Section.Size; x++)
        for (var y = 0; y < Section.Size; y++)
        for (var z = 0; z < Section.Size; z++)
        {
            Vector3i blockPosition = position.FirstBlock + (x, y, z);

            noise[x, y, z] = decorationNoise.GetNoise(blockPosition.X, blockPosition.Y, blockPosition.Z);
        }

        return noise;
    }

    private List<Biome> GetSectionBiomes(SectionPosition position)
    {
        List<Biome> biomes = new();

        Vector2i start = position.FirstBlock.Xz;

        biomes.Add(map.GetSample(start).ActualBiome);
        biomes.Add(map.GetSample(start + (0, Section.Size)).ActualBiome);
        biomes.Add(map.GetSample(start + (Section.Size, 0)).ActualBiome);
        biomes.Add(map.GetSample(start + (Section.Size, Section.Size)).ActualBiome);

        biomes = biomes.Distinct().ToList();

        return biomes;
    }

    private static double GetOffset(Vector2i position, in Map.Sample sample)
    {
        return VMath.MixingBilinearInterpolation(
            sample.Biome00.GetOffset(position),
            sample.Biome10.GetOffset(position),
            sample.Biome01.GetOffset(position),
            sample.Biome11.GetOffset(position),
            sample.SpecialBiome.GetOffset(position),
            sample.BlendFactors);
    }

    /// <summary>
    ///     Fill up the dampening to get the first solid layers of all biomes at the same height.
    /// </summary>
    private static Biome.Dampening CreateFilledDampening(int offset, in Map.Sample sample)
    {
        (int a, int b, int c, int d, int e) depths = (
            sample.Biome00.GetDepthToSolid(sample.Biome00.CalculateDampening(offset)),
            sample.Biome10.GetDepthToSolid(sample.Biome10.CalculateDampening(offset)),
            sample.Biome01.GetDepthToSolid(sample.Biome01.CalculateDampening(offset)),
            sample.Biome11.GetDepthToSolid(sample.Biome11.CalculateDampening(offset)),
            sample.SpecialBiome.GetDepthToSolid(sample.SpecialBiome.CalculateDampening(offset)));

        if (depths.a <= depths.b && depths.a <= depths.c && depths.a <= depths.d) depths.a *= 2;
        else if (depths.b <= depths.a && depths.b <= depths.c && depths.b <= depths.d) depths.b *= 2;
        else if (depths.c <= depths.a && depths.c <= depths.b && depths.c <= depths.d) depths.c *= 2;
        else if (depths.d <= depths.a && depths.d <= depths.b && depths.d <= depths.c) depths.d *= 2;
        else depths.e *= 2;

        var targetDepth = (int) VMath.MixingBilinearInterpolation(depths.a, depths.b, depths.c, depths.d, depths.e, sample.BlendFactors);
        Biome.Dampening dampening = sample.ActualBiome.CalculateDampening(offset);

        int fill = targetDepth - sample.ActualBiome.GetDepthToSolid(dampening);
        fill = Math.Max(val1: 0, fill);

        return dampening with {Width = dampening.Width + fill};
    }

    private static int GetIceWidth(in Map.Sample sample)
    {
        (int a, int b, int c, int d, int e) widths = (
            sample.Biome00.IceWidth,
            sample.Biome10.IceWidth,
            sample.Biome01.IceWidth,
            sample.Biome11.IceWidth,
            sample.SpecialBiome.IceWidth);

        return (int) Math.Round(VMath.MixingBilinearInterpolation(widths.a, widths.b, widths.c, widths.d, widths.e, sample.BlendFactors), MidpointRounding.AwayFromZero);
    }

    private void Initialize()
    {
        using BinaryReader? read = world.GetBlobReader(MapBlobName);
        map.Initialize(read, seed);
    }

    private void Store()
    {
        using BinaryWriter? write = world.GetBlobWriter(MapBlobName);
        if (write != null) map.Store(write);
    }

    private Content GenerateContent(Vector3i position, in Context context)
    {
        if (position.Y == -World.BlockLimit) return new Content(Block.Core);

        int depth = context.WorldHeight - position.Y;
        bool isFilled = position.Y <= SeaLevel;

        if (depth < 0) // A negative depths means that the block is above the world height.
        {
            bool isIce = isFilled && Math.Abs(position.Y - SeaLevel) < context.IceWidth;

            if (isIce) return new Content(Block.Specials.Ice.FullHeightInstance, FluidInstance.Default);

            var content = Content.Default;

            if (depth == -1) content = context.Biome.Cover.GetContent(position, isFilled, context.Sample);

            if (isFilled) content = FillContent(content);

            return content;
        }

        Map.StoneType stoneType = context.GetStoneType(position);

        return depth >= context.Biome.GetTotalWidth(context.Dampening) ? palette.GetStone(stoneType) : GetBiomeContent(depth, isFilled, stoneType, context);
    }

    private static Content GetBiomeContent(int depth, bool isFilled, Map.StoneType stoneType, Context context)
    {
        Content content = context.Biome.GetContent(depth, context.Dampening, stoneType, isFilled);

        if (isFilled) content = FillContent(content);

        return content;
    }

    private static Content FillContent(Content content)
    {
        if (content.Fluid.Fluid != Fluid.None) return content;
        if (content.Block.Block is not IFillable) return content;

        return content with {Fluid = Fluid.Water.AsInstance()};
    }

    private readonly record struct Context
    {
        public int WorldHeight { get; init; }

        public Biome.Dampening Dampening { get; init; }

        public Biome Biome => Sample.ActualBiome;

        public Map.Sample Sample { get; init; }

        public Map Map { private get; init; }

        public int IceWidth { get; init; }

        public Map.StoneType GetStoneType(Vector3i position)
        {
            return Map.GetStoneType(position, Sample);
        }
    }
}
