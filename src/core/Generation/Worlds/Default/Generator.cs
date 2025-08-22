// <copyright file="Generator.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using OpenTK.Mathematics;
using VoxelGame.Core.Actors.Components;
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
using VoxelGame.Core.Logic.Elements.Behaviors.Fluids;
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

    private readonly Cache<(Int32, Int32), ColumnSampleStore> columnCache = new(MathTools.Square((ChunkLoader.LoadDistance + 1) * 2 + 1));

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
        ColumnSampleStore store)
    {
        Map.Sample sample = store.GetSample((x, z));

        Context context = new()
        {
            Map = Map,
            Sample = sample,
            Ground = new Surface
            {
                Height = GetGroundHeight((x, z), sample, out Double heightFraction, out Int32 effectiveOffset),
                HeightFraction = heightFraction,
                EffectiveOffset = effectiveOffset,
                Dampening = CreateFilledDampening(effectiveOffset,
                    (sample.SubBiome00, sample.SubBiome10, sample.SubBiome01, sample.SubBiome11),
                    sample.ActualSubBiome,
                    sample.SubBiomeBlendFactors),
                SubBiome = sample.ActualSubBiome
            },
            Oceanic = sample.ActualOceanicSubBiome != null
                ? new Surface
                {
                    Height = GetOceanicHeight((x, z), sample, out heightFraction, out effectiveOffset),
                    HeightFraction = heightFraction,
                    EffectiveOffset = effectiveOffset,
                    Dampening = CreateFilledDampening(
                        effectiveOffset,
                        (sample.OceanicSubBiome00, sample.OceanicSubBiome10, sample.OceanicSubBiome01, sample.OceanicSubBiome11),
                        sample.ActualOceanicSubBiome,
                        sample.SubBiomeBlendFactors),
                    SubBiome = sample.ActualOceanicSubBiome
                }
                : null
        };

        for (Int32 y = heightRange.start; y < heightRange.end; y++)
            yield return GenerateContent((x, y, z), context);
    }

    /// <inheritdoc cref="IDecorationContext.DecorateSection" />
    internal void DecorateSection(Neighborhood<Section> sections, ColumnSampleStore? store)
    {
        ICollection<SubBiome> sectionSubBiomes = GetDistinctSectionSubBiomes(sections.Center.Position, store);

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
    internal void GenerateStructures(Section section, ColumnSampleStore? store)
    {
        if (IsStructurePlacementAllowed(section.Position, out StructureGenerator? structure, store))
            structure.AttemptPlacement(section, this);
    }

    internal Boolean IsStructurePlacementAllowed(SectionPosition position, [NotNullWhen(returnValue: true)] out StructureGenerator? structure, ColumnSampleStore? store)
    {
        (SubBiome s00, SubBiome s10, SubBiome s01, SubBiome s11) = GetSectionSubBiomes(position, store);

        structure = s00.Structure;

        return structure != null && s00.Structure == s10.Structure && s00.Structure == s01.Structure && s00.Structure == s11.Structure;
    }

    /// <summary>
    ///     Get the ground height for the given column.
    ///     The ground height is the height of solid ground.
    /// </summary>
    /// <param name="column">The column to get the height for.</param>
    /// <param name="sample">A map sample for the column.</param>
    /// <param name="heightFraction">The fraction of the height above the integer part.</param>
    /// <param name="effectiveOffset">The effective offset of the column.</param>
    /// <returns>The ground height, in blocks.</returns>
    public static Int32 GetGroundHeight(Vector2i column, in Map.Sample sample, out Double heightFraction, out Int32 effectiveOffset)
    {
        Double offset = GetOffset(column, sample);
        Double height = sample.Height * Map.MaxHeight;

        var rawHeight = (Int32) height;
        var modifiedHeight = (Int32) (height + offset);

        heightFraction = MathTools.Fraction(height + offset);
        effectiveOffset = modifiedHeight - rawHeight;

        return modifiedHeight;
    }

    /// <summary>
    ///     Get the ground height for the given column.
    /// </summary>
    /// <param name="position">The position to get the height for. The Y component is ignored.</param>
    /// <returns>The ground height.</returns>
    public Int32 GetGroundHeight(Vector3i position)
    {
        return GetGroundHeight(position.Xz, Map.GetSample(position), out _, out _);
    }

    /// <summary>
    ///     Get the height of the oceanic sub-biome for the given column.
    ///     Note that this is not the height of the ocean floor (which would be the ground height) but the height of the biome
    ///     above or at sea level.
    /// </summary>
    public static Int32 GetOceanicHeight(Vector2i column, in Map.Sample sample, out Double heightFraction, out Int32 effectiveOffset)
    {
        if (sample.ActualOceanicSubBiome?.Definition.IsEmpty == true)
        {
            heightFraction = 0;
            effectiveOffset = 0;
        }
        else
        {
            Double height;

            if (sample.ActualOceanicSubBiome?.Definition.IgnoresBlendedOffset == true) height = sample.ActualOceanicSubBiome.GetOffset(column);
            else
                height = MathTools.BiLerp(
                    sample.OceanicSubBiome00?.GetOffset(column) ?? 0,
                    sample.OceanicSubBiome10?.GetOffset(column) ?? 0,
                    sample.OceanicSubBiome01?.GetOffset(column) ?? 0,
                    sample.OceanicSubBiome11?.GetOffset(column) ?? 0,
                    sample.SubBiomeBlendFactors);

            heightFraction = MathTools.Fraction(height);
            effectiveOffset = (Int32) height;
        }

        return effectiveOffset;
    }

    /// <summary>
    ///     Get the sub-biomes for a given section.
    ///     The biomes are determined by sampling each corner of the section.
    /// </summary>
    /// <param name="position">The position of the section.</param>
    /// <param name="store">A column sample store to use, if available.</param>
    /// <returns>A list of the biomes, each biome is only included once.</returns>
    internal ICollection<SubBiome> GetDistinctSectionSubBiomes(SectionPosition position, ColumnSampleStore? store)
    {
        List<SubBiome> sectionSubBiomes = [];

        (SubBiome s00, SubBiome s10, SubBiome s01, SubBiome s11) = GetSectionSubBiomes(position, store);
        sectionSubBiomes.Add(s00);
        sectionSubBiomes.Add(s10);
        sectionSubBiomes.Add(s01);
        sectionSubBiomes.Add(s11);

        sectionSubBiomes = sectionSubBiomes.Distinct().ToList();

        return sectionSubBiomes;
    }

    internal (SubBiome s00, SubBiome s10, SubBiome s01, SubBiome s11) GetSectionSubBiomes(SectionPosition position, ColumnSampleStore? store)
    {
        Vector2i start = position.FirstBlock.Xz;
        const Int32 offset = Section.Size - 1;

        return (
            ColumnSampleStore.GetSample(start + (0, 0), store, Map).ActualSubBiome,
            ColumnSampleStore.GetSample(start + (offset, 0), store, Map).ActualSubBiome,
            ColumnSampleStore.GetSample(start + (0, offset), store, Map).ActualSubBiome,
            ColumnSampleStore.GetSample(start + (offset, offset), store, Map).ActualSubBiome);
    }

    private static Double GetOffset(Vector2i position, in Map.Sample sample)
    {
        // A normal sub-biome cannot be empty.

        if (sample.ActualSubBiome.Definition.IgnoresBlendedOffset) return sample.ActualSubBiome.GetOffset(position);

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
    private static SubBiome.Dampening CreateFilledDampening(
        Int32 offset,
        (SubBiome? s00, SubBiome? s10, SubBiome? s01, SubBiome? s11) subBiomes, SubBiome? actual,
        Vector2d blendFactors)
    {
        (Int32 a, Int32 b, Int32 c, Int32 d) depths = GetDepthsToSolid(offset, subBiomes);

        if (depths.a <= depths.b && depths.a <= depths.c && depths.a <= depths.d) depths.a *= 2;
        else if (depths.b <= depths.a && depths.b <= depths.c && depths.b <= depths.d) depths.b *= 2;
        else if (depths.c <= depths.a && depths.c <= depths.b && depths.c <= depths.d) depths.c *= 2;
        else depths.d *= 2;

        var targetDepth = (Int32) MathTools.BiLerp(depths.a, depths.b, depths.c, depths.d, blendFactors);
        SubBiome.Dampening dampening = actual?.CalculateDampening(offset) ?? new SubBiome.Dampening(offset, offset, Width: 0);

        Int32 fill = targetDepth - actual?.GetDepthToSolid(dampening) ?? 0;
        fill = Math.Max(val1: 0, fill);

        return dampening with {Width = dampening.Width + fill};
    }

    private static (Int32 a, Int32 b, Int32 c, Int32 d) GetDepthsToSolid(Int32 offset, (SubBiome? s00, SubBiome? s10, SubBiome? s01, SubBiome? s11) subBiomes)
    {
        return (
            subBiomes.s00?.GetDepthToSolid(subBiomes.s00.CalculateDampening(offset)) ?? 0,
            subBiomes.s10?.GetDepthToSolid(subBiomes.s10.CalculateDampening(offset)) ?? 0,
            subBiomes.s01?.GetDepthToSolid(subBiomes.s01.CalculateDampening(offset)) ?? 0,
            subBiomes.s11?.GetDepthToSolid(subBiomes.s11.CalculateDampening(offset)) ?? 0
        );
    }

    private Content GenerateContent(Vector3i position, in Context context)
    {
        if (position.Y == -World.BlockLimit) return new Content(Blocks.Instance.Core.CoreBlock);

        Int32 groundDepth = context.Ground.Height - position.Y;
        Boolean isFilled = position.Y <= SeaLevel;

        if (groundDepth < 0) // A negative depths means that the block is above the ground height.
        {
            if (context.Oceanic is not {} oceanic)
                return GetAboveSurfaceContent(position, groundDepth, isFilled, context.Ground, context);

            Int32 oceanicDepth = oceanic.Height - position.Y;

            if (oceanicDepth < 0) // A negative depth means that the block is above the oceanic height.
                return GetAboveSurfaceContent(position, oceanicDepth, isFilled, oceanic, context);

            if (position.Y <= oceanic.Height
                && position.Y > oceanic.Height - oceanic.SubBiome.GetTotalWidth(oceanic.Dampening))
                return GetSurfaceContent(oceanicDepth, position.Y, isFilled, context.GetStoneType(position), oceanic, context);

            return GetAboveSurfaceContent(position, groundDepth, isFilled, context.Ground, context);
        }

        Map.StoneType stoneType = context.GetStoneType(position);

        return groundDepth >= context.Ground.SubBiome.GetTotalWidth(context.Ground.Dampening)
            ? palette.GetStone(stoneType)
            : GetSurfaceContent(groundDepth, position.Y, isFilled, stoneType, context.Ground, context);
    }

    private static Content GetSurfaceContent(Int32 depth, Int32 y, Boolean isFilled, Map.StoneType stoneType, in Surface surface, in Context context)
    {
        Content content = surface.SubBiome.GetContent(depth, y, isFilled, surface.Dampening, stoneType, context.Sample.EstimateTemperature(y));

        if (isFilled) content = FillContent(content);

        return content;
    }

    private static Content GetAboveSurfaceContent(Vector3i position, Int32 depth, Boolean isFilled, in Surface surface, in Context context)
    {
        Int32 localHeightDifferenceToAverageHeight = surface.EffectiveOffset - surface.SubBiome.Definition.Offset;

        Boolean isStuffed = surface.SubBiome.Definition.Stuffer != null
                            && localHeightDifferenceToAverageHeight <= 0
                            && localHeightDifferenceToAverageHeight <= depth;

        var content = Content.Default;

        Map.PositionClimate climate = context.Sample.GetClimate(position.Y);

        if (isStuffed) content = surface.SubBiome.Definition.Stuffer!.GetContent(climate.Temperature);
        else if (depth == -1) content = surface.SubBiome.GetCoverContent(position, isFilled, surface.HeightFraction, climate);

        if (isFilled) content = FillContent(content);

        return content;
    }

    private static Content FillContent(Content content)
    {
        if (!content.Fluid.IsEmpty) return content;
        if (!content.Block.Block.Has<Fillable>()) return content;

        return content with {Fluid = Fluids.Instance.SeaWater.AsInstance()};
    }

    /// <summary>
    ///     A surface is on what sub-biomes are generated and determines their vertical shape.
    /// </summary>
    private readonly record struct Surface
    {
        /// <summary>
        ///     The absolute height of the surface, in blocks.
        ///     The height is determined by the ground height given by the map sample and the local sub-biome offset.
        /// </summary>
        public Int32 Height { get; init; }

        /// <summary>
        ///     The fraction of the height, calculated using the floating point height.
        /// </summary>
        public Double HeightFraction { get; init; }

        /// <summary>
        ///     The effective offset of the surface to the sample height, created by the noise offset and absolute offset of the
        ///     sub-biome.
        /// </summary>
        public Int32 EffectiveOffset { get; init; }

        /// <summary>
        ///     The dampening applied to the column, which is used to get the first solid layers of the sub-biome at the same
        ///     height.
        /// </summary>
        public SubBiome.Dampening Dampening { get; init; }

        /// <summary>
        ///     The sub-biome that is used for the surface.
        /// </summary>
        public SubBiome SubBiome { get; init; }
    }

    private readonly record struct Context
    {
        public Surface Ground { get; init; }

        public Surface? Oceanic { get; init; }

        public Map.Sample Sample { get; init; }

        public Map Map { private get; init; }

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
