﻿// <copyright file="GeneratedStructure.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using OpenTK.Mathematics;
using VoxelGame.Core.Collections;
using VoxelGame.Core.Logic;
using VoxelGame.Core.Logic.Definitions.Structures;
using VoxelGame.Core.Logic.Sections;
using VoxelGame.Core.Utilities;
using VoxelGame.Toolkit.Collections;
using VoxelGame.Toolkit.Noise;

namespace VoxelGame.Core.Generation.Default;

/// <summary>
///     A structure that can be generated in the world.
/// </summary>
public sealed class GeneratedStructure
{
    /// <summary>
    ///     The kind of the structure. Determines placement and generation.
    /// </summary>
    public enum Kind
    {
        /// <summary>
        ///     A surface structure is placed on the surface of the world.
        /// </summary>
        Surface,

        /// <summary>
        ///     An underground structure is placed below the surface of the world.
        /// </summary>
        Underground
    }

    private readonly Vector3i effectiveSectionExtents;
    private readonly Single frequency;

    private readonly Kind kind;
    private readonly Vector3i offset;
    private readonly Structure structure;

    private NoiseGenerator? noise;

    /// <summary>
    ///     Creates a new generated structure.
    /// </summary>
    /// <param name="name">The name of the structure.</param>
    /// <param name="kind">The kind of the structure.</param>
    /// <param name="structure">The structure to generate.</param>
    /// <param name="rarity">The rarity of the structure. A higher value means less common. Must be greater or equal 0.</param>
    /// <param name="offset">An offset to apply to the structure. Must be less than the size of a section.</param>
    public GeneratedStructure(String name, Kind kind, Structure structure, Single rarity, Vector3i offset)
    {
        Debug.Assert(rarity >= 0);
        rarity += 1;

        Name = name;

        this.kind = kind;
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
    public String Name { get; }

    /// <summary>
    ///     Initializes the noise generator.
    /// </summary>
    /// <param name="factory">The noise factory to use.</param>
    public void SetUpNoise(NoiseFactory factory)
    {
        noise = factory.CreateNext()
            .WithType(NoiseType.OpenSimplex2)
            .WithFrequency(frequency)
            .Build();
    }

    /// <summary>
    ///     Tear down the used noise generator.
    /// </summary>
    public void TearDownNoise()
    {
        noise?.Dispose();
        noise = null;
    }

    /// <summary>
    ///     Attempt to place the structure for a given section, if the structure is present in the section.
    /// </summary>
    /// <param name="section">The section to place the structure in.</param>
    /// <param name="generator">The world generator.</param>
    /// <returns>True if the structure was placed, false otherwise.</returns>
    public void AttemptPlacement(Section section, Generator generator)
    {
        for (Int32 dx = -effectiveSectionExtents.X; dx <= effectiveSectionExtents.X; dx++)
        for (Int32 dy = -effectiveSectionExtents.Y; dy <= effectiveSectionExtents.Y; dy++)
        for (Int32 dz = -effectiveSectionExtents.Z; dz <= effectiveSectionExtents.Z; dz++)
        {
            SectionPosition current = section.Position.Offset(dx, dy, dz);

            if (!FilterSection(current, generator)) continue;
            if (!CheckSection(current, out Single random)) continue;

            PlaceIn(section, current, random, generator);

            break;
        }
    }

    private static Boolean FilterSurfaceSection(SectionPosition position, Generator generator)
    {
        Vector2i firstColumn = position.FirstBlock.Xz;
        Vector2i lastColumn = position.LastBlock.Xz;

        Int32 firstHeight = generator.GetWorldHeight(firstColumn);
        Int32 lastHeight = generator.GetWorldHeight(lastColumn);

        return position.Contains(position.FirstBlock with {Y = firstHeight}) &&
               position.Contains(position.LastBlock with {Y = lastHeight});
    }

    private static Boolean FilterUndergroundSection(SectionPosition position, Generator generator)
    {
        Vector2i firstColumn = position.FirstBlock.Xz;
        Vector2i lastColumn = position.LastBlock.Xz;

        Int32 firstHeight = generator.GetWorldHeight(firstColumn);
        Int32 lastHeight = generator.GetWorldHeight(lastColumn);

        Int32 sectionBlockHeight = position.LastBlock.Y;

        return sectionBlockHeight < firstHeight && sectionBlockHeight < lastHeight;
    }

    private Boolean FilterSection(SectionPosition position, Generator generator)
    {
        return kind switch
        {
            Kind.Surface => FilterSurfaceSection(position, generator),
            Kind.Underground => FilterUndergroundSection(position, generator),
            _ => throw new InvalidOperationException()
        };
    }

    private Boolean CheckSection(SectionPosition position, out Single random)
    {
        Debug.Assert(noise != null);

        // Check if there is a local maxima in the noise at the given position.

        Vector3i anchor = (position.X, position.Y, position.Z) - Vector3i.One;
        Array3D<Single> data = noise.GetNoiseGrid(anchor, size: 3);

        random = data[Neighborhood.Center];

        var maxima = true;

        for (var x = 0; x < 3; x++)
        for (var y = 0; y < 3; y++)
        for (var z = 0; z < 3; z++)
        {
            if ((x, y, z) == Neighborhood.Center) continue;

            maxima &= random > data[x, y, z];

            if (!maxima) return false;
        }

        return true;
    }

    private void PlaceIn(Section section, SectionPosition anchor, Single random, Generator generator)
    {
        SectionPosition sectionPosition = section.Position;

        (Vector3i positionInSection, Orientation orientation) = DeterminePlacement(anchor, random, generator);

        Vector3i position = anchor.FirstBlock + positionInSection + offset;

        structure.SetStructureSeed(random.GetHashCode());
        structure.PlacePartial(new SectionGrid(section), position, sectionPosition.FirstBlock, sectionPosition.LastBlock, orientation);
    }

    private static Int32 DetermineSurfacePlacement(SectionPosition section, (Int32 x, Int32 z) position, Generator generator)
    {
        Vector2i column = (section.FirstBlock + (position.x, 0, position.z)).Xz;

        return Generator.GetWorldHeight(column, generator.Map.GetSample(column), out _) - section.FirstBlock.Y;
    }

    private static Int32 DetermineUndergroundPlacement(Random randomizer)
    {
        return randomizer.Next(Section.Size);
    }

    private (Vector3i position, Orientation orientation) DeterminePlacement(SectionPosition section, Single random, Generator generator)
    {
        Random randomizer = new(random.GetHashCode());

        Vector3i position;
        position.X = randomizer.Next(Section.Size);
        position.Z = randomizer.Next(Section.Size);

        position.Y = kind switch
        {
            Kind.Surface => DetermineSurfacePlacement(section, (position.X, position.Z), generator),
            Kind.Underground => DetermineUndergroundPlacement(randomizer),
            _ => throw new InvalidOperationException()
        };

        Orientation orientation = randomizer.NextOrientation();

        return (position, orientation);
    }

    /// <summary>
    ///     Search for the structure, starting from a position.
    /// </summary>
    public IEnumerable<Vector3i> Search(Vector3i start, UInt32 maxDistance, Generator generator)
    {
        var maxSectionDistance = (Int32) Math.Clamp(maxDistance / Section.Size + 1, min: 0, 2 * World.SectionLimit);

        for (var d = 0; d < maxSectionDistance; d++)
            foreach (Vector3i position in SearchAtDistance(start, d, generator))
                yield return position;
    }

    private IEnumerable<Vector3i> SearchAtSurfaceDistance(Vector3i anchor, Int32 distance, Generator generator)
    {
        SectionPosition center = SectionPosition.From(anchor);

        for (Int32 dx = -distance; dx <= distance; dx++)
        {
            Int32 dz = -distance;

            while (dz <= distance)
            {
                SectionPosition current = center.Offset(dx, y: 0, dz);

                if (!World.IsInLimits(current)) continue;

                current = SectionPosition.From(current.FirstBlock with {Y = generator.GetWorldHeight(current.FirstBlock.Xz)});
                Int32 dy = current.Y - center.Y;

                if (Math.Abs(dx) != distance && Math.Abs(dz) != distance)
                {
                    dz = distance;

                    continue;
                }

                if (SearchInSection(generator, dx, dy, dz, center, out Vector3i found))
                    yield return found;

                dz++;
            }
        }
    }

    private IEnumerable<Vector3i> SearchAtUndergroundDistance(Vector3i anchor, Int32 distance, Generator generator)
    {
        IEnumerable<Vector3i> SearchRow(SectionPosition sectionPosition, Int32 dx, Int32 dy)
        {
            Int32 dz = -distance;

            while (dz <= distance)
            {
                SectionPosition current = sectionPosition.Offset(dx, dy, dz);

                if (!World.IsInLimits(current)) continue;

                if (Math.Abs(dx) != distance && Math.Abs(dy) != distance && Math.Abs(dz) != distance)
                {
                    dz = distance;

                    continue;
                }

                if (SearchInSection(generator, dx, dy, dz, sectionPosition, out Vector3i found))
                    yield return found;

                dz++;
            }
        }

        SectionPosition center = SectionPosition.From(anchor);

        for (Int32 dx = -distance; dx <= distance; dx++)
        for (Int32 dy = -distance; dy <= distance; dy++)
            foreach (Vector3i position in SearchRow(center, dx, dy))
                yield return position;
    }

    private IEnumerable<Vector3i> SearchAtDistance(Vector3i anchor, Int32 distance, Generator generator)
    {
        return kind switch
        {
            Kind.Surface => SearchAtSurfaceDistance(anchor, distance, generator),
            Kind.Underground => SearchAtUndergroundDistance(anchor, distance, generator),
            _ => throw new InvalidOperationException()
        };
    }

    private Boolean FilterSectionByBiome(SectionPosition section, Generator generator)
    {
        ICollection<Biome> biomes = generator.GetSectionBiomes(section, columns: null);

        if (biomes.Count != 1) return false;

        return biomes.First().Structure == this;
    }

    private Boolean SearchInSection(Generator generator, Int32 dx, Int32 dy, Int32 dz, SectionPosition position, out Vector3i found)
    {
        found = default;

        SectionPosition current = position.Offset(dx, dy, dz);

        if (!FilterSectionByBiome(current, generator)) return false;
        if (!FilterSection(current, generator)) return false;
        if (!CheckSection(current, out Single random)) return false;

        found = current.FirstBlock + DeterminePlacement(current, random, generator).position + offset;

        return true;
    }
}
