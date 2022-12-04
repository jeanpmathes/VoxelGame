﻿// <copyright file="GeneratedStructure.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using OpenTK.Mathematics;
using VoxelGame.Core.Logic;
using VoxelGame.Core.Logic.Structures;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Core.Generation.Default;

/// <summary>
///     A structure that can be generated in the world.
/// </summary>
public class GeneratedStructure
{
    private readonly Vector3i effectiveSectionExtents;
    private readonly float frequency;
    private readonly Vector3i offset;
    private readonly Structure structure;

    private FastNoiseLite noise = null!;

    /// <summary>
    ///     Creates a new generated structure.
    /// </summary>
    /// <param name="name">The name of the structure.</param>
    /// <param name="structure">The structure to generate.</param>
    /// <param name="rarity">The rarity of the structure. A higher value means less common. Must be greater or equal 0.</param>
    /// <param name="offset">An offset to apply to the structure. Must be less than the size of a section.</param>
    public GeneratedStructure(string name, Structure structure, float rarity, Vector3i offset)
    {
        Debug.Assert(rarity >= 0);
        rarity += 1;

        Name = name;

        this.structure = structure;

        Debug.Assert(Math.Abs(offset.X) < Section.Size);
        Debug.Assert(Math.Abs(offset.Y) < Section.Size);
        Debug.Assert(Math.Abs(offset.Z) < Section.Size);

        this.offset = offset;

        effectiveSectionExtents = (structure.Extents + new Vector3i(offset.Absolute().Xz.MaxComponent())) / Section.Size + Vector3i.One;
        frequency = 1.0f / (effectiveSectionExtents.MaxComponent() * 2 * 2 * rarity);
    }

    /// <summary>
    ///     Get the name of the structure.
    /// </summary>
    public string Name { get; }

    /// <summary>
    ///     Initializes the noise generator.
    /// </summary>
    /// <param name="factory">The noise factory to use.</param>
    public void Setup(NoiseFactory factory)
    {
        noise = factory.GetNextNoise();
        noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
        noise.SetFrequency(frequency);
    }

    /// <summary>
    ///     Attempt to place the structure for a given section, if the structure is present in the section.
    /// </summary>
    /// <param name="section">The section to place the structure in.</param>
    /// <param name="position">The position of the section.</param>
    /// <param name="generator">The world generator.</param>
    /// <returns>True if the structure was placed, false otherwise.</returns>
    public bool AttemptPlacement(Section section, SectionPosition position, Generator generator)
    {
        for (int dx = -effectiveSectionExtents.X; dx <= effectiveSectionExtents.X; dx++)
        for (int dy = -effectiveSectionExtents.Y; dy <= effectiveSectionExtents.Y; dy++)
        for (int dz = -effectiveSectionExtents.Z; dz <= effectiveSectionExtents.Z; dz++)
        {
            SectionPosition current = position.Offset(dx, dy, dz);

            if (!FilterSection(current, generator)) continue;
            if (!CheckSection(current, out float random)) continue;

            PlaceIn(section, position, current, random, generator);

            return true;
        }

        return false;
    }

    private static bool FilterSection(SectionPosition position, Generator generator)
    {
        // Filter out sections that are not at surface level.

        Vector2i firstColumn = position.FirstBlock.Xz;
        Vector2i lastColumn = position.LastBlock.Xz;

        int firstHeight = generator.GetWorldHeight(firstColumn);
        int lastHeight = generator.GetWorldHeight(lastColumn);

        return position.Contains(position.FirstBlock with {Y = firstHeight}) &&
               position.Contains(position.LastBlock with {Y = lastHeight});
    }

    private bool CheckSection(SectionPosition position, out float random)
    {
        // Check if there is a local maxima in the noise at the given position.

        random = noise.GetNoise(position.X, position.Y, position.Z);

        var maxima = true;

        for (int dx = -1; dx <= 1; dx++)
        for (int dy = -1; dy <= 1; dy++)
        for (int dz = -1; dz <= 1; dz++)
        {
            if (dx == 0 && dy == 0 && dz == 0) continue;

            float value = noise.GetNoise(position.X + dx, position.Y + dy, position.Z + dz);

            maxima &= random > value;

            if (!maxima) return false;
        }

        return true;
    }

    private void PlaceIn(Section section, SectionPosition sectionPosition, SectionPosition anchor, float random, Generator generator)
    {
        (Vector3i positionInSection, Orientation orientation) = DeterminePlacement(anchor, random, generator);

        Vector3i position = anchor.FirstBlock + positionInSection + offset;

        structure.SetStructureSeed(random.GetHashCode());
        structure.PlacePartial(new SectionGrid(section, sectionPosition), position, sectionPosition.FirstBlock, sectionPosition.LastBlock, orientation);
    }

    private static (Vector3i position, Orientation orientation) DeterminePlacement(SectionPosition section, float random, Generator generator)
    {
        Random randomizer = new(random.GetHashCode());

        Vector3i position;
        position.X = randomizer.Next(Section.Size);
        position.Z = randomizer.Next(Section.Size);

        Vector2i column = (section.FirstBlock + (position.X, 0, position.Z)).Xz;
        position.Y = Generator.GetWorldHeight(column, generator.Map.GetSample(column), out _) - section.FirstBlock.Y;

        Orientation orientation = randomizer.NextOrientation();

        return (position, orientation);
    }

    /// <summary>
    ///     Search for the structure, starting from a position.
    /// </summary>
    public IEnumerable<Vector3i> Search(Vector3i start, uint maxDistance, Generator generator)
    {
        var maxSectionDistance = (int) Math.Clamp(maxDistance / Section.Size + 1, min: 0, World.SectionLimit);

        for (var d = 0; d < maxSectionDistance; d++)
            foreach (Vector3i position in SearchAtDistance(start, d, generator))
                yield return position;
    }

    private IEnumerable<Vector3i> SearchAtDistance(Vector3i anchor, int distance, Generator generator)
    {
        SectionPosition center = SectionPosition.From(anchor);

        for (int dx = -distance; dx <= distance; dx++)
        for (int dy = -distance; dy <= distance; dy++)
        for (int dz = -distance; dz <= distance; dz++)
            foreach (Vector3i position in SearchInSection(distance, generator, dx, dy, dz, center))
                yield return position;
    }

    private IEnumerable<Vector3i> SearchInSection(int distance, Generator generator, int dx, int dy, int dz, SectionPosition position)
    {
        if (Math.Abs(dx) != distance && Math.Abs(dy) != distance && Math.Abs(dz) != distance) yield break;

        SectionPosition current = position.Offset(dx, dy, dz);

        if (!FilterSection(current, generator)) yield break;
        if (!CheckSection(current, out float random)) yield break;

        yield return current.FirstBlock + DeterminePlacement(current, random, generator).position + offset;
    }
}
