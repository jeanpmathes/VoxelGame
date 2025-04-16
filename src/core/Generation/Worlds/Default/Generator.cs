// <copyright file="Generator.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using OpenTK.Mathematics;
using VoxelGame.Core.Actors;
using VoxelGame.Core.Collections;
using VoxelGame.Core.Generation.Worlds.Default.Biomes;
using VoxelGame.Core.Generation.Worlds.Default.Decorations;
using VoxelGame.Core.Generation.Worlds.Default.Palettes;
using VoxelGame.Core.Generation.Worlds.Default.Search;
using VoxelGame.Core.Generation.Worlds.Default.Structures;
using VoxelGame.Core.Generation.Worlds.Default.SubBiomes;
using VoxelGame.Core.Logic;
using VoxelGame.Core.Logic.Chunks;
using VoxelGame.Core.Logic.Elements;
using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Logic.Sections;
using VoxelGame.Core.Profiling;
using VoxelGame.Core.Updates;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Utilities.Resources;
using VoxelGame.Logging;
using VoxelGame.Toolkit.Collections;
using VoxelGame.Toolkit.Noise;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Core.Generation.Worlds.Default;

/// <summary>
///     The default world generator.
/// </summary>
public sealed partial class Generator : IWorldGenerator
{
    private const Int32 SeaLevel = 0;
    private const String MapBlobName = "default_map";

    private static Palette? loadedPalette;
    private static BiomeDistributionDefinition? loadedBiomeDistribution;

    private static List<StructureGeneratorDefinition> loadedStructures = [];
    private static List<SubBiomeDefinition> loadedSubBiomes = [];
    private static List<BiomeDefinition> loadedBiomes = [];

    private readonly Cache<(Int32, Int32), ColumnSampleStore> columnCache = new(MathTools.Square((Player.LoadDistance + 1) * 2 + 1));

    private readonly Palette palette;

    private readonly NoiseGenerator decorationNoise;

    private readonly List<StructureGenerator> structures = [];
    private readonly List<SubBiome> subBiomes = [];
    private readonly List<Biome> biomes = [];

    private readonly Searcher search;

    private Generator(IWorldGeneratorContext context, Palette palette,
        BiomeDistributionDefinition biomeDistributionDefinition,
        IEnumerable<StructureGeneratorDefinition> structureDefinitions,
        IEnumerable<SubBiomeDefinition> subBiomeDefinitions,
        IEnumerable<BiomeDefinition> biomeDefinitions)
    {
        search = new Searcher(this);

        this.palette = palette;

        // Used for map generation and sampling.
        NoiseFactory mapNoiseFactory = new(context.Seed.upper);

        // Used for details in biomes, structures and decoration.
        NoiseFactory worldNoiseFactory = new(context.Seed.lower);

        Dictionary<BiomeDefinition, Biome> biomeMap = new();
        Dictionary<SubBiomeDefinition, SubBiome> subBiomeMap = new();
        Dictionary<StructureGeneratorDefinition, StructureGenerator> structureMap = new();

        Dictionary<String, StructureGenerator> structuresByName = new();
        Dictionary<String, SubBiome> subBiomesByName = new();
        Dictionary<String, Biome> biomesByName = new();

        using (logger.BeginTimedSubScoped("Structures Setup", context.Timer))
        {
            foreach (StructureGeneratorDefinition definition in structureDefinitions.OrderBy(e => e.Identifier.ToString()))
            {
                StructureGenerator generator = new(worldNoiseFactory, definition);

                structureMap.Add(definition, generator);
                structures.Add(generator);

                structuresByName.Add(definition.Name, generator);
            }
        }

        using (logger.BeginTimedSubScoped("Sub-Biomes Setup", context.Timer))
        {
            foreach (SubBiomeDefinition definition in subBiomeDefinitions.OrderBy(e => e.Identifier.ToString()))
            {
                SubBiome subBiome = new(worldNoiseFactory, definition, structureMap);

                subBiomeMap.Add(definition, subBiome);
                subBiomes.Add(subBiome);

                subBiomesByName.Add(definition.Name, subBiome);
            }
        }

        using (logger.BeginTimedSubScoped("Biomes Setup", context.Timer))
        {
            foreach (BiomeDefinition definition in biomeDefinitions.OrderBy(e => e.Identifier.ToString()))
            {
                Biome biome = new(definition, subBiomeMap);

                biomeMap.Add(definition, biome);
                biomes.Add(biome);

                biomesByName.Add(definition.Name, biome);
            }

            Biomes = new BiomeDistribution(biomeDistributionDefinition, biomeMap);
        }

        using (logger.BeginTimedSubScoped("Map Setup", context.Timer))
        {
            Map = new Map(Biomes);

            Map.Initialize(context, MapBlobName, mapNoiseFactory, out Boolean dirty);

            if (dirty)
                Map.Store(context, MapBlobName);
        }

        decorationNoise = worldNoiseFactory.CreateNext()
            .WithType(NoiseType.GradientNoise)
            .WithFrequency(frequency: 0.5f)
            .Build();

        search.InitializeSearch(structuresByName, subBiomesByName, biomesByName);

        LogCreatedWorldGenerator(logger, nameof(Default));
    }

    /// <summary>
    ///     The biomes and their distribution.
    /// </summary>
    public BiomeDistribution Biomes { get; }

    /// <summary>
    ///     Get the map used by this generator.
    /// </summary>
    public Map Map { get; }

    /// <inheritdoc />
    public static ICatalogEntry CreateResourceCatalog()
    {
        return new Catalog();
    }

    /// <inheritdoc />
    public static void LinkResources(IResourceContext context)
    {
        context.Require<Palette>(palette =>
            context.Require<BiomeDistributionDefinition>(biomeDistribution =>
            {
                loadedPalette = palette;
                loadedBiomeDistribution = biomeDistribution;

                loadedStructures = context.GetAll<StructureGeneratorDefinition>().ToList();
                loadedSubBiomes = context.GetAll<SubBiomeDefinition>().ToList();
                loadedBiomes = context.GetAll<BiomeDefinition>().ToList();

                return [];
            }));
    }

    /// <inheritdoc />
    public static IWorldGenerator? Create(IWorldGeneratorContext context)
    {
        if (loadedPalette == null || loadedBiomeDistribution == null)
            return null;

        return new Generator(context, loadedPalette, loadedBiomeDistribution, loadedStructures, loadedSubBiomes, loadedBiomes);
    }

    /// <inheritdoc />
    public IGenerationContext CreateGenerationContext(ChunkPosition hint)
    {
        return new GenerationContext(this, hint);
    }

    /// <inheritdoc />
    public IDecorationContext CreateDecorationContext(ChunkPosition hint, Int32 extents = 0)
    {
        return new DecorationContext(this, hint, extents);
    }

    /// <inheritdoc />
    public Operation EmitWorldInfo(DirectoryInfo path)
    {
        return Map.EmitWorldInfo(path);
    }

    /// <inheritdoc />
    public IEnumerable<Vector3i>? SearchNamedGeneratedElements(Vector3i start, String name, UInt32 maxDistance)
    {
        return search.Search(start, name, maxDistance);
    }

    /// <inheritdoc />
    IMap IWorldGenerator.Map => Map;

    /// <summary>
    ///     Get the samples for a chunk position, if available.
    /// </summary>
    internal ColumnSampleStore? GetColumns(ChunkPosition position)
    {
        columnCache.TryGet((position.X, position.Z), out ColumnSampleStore? store);

        return store;
    }

    /// <summary>
    ///     Add the samples for a chunk position to the storage.
    /// </summary>
    internal void AddColumns(ColumnSampleStore store)
    {
        columnCache.Add(store.Key, store);
    }

    /// <inheritdoc cref="IGenerationContext.GenerateColumn" />
    internal IEnumerable<Content> GenerateColumn(
        Int32 x, Int32 z,
        (Int32 start, Int32 end) heightRange,
        ColumnSampleStore columns)
    {
        Map.Sample sample = columns.GetSample((x, z));

        Context context = new()
        {
            Map = Map,
            Sample = sample,
            WorldHeight = GetWorldHeight((x, z), sample, out Double heightFraction, out Int32 effectiveOffset),
            WorldHeightFraction = heightFraction,
            EffectiveOffset = effectiveOffset,
            Dampening = CreateFilledDampening(effectiveOffset, sample),
            IceWidth = GetIceWidth(sample)
        };

        for (Int32 y = heightRange.start; y < heightRange.end; y++)
            yield return GenerateContent((x, y, z), context);
    }

    /// <inheritdoc cref="IDecorationContext.DecorateSection" />
    internal void DecorateSection(Neighborhood<Section> sections, ColumnSampleStore? columns)
    {
        ICollection<SubBiome> sectionSubBiomes = GetSectionSubBiomes(sections.Center.Position, columns);

        Dictionary<Decoration, Single> decorationToRarity = new();
        Dictionary<Decoration, HashSet<SubBiome>> decorationToSubBiomes = new();

        foreach (SubBiome subBiome in sectionSubBiomes)
        foreach ((Decoration decoration, Single rarity) in subBiome.Definition.Decorations)
        {
            // A lower rarity means a higher chance of placement, and rarity can be any number from 0 to infinity.

            decorationToRarity[decoration] = Math.Min(rarity, decorationToRarity.GetValueOrDefault(decoration, Single.PositiveInfinity));
            decorationToSubBiomes.GetOrAdd(decoration).Add(subBiome);
        }

        Array3D<Single> noise = decorationNoise.GetNoiseGrid(sections.Center.Position.FirstBlock, Section.Size);

        var index = 0;

        foreach ((Decoration decoration, Single rarity) in decorationToRarity.OrderByDescending(d => d.Key.Size).ThenBy(d => d.Key.Name))
        {
            Decoration.Context context = new(sections.Center.Position, sections, decorationToSubBiomes[decoration], noise, rarity, index++, palette, this);

            decoration.Place(context);
        }
    }

    /// <inheritdoc cref="IGenerationContext.GenerateStructures" />
    internal void GenerateStructures(Section section, ColumnSampleStore? columns)
    {
        ICollection<SubBiome> sectionBiomes = GetSectionSubBiomes(section.Position, columns);

        if (sectionBiomes.Count != 1) return;

        sectionBiomes.First().Structure?.AttemptPlacement(section, this);
    }

    /// <summary>
    ///     Get the world height for the given column.
    /// </summary>
    /// <param name="column">The column to get the height for.</param>
    /// <param name="sample">A map sample for the column.</param>
    /// <param name="heightFraction">The fraction of the height above the integer part.</param>
    /// <param name="effectiveOffset">The effective offset of the column.</param>
    /// <returns>The world height, in blocks.</returns>
    public static Int32 GetWorldHeight(Vector2i column, in Map.Sample sample, out Double heightFraction, out Int32 effectiveOffset)
    {
        Double offset = GetOffset(column, sample);
        Double height = sample.Height * Map.MaxHeight;

        var rawHeight = (Int32) height;
        var modifiedHeight = (Int32) (height + offset);

        heightFraction = MathTools.Fraction(height + offset);
        effectiveOffset = rawHeight - modifiedHeight;

        return modifiedHeight;
    }

    /// <summary>
    ///     Get the world height for the given column.
    /// </summary>
    /// <param name="position">The position to get the height for. The Y component is ignored.</param>
    /// <returns>The world height.</returns>
    public Int32 GetWorldHeight(Vector3i position)
    {
        return GetWorldHeight(position.Xz, Map.GetSample(position), out _, out _);
    }

    /// <summary>
    ///     Get the sub-biomes for a given section.
    ///     The biomes are determined by sampling each corner of the section.
    /// </summary>
    /// <param name="position">The position of the section.</param>
    /// <param name="columns">A column sample store to use, if available.</param>
    /// <returns>A list of the biomes, each biome is only included once.</returns>
    internal ICollection<SubBiome> GetSectionSubBiomes(SectionPosition position, ColumnSampleStore? columns)
    {
        List<SubBiome> sectionSubBiomes = [];

        Vector2i start = position.FirstBlock.Xz;
        const Int32 offset = Section.Size - 1;

        sectionSubBiomes.Add(ColumnSampleStore.GetSample(start + (0, 0), columns, Map).ActualSubBiome);
        sectionSubBiomes.Add(ColumnSampleStore.GetSample(start + (offset, 0), columns, Map).ActualSubBiome);
        sectionSubBiomes.Add(ColumnSampleStore.GetSample(start + (0, offset), columns, Map).ActualSubBiome);
        sectionSubBiomes.Add(ColumnSampleStore.GetSample(start + (offset, offset), columns, Map).ActualSubBiome);

        sectionSubBiomes = sectionSubBiomes.Distinct().ToList();

        return sectionSubBiomes;
    }

    private static Double GetOffset(Vector2i position, in Map.Sample sample)
    {
        return MathTools.BiLerp(
            sample.SubBiome00.GetOffset(position),
            sample.SubBiome10.GetOffset(position),
            sample.SubBiome01.GetOffset(position),
            sample.SubBiome11.GetOffset(position),
            sample.SubBiomeBlendFactors);
    }

    /// <summary>
    ///     Fill up the dampening to get the first solid layers of all biomes at the same height.
    /// </summary>
    private static SubBiome.Dampening CreateFilledDampening(Int32 offset, in Map.Sample sample)
    {
        (Int32 a, Int32 b, Int32 c, Int32 d) depths = (
            sample.SubBiome00.GetDepthToSolid(sample.SubBiome00.CalculateDampening(offset)),
            sample.SubBiome10.GetDepthToSolid(sample.SubBiome10.CalculateDampening(offset)),
            sample.SubBiome01.GetDepthToSolid(sample.SubBiome01.CalculateDampening(offset)),
            sample.SubBiome11.GetDepthToSolid(sample.SubBiome11.CalculateDampening(offset)));

        if (depths.a <= depths.b && depths.a <= depths.c && depths.a <= depths.d) depths.a *= 2;
        else if (depths.b <= depths.a && depths.b <= depths.c && depths.b <= depths.d) depths.b *= 2;
        else if (depths.c <= depths.a && depths.c <= depths.b && depths.c <= depths.d) depths.c *= 2;
        else depths.d *= 2;

        var targetDepth = (Int32) MathTools.BiLerp(depths.a, depths.b, depths.c, depths.d, sample.SubBiomeBlendFactors);
        SubBiome.Dampening dampening = sample.ActualSubBiome.CalculateDampening(offset);

        Int32 fill = targetDepth - sample.ActualSubBiome.GetDepthToSolid(dampening);
        fill = Math.Max(val1: 0, fill);

        return dampening with {Width = dampening.Width + fill};
    }

    private static Int32 GetIceWidth(in Map.Sample sample)
    {
        (Int32 a, Int32 b, Int32 c, Int32 d) widths = (
            sample.SubBiome00.Definition.IceWidth,
            sample.SubBiome10.Definition.IceWidth,
            sample.SubBiome01.Definition.IceWidth,
            sample.SubBiome11.Definition.IceWidth);

        return (Int32) Math.Round(MathTools.BiLerp(widths.a, widths.b, widths.c, widths.d, sample.SubBiomeBlendFactors), MidpointRounding.AwayFromZero);
    }

    private Content GenerateContent(Vector3i position, in Context context)
    {
        if (position.Y == -World.BlockLimit) return new Content(Blocks.Instance.Core);

        Int32 depth = context.WorldHeight - position.Y;
        Boolean isFilled = position.Y <= SeaLevel;

        if (depth < 0) // A negative depths means that the block is above the world height.
        {
            Boolean isStuffed = context is {SubBiome.Definition.Stuffer: not null, EffectiveOffset: > 0} && context.EffectiveOffset + context.SubBiome.Definition.Offset >= -depth;
            Boolean isIce = isFilled && Math.Abs(position.Y - SeaLevel) < context.IceWidth;

            if (isIce && !isStuffed) return new Content(Blocks.Instance.Specials.Ice.FullHeightInstance, FluidInstance.Default);

            var content = Content.Default;

            if (isStuffed) content = context.SubBiome.Definition.Stuffer!.GetContent();
            else if (depth == -1) content = context.SubBiome.GetCoverContent(position, isFilled, context.WorldHeightFraction, context.Sample);

            if (isFilled) content = FillContent(content);

            return content;
        }

        Map.StoneType stoneType = context.GetStoneType(position);

        return depth >= context.SubBiome.GetTotalWidth(context.Dampening) ? palette.GetStone(stoneType) : GetBiomeContent(depth, position.Y, isFilled, stoneType, context);
    }

    private static Content GetBiomeContent(Int32 depth, Int32 y, Boolean isFilled, Map.StoneType stoneType, in Context context)
    {
        Content content = context.SubBiome.GetContent(depth, y, context.Dampening, stoneType, isFilled);

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

        public Double WorldHeightFraction { get; init; }

        public Int32 EffectiveOffset { get; init; }

        public SubBiome.Dampening Dampening { get; init; }

        public SubBiome SubBiome => Sample.ActualSubBiome;

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

    [LoggerMessage(EventId = LogID.DefaultGenerator + 0, Level = LogLevel.Information, Message = "Created '{Name}' world generator")]
    private static partial void LogCreatedWorldGenerator(ILogger logger, String name);

    #endregion LOGGING

    #region DISPOSABLE

    private Boolean disposed;

    private void Dispose(Boolean disposing)
    {
        if (disposed) return;

        if (disposing)
        {
            decorationNoise.Dispose();

            foreach (StructureGenerator structure in structures)
                structure.Dispose();

            foreach (SubBiome subBiome in subBiomes)
                subBiome.Dispose();

            foreach (Biome biome in biomes)
                biome.Dispose();

            Map.Dispose();
        }
        else
        {
            Throw.ForMissedDispose(this);
        }

        disposed = true;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///     Finalizer.
    /// </summary>
    ~Generator()
    {
        Dispose(disposing: false);
    }

    #endregion
}
