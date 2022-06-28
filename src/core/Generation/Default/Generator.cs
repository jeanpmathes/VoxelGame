// <copyright file="Generator.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System.Collections.Generic;
using System.IO;
using OpenTK.Mathematics;
using VoxelGame.Core.Logic;

namespace VoxelGame.Core.Generation.Default;

/// <summary>
///     The default world generator.
/// </summary>
public class Generator : IWorldGenerator
{
    private const int SeaLevel = 0;

    private const string MapBlobName = "default_map";
    private readonly Map map = new();

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

        Initialize();
        Store();
    }

    /// <inheritdoc />
    public IEnumerable<uint> GenerateColumn(int x, int z, (int start, int end) heightRange)
    {
        for (int y = heightRange.start; y < heightRange.end; y++) yield return GenerateBlock((x, y, z));
    }

    private void Initialize()
    {
        using Stream? read = world.GetBlobReader(MapBlobName);
        map.Initialize(read);
    }

    private void Store()
    {
        using Stream? write = world.GetBlobWriter(MapBlobName);
        if (write != null) map.Store(write);
    }

    private uint GenerateBlock(Vector3i position)
    {
        if (position.Y == -World.BlockLimit) return palette.Core;
        if (position.Y <= SeaLevel) return palette.Water;

        return palette.Empty;
    }
}
