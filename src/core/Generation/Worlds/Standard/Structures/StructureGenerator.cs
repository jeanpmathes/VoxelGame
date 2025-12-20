// <copyright file="StructureGenerator.cs" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2025 Jean Patrick Mathes
//      
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
//     
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//     
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <https://www.gnu.org/licenses/>.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Core.Collections;
using VoxelGame.Core.Logic.Sections;
using VoxelGame.Core.Utilities;
using VoxelGame.Toolkit.Collections;
using VoxelGame.Toolkit.Noise;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Core.Generation.Worlds.Standard.Structures;

/// <summary>
///     An instance of a structure generator, containing a specific noise generator.
/// </summary>
public sealed class StructureGenerator : IDisposable
{
    private readonly NoiseGenerator noise;

    /// <summary>
    ///     Creates a new instance of the <see cref="StructureGenerator" /> class.
    /// </summary>
    /// <param name="factory">The noise factory to use.</param>
    /// <param name="definition">The definition of the structure generator.</param>
    public StructureGenerator(NoiseFactory factory, StructureGeneratorDefinition definition)
    {
        Definition = definition;

        noise = factory.CreateNext()
            .WithType(NoiseType.GradientNoise)
            .WithFrequency(definition.Frequency)
            .Build();
    }

    /// <summary>
    ///     The definition of the structure generator.
    /// </summary>
    public StructureGeneratorDefinition Definition { get; }

    #region DISPOSABLE

    /// <inheritdoc />
    public void Dispose()
    {
        noise.Dispose();
    }

    #endregion DISPOSABLE

    /// <summary>
    ///     Attempt to place the structure for a given section, if the structure is present in the section.
    /// </summary>
    /// <param name="section">The section to place the structure in.</param>
    /// <param name="generator">The world generator.</param>
    /// <returns>True if the structure was placed, false otherwise.</returns>
    public void AttemptPlacement(Section section, Generator generator)
    {
        Vector3i extents = Definition.EffectiveSectionExtents;

        for (Int32 dx = -extents.X; dx <= extents.X; dx++)
        for (Int32 dy = -extents.Y; dy <= extents.Y; dy++)
        for (Int32 dz = -extents.Z; dz <= extents.Z; dz++)
        {
            SectionPosition current = section.Position.Offset(dx, dy, dz);

            if (!IsSectionSupportingPlacement(current, generator)) continue;
            if (!IsSectionContainingStructure(current, out Single random)) continue;

            PlaceIn(section, current, random, generator);

            break;
        }
    }

    /// <summary>
    ///     Check if the structure would be placed in a specific section.
    /// </summary>
    /// <param name="section">The position of the section to check.</param>
    /// <param name="generator">The world generator.</param>
    /// <param name="position">The position of the structure if it would be placed.</param>
    /// <returns>True if the structure would be placed, false otherwise.</returns>
    public Boolean CheckPlacement(SectionPosition section, Generator generator, out Vector3i position)
    {
        position = default;

        if (!IsSectionSupportingPlacement(section, generator)) return false;
        if (!IsSectionContainingStructure(section, out Single random)) return false;

        position = section.FirstBlock + DeterminePlacement(section, random, generator).position + Definition.Offset;

        return true;
    }

    private static Boolean FilterSurfaceSection(SectionPosition position, Generator generator)
    {
        Int32 firstHeight = generator.GetGroundHeight(position.FirstBlock);
        Int32 lastHeight = generator.GetGroundHeight(position.LastBlock);

        return position.Contains(position.FirstBlock with {Y = firstHeight}) &&
               position.Contains(position.LastBlock with {Y = lastHeight});
    }

    private static Boolean FilterUndergroundSection(SectionPosition position, Generator generator)
    {
        Int32 firstHeight = generator.GetGroundHeight(position.FirstBlock);
        Int32 lastHeight = generator.GetGroundHeight(position.LastBlock);

        Int32 sectionBlockHeight = position.LastBlock.Y;

        return sectionBlockHeight < firstHeight && sectionBlockHeight < lastHeight;
    }

    private Boolean IsSectionSupportingPlacement(SectionPosition position, Generator generator)
    {
        return Definition.Placement switch
        {
            StructureGeneratorDefinition.Kind.Surface => FilterSurfaceSection(position, generator),
            StructureGeneratorDefinition.Kind.Underground => FilterUndergroundSection(position, generator),
            _ => throw Exceptions.UnsupportedEnumValue(Definition.Placement)
        };
    }

    private Boolean IsSectionContainingStructure(SectionPosition position, out Single random)
    {
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

        Vector3i position = anchor.FirstBlock + positionInSection + Definition.Offset;

        Definition.Structure.PlacePartial(random.GetHashCode(), new SectionGrid(section), position, sectionPosition.FirstBlock, sectionPosition.LastBlock, orientation);
    }

    private static Int32 DetermineSurfacePlacement(SectionPosition section, (Int32 x, Int32 z) column, Generator generator)
    {
        Vector3i position = section.FirstBlock + (column.x, 0, column.z);

        return generator.GetGroundHeight(position) - section.FirstBlock.Y;
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

        position.Y = Definition.Placement switch
        {
            StructureGeneratorDefinition.Kind.Surface => DetermineSurfacePlacement(section, (position.X, position.Z), generator),
            StructureGeneratorDefinition.Kind.Underground => DetermineUndergroundPlacement(randomizer),
            _ => throw Exceptions.UnsupportedEnumValue(Definition.Placement)
        };

        Orientation orientation = randomizer.NextOrientation();

        return (position, orientation);
    }
}
