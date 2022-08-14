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
        int offset = GetOffset((x, z), sample);

        Context context = new()
        {
            Biome = sample.ActualBiome,
            BorderStrength = sample.BorderStrength,
            WorldHeight = (int) (sample.Height * Height) + offset,
            Dampening = sample.ActualBiome.CalculateDampening(offset)
        };

        for (int y = heightRange.start; y < heightRange.end; y++) yield return GenerateBlock((x, y, z), context);
    }

    /// <inheritdoc />
    public void EmitViews(string path)
    {
        map.EmitViews(path);
    }

    /// <inheritdoc />
    public IMap Map => map;

    private static int GetOffset(Vector2i position, in Map.Sample sample)
    {
        double offset = VMath.Blerp(
            sample.Biome00.GetOffset(position),
            sample.Biome10.GetOffset(position),
            sample.Biome01.GetOffset(position),
            sample.Biome11.GetOffset(position),
            sample.BlendX,
            sample.BlendY);

        return (int) Math.Round(offset, MidpointRounding.AwayFromZero);
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

    private uint GenerateBlock(Vector3i position, in Context context)
    {
        if (position.Y == -World.BlockLimit) return palette.Core;

        int depth = context.WorldHeight - position.Y;

        if (depth < 0) return position.Y <= SeaLevel ? palette.Water : palette.Empty;

        Map.StoneType stoneType = GetStoneType(position, context);

        return depth >= context.Biome.GetTotalWidth(context.Dampening) ? palette.GetStone(stoneType) : context.Biome.GetData(depth, context.Dampening, stoneType, position.Y <= SeaLevel);
    }

    private Map.StoneType GetStoneType(Vector3i position, in Context context)
    {
        const int offsetStrength = 1000;

        double angle = transitionNoise.GetNoise(position.X, position.Y, position.Z) * Math.PI;
        Vector2d offset2D = VMath.CreateVectorFromAngle(angle);

        Vector3d offset = new(offset2D.X * context.BorderStrength.X, y: 0, offset2D.Y * context.BorderStrength.Y);
        offset *= offsetStrength;

        Vector3i samplingPosition = position + offset.RoundedToInt(MidpointRounding.ToZero);

        return map.GetStoneType(samplingPosition.Xz);
    }

    private record struct Context
    {
        public int WorldHeight { get; init; }

        public Biome.Dampening Dampening { get; init; }

        public Biome Biome { get; init; }

        public Vector2d BorderStrength { get; init; }
    }
}
