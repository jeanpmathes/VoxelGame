// <copyright file="Generator.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Linq;
using OpenTK.Mathematics;
using VoxelGame.Core.Utilities;
using VoxelGame.Toolkit.Collections;

namespace VoxelGame.Core.Generation.Dungeons;

/// <summary>
///     Generates dungeons.
/// </summary>
public class Generator
{
    private const Double AreaChance = 0.3;
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

        Area start = new(AreaCategory.Start);
        Area end = new(AreaCategory.End);
        AreaDeck deck = new([new Area(), new Area(), new Area()]);

        AreaGrid areas = new(parameters.Size, start);

        Vector2i direction = new(x: 1, y: 0);
        var completed = false;

        while (!completed)
        {
            if (areas.IsFinal(direction))
            {
                areas.Move(direction, end);
                completed = true;
            }

            if (!areas.CanMove(direction))
                break;

            Area current = deck.Draw(random, AreaChance) ?? new Area(AreaCategory.Corridor);

            areas.Move(direction, current);
        }

        return areas.GetArray2D();
    }

    private sealed class AreaGrid
    {
        private readonly Array2D<Area?> areas;

        private Vector2i position;
        private (Area area, Vector2i position) previous;

        public AreaGrid(Int32 size, Area start)
        {
            areas = new Array2D<Area?>(size);
            position = new Vector2i(x: 0, size / 2);

            areas[position] = start;
            previous = (start, position);
        }

        public Boolean CanMove(Vector2i direction)
        {
            Vector2i destination = position + direction;

            return areas.IsInBounds(destination) && areas[destination] == null;
        }

        public Boolean IsFinal(Vector2i direction)
        {
            Vector2i target = position + direction;

            if (!areas.IsInBounds(target))
                return false;

            return !CanReachFromTarget(Orientation.East);

            return !Orientations.All.Any(CanReachFromTarget);

            Boolean CanReachFromTarget(Orientation orientation)
            {
                Vector2i destination = target + orientation.ToVector2i();

                return destination != position && areas.IsInBounds(destination) && areas[destination] == null;
            }
        }

        public void Move(Vector2i direction, Area area)
        {
            position += direction;
            areas[position] = area;

            Connect(area);
        }

        private void Connect(Area area)
        {
            Vector2i incoming = position - previous.position;
            var orientation = incoming.ToOrientation();

            previous.area.Connect(orientation.Opposite());
            area.Connect(orientation);

            previous = (area, position);
        }

        public Array2D<Area?> GetArray2D()
        {
            return areas;
        }
    }
}
