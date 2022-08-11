// <copyright file="Map.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Extensions.Logging;
using OpenTK.Mathematics;
using VoxelGame.Core.Logic;
using VoxelGame.Core.Utilities;
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
    ///     The size of a map cell.
    /// </summary>
    private const int CellSize = 100_000;

    private const int MinimumWidth = (int) (World.BlockLimit * 2) / CellSize;
    private const int Width = MinimumWidth + 2;
    private const int CellCount = Width * Width;
    private static readonly ILogger logger = LoggingHelper.CreateLogger<Map>();

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

    /// <inheritdoc />
    public string GetPositionDebugData(Vector3d position)
    {
        Vector2i samplingPosition = position.Floor().Xz;
        Sample sample = GetSample(samplingPosition);

        return $"{nameof(Map)}: {sample.Height:F2} {sample.Biome} {GetStoneType(samplingPosition)}";
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
    }

    private void Generate(int seed)
    {
        Debug.Assert(data == null);
        data = new Data();

        logger.LogDebug(Events.WorldGeneration, "Generating map");

        var stopwatch = Stopwatch.StartNew();

        GenerateTerrain(data, seed);
        GenerateTemperature(data);
        GenerateMoisture(data);

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
        EmitMoistureView(data, path);
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
            cell.moisture = reader.ReadSingle();
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
            writer.Write(cell.moisture);
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

        double tx = VMath.InverseLerp(p1.X, p2.X, position.X);
        double ty = VMath.InverseLerp(p1.Y, p2.Y, position.Y);

        tx = ApplyBiomeChangeFunction(tx);
        ty = ApplyBiomeChangeFunction(ty);

        const int extents = Width / 2;

        ref readonly Cell closest = ref data.GetCell(xP + extents, yP + extents);

        ref readonly Cell c00 = ref data.GetCell(x1 + extents, y1 + extents);
        ref readonly Cell c10 = ref data.GetCell(x2 + extents, y1 + extents);
        ref readonly Cell c01 = ref data.GetCell(x1 + extents, y2 + extents);
        ref readonly Cell c11 = ref data.GetCell(x2 + extents, y2 + extents);

        var temperature = (float) VMath.Blerp(c00.temperature, c10.temperature, c01.temperature, c11.temperature, tx, ty);
        var moisture = (float) VMath.Blerp(c00.moisture, c10.moisture, c01.moisture, c11.moisture, tx, ty);

        double GetBorderStrength(double t)
        {
            return (t > 0.5 ? 1 - t : t) * 2;
        }

        return new Sample
        {
            Height = (float) VMath.Blerp(c00.height, c10.height, c01.height, c11.height, tx, ty),
            Biome = closest.IsLand ? biomes.GetBiome(temperature, moisture) : Biome.Ocean,
            BorderStrength = (GetBorderStrength(tx), GetBorderStrength(ty))
        };
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
    public StoneType GetStoneType(Vector2i position)
    {
        Debug.Assert(data != null);

        int xP = DivideByCellSize(position.X);
        int yP = DivideByCellSize(position.Y);

        xP = Math.Clamp(xP, min: 0, Width - 1);
        yP = Math.Clamp(yP, min: 0, Width - 1);

        const int extents = Width / 2;

        ref readonly Cell cell = ref data.GetCell(xP + extents, yP + extents);

        return cell.stoneType;
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
        ///     The biome of the sample.
        /// </summary>
        public Biome Biome { get; init; }

        /// <summary>
        /// The strength of the border, e.g. how close to the edge the sample is. This is a value in the range [0, 1] on every axis.
        /// </summary>
        public Vector2d BorderStrength { get; init; }
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
        ///     The moisture of the cell, in the range [0, 1].
        /// </summary>
        public float moisture;

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

    private static double ApplyBiomeChangeFunction(double t)
    {
        if (t <= pointA.X) return pointA.Y;

        if (t <= pointB.X) return MathHelper.Lerp(pointA.Y, pointB.Y, VMath.InverseLerp(pointA.X, pointB.X, t));

        if (t <= pointC.X) return MathHelper.Lerp(pointB.Y, pointC.Y, VMath.InverseLerp(pointB.X, pointC.X, t));

        if (t <= pointD.X) return MathHelper.Lerp(pointC.Y, pointD.Y, VMath.InverseLerp(pointC.X, pointD.X, t));

        if (t <= pointE.X) return MathHelper.Lerp(pointD.Y, pointE.Y, VMath.InverseLerp(pointD.X, pointE.X, t));

        if (t <= pointF.X) return MathHelper.Lerp(pointE.Y, pointF.Y, VMath.InverseLerp(pointE.X, pointF.X, t));

        if (t <= pointG.X) return MathHelper.Lerp(pointF.Y, pointG.Y, VMath.InverseLerp(pointF.X, pointG.X, t));

        return pointG.Y;
    }

    #endregion BIOME CHANGE FUNCTION
}
