// <copyright file="Map.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OpenTK.Mathematics;
using VoxelGame.Core.Collections;
using VoxelGame.Core.Collections.Properties;
using VoxelGame.Core.Generation.Worlds.Default.Biomes;
using VoxelGame.Core.Logic;
using VoxelGame.Core.Profiling;
using VoxelGame.Core.Serialization;
using VoxelGame.Core.Updates;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Utilities.Units;
using VoxelGame.Core.Visuals;
using VoxelGame.Logging;
using VoxelGame.Toolkit.Collections;
using VoxelGame.Toolkit.Noise;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Core.Generation.Worlds.Default;

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
    public enum CellConditions : Byte
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
        Rift = 1 << 2
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
    public const Int32 MaxHeight = 10_000;

    /// <summary>
    ///     The size of a map cell, in meters / blocks.
    /// </summary>
    public const Int32 CellSize = 20_000;

    private const Int32 CellSizeHalf = CellSize / 2;

    private const Int32 MinimumWidth = (Int32) (World.BlockLimit * 2) / CellSize;
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

    private static readonly Polyline mountainStrengthFunction = new()
    {
        Left = _ => 0.0,
        Points =
        {
            (0.3, 0.0),
            (0.4, 0.4)
        },
        Right = x => x
    };

    private static readonly Polyline depthStrengthFunction = new()
    {
        Left = _ => 1.0,
        Points =
        {
            (0.0, 1.0),
            (0.01, 0.0) // Here 0.01 is the maximum coastline height.
        },
        Right = _ => 0.0
    };

    private static readonly Polyline distanceStrengthFunction = new()
    {
        Left = _ => 1.0,
        Points =
        {
            (0.0, 1.0),
            (0.02, 0.0) // Here 0.01 is the maximum coastline width.
        },
        Right = _ => 0.0
    };

    private static readonly Polyline oceanStrengthFunction = new()
    {
        Left = x => -x,
        Points =
        {
            (0.4, -0.4),
            (0.5, 0.5)
        },
        Right = x => x
    };

    private static readonly Polyline flattenedHeightFunction = new()
    {
        Left = _ => 0.0,
        Points =
        {
            (0.0, 0.0),
            (0.0125, 0.005),
            (0.025, 0.025)
        },
        Right = x => x
    };

    private static readonly Polyline cliffFactorFunction = new()
    {
        Left = _ => 0.0,
        Points =
        {
            (0.0, 0.0),
            (0.5, 0.1),
            (0.55, 1.0)
        },
        Right = _ => 1.0
    };

    private readonly BiomeDistribution biomes;

    private readonly GeneratingNoise generatingNoise = new();

    private Data? data;

    private NoiseGenerator2D samplingNoise = null!;
    private NoiseGenerator2D stoneNoise = null!;

    /// <summary>
    ///     Create a new map.
    /// </summary>
    /// <param name="biomes">The biome distribution used by the generator.</param>
    public Map(BiomeDistribution biomes)
    {
        this.biomes = biomes;
    }

    /// <inheritdoc />
    public Property GetPositionDebugData(Vector3d position)
    {
        Vector3i samplingPosition = position.Floor();
        Sample sample = GetSample(samplingPosition);

        return new Group(nameof(Default),
        [
            new Message("Biome", sample.ActualBiome.Definition.Name),
            new Measure("Height", sample.GetRealHeight())
        ]);
    }

    /// <inheritdoc />
    public (ColorS block, ColorS fluid) GetPositionTint(Vector3d position)
    {
        Vector3i samplingPosition = position.Floor();
        Sample sample = GetSample(samplingPosition);

        Single temperature = NormalizeTemperature(sample.GetRealTemperature(position.Y));

        ColorS block = ColorS.Mix(ColorS.Mix(blockTintCold, blockTintWarm, temperature), ColorS.Mix(blockTintDry, blockTintMoist, sample.Humidity));
        ColorS fluid = ColorS.Mix(fluidTintCold, fluidTintWarm, temperature);

        return (block, fluid);
    }

    /// <inheritdoc />
    public Temperature GetTemperature(Vector3d position)
    {
        Vector3i samplingPosition = position.Floor();
        Sample sample = GetSample(samplingPosition);

        return sample.GetRealTemperature(position.Y);
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

    private void SetUpSamplingNoise(NoiseFactory factory)
    {
        samplingNoise = new NoiseGenerator2D(CreateSamplingNoise);
        stoneNoise = new NoiseGenerator2D(CreateStoneNoise);

        NoiseGenerator CreateSamplingNoise()
        {
            return factory.CreateNext()
                .WithType(NoiseType.GradientNoise)
                .WithFrequency(frequency: 0.01f)
                .WithFractals()
                .WithOctaves(octaves: 5)
                .WithLacunarity(lacunarity: 2.0f)
                .WithGain(gain: 0.5f)
                .WithWeightedStrength(weightedStrength: 0.0f)
                .Build();
        }

        NoiseGenerator CreateStoneNoise()
        {
            return factory.CreateNext()
                .WithType(NoiseType.GradientNoise)
                .WithFrequency(frequency: 0.05f)
                .WithFractals()
                .WithOctaves(octaves: 2)
                .WithLacunarity(lacunarity: 2.0f)
                .WithGain(gain: 0.5f)
                .WithWeightedStrength(weightedStrength: 0.0f)
                .Build();
        }
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

        SetUpGeneratingNoise(factory);

        if (data == null)
        {
            Generate();
            dirty = true;
        }

        SetUpSamplingNoise(factory);
    }

    private void SetUpGeneratingNoise(NoiseFactory factory)
    {
        generatingNoise.Pieces = factory.CreateNext()
            .WithType(NoiseType.CellularNoise)
            .WithFrequency(frequency: 0.05f)
            .Build();

        generatingNoise.Stone = factory.CreateNext()
            .WithType(NoiseType.GradientNoise)
            .WithFrequency(frequency: 0.08f)
            .Build();
    }

    private void Generate()
    {
        Debug.Assert(data == null);
        data = new Data();

        LogGeneratingMap(logger);

        using Timer? timer = Timer.Start("Map Generation", TimingStyle.Once, Profile.GetSingleUseActiveProfiler());

        GenerateTerrain(data, generatingNoise);
        GenerateTemperature(data);
        GenerateHumidity(data);

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
        return GetSample(position.Xz, grid2D: null);
    }

    /// <summary>
    ///     Get a noise grid for the given position and size.
    /// </summary>
    /// <param name="position">The position of the grid.</param>
    /// <param name="size">The size of the grid.</param>
    /// <returns>The noise grid.</returns>
    internal NoiseGrid2D GetNoiseGrid(Vector2i position, Int32 size)
    {
        return samplingNoise.GetNoiseGrid(position, size);
    }

    /// <summary>
    ///     Get a sample of the map at the given coordinates.
    /// </summary>
    /// <param name="column">
    ///     The column to sample from, in block coordinates.
    ///     This is simply the X and Z coordinates of a block column.
    /// </param>
    /// <param name="grid2D">
    ///     An optional noise grid.
    ///     Serves to optimize sampling by allowing to batch noise generation.
    /// </param>
    /// <returns>The sample.</returns>
    internal Sample GetSample(Vector2i column, NoiseGrid2D? grid2D)
    {
        Debug.Assert(data != null);

        Int32 xP = DivideByCellSize(column.X);
        Int32 yP = DivideByCellSize(column.Y);

        Int32 xN = GetNearestNeighbor(column.X);
        Int32 yN = GetNearestNeighbor(column.Y);

        (Int32 x1, Int32 x2) = MathTools.MinMax(xP, xN);
        (Int32 y1, Int32 y2) = MathTools.MinMax(yP, yN);

        // Both p1 and p2 are centers of cells - between them is a cell-sized area which contains the sampling position.
        // This is done as the cell values apply only to the center of the cell, everything else is interpolated.

        Vector2d p1 = new Vector2d(x1, y1) * CellSize + new Vector2d(CellSizeHalf);
        Vector2d p2 = new Vector2d(x2, y2) * CellSize + new Vector2d(CellSizeHalf);

        Double tX = MathTools.InverseLerp(p1.X, p2.X, column.X);
        Double tY = MathTools.InverseLerp(p1.Y, p2.Y, column.Y);

        tX = ApplyBiomeChangeFunction(tX);
        tY = ApplyBiomeChangeFunction(tY);

        const Double transitionFactor = 0.015;

        Vector2 noise = samplingNoise.GetNoise(column, grid2D);

        Double blendX = tX + noise.X * GetBorderStrength(tX) * transitionFactor;
        Double blendY = tY + noise.Y * GetBorderStrength(tY) * transitionFactor;

        ref readonly Cell c00 = ref data.GetCell(x1 + WidthHalf, y1 + WidthHalf);
        ref readonly Cell c10 = ref data.GetCell(x2 + WidthHalf, y1 + WidthHalf);
        ref readonly Cell c01 = ref data.GetCell(x1 + WidthHalf, y2 + WidthHalf);
        ref readonly Cell c11 = ref data.GetCell(x2 + WidthHalf, y2 + WidthHalf);

        var temperature = (Single) MathTools.BiLerp(c00.temperature, c10.temperature, c01.temperature, c11.temperature, blendX, blendY);
        var humidity = (Single) MathTools.BiLerp(c00.humidity, c10.humidity, c01.humidity, c11.humidity, blendX, blendY);
        var height = (Single) MathTools.BiLerp(c00.height, c10.height, c01.height, c11.height, blendX, blendY);

        Single mountainStrength = GetMountainStrength(c00, c10, c01, c11, height, (blendX, blendY));
        Single coastlineStrength = GetCoastlineStrength(c00, c10, c01, c11, ref height, (blendX, blendY), out Boolean isCliff);

        Biome specialBiome;
        Single specialStrength;

        if (mountainStrength > coastlineStrength)
        {
            specialBiome = biomes.GetMountainBiome();
            specialStrength = mountainStrength;
        }
        else
        {
            specialBiome = biomes.GetCoastlineBiome(temperature, humidity, isCliff);
            specialStrength = coastlineStrength;
        }

        Biome actual = MathTools.SelectByWeight(GetBiome(c00), GetBiome(c10), GetBiome(c01), GetBiome(c11), specialBiome, (blendX, blendY, specialStrength));

        return new Sample
        {
            Height = height,
            Temperature = temperature,
            Humidity = humidity,
            BlendFactors = (blendX, blendY, specialStrength),
            ActualBiome = actual,
            Biome00 = GetBiome(c00),
            Biome10 = GetBiome(c10),
            Biome01 = GetBiome(c01),
            Biome11 = GetBiome(c11),
            SpecialBiome = specialBiome,
            StoneData = (c00.stoneType, c10.stoneType, c01.stoneType, c11.stoneType, tX, tY)
        };
    }

    private static Int32 GetNearestNeighbor(Int32 number)
    {
        const Int32 halfCellSize = CellSize / 2;

        Int32 subject = DivideByCellSize(number);
        Int32 a = DivideByCellSize(number - halfCellSize);
        Int32 b = DivideByCellSize(number + halfCellSize);

        return a == subject ? b : a;
    }

    private Biome GetBiome(in Cell cell)
    {
        return cell.IsLand ? biomes.GetBiome(cell.temperature, cell.humidity) : biomes.GetOceanBiome(cell.temperature, cell.humidity);
    }

    private static Single GetMountainStrength(in Cell c00, in Cell c10, in Cell c01, in Cell c11, Single height, Vector2d blend)
    {
        (Double w1, Double w2, Double w3, Double w4) = GetMountainSlopeWeights(blend.X, blend.Y);

        Double e1 = GetSurfaceHeightDifference(c00, c10) * GetBorderStrength(blend.X) * w1;
        Double e2 = GetSurfaceHeightDifference(c01, c11) * GetBorderStrength(blend.X) * w2;

        Double e3 = GetSurfaceHeightDifference(c00, c01) * GetBorderStrength(blend.Y) * w3;
        Double e4 = GetSurfaceHeightDifference(c10, c11) * GetBorderStrength(blend.Y) * w4;

        var slopeMountainStrength = (Single) (e1 + e2 + e3 + e4);
        Single mountainStrength = Math.Min(slopeMountainStrength + height / 1.2f, val2: 1.0f);

        mountainStrength = (Single) mountainStrengthFunction.Evaluate(mountainStrength);

        return mountainStrength;
    }

    private static Single GetCoastlineStrength(in Cell c00, in Cell c10, in Cell c01, in Cell c11, ref Single height, Vector2d blend, out Boolean isCliff)
    {
        var depthStrength = (Single) depthStrengthFunction.Evaluate(height);

        static Vector2d FindClosestZero(Double f00, Double f10, Double f01, Double f11, Double x, Double y)
        {
            Vector2d grad = MathTools.GradBiLerp(f00, f10, f01, f11, x, y);
            Double dv = Vector2d.Dot(grad, Vector2d.Normalize(grad));

            Double k = MathTools.BiLerp(f00, f10, f01, f11, x, y) / dv;

            return new Vector2d(x, y) - k * Vector2d.Normalize(grad);
        }

        Double distanceToZero = (blend - FindClosestZero(c00.height, c10.height, c01.height, c11.height, blend.X, blend.Y)).Length;

        if (Double.IsNaN(distanceToZero))
            // All four heights are the same, so there is no gradient.
            distanceToZero = MathTools.NearlyZero(c00.height) ? 0 : 1;

        var distanceStrength = (Single) distanceStrengthFunction.Evaluate(distanceToZero);

        Double GetOceanStrength(in Cell c)
        {
            return c.IsLand ? 0.0 : 1.0;
        }

        var oceanStrength = (Single) MathTools.BiLerp(GetOceanStrength(c00), GetOceanStrength(c10), GetOceanStrength(c01), GetOceanStrength(c11), blend.X, blend.Y);

        Single coastlineStrength;

        if (height < 0.0f)
        {
            coastlineStrength = depthStrength - oceanStrength;
        }
        else
        {
            // It is possible that the ocean strength is greater 0.5 above the water height.
            // To prevent ocean biome above the water, the coastline strength must be greater 0.5 in that case.

            oceanStrength = (Single) oceanStrengthFunction.Evaluate(oceanStrength);

            coastlineStrength = depthStrength + oceanStrength;
        }

        coastlineStrength = Math.Clamp(coastlineStrength, min: 0.0f, max: 1.0f);
        coastlineStrength = Math.Max(coastlineStrength, distanceStrength);

        Single GetFlattenedHeight(Single height)
        {
            Single sign = Math.Sign(height);

            var flattenedHeight = (Single) flattenedHeightFunction.Evaluate(Math.Abs(height));

            return sign * flattenedHeight;
        }

        Single GetCliffFactor()
        {
            return (Single) cliffFactorFunction.Evaluate(1.0 - distanceStrength);
        }

        Single GetSurfaceHeight(in Cell c)
        {
            return c.IsLand ? c.height : 0.0f;
        }

        var cliffStrength = (Single) MathTools.BiLerp(GetSurfaceHeight(c00), GetSurfaceHeight(c10), GetSurfaceHeight(c01), GetSurfaceHeight(c11), blend.X, blend.Y);

        const Single maxBeachHeight = 0.001f;

        height = MathHelper.Lerp(GetFlattenedHeight(height), GetCliffFactor() * height, cliffStrength);
        isCliff = height > maxBeachHeight;

        return coastlineStrength;
    }

    private static Single GetSurfaceHeightDifference(in Cell a, in Cell b)
    {
        if (a.IsLand && b.IsLand) return Math.Abs(a.height - b.height);

        return 0;
    }

    private static (Double, Double, Double, Double) GetMountainSlopeWeights(Double x, Double y)
    {
        Double w1 = 1 - x;
        Double w2 = 1 - y;
        Double w3 = x;
        Double w4 = y;

        Double sum = w1 + w2 + w3 + w4;

        return (w1 / sum, w2 / sum, w3 / sum, w4 / sum);
    }

    private static Int32 DivideByCellSize(Int32 number)
    {
        Int32 result = number / CellSize;

        // We round away from zero for negative numbers, so that overall we always round towards negative infinity.

        return number < 0 && number != CellSize * result ? result - 1 : result;
    }

    /// <summary>
    ///     Get the cell position that contains the given block position.
    /// </summary>
    /// <param name="world">The block / world position.</param>
    /// <returns>The cell position.</returns>
    public static Vector2i GetCellIndex(Vector3i world)
    {
        return new Vector2i(DivideByCellSize(world.X), DivideByCellSize(world.Z));
    }

    /// <summary>
    ///     Transform a cell position to a world position, using the center of the cell.
    /// </summary>
    /// <param name="cell">The cell position.</param>
    /// <param name="y">The y position.</param>
    /// <returns>The world position.</returns>
    public static Vector3i GetCellCenter(Vector2i cell, Int32 y)
    {
        Vector2i position = cell * CellSize + new Vector2i(CellSizeHalf);

        return new Vector3i(position.X, y, position.Y);
    }

    /// <summary>
    ///     Check whether a cell is within the map limits.
    /// </summary>
    /// <param name="cell"></param>
    /// <returns></returns>
    public static Boolean IsInLimits(Vector2i cell)
    {
        return cell.X + WidthHalf is >= 0 and < Width && cell.Y + WidthHalf is >= 0 and < Width;
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

        const Double transitionFactor = 0.05;

        Double stoneX = sample.StoneData.tX + stoneNoise.X.GetNoise(position) * GetBorderStrength(sample.StoneData.tX) * transitionFactor;
        Double stoneY = sample.StoneData.tY + stoneNoise.Y.GetNoise(position) * GetBorderStrength(sample.StoneData.tY) * transitionFactor;

        return MathTools.SelectByWeight(sample.StoneData.stone00, sample.StoneData.stone10, sample.StoneData.stone01, sample.StoneData.stone11, (stoneX, stoneY));
    }

    /// <summary>
    ///     Get the border strength from a blend factor.
    /// </summary>
    private static Double GetBorderStrength(Double t)
    {
        return (t > 0.5 ? 1 - t : t) * 2;
    }

    private sealed class NoiseGenerator2D(Func<NoiseGenerator> factory) : IDisposable
    {
        public NoiseGenerator X { get; } = factory();
        public NoiseGenerator Y { get; } = factory();

        public void Dispose()
        {
            X.Dispose();
            Y.Dispose();
        }

        public Vector2 GetNoise(Vector2i position, NoiseGrid2D? grid2D)
        {
            return grid2D?.GetNoise(position) ?? (X.GetNoise(position), Y.GetNoise(position));
        }

        public NoiseGrid2D GetNoiseGrid(Vector2i position, Int32 size)
        {
            Array2D<Single> x = X.GetNoiseGrid(position, size);
            Array2D<Single> y = Y.GetNoiseGrid(position, size);

            return new NoiseGrid2D(position, x, y);
        }
    }

    /// <summary>
    ///     Contains two 2D noise grids, used during sampling.
    /// </summary>
    /// <param name="Base">The base position of the noise grids.</param>
    /// <param name="X">The first noise grid.</param>
    /// <param name="Y">The second noise grid.</param>
    internal sealed record NoiseGrid2D(Vector2i Base, Array2D<Single> X, Array2D<Single> Y)
    {
        /// <summary>
        ///     Get the noise at the given position.
        /// </summary>
        /// <param name="position">The position to get the noise at.</param>
        /// <returns>The noise at the given position.</returns>
        public Vector2 GetNoise(Vector2i position)
        {
            Vector2i relative = position - Base;

            return (X[relative], Y[relative]);
        }
    }

    private sealed class GeneratingNoise : IDisposable
    {
        public NoiseGenerator Pieces { get; set; } = null!;
        public NoiseGenerator Stone { get; set; } = null!;

        public void Dispose()
        {
            Pieces.Dispose();
            Stone.Dispose();
        }
    }

    /// <summary>
    ///     A sample of the map.
    /// </summary>
    public readonly record struct Sample
    {
        /// <summary>
        ///     The height of the sample.
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
        ///     Get the biome <c>00</c>.
        /// </summary>
        public Biome Biome00 { get; init; }

        /// <summary>
        ///     Get the biome <c>10</c>.
        /// </summary>
        public Biome Biome10 { get; init; }

        /// <summary>
        ///     Get the biome <c>01</c>.
        /// </summary>
        public Biome Biome01 { get; init; }

        /// <summary>
        ///     Get the biome <c>11</c>.
        /// </summary>
        public Biome Biome11 { get; init; }

        /// <summary>
        ///     Get the special biome.
        /// </summary>
        public Biome SpecialBiome { get; init; }

        /// <summary>
        ///     Get the blending factors. The factor on the z axis is the factor for a special biome.
        /// </summary>
        public Vector3d BlendFactors { get; init; }

        /// <summary>
        ///     Data regarding the stone composition.
        /// </summary>
        public (StoneType stone00, StoneType stone10, StoneType stone01, StoneType stone11, Double tX, Double tY) StoneData { get; init; }

        /// <summary>
        ///     Get the temperature at a given height.
        /// </summary>
        /// <param name="y">The height, in meters.</param>
        /// <returns>The temperature.</returns>
        public Temperature GetRealTemperature(Double y)
        {
            // The ground height follows the actual height of the sample, but mountains and oceans are ignored.
            // This is necessary to have more realistic lower temperature on mountains.

            Double groundHeight = Math.Clamp(Height * MaxHeight, min: 0.0, MaxHeight * 0.3);

            return GetTemperatureAtHeight(ConvertTemperatureToCelsius(Temperature), Humidity, y - groundHeight);
        }

        /// <summary>
        ///     Get the height of the sample.
        /// </summary>
        /// <returns>The height.</returns>
#pragma warning disable S4049 // Consistency.
        public Length GetRealHeight()
#pragma warning restore S4049
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
    }

    #region BIOME CHANGE FUNCTION

    private static readonly Vector2d pointA = new(x: 0.00, y: 0.00);
    private static readonly Vector2d pointB = new(x: 0.40, y: 0.01);
    private static readonly Vector2d pointC = new(x: 0.45, y: 0.05);
    private static readonly Vector2d pointD = new(x: 0.50, y: 0.50);
    private static readonly Vector2d pointE = Vector2d.One - pointC;
    private static readonly Vector2d pointF = Vector2d.One - pointB;
    private static readonly Vector2d pointG = Vector2d.One - pointA;

    private static readonly Polyline biomeChangeFunction = new()
    {
        Points =
        {
            pointA,
            pointB,
            pointC,
            pointD,
            pointE,
            pointF,
            pointG
        }
    };

    private static Double ApplyBiomeChangeFunction(Double t)
    {
        return biomeChangeFunction.Evaluate(t);
    }

    #endregion BIOME CHANGE FUNCTION

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
            samplingNoise.Dispose();
            stoneNoise.Dispose();
            generatingNoise.Dispose();
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
    ~Map()
    {
        Dispose(disposing: false);
    }

    #endregion
}
