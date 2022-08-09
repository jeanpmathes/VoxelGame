// <copyright file="Generator.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Logging;
using OpenTK.Mathematics;
using VoxelGame.Core.Logic;
using VoxelGame.Core.Utilities;
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

    private readonly Map map;

    private readonly Palette palette = new();

    private readonly int seed;

    private readonly FastNoiseLite transitionNoise;
    private readonly World world;

    /// <summary>
    ///     Creates a new default world generator.
    /// </summary>
    /// <param name="world">The world to generate.</param>
    public Generator(World world)
    {
        this.world = world;
        seed = world.Seed;

        Biome.Setup(seed);

        map = new Map(BiomeDistribution.Default);

        Initialize();
        Store();

        transitionNoise = new FastNoiseLite(seed);
        transitionNoise.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
        transitionNoise.SetFrequency(frequency: 0.23f);

        logger.LogInformation(Events.WorldGeneration, "Created '{Name}' world generator", nameof(Default));
    }

    /// <inheritdoc />
    public IEnumerable<uint> GenerateColumn(int x, int z, (int start, int end) heightRange)
    {
        Map.Sample sample = map.GetSample((x, z));
        var offset = (int) sample.Biome.GetOffset((x, z));

        Context context = new()
        {
            WorldHeight = (int) (sample.Height * Height) + offset,
            Dampening = sample.Biome.CalculateDampening(offset)
        };

        for (int y = heightRange.start; y < heightRange.end; y++) yield return GenerateBlock((x, y, z), sample, context);
    }

    /// <inheritdoc />
    public void EmitViews(string path)
    {
        map.EmitViews(path);
    }

    /// <inheritdoc />
    public IMap Map => map;

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

    private uint GenerateBlock(Vector3i position, in Map.Sample sample, in Context context)
    {
        if (position.Y == -World.BlockLimit) return palette.Core;

        int depth = context.WorldHeight - position.Y;

        if (depth < 0) return position.Y <= SeaLevel ? palette.Water : palette.Empty;

        Map.StoneType stoneType = GetStoneType(position, sample);

        return depth >= sample.Biome.GetTotalWidth(context.Dampening) ? palette.GetStone(stoneType) : sample.Biome.GetData(depth, context.Dampening, stoneType, position.Y <= SeaLevel);
    }

    private Map.StoneType GetStoneType(Vector3i position, in Map.Sample sample)
    {
        const int offsetStrength = 1000;

        double angle = transitionNoise.GetNoise(position.X, position.Y, position.Z) * Math.PI;
        Vector2d offset2D = VMath.CreateVectorFromAngle(angle);

        Vector3d offset = new(offset2D.X * sample.BorderStrength.X, y: 0, offset2D.Y * sample.BorderStrength.Y);
        offset *= offsetStrength;

        Vector3i samplingPosition = position + offset.RoundedToInt(MidpointRounding.ToZero);

        return map.GetStoneType(samplingPosition.Xz);
    }

    private record struct Context
    {
        public int WorldHeight { get; init; }

        public Biome.Dampening Dampening { get; init; }
    }
}
