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

    private readonly World world;

    /// <summary>
    ///     Creates a new default world generator.
    /// </summary>
    /// <param name="world">The world to generate.</param>
    public Generator(World world)
    {
        this.world = world;
        seed = world.Seed;

        Biome.Setup(seed, palette);

        map = new Map(BiomeDistribution.Default);

        Initialize();
        Store();

        logger.LogInformation(Events.WorldGeneration, "Created '{Name}' world generator", nameof(Default));
    }

    /// <inheritdoc />
    public IEnumerable<uint> GenerateColumn(int x, int z, (int start, int end) heightRange)
    {
        Map.Sample sample = map.GetSample((x, z));

        double offset = GetOffset((x, z), sample);
        double height = sample.Height * Height;

        var rawHeight = (int) height;
        var modifiedHeight = (int) (height + offset);
        int effectiveOffset = rawHeight - modifiedHeight;

        Context context = new()
        {
            Map = map,
            Sample = sample,
            WorldHeight = modifiedHeight,
            Dampening = CreateFilledDampening(effectiveOffset, sample)
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

    private static double GetOffset(Vector2i position, in Map.Sample sample)
    {
        return VMath.MixingBilinearInterpolation(
            sample.Biome00.GetOffset(position),
            sample.Biome10.GetOffset(position),
            sample.Biome01.GetOffset(position),
            sample.Biome11.GetOffset(position),
            sample.SpecialBiome.GetOffset(position),
            sample.BlendFactors);
    }

    /// <summary>
    ///     Fill up the dampening to get the first solid layers of all biomes at the same height.
    /// </summary>
    private static Biome.Dampening CreateFilledDampening(int offset, in Map.Sample sample)
    {
        (int a, int b, int c, int d, int e) depths = (
            sample.Biome00.GetDepthToSolid(sample.Biome00.CalculateDampening(offset)),
            sample.Biome10.GetDepthToSolid(sample.Biome10.CalculateDampening(offset)),
            sample.Biome01.GetDepthToSolid(sample.Biome01.CalculateDampening(offset)),
            sample.Biome11.GetDepthToSolid(sample.Biome11.CalculateDampening(offset)),
            sample.SpecialBiome.GetDepthToSolid(sample.SpecialBiome.CalculateDampening(offset)));

        if (depths.a <= depths.b && depths.a <= depths.c && depths.a <= depths.d) depths.a *= 2;
        else if (depths.b <= depths.a && depths.b <= depths.c && depths.b <= depths.d) depths.b *= 2;
        else if (depths.c <= depths.a && depths.c <= depths.b && depths.c <= depths.d) depths.c *= 2;
        else if (depths.d <= depths.a && depths.d <= depths.b && depths.d <= depths.c) depths.d *= 2;
        else depths.e *= 2;

        var targetDepth = (int) VMath.MixingBilinearInterpolation(depths.a, depths.b, depths.c, depths.d, depths.e, sample.BlendFactors);
        Biome.Dampening dampening = sample.ActualBiome.CalculateDampening(offset);

        int fill = targetDepth - sample.ActualBiome.GetDepthToSolid(dampening);
        fill = Math.Max(val1: 0, fill);

        return dampening with {Width = dampening.Width + fill};
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

        Map.StoneType stoneType = context.GetStoneType(position);

        return depth >= context.Biome.GetTotalWidth(context.Dampening) ? palette.GetStone(stoneType) : context.Biome.GetData(depth, context.Dampening, stoneType, position.Y <= SeaLevel);
    }

    private readonly record struct Context
    {
        public int WorldHeight { get; init; }

        public Biome.Dampening Dampening { get; init; }

        public Biome Biome => Sample.ActualBiome;

        public Map.Sample Sample { private get; init; }

        public Map Map { private get; init; }

        public Map.StoneType GetStoneType(Vector3i position)
        {
            return Map.GetStoneType(position, Sample);
        }
    }
}
