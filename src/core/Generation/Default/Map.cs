// <copyright file="Map.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using Microsoft.Extensions.Logging;
using OpenTK.Mathematics;
using VoxelGame.Core.Collections;
using VoxelGame.Core.Logic;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Visuals;
using VoxelGame.Logging;

namespace VoxelGame.Core.Generation.Default;

/// <summary>
///     Represents a rough overview over the entire world, as a 2D map.
///     This class can load, store and generate these maps.
/// </summary>
public partial class Map : IMap
{
    /// <summary>
    ///     Additional cell data that is stored as flags.
    /// </summary>
    [Flags]
    #pragma warning disable S4022
    public enum CellConditions : byte
    #pragma warning restore S4022
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
    ///     The stone type of a cell.
    /// </summary>
    #pragma warning disable S4022
    public enum StoneType : byte
    #pragma warning restore S4022
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
    ///     Height of the highest mountains and deepest oceans.
    /// </summary>
    public const int MaxHeight = 10_000;

    /// <summary>
    ///     The size of a map cell.
    /// </summary>
    private const int CellSize = 100_000;

    private const int MinimumWidth = (int) (World.BlockLimit * 2) / CellSize;
    private const int Width = MinimumWidth + 2;
    private const int CellCount = Width * Width;

    private const double MinTemperature = -5.0;
    private const double MaxTemperature = 30.0;
    private static readonly ILogger logger = LoggingHelper.CreateLogger<Map>();

    private static readonly Color blockTintWarm = Color.LightGreen;
    private static readonly Color blockTintCold = Color.DarkGreen;
    private static readonly Color blockTintMoist = Color.LawnGreen;
    private static readonly Color blockTintDry = Color.Olive;

    private static readonly Color fluidTintWarm = Color.LightBlue;
    private static readonly Color fluidTintCold = Color.DarkBlue;

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

    private readonly FastNoiseLite xNoise = new();
    private readonly FastNoiseLite yNoise = new();

    private Data? data;

    /// <summary>
    ///     Create a new map.
    /// </summary>
    /// <param name="biomes">The biome distribution used by the generator.</param>
    public Map(BiomeDistribution biomes)
    {
        this.biomes = biomes;
    }

    /// <inheritdoc />
    public string GetPositionDebugData(Vector3d position)
    {
        Vector3i samplingPosition = position.Floor();
        Sample sample = GetSample(samplingPosition.Xz);

        return $"M: [{nameof(Default)}] {sample.Height:F2} {sample.ActualBiome} {GetStoneType(samplingPosition, sample)}";
    }

    /// <inheritdoc />
    public (TintColor block, TintColor fluid) GetPositionTint(Vector3d position)
    {
        Vector2i samplingPosition = position.Floor().Xz;
        Sample sample = GetSample(samplingPosition);

        float temperature = NormalizeTemperature(sample.GetTemperatureInCelsius(position.Y));

        Color block = Colors.Mix(Colors.Mix(blockTintCold, blockTintWarm, temperature), Colors.Mix(blockTintDry, blockTintMoist, sample.Humidity));
        Color fluid = Colors.Mix(fluidTintCold, fluidTintWarm, temperature);

        return (new TintColor(block), new TintColor(fluid));
    }

    /// <inheritdoc />
    public double GetTemperature(Vector3d position)
    {
        Vector2i samplingPosition = position.Floor().Xz;
        Sample sample = GetSample(samplingPosition);

        return sample.GetTemperatureInCelsius(position.Y);
    }

    private static double ConvertTemperatureToCelsius(float temperature)
    {
        return MathHelper.Lerp(MinTemperature, MaxTemperature, temperature);
    }

    private static float NormalizeTemperature(double temperature)
    {
        return (float) VMath.InverseLerp(MinTemperature, MaxTemperature, temperature);
    }

    private static double GetTemperatureAtHeight(double groundTemperature, float humidity, double heightAboveGround)
    {
        if (heightAboveGround < 0) return groundTemperature;

        double decreaseFactor = MathHelper.Lerp(start: 10.0, end: 5.0, humidity);

        return groundTemperature - decreaseFactor * heightAboveGround / 1000.0;
    }

    private void SetupNoise(int seed)
    {
        void Setup(FastNoiseLite noise, int specificSeed)
        {
            noise.SetSeed(specificSeed);
            noise.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
            noise.SetFrequency(frequency: 0.01f);

            noise.SetFractalType(FastNoiseLite.FractalType.FBm);
            noise.SetFractalOctaves(octaves: 5);
            noise.SetFractalLacunarity(lacunarity: 2.0f);
            noise.SetFractalGain(gain: 0.5f);
            noise.SetFractalWeightedStrength(weightedStrength: 0.0f);
        }

        Setup(xNoise, seed);
        Setup(yNoise, ~seed);
    }

    /// <summary>
    ///     Initialize the map. If available, it can be loaded from a stream.
    ///     If loading is not possible, it will be generated.
    /// </summary>
    /// <param name="reader">The reader to load the map from.</param>
    /// <param name="seed">The seed to use for the map generation.</param>
    public void Initialize(BinaryReader? reader, int seed)
    {
        logger.LogDebug(Events.WorldGeneration, "Initializing map");

        if (reader != null)
        {
            Load(reader);
        }

        if (data == null) Generate(seed);

        SetupNoise(seed);
    }

    private void Generate(int seed)
    {
        Debug.Assert(data == null);
        data = new Data();

        logger.LogDebug(Events.WorldGeneration, "Generating map");

        var stopwatch = Stopwatch.StartNew();

        GenerateTerrain(data, seed);
        GenerateTemperature(data);
        GenerateHumidity(data);

        stopwatch.Stop();

        logger.LogInformation(Events.WorldGeneration, "Generated map in {Time}s", stopwatch.Elapsed.TotalSeconds);
    }

    /// <summary>
    ///     Emit views of different map values.
    /// </summary>
    /// <param name="path">The path to a directory to save the views to.</param>
    public void EmitViews(string path)
    {
        Debug.Assert(data != null);

        EmitTerrainView(data, path);
        EmitStoneView(data, path);
        EmitTemperatureView(data, path);
        EmitHumidityView(data, path);
        EmitBiomeView(data, biomes, path);
    }

    private void Load(BinaryReader reader)
    {
        Debug.Assert(data == null);
        Data loaded = new();

        Cell LoadCell()
        {
            Cell cell;

            cell.continent = reader.ReadInt16();
            cell.height = reader.ReadSingle();
            cell.temperature = reader.ReadSingle();
            cell.humidity = reader.ReadSingle();
            cell.conditions = (CellConditions) reader.ReadByte();
            cell.stoneType = (StoneType) reader.ReadByte();

            return cell;
        }

        try
        {
            for (var i = 0; i < CellCount; i++) loaded.cells[i] = LoadCell();
        }
        catch (EndOfStreamException)
        {
            logger.LogError(Events.WorldGeneration, "Failed to load map, reached end of stream");

            return;
        }

        data = loaded;

        logger.LogDebug(Events.WorldGeneration, "Loaded map");
    }

    /// <summary>
    ///     Store the map to a stream.
    /// </summary>
    /// <param name="writer">The writer to write the map.</param>
    public void Store(BinaryWriter writer)
    {
        Debug.Assert(data != null);

        void StoreCell(in Cell cell)
        {
            writer.Write(cell.continent);
            writer.Write(cell.height);
            writer.Write(cell.temperature);
            writer.Write(cell.humidity);
            writer.Write((byte) cell.conditions);
            writer.Write((byte) cell.stoneType);
        }

        for (var i = 0; i < CellCount; i++) StoreCell(data.cells[i]);
    }

    /// <summary>
    ///     Get a sample of the map at the given coordinates.
    /// </summary>
    /// <param name="position">The world position (just XZ) of the sample.</param>
    /// <returns>The sample.</returns>
    public Sample GetSample(Vector2i position)
    {
        Debug.Assert(data != null);

        static int GetNearestNeighbor(int number)
        {
            const int halfCellSize = CellSize / 2;

            int subject = DivideByCellSize(number);
            int a = DivideByCellSize(number - halfCellSize);
            int b = DivideByCellSize(number + halfCellSize);

            return a == subject ? b : a;
        }

        int xP = DivideByCellSize(position.X);
        int yP = DivideByCellSize(position.Y);

        int xN = GetNearestNeighbor(position.X);
        int yN = GetNearestNeighbor(position.Y);

        (int x1, int x2) = VMath.MinMax(xP, xN);
        (int y1, int y2) = VMath.MinMax(yP, yN);

        const int halfCellSize = CellSize / 2;

        Vector2d p1 = new Vector2d(x1, y1) * CellSize + new Vector2d(halfCellSize, halfCellSize);
        Vector2d p2 = new Vector2d(x2, y2) * CellSize + new Vector2d(halfCellSize, halfCellSize);

        double tX = VMath.InverseLerp(p1.X, p2.X, position.X);
        double tY = VMath.InverseLerp(p1.Y, p2.Y, position.Y);

        tX = ApplyBiomeChangeFunction(tX);
        tY = ApplyBiomeChangeFunction(tY);

        const double transitionFactor = 0.015;

        double blendX = tX + xNoise.GetNoise(position.X, position.Y) * GetBorderStrength(tX) * transitionFactor;
        double blendY = tY + yNoise.GetNoise(position.X, position.Y) * GetBorderStrength(tY) * transitionFactor;

        const int extents = Width / 2;

        ref readonly Cell c00 = ref data.GetCell(x1 + extents, y1 + extents);
        ref readonly Cell c10 = ref data.GetCell(x2 + extents, y1 + extents);
        ref readonly Cell c01 = ref data.GetCell(x1 + extents, y2 + extents);
        ref readonly Cell c11 = ref data.GetCell(x2 + extents, y2 + extents);

        var temperature = (float) VMath.BiLerp(c00.temperature, c10.temperature, c01.temperature, c11.temperature, blendX, blendY);
        var humidity = (float) VMath.BiLerp(c00.humidity, c10.humidity, c01.humidity, c11.humidity, blendX, blendY);
        var height = (float) VMath.BiLerp(c00.height, c10.height, c01.height, c11.height, blendX, blendY);

        float mountainStrength = GetMountainStrength(c00, c10, c01, c11, height, (blendX, blendY));
        float coastlineStrength = GetCoastlineStrength(c00, c10, c01, c11, ref height, (blendX, blendY), out bool isCliff);

        Biome specialBiome;
        float specialStrength;

        if (mountainStrength > coastlineStrength)
        {
            specialBiome = Biome.Mountains;
            specialStrength = mountainStrength;
        }
        else
        {
            specialBiome = biomes.GetCoastlineBiome(temperature, humidity, isCliff);
            specialStrength = coastlineStrength;
        }

        Biome GetBiome(in Cell cell)
        {
            return cell.IsLand ? biomes.GetBiome(cell.temperature, cell.humidity) : biomes.GetOceanBiome(cell.temperature, cell.humidity);
        }

        Biome actual = VMath.SelectByWeight(GetBiome(c00), GetBiome(c10), GetBiome(c01), GetBiome(c11), specialBiome, (blendX, blendY, specialStrength));

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

    private static float GetMountainStrength(in Cell c00, in Cell c10, in Cell c01, in Cell c11, float height, Vector2d blend)
    {
        (double w1, double w2, double w3, double w4) = GetMountainSlopeWeights(blend.X, blend.Y);

        double e1 = GetSurfaceHeightDifference(c00, c10) * GetBorderStrength(blend.X) * w1;
        double e2 = GetSurfaceHeightDifference(c01, c11) * GetBorderStrength(blend.X) * w2;

        double e3 = GetSurfaceHeightDifference(c00, c01) * GetBorderStrength(blend.Y) * w3;
        double e4 = GetSurfaceHeightDifference(c10, c11) * GetBorderStrength(blend.Y) * w4;

        var slopeMountainStrength = (float) (e1 + e2 + e3 + e4);
        float mountainStrength = Math.Min(slopeMountainStrength + height / 1.2f, val2: 1.0f);

        mountainStrength = (float) mountainStrengthFunction.Evaluate(mountainStrength);

        return mountainStrength;
    }

    private static float GetCoastlineStrength(in Cell c00, in Cell c10, in Cell c01, in Cell c11, ref float height, Vector2d blend, out bool isCliff)
    {
        var depthStrength = (float) depthStrengthFunction.Evaluate(height);

        static Vector2d FindClosestZero(double f00, double f10, double f01, double f11, double x, double y)
        {
            Vector2d grad = VMath.GradBiLerp(f00, f10, f01, f11, x, y);
            double dv = Vector2d.Dot(grad, Vector2d.Normalize(grad));

            double k = VMath.BiLerp(f00, f10, f01, f11, x, y) / dv;

            return new Vector2d(x, y) - k * Vector2d.Normalize(grad);
        }

        double distanceToZero = (blend - FindClosestZero(c00.height, c10.height, c01.height, c11.height, blend.X, blend.Y)).Length;

        if (double.IsNaN(distanceToZero))
            // All four heights are the same, so there is no gradient.
            distanceToZero = VMath.NearlyZero(c00.height) ? 0 : 1;

        var distanceStrength = (float) distanceStrengthFunction.Evaluate(distanceToZero);

        double GetOceanStrength(in Cell c)
        {
            return c.IsLand ? 0.0 : 1.0;
        }

        var oceanStrength = (float) VMath.BiLerp(GetOceanStrength(c00), GetOceanStrength(c10), GetOceanStrength(c01), GetOceanStrength(c11), blend.X, blend.Y);

        float coastlineStrength;

        if (height < 0.0f)
        {
            coastlineStrength = depthStrength - oceanStrength;
        }
        else
        {
            // It is possible that the ocean strength is greater 0.5 above the water height.
            // To prevent ocean biome above the water, the coastline strength must be greater 0.5 in that case.

            oceanStrength = (float) oceanStrengthFunction.Evaluate(oceanStrength);

            coastlineStrength = depthStrength + oceanStrength;
        }

        coastlineStrength = Math.Clamp(coastlineStrength, min: 0.0f, max: 1.0f);
        coastlineStrength = Math.Max(coastlineStrength, distanceStrength);

        float GetFlattenedHeight(float height)
        {
            float sign = Math.Sign(height);

            var flattenedHeight = (float) flattenedHeightFunction.Evaluate(Math.Abs(height));

            return sign * flattenedHeight;
        }

        float GetCliffFactor()
        {
            return (float) cliffFactorFunction.Evaluate(1.0 - distanceStrength);
        }

        float GetSurfaceHeight(in Cell c)
        {
            return c.IsLand ? c.height : 0.0f;
        }

        var cliffStrength = (float) VMath.BiLerp(GetSurfaceHeight(c00), GetSurfaceHeight(c10), GetSurfaceHeight(c01), GetSurfaceHeight(c11), blend.X, blend.Y);

        const float maxBeachHeight = 0.001f;

        height = MathHelper.Lerp(GetFlattenedHeight(height), GetCliffFactor() * height, cliffStrength);
        isCliff = height > maxBeachHeight;

        return coastlineStrength;
    }

    private static float GetSurfaceHeightDifference(in Cell a, in Cell b)
    {
        if (a.IsLand && b.IsLand) return Math.Abs(a.height - b.height);

        return 0;
    }

    private static (double, double, double, double) GetMountainSlopeWeights(double x, double y)
    {
        double w1 = 1 - x;
        double w2 = 1 - y;
        double w3 = x;
        double w4 = y;

        double sum = w1 + w2 + w3 + w4;

        return (w1 / sum, w2 / sum, w3 / sum, w4 / sum);
    }

    private static int DivideByCellSize(int number)
    {
        int result = number / CellSize;
        int adjusted = number < 0 && number != CellSize * result ? result - 1 : result;

        return adjusted;
    }

    /// <summary>
    ///     Get the stone type at a given position.
    /// </summary>
    public StoneType GetStoneType(Vector3i position, in Sample sample)
    {
        Debug.Assert(data != null);

        const double transitionFactor = 0.05;
        const double scalingFactor = 5.0;

        Vector3d scaledPosition = position.ToVector3d() * scalingFactor;

        double stoneX = sample.StoneData.tX + xNoise.GetNoise(scaledPosition.X, scaledPosition.Y, scaledPosition.Z) * GetBorderStrength(sample.StoneData.tX) * transitionFactor;
        double stoneY = sample.StoneData.tY + yNoise.GetNoise(scaledPosition.X, scaledPosition.Y, scaledPosition.Z) * GetBorderStrength(sample.StoneData.tY) * transitionFactor;

        return VMath.SelectByWeight(sample.StoneData.stone00, sample.StoneData.stone10, sample.StoneData.stone01, sample.StoneData.stone11, (stoneX, stoneY));
    }

    /// <summary>
    ///     Get the border strength from a blend factor.
    /// </summary>
    private static double GetBorderStrength(double t)
    {
        return (t > 0.5 ? 1 - t : t) * 2;
    }

    /// <summary>
    ///     A sample of the map.
    /// </summary>
    public record struct Sample
    {
        /// <summary>
        ///     The height of the sample.
        /// </summary>
        public float Height { get; init; }

        /// <summary>
        ///     The temperature of the sample. Use <see cref="GetTemperatureInCelsius"/> to retrieve the temperature.
        /// </summary>
        public float Temperature { private get; init; }

        /// <summary>
        ///     The humidity of the sample.
        /// </summary>
        public float Humidity { get; init; }

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
        public (StoneType stone00, StoneType stone10, StoneType stone01, StoneType stone11, double tX, double tY) StoneData { get; init; }

        /// <summary>
        ///     Get the temperature at a given height.
        /// </summary>
        /// <param name="y">The height.</param>
        /// <returns>The temperature, in degrees Celsius.</returns>
        public readonly double GetTemperatureInCelsius(double y)
        {
            double groundHeight = Math.Clamp(Height * MaxHeight, min: 0.0, MaxHeight * 0.3);

            return GetTemperatureAtHeight(ConvertTemperatureToCelsius(Temperature), Humidity, y - groundHeight);
        }
    }

    private record struct Cell
    {
        /// <summary>
        ///     Flags for different cell conditions.
        /// </summary>
        public CellConditions conditions;

        /// <summary>
        ///     The continent id of the cell. The ids are not contiguous, but are unique.
        /// </summary>
        public short continent;

        /// <summary>
        ///     The height of the cell, in the range [-1, 1].
        /// </summary>
        public float height;

        /// <summary>
        ///     The humidity of the cell, in the range [0, 1].
        /// </summary>
        public float humidity;

        /// <summary>
        ///     The height of the cell.
        /// </summary>
        public StoneType stoneType;

        /// <summary>
        ///     The temperature of the cell, in the range [0, 1].
        /// </summary>
        public float temperature;

        public bool IsLand => height > 0.0f;
    }

    private sealed class Data
    {
        public readonly Cell[] cells = new Cell[CellCount];

        public ref Cell GetCell(int x, int y)
        {
            return ref Get(cells, x, y);
        }

        public ref Cell GetCell(Vector2i position)
        {
            return ref Get(cells, position);
        }

        public static ref T Get<T>(in T[] array, int x, int y)
        {
            return ref array[x + y * Width];
        }

        public static ref T Get<T>(in T[] array, Vector2i position)
        {
            return ref Get(array, position.X, position.Y);
        }

        public static Vector2i GetPosition(int index)
        {
            int x = index % Width;
            int y = index / Width;

            return new Vector2i(x, y);
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

    private static double ApplyBiomeChangeFunction(double t)
    {
        return biomeChangeFunction.Evaluate(t);
    }

    #endregion BIOME CHANGE FUNCTION
}
