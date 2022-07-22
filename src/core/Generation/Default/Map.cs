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
using VoxelGame.Logging;

namespace VoxelGame.Core.Generation.Default;

/// <summary>
///     Represents a rough overview over the entire world, as a 2D map.
///     This class can load, store and generate these maps.
/// </summary>
public partial class Map
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
    ///     The size of a map cell.
    /// </summary>
    private const int CellSize = 100_000;

    private const int Width = (int) World.BlockLimit / CellSize;
    private const int CellCount = Width * Width;
    private static readonly ILogger logger = LoggingHelper.CreateLogger<Map>();

    private readonly string debugPath;

    private Data? data;

    private bool dirty;

    /// <summary>
    ///     Create a new map.
    /// </summary>
    /// <param name="debugPath">The path at which debug artifacts are created.</param>
    public Map(string debugPath)
    {
        this.debugPath = debugPath;
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
        dirty = true;

        logger.LogDebug(Events.WorldGeneration, "Generating map");

        GenerateTerrain(data, seed);
        GenerateTemperature(data);
        GenerateMoisture(data);

        EmitTerrainView(data, debugPath);
        EmitTemperatureView(data, debugPath);
        EmitMoistureView(data, debugPath);

        logger.LogInformation(Events.WorldGeneration, "Generated map");
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

        if (!dirty) return;

        void StoreCell(in Cell cell)
        {
            writer.Write(cell.continent);
            writer.Write(cell.height);
            writer.Write(cell.temperature);
            writer.Write(cell.moisture);
            writer.Write((byte) cell.conditions);
        }

        for (var i = 0; i < CellCount; i++) StoreCell(data.cells[i]);
    }

    private record struct Cell
    {
        public CellConditions conditions;
        public short continent;
        public float height;
        public float moisture;
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
}
