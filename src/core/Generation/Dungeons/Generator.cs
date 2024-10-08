// <copyright file="Generator.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Toolkit.Collections;

namespace VoxelGame.Core.Generation.Dungeons;

/// <summary>
///     Generates dungeons.
/// </summary>
public class Generator
{
    private readonly Parameters parameters;

    /// <summary>
    ///     Create a new dungeon generator.
    /// </summary>
    /// <param name="parameters">The parameters for the generator.</param>
    public Generator(Parameters parameters)
    {
        this.parameters = parameters;
    }

    /// <summary>
    ///     Generate a dungeon.
    /// </summary>
    /// <param name="seed">The seed for the generation.</param>
    public Array2D<Area?> Generate(Int32 seed)
    {
        Random random = new(seed);

        Array2D<Area?> areas = new(parameters.Size);

        Area start = new(AreaCategory.Start);
        Area end = new(AreaCategory.End);

        AreaDeck deck = new([new Area(), new Area(), new Area()]);

        Vector2i position = new(x: 0, parameters.Size / 2);

        areas[position] = start;

        Vector2i direction = new(x: 1, y: 0);

        while (deck.Count > 0)
        {
            position += direction;

            Area current = deck.Draw(random);

            areas[position] = current;
        }

        areas[position] = end;

        return areas;
    }
}
