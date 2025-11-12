// <copyright file="Map.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OpenTK.Mathematics;
using VoxelGame.Annotations.Attributes;
using VoxelGame.Core.Collections.Properties;
using VoxelGame.Core.Generation.Worlds.Standard.Biomes;
using VoxelGame.Core.Generation.Worlds.Standard.SubBiomes;
using VoxelGame.Core.Logic;
using VoxelGame.Core.Profiling;
using VoxelGame.Core.Serialization;
using VoxelGame.Core.Updates;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Utilities.Units;
using VoxelGame.Core.Visuals;
using VoxelGame.Logging;
using VoxelGame.Toolkit.Collections;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Core.Generation.Worlds.Standard;

/// <summary>
///     Represents a rough overview over the entire world, as a 2D map.
///     This class can load, store and generate these maps.
///     The map itself is organized in cells, each cell containing information about the terrain, temperature and humidity.
/// </summary>
public sealed partial class Map : IMap, IDisposable
{
    /// <summary>
    ///     Additional cell data that is stored as flags.
    /// </summary>
    [Flags]
    public enum CellConditions : UInt16
    {
        /// <summary>
        ///     No conditions.
        /// </summary>
        None = 0,

        /// <summary>
        ///     Marks a cell as having vulcanism.
        /// </summary>
        Vulcanism = 1 << 0,

        /// <summary>
        ///     Marks a cell as having very strong seismic activity.
        /// </summary>
        SeismicActivity = 1 << 1,

        /// <summary>
        ///     Marks a cell as having a rift valley.
        /// </summary>
        Rift = 1 << 2,

        /// <summary>
        ///     Marks a cell as being a coastline.
        /// </summary>
        Coastline = 1 << 3,

        /// <summary>
        ///     Marks a cell as being very mountainous.
        /// </summary>
        Mountainous = 1 << 4,

        /// <summary>
        ///     Marks a cell as having a cliff at the northern side.
        /// </summary>
        CliffNorth = 1 << 5,

        /// <summary>
        ///     Marks a cell as having a cliff at the southern side.
        /// </summary>
        CliffSouth = 1 << 6,

        /// <summary>
        ///     Marks a cell as having a cliff at the eastern side.
        /// </summary>
        CliffEast = 1 << 7,

        /// <summary>
        ///     Marks a cell as having a cliff at the western side.
        /// </summary>
        CliffWest = 1 << 8
    }

    /// <summary>
    ///     The type of stone that a cell is made of.
    /// </summary>
    public enum StoneType : Byte
    {
        /// <summary>
        ///     Sandstone.
        /// </summary>
        Sandstone,

        /// <summary>
        ///     Granite.
        /// </summary>
        Granite,

        /// <summary>
        ///     Limestone.
        /// </summary>
        Limestone,

        /// <summary>
        ///     Marble.
        /// </summary>
        Marble
    }

    /// <summary>
    ///     Height of the highest mountains and deepest oceans, in meters / blocks.
    /// </summary>
    public const Int32 MaxHeight = 5_000;

    /// <summary>
    ///     The size of a map cell, in meters / blocks.
    ///     This is essentially the biome sampling grid size.
    /// </summary>
    public const Int32 CellSize = 200;

    /// <summary>
    ///     The size of the grid used to sample sub-biomes.
    /// </summary>
    public const Int32 SubBiomeGridSize = CellSize / 10;

    private const Int32 MinimumWidth = (Int32) (World.BlockLimit * 2) / CellSize;
    private const Int32 MinimumWidthHalf = MinimumWidth / 2;
    private const Int32 Width = MinimumWidth + 2;
    private const Int32 WidthHalf = Width / 2;

    private const Double MinTemperature = -5.0;
    private const Double MaxTemperature = 30.0;

    private static readonly ColorS blockTintWarm = ColorS.LightGreen;
    private static readonly ColorS blockTintCold = ColorS.ForrestGreen;
    private static readonly ColorS blockTintMoist = ColorS.LawnGreen;
    private static readonly ColorS blockTintDry = ColorS.Olive;

    private static readonly ColorS fluidTintWarm = ColorS.LightSeaGreen;
    private static readonly ColorS fluidTintCold = ColorS.MediumBlue;

    private readonly BiomeDistribution biomes;

    private Data? data;

    /// <summary>
    ///     Create a new map.
    /// </summary>
    /// <param name="biomes">The biome distribution used by the generator.</param>
    public Map(BiomeDistribution biomes)
    {
        this.biomes = biomes;
    }

    [LateInitialization]
    internal partial SamplingNoise SamplingNoise { get; private set; }
    
    [LateInitialization]
    internal partial GeneratingNoise GeneratingNoise { get; private set; }

    /// <inheritdoc />
    public Property GetPositionDebugData(Vector3d position)
    {
        Vector3i samplingPosition = position.Floor();
        Sample sample = GetSample(samplingPosition);

        return new Group(nameof(Standard),
        [
            new Message("Biome", sample.ActualBiome.Definition.Name),
            new Message("Sub-Biome", sample.ActualSubBiome.Definition.Name),
            new Message("Oceanic Sub-Biome", sample.ActualOceanicSubBiome?.Definition.Name ?? "none"),
            new Measure("Height", sample.EstimateHeight())
        ]);
    }

    /// <inheritdoc />
    public (ColorS block, ColorS fluid) GetPositionTint(Vector3d position)
    {
        Vector3i samplingPosition = position.Floor();
        Sample sample = GetSample(samplingPosition);

        Single temperature = NormalizeTemperature(sample.EstimateTemperature(position.Y));

        ColorS block = ColorS.Mix(ColorS.Mix(blockTintCold, blockTintWarm, temperature), ColorS.Mix(blockTintDry, blockTintMoist, sample.Humidity));
        ColorS fluid = ColorS.Mix(fluidTintCold, fluidTintWarm, temperature);

        return (block, fluid);
    }

    /// <inheritdoc />
    public Temperature GetTemperature(Vector3d position)
    {
        Vector3i samplingPosition = position.Floor();
        Sample sample = GetSample(samplingPosition);

        return sample.EstimateTemperature(position.Y);
    }

    /// <summary>
    ///     Check whether the given conditions contain any cliffs.
    /// </summary>
    public static Boolean HasCliff(CellConditions conditions)
    {
        const Int32 mask = (Int32) CellConditions.CliffNorth | (Int32) CellConditions.CliffSouth
                                                             | (Int32) CellConditions.CliffEast
                                                             | (Int32) CellConditions.CliffWest;

        return (mask & (Int32) conditions) != 0;
    }

    private static Temperature ConvertTemperatureToCelsius(Single temperature)
    {
        return new Temperature
        {
            DegreesCelsius = MathHelper.Lerp(MinTemperature, MaxTemperature, temperature)
        };
    }

    private static Single NormalizeTemperature(Temperature temperature)
    {
        return (Single) MathTools.InverseLerp(MinTemperature, MaxTemperature, temperature.DegreesCelsius);
    }

    private static Temperature GetTemperatureAtHeight(Temperature groundTemperature, Single humidity, Double heightAboveGround)
    {
        if (heightAboveGround < 0) return groundTemperature;

        Double decreaseFactor = MathHelper.Lerp(start: 10.0, end: 5.0, humidity);

        return new Temperature
        {
            DegreesCelsius = groundTemperature.DegreesCelsius - decreaseFactor * heightAboveGround / 1000.0
        };
    }

    /// <summary>
    ///     Initialize the map. If available, it will be loaded from a blob.
    ///     If loading is not possible, it will be generated.
    /// </summary>
    /// <param name="context">The generation context to use.</param>
    /// <param name="blob">The name of the blob to load, or null to generate a new map.</param>
    /// <param name="factory">The factory to use for noise generator creation.</param>
    /// <param name="dirty">
    ///     Whether the map is dirty and needs to be saved.
    ///     Will be true if the map is generated, false if it is just loaded.
    /// </param>
    public void Initialize(IWorldGeneratorContext context, String? blob, NoiseFactory factory, out Boolean dirty)
    {
        LogInitializingMap(logger);

        dirty = false;

        if (blob != null)
            Load(context, blob);

        GeneratingNoise = new GeneratingNoise(factory);

        if (data == null)
        {
            Generate();
            dirty = true;
        }

        SamplingNoise = new SamplingNoise(factory);
    }

    private void Generate()
    {
        Debug.Assert(data == null);
        data = new Data();

        LogGeneratingMap(logger);

        using Timer? timer = Timer.Start("Map Generation", TimingStyle.Once, Profile.GetSingleUseActiveProfiler());

        GenerateTerrain(data, GeneratingNoise);
        GenerateTemperature(data);
        GenerateHumidity(data);
        GenerateAdditionalSpecialConditions(data);

        LogGeneratedMap(logger, timer?.Elapsed ?? default);
    }

    /// <summary>
    ///     Emit info about different map values.
    /// </summary>
    /// <param name="path">The path to a directory to save the info to.</param>
    public Operation EmitWorldInfo(DirectoryInfo path)
    {
        Debug.Assert(data != null);

        return Operations.Launch(async token =>
        {
            await Task.WhenAll(
                EmitTerrainViewAsync(data, path, token),
                EmitStoneViewAsync(data, path, token),
                EmitContinentViewAsync(data, path, token),
                EmitTemperatureViewAsync(data, path, token),
                EmitHumidityViewAsync(data, path, token),
                EmitBiomeViewAsync(data, biomes, path, token)).InAnyContext();
        });
    }

    private void Load(IWorldGeneratorContext context, String blob)
    {
        var loaded = context.ReadBlob<Data>(blob);

        if (loaded == null)
        {
            // The data field is set when generating the map.
            // Setting it here would prevent the map from being generated.

            LogCouldNotLoadMap(logger);
        }
        else
        {
            data = loaded;

            LogLoadedMap(logger);
        }
    }

    /// <summary>
    ///     Store the map to a blob.
    /// </summary>
    /// <param name="context">The generation context to use.</param>
    /// <param name="blob">The name of the blob to store the map in.</param>
    public void Store(IWorldGeneratorContext context, String blob)
    {
        Debug.Assert(data != null);

        context.WriteBlob(blob, data);
    }

    /// <summary>
    ///     Get a sample of the map at the given coordinates.
    /// </summary>
    /// <param name="position">The world position where to sample.</param>
    /// <returns>The sample.</returns>
    public Sample GetSample(Vector3i position)
    {
        return GetSample(position.Xz, store: null);
    }

    /// <summary>
    ///     Get a sample of the map at the given coordinates.
    /// </summary>
    /// <param name="column">
    ///     The column to sample from, in block coordinates.
    ///     This is simply the X and Z coordinates of a block column.
    /// </param>
    /// <param name="store">
    ///     An optional noise store.
    ///     Serves to optimize sampling by allowing to batch noise generation.
    /// </param>
    /// <returns>The sample.</returns>
    internal Sample GetSample(Vector2i column, SamplingNoiseStore? store)
    {
        Debug.Assert(data != null);

        Biome actualBiome = DetermineColumnBiome(column, out Boolean oceanic, store, cacheHint: 0);
        (Single temperature, Single humidity, Single height) = GetColumnValues(column, store, cacheHint: 0, out Vector2i shiftedColumn);

        (Vector2i p1, Vector2i p2, Vector2d subBiomeBlend) = GetSubBiomeSamplingPoints(shiftedColumn);

        (SubBiome s00, SubBiome? os00) = DetermineSubBiome((p1.X, p1.Y), oceanic, store, cacheHint: 1);
        (SubBiome s10, SubBiome? os10) = DetermineSubBiome((p2.X, p1.Y), oceanic, store, cacheHint: 2);
        (SubBiome s01, SubBiome? os01) = DetermineSubBiome((p1.X, p2.Y), oceanic, store, cacheHint: 3);
        (SubBiome s11, SubBiome? os11) = DetermineSubBiome((p2.X, p2.Y), oceanic, store, cacheHint: 4);

        SubBiome actualSubBiome = MathTools.SelectByWeight(s00, s10, s01, s11, subBiomeBlend);

        SubBiome? actualOceanicSubBiome = null;

        if (oceanic && actualBiome.Definition.IsOceanic) actualOceanicSubBiome = MathTools.SelectByWeight(os00, os10, os01, os11, subBiomeBlend);

        return new Sample
        {
            Height = height,
            Temperature = temperature,
            Humidity = humidity,
            ActualBiome = actualBiome,
            ActualSubBiome = actualSubBiome,
            SubBiome00 = s00,
            SubBiome10 = s10,
            SubBiome01 = s01,
            SubBiome11 = s11,
            SubBiomeBlendFactors = subBiomeBlend,
            ActualOceanicSubBiome = actualOceanicSubBiome,
            OceanicSubBiome00 = os00,
            OceanicSubBiome10 = os10,
            OceanicSubBiome01 = os01,
            OceanicSubBiome11 = os11,
            Column = column,
            StoneData = GetColumnStoneData(column, store, cacheHint: 0) // We use column here because the method will do shifting itself.
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Biome DetermineColumnBiome(Vector2i column, out Boolean oceanic, SamplingNoiseStore? store, Int32 cacheHint)
    {
        Debug.Assert(data != null);

        (Vector2i c1, Vector2i c2, Vector2d biomeBlend) = GetSamplingCells(column, store, cacheHint, out _);

        ref readonly Cell c00 = ref data.GetCell(c1.X + WidthHalf, c1.Y + WidthHalf);
        ref readonly Cell c10 = ref data.GetCell(c2.X + WidthHalf, c1.Y + WidthHalf);
        ref readonly Cell c01 = ref data.GetCell(c1.X + WidthHalf, c2.Y + WidthHalf);
        ref readonly Cell c11 = ref data.GetCell(c2.X + WidthHalf, c2.Y + WidthHalf);

        Biome b00 = GetBiome(c00);
        Biome b10 = GetBiome(c10);
        Biome b01 = GetBiome(c01);
        Biome b11 = GetBiome(c11);

        oceanic = b00.Definition.IsOceanic || b10.Definition.IsOceanic || b01.Definition.IsOceanic || b11.Definition.IsOceanic;

        return MathTools.SelectByWeight(b00, b10, b01, b11, biomeBlend);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private (Single temperature, Single humidity, Single height) GetColumnValues(Vector2i column, SamplingNoiseStore? store, Int32 cacheHint, out Vector2i point)
    {
        Debug.Assert(data != null);

        (Vector2i c1, Vector2i c2, Vector2d biomeBlend) = GetSamplingCells(column, store, cacheHint, out _);
        point = MathTools.Lerp(GetCellCenter(c1), GetCellCenter(c2), biomeBlend).Floor();

        ref readonly Cell c00 = ref data.GetCell(c1.X + WidthHalf, c1.Y + WidthHalf);
        ref readonly Cell c10 = ref data.GetCell(c2.X + WidthHalf, c1.Y + WidthHalf);
        ref readonly Cell c01 = ref data.GetCell(c1.X + WidthHalf, c2.Y + WidthHalf);
        ref readonly Cell c11 = ref data.GetCell(c2.X + WidthHalf, c2.Y + WidthHalf);

        var temperature = (Single) MathTools.BiLerp(c00.temperature, c10.temperature, c01.temperature, c11.temperature, biomeBlend);
        var humidity = (Single) MathTools.BiLerp(c00.humidity, c10.humidity, c01.humidity, c11.humidity, biomeBlend);

        Single height = GetHeight(c00, c10, c01, c11, biomeBlend);

        return (temperature, humidity, height);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private (StoneType stone00, StoneType stone10, StoneType stone01, StoneType stone11, Vector2d t) GetColumnStoneData(Vector2i column, SamplingNoiseStore? store, Int32 cacheHint)
    {
        Debug.Assert(data != null);

        (Vector2i c1, Vector2i c2, Vector2d _) = GetSamplingCells(column, store, cacheHint, out Vector2d t);

        ref readonly Cell c00 = ref data.GetCell(c1.X + WidthHalf, c1.Y + WidthHalf);
        ref readonly Cell c10 = ref data.GetCell(c2.X + WidthHalf, c1.Y + WidthHalf);
        ref readonly Cell c01 = ref data.GetCell(c1.X + WidthHalf, c2.Y + WidthHalf);
        ref readonly Cell c11 = ref data.GetCell(c2.X + WidthHalf, c2.Y + WidthHalf);

        return (c00.stoneType, c10.stoneType, c01.stoneType, c11.stoneType, t);
    }

    private (SubBiome subBiome, SubBiome? oceaninSubBiome) DetermineSubBiome(Vector2i column, Boolean oceanic, SamplingNoiseStore? store, Int32 cacheHint)
    {
        Biome biome = DetermineColumnBiome(column, out _, store, cacheHint);

        Single subBiomeNoise = SamplingNoise.GetSubBiomeDeterminationNoise(column, store, cacheHint);
        Single subBiomeValue = Math.Abs(subBiomeNoise);

        SubBiome subBiome = biome.ChooseSubBiome(subBiomeValue);

        if (!oceanic || !biome.Definition.IsOceanic)
            return (subBiome, null);

        Single oceanicSubBiomeNoise = SamplingNoise.GetOceanicSubBiomeDeterminationNoise(column, store, cacheHint);
        Single oceanicSubBiomeValue = Math.Abs(oceanicSubBiomeNoise);

        SubBiome oceanicSubBiome = biome.ChooseOceanicSubBiome(oceanicSubBiomeValue);

        return (subBiome, oceanicSubBiome);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private (Vector2i c1, Vector2i c2, Vector2d blend) GetSamplingCells(Vector2i column, SamplingNoiseStore? store, Int32 cacheHint, out Vector2d t)
    {
        Vector2i cell = GetCellFromColumn(column);
        Vector2i neighbor = GetNearestNeighbor(column, CellSize);

        (Vector2i c1, Vector2i c2) = MathTools.MinMax(cell, neighbor);

        Vector2d p1 = GetCellCenter(c1);
        Vector2d p2 = GetCellCenter(c2);

        // The cell centers create a cell-sized rectangle, in which the values are interpolated.

        t = MathTools.InverseLerp(p1, p2, column);

        Vector2d noise = SamplingNoise.GetCellSamplingOffsetNoise(column, store, cacheHint);
        Vector2d blend = t + noise * 0.2;

        ReAlignSamplingCells(ref blend, ref c1, ref c2);

        return (c1, c2, blend);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ReAlignSamplingCells(ref Vector2d blend, ref Vector2i c1, ref Vector2i c2)
    {
        if (blend.X < 0.0)
        {
            blend.X += 1.0;
            c1.X -= 1;
            c2.X -= 1;
        }
        else if (blend.X > 1.0)
        {
            blend.X -= 1.0;
            c1.X += 1;
            c2.X += 1;
        }

        if (blend.Y < 0.0)
        {
            blend.Y += 1.0;
            c1.Y -= 1;
            c2.Y -= 1;
        }
        else if (blend.Y > 1.0)
        {
            blend.Y -= 1.0;
            c1.Y += 1;
            c2.Y += 1;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Single GetHeight(in Cell c00, in Cell c10, in Cell c01, in Cell c11, Vector2d blend)
    {
        var defaultHeight = (Single) MathTools.BiLerp(c00.height, c10.height, c01.height, c11.height, blend);

        CellConditions total = c00.conditions | c10.conditions | c01.conditions | c11.conditions;

        if (!HasCliff(total))
            return defaultHeight;

        var s00 = 0.0f;
        var s10 = 0.0f;
        var s01 = 0.0f;
        var s11 = 0.0f;

        Single unavailable = Single.NaN;

        if (ApplyCliffs(c00, ref unavailable, ref s01, ref s10, ref unavailable))
            s00 = 1.0f;

        if (ApplyCliffs(c10, ref unavailable, ref s11, ref unavailable, ref s00))
            s10 = 1.0f;

        if (ApplyCliffs(c01, ref s00, ref unavailable, ref s11, ref unavailable))
            s01 = 1.0f;

        if (ApplyCliffs(c11, ref s10, ref unavailable, ref unavailable, ref s01))
            s11 = 1.0f;

        var cliffStrength = (Single) MathTools.BiLerp(s00, s10, s01, s11, blend);

        if (cliffStrength < 0.5f)
            return defaultHeight;

        cliffStrength -= 0.5f;
        cliffStrength *= 2.0f;

        var cliffHeight = (Single) MathTools.BiLerp(
            GetCliffHeight(c00, Single.NegativeInfinity, c01.height, c10.height, Single.NegativeInfinity),
            GetCliffHeight(c10, Single.NegativeInfinity, c11.height, Single.NegativeInfinity, c00.height),
            GetCliffHeight(c01, c00.height, Single.NegativeInfinity, c11.height, Single.NegativeInfinity),
            GetCliffHeight(c11, c10.height, Single.NegativeInfinity, Single.NegativeInfinity, c01.height),
            blend);

        return Single.Lerp(defaultHeight, cliffHeight, cliffStrength);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Boolean ApplyCliffs(in Cell cell, ref Single north, ref Single south, ref Single east, ref Single west)
    {
        if (!HasCliff(cell.conditions))
            return false;

        var isRaisedCliff = false;

        if (!Single.IsNaN(north) && cell.conditions.HasFlag(CellConditions.CliffNorth))
        {
            north = 1.0f;
            isRaisedCliff = true;
        }

        if (!Single.IsNaN(south) && cell.conditions.HasFlag(CellConditions.CliffSouth))
        {
            south = 1.0f;
            isRaisedCliff = true;
        }

        if (!Single.IsNaN(east) && cell.conditions.HasFlag(CellConditions.CliffEast))
        {
            east = 1.0f;
            isRaisedCliff = true;
        }

        if (!Single.IsNaN(west) && cell.conditions.HasFlag(CellConditions.CliffWest))
        {
            west = 1.0f;
            isRaisedCliff = true;
        }

        return isRaisedCliff;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Single GetCliffHeight(in Cell cell, Single northHeight, Single southHeight, Single eastHeight, Single westHeight)
    {
        if (!HasCliff(cell.conditions))
            return cell.height;

        Single height = cell.height;

        if (cell.conditions.HasFlag(CellConditions.CliffNorth))
            height = Math.Max(height, northHeight);

        if (cell.conditions.HasFlag(CellConditions.CliffSouth))
            height = Math.Max(height, southHeight);

        if (cell.conditions.HasFlag(CellConditions.CliffEast))
            height = Math.Max(height, eastHeight);

        if (cell.conditions.HasFlag(CellConditions.CliffWest))
            height = Math.Max(height, westHeight);

        return height * 0.95f;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector2i GetNearestNeighbor(Vector2i column, Int32 gridSize)
    {
        Vector2i cell = GetGridCellFromColumn(column, gridSize);

        Vector2i neighborA = GetGridCellFromColumn(column - new Vector2i(gridSize / 2), gridSize);
        Vector2i neighborB = GetGridCellFromColumn(column + new Vector2i(gridSize / 2), gridSize);

        Int32 x = cell.X == neighborA.X ? neighborB.X : neighborA.X;
        Int32 y = cell.Y == neighborA.Y ? neighborB.Y : neighborA.Y;

        return new Vector2i(x, y);
    }

    private Biome GetBiome(in Cell cell)
    {
        return GetBiome(biomes, cell);
    }

    private static Biome GetBiome(BiomeDistribution biomes, in Cell cell)
    {
        return biomes.DetermineBiome(cell.conditions, cell.temperature, cell.humidity, cell.IsLand);
    }

    private static (Vector2i p1, Vector2i p2, Vector2d blend) GetSubBiomeSamplingPoints(Vector2i column)
    {
        Vector2i cell = GetGridCellFromColumn(column, SubBiomeGridSize);
        Vector2i neighbor = GetNearestNeighbor(column, SubBiomeGridSize);

        (Vector2i g1, Vector2i g2) = MathTools.MinMax(cell, neighbor);

        Vector2i p1 = GetGridCellCenter(g1, SubBiomeGridSize);
        Vector2i p2 = GetGridCellCenter(g2, SubBiomeGridSize);

        Vector2d blend = MathTools.InverseLerp(p1, p2, column);

        return (p1, p2, blend);
    }

    /// <summary>
    ///     Get the cell (coordinates) that contains the given column.
    /// </summary>
    /// <param name="column">The column to get the cell for.</param>
    /// <returns>The cell coordinates.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2i GetCellFromColumn(Vector2i column)
    {
        return GetGridCellFromColumn(column, CellSize);
    }

    /// <summary>
    ///     Get the sub-biome grid cell (coordinates) that contains the given column.
    /// </summary>
    /// <param name="column">The column to get the cell for.</param>
    /// <returns>The sub-biome grid cell coordinates.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2i GetSubBiomeGridCellFromColumn(Vector2i column)
    {
        return GetGridCellFromColumn(column, SubBiomeGridSize);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector2i GetGridCellFromColumn(Vector2i column, Int32 gridSize)
    {
        Vector2i divided = column / gridSize;

        // We round away from zero for negative numbers, so that overall we always round towards negative infinity.

        return new Vector2i(
            column.X < 0 && column.X != gridSize * divided.X ? divided.X - 1 : divided.X,
            column.Y < 0 && column.Y != gridSize * divided.Y ? divided.Y - 1 : divided.Y);
    }

    /// <summary>
    ///     Get the center of a grid cell, as a world-position column.
    /// </summary>
    /// <param name="cell">The cell coordinates.</param>
    /// <param name="gridSize">The size of the grid.</param>
    /// <returns>The column (in block coordinates).</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector2i GetGridCellCenter(Vector2i cell, Int32 gridSize)
    {
        return cell * gridSize + new Vector2i(gridSize / 2);
    }

    /// <summary>
    ///     Get the center of a cell, as a world-position column.
    /// </summary>
    /// <param name="cell">The cell coordinates.</param>
    /// <returns>The column (in block coordinates).</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector2i GetCellCenter(Vector2i cell)
    {
        return GetGridCellCenter(cell, CellSize);
    }

    /// <summary>
    ///     Transform a cell position to a world position, using the center of the cell.
    /// </summary>
    /// <param name="cell">The cell position.</param>
    /// <param name="y">The y position.</param>
    /// <returns>The world position.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3i GetCellCenter(Vector2i cell, Int32 y)
    {
        Vector2i column = GetCellCenter(cell);

        return new Vector3i(column.X, y, column.Y);
    }

    /// <summary>
    ///     Transform a sub-biome grid cell position to a world position, using the center of the grid cell.
    /// </summary>
    /// <param name="cell">The sub-biome grid cell position.</param>
    /// <param name="y">The y position.</param>
    /// <returns>The world position.</returns>
    public static Vector3i GetSubBiomeGridCellCenter(Vector2i cell, Int32 y)
    {
        Vector2i column = GetGridCellCenter(cell, SubBiomeGridSize);

        return new Vector3i(column.X, y, column.Y);
    }

    /// <summary>
    ///     Check whether a cell is within the map limits and valid to sample.
    ///     The actual map is larger to ensure that all valid cells have neighbors.
    /// </summary>
    public static Boolean IsValidCell(Vector2i cell)
    {
        return cell.X + MinimumWidthHalf is >= 0 and < MinimumWidth && cell.Y + MinimumWidthHalf is >= 0 and < MinimumWidth;
    }

    /// <summary>
    ///     Check whether a sub-biome grid cell is within the map limits and valid to sample.
    /// </summary>
    public static Boolean IsValidSubBiomeGridCell(Vector2i cell)
    {
        Vector3i worldPosition = GetSubBiomeGridCellCenter(cell, y: 0);
        Vector2i correspondingCell = GetCellFromColumn(worldPosition.Xz);

        return IsValidCell(correspondingCell);
    }

    /// <summary>
    ///     Get the stone type at a given position.
    /// </summary>
    public StoneType GetStoneType(Vector3i position, in Sample sample)
    {
        Debug.Assert(data != null);

        if (sample.StoneData.stone00 == sample.StoneData.stone10 &&
            sample.StoneData.stone00 == sample.StoneData.stone01 &&
            sample.StoneData.stone00 == sample.StoneData.stone11)
            return sample.StoneData.stone00;

        Vector2d noise = SamplingNoise.GetStoneSamplingOffsetNoise(position.Xz);
        Vector2d stone = sample.StoneData.t + noise * 0.05;

        return MathTools.SelectByWeight(sample.StoneData.stone00, sample.StoneData.stone10, sample.StoneData.stone01, sample.StoneData.stone11, stone);
    }

    /// <summary>
    ///     Describes the climate of a block position.
    /// </summary>
    public readonly record struct PositionClimate
    {
        /// <see cref="Sample.Temperature" />
        public Single SampledTemperature { get; init; }

        /// <see cref="Sample.Humidity" />
        public Single SampledHumidity { get; init; }

        /// <summary>
        ///     The temperature of the position.
        /// </summary>
        public Temperature Temperature { get; init; }
    }

    /// <summary>
    ///     A sample of the map, providing information to generate a column.
    /// </summary>
    public readonly record struct Sample
    {
        /// <summary>
        ///     The height of the sample, relative to the maximum height.
        /// </summary>
        public Single Height { get; init; }

        /// <summary>
        ///     The temperature of the sample, in range [0, 1]. Use <see cref="GetTemperatureAtHeight" /> to retrieve the
        ///     temperature.
        /// </summary>
        public Single Temperature { get; init; }

        /// <summary>
        ///     The humidity of the sample, in range [0, 1].
        /// </summary>
        public Single Humidity { get; init; }

        /// <summary>
        ///     Get the actual biome at the sample position.
        /// </summary>
        public Biome ActualBiome { get; init; }

        /// <summary>
        ///     Get the actual sub-biome at the sample position.
        /// </summary>
        public SubBiome ActualSubBiome { get; init; }

        /// <summary>
        ///     Get the sub-biome <c>00</c>.
        /// </summary>
        public SubBiome SubBiome00 { get; init; }

        /// <summary>
        ///     Get the sub-biome <c>10</c>.
        /// </summary>
        public SubBiome SubBiome10 { get; init; }

        /// <summary>
        ///     Get the sub-biome <c>01</c>.
        /// </summary>
        public SubBiome SubBiome01 { get; init; }

        /// <summary>
        ///     Get the sub-biome <c>11</c>.
        /// </summary>
        public SubBiome SubBiome11 { get; init; }

        /// <summary>
        ///     Get the blending factors on the sub-biome level.
        ///     These are also used for the oceanic sub-biome.
        /// </summary>
        public Vector2d SubBiomeBlendFactors { get; init; }

        /// <summary>
        ///     Actual oceanic sub-biome present at this sample location.
        ///     In contrast to normal sub-biomes, the oceanic sub-biome is applied to water level instead of ground level.
        /// </summary>
        public SubBiome? ActualOceanicSubBiome { get; init; }

        /// <summary>
        ///     Get the oceanic sub-biome <c>00</c>.
        /// </summary>
        public SubBiome? OceanicSubBiome00 { get; init; }

        /// <summary>
        ///     Get the oceanic sub-biome <c>10</c>.
        /// </summary>
        public SubBiome? OceanicSubBiome10 { get; init; }

        /// <summary>
        ///     Get the oceanic sub-biome <c>01</c>.
        /// </summary>
        public SubBiome? OceanicSubBiome01 { get; init; }

        /// <summary>
        ///     Get the oceanic sub-biome <c>11</c>.
        /// </summary>
        public SubBiome? OceanicSubBiome11 { get; init; }

        /// <summary>
        ///     Data regarding the stone composition.
        /// </summary>
        public (StoneType stone00, StoneType stone10, StoneType stone01, StoneType stone11, Vector2d t) StoneData { get; init; }

        /// <summary>
        ///     The column where the sample was taken from.
        /// </summary>
        public Vector2i Column { get; init; }

        /// <summary>
        ///     Get the height of the ground at the sample column.
        /// </summary>
        public Int32 GroundHeight => Generator.GetGroundHeight(Column, this, out _, out _);

        /// <summary>
        ///     Get the height of the oceanic surface at the sample column.
        ///     This is either the water level or the height of the oceanic sub-biome, whichever is higher.
        /// </summary>
        public Int32 OceanicHeight => Generator.GetOceanicHeight(Column, this, out _, out _);

        /// <summary>
        ///     Get the temperature at a given height.
        /// </summary>
        /// <param name="y">The height, in meters.</param>
        /// <returns>The temperature.</returns>
        public Temperature EstimateTemperature(Double y)
        {
            // The ground height follows the actual height of the sample, but mountains and oceans are ignored.
            // This is necessary to have a more realistic lower temperature on mountains.

            Double groundHeight = Math.Clamp(Height * MaxHeight, min: 0.0, MaxHeight * 0.3);

            return GetTemperatureAtHeight(ConvertTemperatureToCelsius(Temperature), Humidity, y - groundHeight);
        }

        /// <summary>
        ///     Get the climate at a given position.
        /// </summary>
        /// <param name="y">The Y coordinate of the position, in blocks. The sample determines the rest of the position.</param>
        /// <returns>The climate at the position.</returns>
        public PositionClimate GetClimate(Int32 y)
        {
            return new PositionClimate
            {
                SampledHumidity = Humidity,
                SampledTemperature = Temperature,
                Temperature = EstimateTemperature(y)
            };
        }

        /// <summary>
        ///     Get the (ground) height of the sample.
        ///     Note that this does not consider sub-biome-dependent height offsets.
        /// </summary>
        /// <returns>The height.</returns>
        public Length EstimateHeight()
        {
            return new Length {Meters = Height * MaxHeight};
        }
    }

    private record struct Cell : IValue
    {
        /// <summary>
        ///     Flags for different cell conditions.
        /// </summary>
        public CellConditions conditions;

        /// <summary>
        ///     The continent id of the cell. The ids are not contiguous, but are unique.
        /// </summary>
        public Int16 continent;

        /// <summary>
        ///     The height of the cell, in the range [-1, 1].
        /// </summary>
        public Single height;

        /// <summary>
        ///     The humidity of the cell, in the range [0, 1].
        /// </summary>
        public Single humidity;

        /// <summary>
        ///     The height of the cell.
        /// </summary>
        public StoneType stoneType;

        /// <summary>
        ///     The temperature of the cell, in the range [0, 1].
        /// </summary>
        public Single temperature;

        public Boolean IsLand => height > 0.0f;

        /// <inheritdoc />
        public void Serialize(Serializer serializer)
        {
            serializer.Serialize(ref conditions);
            serializer.Serialize(ref continent);
            serializer.Serialize(ref height);
            serializer.Serialize(ref humidity);
            serializer.Serialize(ref stoneType);
            serializer.Serialize(ref temperature);
        }
    }

    private sealed class Data : IEntity
    {
        private readonly Array2D<Cell> cells = new(Width);

        /// <inheritdoc />
        public static UInt32 CurrentVersion => 1;

        /// <inheritdoc />
        public void Serialize(Serializer serializer, IEntity.Header header)
        {
            serializer.SerializeValues(cells);
        }

        public ref Cell GetCell(Int32 x, Int32 y)
        {
            return ref cells[x, y];
        }

        public ref Cell GetCell(Vector2i position)
        {
            return ref cells[position];
        }

        public static Boolean IsInLimits(Int32 x, Int32 y)
        {
            return x is >= 0 and < Width && y is >= 0 and < Width;
        }
    }

    #region LOGGING

    private static readonly ILogger logger = LoggingHelper.CreateLogger<Map>();

    [LoggerMessage(EventId = LogID.DefaultMap + 0, Level = LogLevel.Debug, Message = "Initializing map")]
    private static partial void LogInitializingMap(ILogger logger);

    [LoggerMessage(EventId = LogID.DefaultMap + 1, Level = LogLevel.Debug, Message = "Generating map")]
    private static partial void LogGeneratingMap(ILogger logger);

    [LoggerMessage(EventId = LogID.DefaultMap + 2, Level = LogLevel.Information, Message = "Generated map in {Duration}")]
    private static partial void LogGeneratedMap(ILogger logger, Duration duration);

    [LoggerMessage(EventId = LogID.DefaultMap + 3, Level = LogLevel.Debug, Message = "Loaded map")]
    private static partial void LogLoadedMap(ILogger logger);

    [LoggerMessage(EventId = LogID.DefaultMap + 4, Level = LogLevel.Information, Message = "Could not load map, either it does not yet exist or is corrupted")]
    private static partial void LogCouldNotLoadMap(ILogger logger);

    #endregion LOGGING

    #region DISPOSABLE

    private Boolean disposed;

    private void Dispose(Boolean disposing)
    {
        if (disposed) return;

        if (disposing)
        {
            GeneratingNoise.Dispose();
            SamplingNoise.Dispose();
        }
        else
        {
            ExceptionTools.ThrowForMissedDispose(this);
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
    ~Map()
    {
        Dispose(disposing: false);
    }

    #endregion DISPOSABLE
}
