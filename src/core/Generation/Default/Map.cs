// <copyright file="Map.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System.Diagnostics;
using System.IO;
using Microsoft.Extensions.Logging;
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

        GenerateContinents(data, seed);
        EmitContinentView(data, debugPath);

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
            cell.isLand = reader.ReadBoolean();

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

        void StoreCell(Cell cell)
        {
            writer.Write(cell.continent);
            writer.Write(cell.isLand);
        }

        foreach (Cell cell in data.cells) StoreCell(cell);
    }

    #pragma warning disable S3898
    private struct Cell
    #pragma warning restore S3898
    {
        public short continent;
        public bool isLand;
    }

    private sealed class Data
    {
        public readonly Cell[] cells = new Cell[CellCount];

        public ref Cell GetCell(int x, int y)
        {
            return ref cells[x + y * Width];
        }
    }
}
