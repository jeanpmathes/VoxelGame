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
public class Map
{
    /// <summary>
    ///     The size of a map cell.
    /// </summary>
    private const int CellSize = 100_000;

    private const int MapWidth = (int) World.BlockLimit / CellSize;
    private const int CellCount = MapWidth * MapWidth;
    private static readonly ILogger logger = LoggingHelper.CreateLogger<Map>();

    private Data? data;

    private bool dirty;

    /// <summary>
    ///     Initialize the map. If available, it can be loaded from a stream.
    ///     If loading is not possible, it will be generated.
    /// </summary>
    /// <param name="stream">The stream to load the map from.</param>
    public void Initialize(Stream? stream)
    {
        logger.LogDebug(Events.WorldGeneration, "Initializing map");

        if (stream != null)
        {
            Load(stream);
            if (data == null) logger.LogError(Events.WorldGeneration, "Failed to load map");
        }

        if (data == null) Generate();
    }

    private void Generate()
    {
        Debug.Assert(data == null);
        data = new Data();
        dirty = true;

        logger.LogDebug(Events.WorldGeneration, "Generating map");

        logger.LogInformation(Events.WorldGeneration, "Generated map");
    }

    private void Load(Stream stream)
    {
        Debug.Assert(data == null);
        Data loaded = new();

        Cell LoadCell()
        {
            Cell cell;

            cell.value = (byte) stream.ReadByte();

            return cell;
        }

        for (var i = 0; i < CellCount; i++)
        {
            if (stream.Position == stream.Length) return;

            loaded.cells[i] = LoadCell();
        }

        data = loaded;

        logger.LogDebug(Events.WorldGeneration, "Loaded map");
    }

    /// <summary>
    ///     Store the map to a stream.
    /// </summary>
    /// <param name="stream">The stream to store the map to.</param>
    public void Store(Stream stream)
    {
        Debug.Assert(data != null);

        if (!dirty) return;

        void StoreCell(Cell cell)
        {
            stream.WriteByte(cell.value);
        }

        foreach (Cell cell in data.cells) StoreCell(cell);
    }

    #pragma warning disable S3898
    private struct Cell
    #pragma warning restore S3898
    {
        public byte value;
    }

    private sealed class Data
    {
        public readonly Cell[] cells = new Cell[CellCount];
    }
}
