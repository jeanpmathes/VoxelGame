// <copyright file="Generator.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System.Collections.Generic;
using OpenTK.Mathematics;
using VoxelGame.Core.Collections;
using VoxelGame.Core.Logic;

namespace VoxelGame.Core.Generation.Water;

/// <summary>
///     Generates a world made out of water.
/// </summary>
public class Generator : IWorldGenerator
{
    private readonly Content core = new(Block.Core);
    private readonly Content empty = Content.Default;
    private readonly Content water = new(fluid: Fluid.Water);

    private readonly int waterLevel;

    /// <summary>
    ///     Create a new water generator.
    /// </summary>
    /// <param name="waterLevel">The water level (inclusive) below which the world is filled with water.</param>
    public Generator(int waterLevel = 0)
    {
        this.waterLevel = waterLevel;
    }

    /// <inheritdoc />
    public IMap Map { get; } = new Map();

    /// <inheritdoc />
    public IEnumerable<Content> GenerateColumn(int x, int z, (int start, int end) heightRange)
    {
        for (int y = heightRange.start; y < heightRange.end; y++) yield return GenerateContent((x, y, z));
    }

    /// <inheritdoc />
    public void DecorateSection(SectionPosition position, Array3D<Section> sections)
    {
        // No decoration.
    }

    /// <inheritdoc />
    public void EmitViews(string path)
    {
        // No views to emit.
    }

    /// <inheritdoc />
    public void GenerateStructures(Section section, SectionPosition position)
    {
        // No structures to generate.
    }

    /// <inheritdoc />
    public IEnumerable<Vector3i>? SearchNamedGeneratedElements(Vector3i start, string name, uint maxDistance)
    {
        #pragma warning disable S1168 // A null-return indicates that the name is not valid, which is different from not finding anything.
        return null;
        #pragma warning restore S1168
    }

    private Content GenerateContent(Vector3i position)
    {
        if (position.Y == -World.BlockLimit) return core;

        return position.Y <= waterLevel ? water : empty;
    }
}
