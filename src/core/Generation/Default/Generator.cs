// <copyright file="Generator.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Logging;
using OpenTK.Mathematics;
using VoxelGame.Core.Logic;
using VoxelGame.Logging;

namespace VoxelGame.Core.Generation.Default;

/// <summary>
///     The default world generator.
/// </summary>
public class Generator : IWorldGenerator
{
    private const int SeaLevel = 0;

    /// <summary>
    ///     Height of the highest mountains and deepest oceans.
    /// </summary>
    private const int Height = 10_000;

    private const string MapBlobName = "default_map";
    private static readonly ILogger logger = LoggingHelper.CreateLogger<Generator>();

    private readonly BiomeDistribution biomes;
    private readonly Map map;

    private readonly Palette palette = new();

    private readonly int seed;
    private readonly World world;

    /// <summary>
    ///     Creates a new default world generator.
    /// </summary>
    /// <param name="world">The world to generate.</param>
    public Generator(World world)
    {
        this.world = world;
        seed = world.Seed;

        biomes = BiomeDistribution.Default;
        Biome.Setup(seed);

        map = new Map(biomes);

        Initialize();
        Store();

        logger.LogInformation(Events.WorldGeneration, "Created '{Name}' world generator", nameof(Default));
    }

    /// <inheritdoc />
    public IEnumerable<uint> GenerateColumn(int x, int z, (int start, int end) heightRange)
    {
        Map.Sample sample = map.GetSample((x, z));

        for (int y = heightRange.start; y < heightRange.end; y++) yield return GenerateBlock((x, y, z), sample);
    }

    /// <inheritdoc />
    public void EmitViews(string path)
    {
        map.EmitViews(path);
    }

    private void Initialize()
    {
        using BinaryReader? read = world.GetBlobReader(MapBlobName);
        map.Initialize(read, seed);
    }

    private void Store()
    {
        using BinaryWriter? write = world.GetBlobWriter(MapBlobName);
        if (write != null) map.Store(write);
    }

    private uint GenerateBlock(Vector3i position, in Map.Sample sample)
    {
        double worldHeight = sample.Height * Height + sample.Biome.GetOffset(position.Xz);

        if (position.Y == -World.BlockLimit) return palette.Core;

        if (position.Y <= worldHeight) return palette.Land;

        if (position.Y <= SeaLevel) return palette.Water;

        return palette.Empty;
    }
}
