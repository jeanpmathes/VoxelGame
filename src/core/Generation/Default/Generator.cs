// <copyright file="Generator.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using OpenTK.Mathematics;
using VoxelGame.Core.Collections;
using VoxelGame.Core.Generation.Default.Deco;
using VoxelGame.Core.Logic;
using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Profiling;
using VoxelGame.Core.Utilities;
using VoxelGame.Logging;

namespace VoxelGame.Core.Generation.Default;

/// <summary>
///     The default world generator.
/// </summary>
public partial class Generator : IWorldGenerator
{
    private const Int32 SeaLevel = 0;

    private const String MapBlobName = "default_map";

    private readonly FastNoiseLite decorationNoise;

    private readonly Palette palette = new();

    /// <summary>
    ///     Used for map generation and sampling.
    /// </summary>
#pragma warning disable S1450 // Used for documentation purposes.
    private readonly NoiseFactory mapNoiseFactory;
#pragma warning restore S1450

    /// <summary>
    ///     Used for details in biomes, structures and decoration.
    /// </summary>
#pragma warning disable S1450 // Used for documentation purposes.
    private readonly NoiseFactory worldNoiseFactory;
#pragma warning restore S1450

    /// <summary>
    ///     Creates a new default world generator.
    /// </summary>
    /// <param name="world">The world to generate.</param>
    /// <param name="timer">A timer to measure initialization behavior.</param>
    public Generator(World world, Timer? timer)
    {
        mapNoiseFactory = new NoiseFactory(world.Seed.upper);
        worldNoiseFactory = new NoiseFactory(world.Seed.lower);

        Biomes biomes;

        using (logger.BeginTimedSubScoped("Biomes Setup", timer))
        {
            biomes = Biomes.Load();
            biomes.Setup(worldNoiseFactory, palette);
        }

        using (logger.BeginTimedSubScoped("Structures Setup", timer))
        {
            Structures.Instance.Setup(worldNoiseFactory);
        }

        using (logger.BeginTimedSubScoped("Map Setup", timer))
        {
            Map = new Map(BiomeDistribution.CreateDefault(biomes));

            Map.Initialize(world.Data, MapBlobName, mapNoiseFactory, out Boolean dirty);

            if (dirty)
                Map.Store(world.Data, MapBlobName);
        }

        decorationNoise = worldNoiseFactory.GetNextNoise();
        decorationNoise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
        decorationNoise.SetFrequency(frequency: 0.5f);

        LogCreatedWorldGenerator(logger, nameof(Default));
    }

    /// <summary>
    ///     Get the map used by this generator.
    /// </summary>
    public Map Map { get; }

    /// <inheritdoc />
    public IEnumerable<Content> GenerateColumn(Int32 x, Int32 z, (Int32 start, Int32 end) heightRange)
    {
        Map.Sample sample = Map.GetSample((x, z));

        Context context = new()
        {
            Map = Map,
            Sample = sample,
            WorldHeight = GetWorldHeight((x, z), sample, out Int32 effectiveOffset),
            Dampening = CreateFilledDampening(effectiveOffset, sample),
            IceWidth = GetIceWidth(sample)
        };

        for (Int32 y = heightRange.start; y < heightRange.end; y++) yield return GenerateContent((x, y, z), context);
    }

    /// <inheritdoc />
    public void DecorateSection(SectionPosition position, Array3D<Section> sections)
    {
        Debug.Assert(sections.Length == 3);

        ICollection<Biome> biomes = GetSectionBiomes(position);

        HashSet<Decoration> decorations = [];
        Dictionary<Decoration, HashSet<Biome>> decorationToBiomes = new();

        foreach (Biome biome in biomes)
        foreach (Decoration decoration in biome.Decorations)
        {
            decorations.Add(decoration);
            decorationToBiomes.GetOrAdd(decoration).Add(biome);
        }

        Debug.Assert(decorations.GroupBy(d => d.Name).All(g => g.Count() <= 1), "Duplicate decoration names or cloned decorations.");

        Array3D<Single> noise = GenerateDecorationNoise(position);

        var index = 0;

        foreach (Decoration decoration in decorations.OrderByDescending(d => d.Size).ThenBy(d => d.Name))
        {
            Decoration.Context context = new(position, sections, decorationToBiomes[decoration], noise, index++, palette, this);

            decoration.Place(context);
        }
    }

    /// <inheritdoc />
    public void EmitViews(DirectoryInfo path)
    {
        Map.EmitViews(path);
    }

    /// <inheritdoc />
    public void GenerateStructures(Section section, SectionPosition position)
    {
        ICollection<Biome> biomes = GetSectionBiomes(position);

        if (biomes.Count != 1) return;

        biomes.First().Structure?.AttemptPlacement(section, position, this);
    }

    /// <inheritdoc />
    public IEnumerable<Vector3i>? SearchNamedGeneratedElements(Vector3i start, String name, UInt32 maxDistance)
    {
        return Structures.Instance.Search(start, name, maxDistance, this);
    }

    /// <inheritdoc />
    IMap IWorldGenerator.Map => Map;

    /// <summary>
    ///     Prepare all required systems to use the generator.
    /// </summary>
    public static void Prepare(LoadingContext loadingContext)
    {
        using (loadingContext.BeginStep("Default Generator"))
        {
            Decorations.Initialize(loadingContext);
            Structures.Initialize(loadingContext);
        }
    }

    /// <summary>
    ///     Get the world height for the given column.
    /// </summary>
    /// <param name="column">The column to get the height for.</param>
    /// <param name="sample">A map sample for the column.</param>
    /// <param name="effectiveOffset">The effective offset of the column.</param>
    /// <returns>The world height.</returns>
    public static Int32 GetWorldHeight(Vector2i column, in Map.Sample sample, out Int32 effectiveOffset)
    {
        Double offset = GetOffset(column, sample);
        Double height = sample.Height * Map.MaxHeight;

        var rawHeight = (Int32) height;
        var modifiedHeight = (Int32) (height + offset);
        effectiveOffset = rawHeight - modifiedHeight;

        return modifiedHeight;
    }

    /// <summary>
    ///     Get the world height for the given column.
    /// </summary>
    /// <param name="column">The column to get the height for.</param>
    /// <returns>The world height.</returns>
    public Int32 GetWorldHeight(Vector2i column)
    {
        return GetWorldHeight(column, Map.GetSample(column), out _);
    }

    private Array3D<Single> GenerateDecorationNoise(SectionPosition position)
    {
        var noise = new Array3D<Single>(Section.Size);

        for (var x = 0; x < Section.Size; x++)
        for (var y = 0; y < Section.Size; y++)
        for (var z = 0; z < Section.Size; z++)
        {
            Vector3i blockPosition = position.FirstBlock + (x, y, z);

            noise[x, y, z] = decorationNoise.GetNoise(blockPosition.X, blockPosition.Y, blockPosition.Z);
        }

        return noise;
    }

    /// <summary>
    ///     Get the biomes for a given section.
    ///     The biomes are determined by sampling each corner of the section.
    /// </summary>
    /// <param name="position">The position of the section.</param>
    /// <returns>A list of the biomes, each biome is only included once.</returns>
    public ICollection<Biome> GetSectionBiomes(SectionPosition position)
    {
        List<Biome> biomes = new();

        Vector2i start = position.FirstBlock.Xz;

        biomes.Add(Map.GetSample(start).ActualBiome);
        biomes.Add(Map.GetSample(start + (0, Section.Size)).ActualBiome);
        biomes.Add(Map.GetSample(start + (Section.Size, 0)).ActualBiome);
        biomes.Add(Map.GetSample(start + (Section.Size, Section.Size)).ActualBiome);

        biomes = biomes.Distinct().ToList();

        return biomes;
    }

    private static Double GetOffset(Vector2i position, in Map.Sample sample)
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
    private static Biome.Dampening CreateFilledDampening(Int32 offset, in Map.Sample sample)
    {
        (Int32 a, Int32 b, Int32 c, Int32 d, Int32 e) depths = (
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

        var targetDepth = (Int32) VMath.MixingBilinearInterpolation(depths.a, depths.b, depths.c, depths.d, depths.e, sample.BlendFactors);
        Biome.Dampening dampening = sample.ActualBiome.CalculateDampening(offset);

        Int32 fill = targetDepth - sample.ActualBiome.GetDepthToSolid(dampening);
        fill = Math.Max(val1: 0, fill);

        return dampening with {Width = dampening.Width + fill};
    }

    private static Int32 GetIceWidth(in Map.Sample sample)
    {
        (Int32 a, Int32 b, Int32 c, Int32 d, Int32 e) widths = (
            sample.Biome00.IceWidth,
            sample.Biome10.IceWidth,
            sample.Biome01.IceWidth,
            sample.Biome11.IceWidth,
            sample.SpecialBiome.IceWidth);

        return (Int32) Math.Round(VMath.MixingBilinearInterpolation(widths.a, widths.b, widths.c, widths.d, widths.e, sample.BlendFactors), MidpointRounding.AwayFromZero);
    }

    private Content GenerateContent(Vector3i position, in Context context)
    {
        if (position.Y == -World.BlockLimit) return new Content(Blocks.Instance.Core);

        Int32 depth = context.WorldHeight - position.Y;
        Boolean isFilled = position.Y <= SeaLevel;

        if (depth < 0) // A negative depths means that the block is above the world height.
        {
            Boolean isIce = isFilled && Math.Abs(position.Y - SeaLevel) < context.IceWidth;

            if (isIce) return new Content(Blocks.Instance.Specials.Ice.FullHeightInstance, FluidInstance.Default);

            var content = Content.Default;

            if (depth == -1) content = context.Biome.Cover.GetContent(position, isFilled, context.Sample);

            if (isFilled) content = FillContent(content);

            return content;
        }

        Map.StoneType stoneType = context.GetStoneType(position);

        return depth >= context.Biome.GetTotalWidth(context.Dampening) ? palette.GetStone(stoneType) : GetBiomeContent(depth, isFilled, stoneType, context);
    }

    private static Content GetBiomeContent(Int32 depth, Boolean isFilled, Map.StoneType stoneType, Context context)
    {
        Content content = context.Biome.GetContent(depth, context.Dampening, stoneType, isFilled);

        if (isFilled) content = FillContent(content);

        return content;
    }

    private static Content FillContent(Content content)
    {
        if (!content.Fluid.IsEmpty) return content;
        if (content.Block.Block is not IFillable) return content;

        return content with {Fluid = Fluids.Instance.SeaWater.AsInstance()};
    }

    private readonly record struct Context
    {
        public Int32 WorldHeight { get; init; }

        public Biome.Dampening Dampening { get; init; }

        public Biome Biome => Sample.ActualBiome;

        public Map.Sample Sample { get; init; }

        public Map Map { private get; init; }

        public Int32 IceWidth { get; init; }

        public Map.StoneType GetStoneType(Vector3i position)
        {
            return Map.GetStoneType(position, Sample);
        }
    }

    #region LOGGING

    private static readonly ILogger logger = LoggingHelper.CreateLogger<Generator>();

    [LoggerMessage(EventId = Events.WorldGeneration, Level = LogLevel.Information, Message = "Created '{Name}' world generator")]
    private static partial void LogCreatedWorldGenerator(ILogger logger, String name);

    #endregion LOGGING
}
