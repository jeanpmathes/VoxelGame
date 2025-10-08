// <copyright file="Structure.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Core.Logic.Elements;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Core.Logic.Definitions.Structures;

/// <summary>
///     A structure provides position content (Blocks, Fluids) that can be added to the world.
///     Different type of structures are possible, for example static structures that are loaded or dynamically generated
///     structures.
/// </summary>
public abstract class Structure
{
    /// <summary>
    ///     Get the extents of the structure.
    /// </summary>
    public abstract Vector3i Extents { get; }

    /// <summary>
    ///     Get the content of the structure at the given offset.
    /// </summary>
    /// <param name="offset">The offset, must be within the extents.</param>
    /// <param name="random">A random value, or 0 if the structure does not contain randomness.</param>
    /// <returns>
    ///     The content at the given offset and a bool indicating whether to overwrite blocks. Can be null if the
    ///     structure does not contain anything at the given offset.
    /// </returns>
    protected abstract (Content content, Boolean overwrite)? GetContent(Vector3i offset, Single random);

    /// <summary>
    ///     Get a random value generator, or <c>null</c> if the structure does not contain randomness.
    /// </summary>
    /// <param name="seed">The seed to use for the randomness.</param>
    /// <returns>The random number generator, or <c>null</c> if the structure does not contain randomness.</returns>
    protected abstract Random? GetRandomness(Int32 seed);

    /// <summary>
    ///     Place the structure in a grid at the given position.
    /// </summary>
    /// <param name="seed">The seed to use for the structure. Structures may ignore this.</param>
    /// <param name="grid">The grid to place the structure in.</param>
    /// <param name="position">The position to place the structure at.</param>
    /// <param name="orientation">
    ///     The orientation to place with. Structures are exported with orientation
    ///     <see cref="Orientation.North" />.
    /// </param>
    public void Place(Int32 seed, IGrid grid, Vector3i position, Orientation orientation = Orientation.North)
    {
        Random? randomness = GetRandomness(seed);

        Vector3i orientedExtents = GetOrientedExtents(orientation);

        for (var x = 0; x < orientedExtents.X; x++)
        for (var y = 0; y < orientedExtents.Y; y++)
        for (var z = 0; z < orientedExtents.Z; z++)
            PlaceContent(randomness, grid, position, orientation, (x, y, z));
    }

    /// <summary>
    ///     Place only a part of the structure in a grid into a given area.
    ///     If the area is larger than needed, the rest will be ignored.
    /// </summary>
    /// <param name="seed">The seed to use for the structure. Structures may ignore this.</param>
    /// <param name="grid">The grid to place the structure in.</param>
    /// <param name="position">The position to place the structure at.</param>
    /// <param name="first">The first block of the area to place in.</param>
    /// <param name="last">The last block of the area to place in.</param>
    /// <param name="orientation">
    ///     The orientation to place with. Structures are exported with orientation
    ///     <see cref="Orientation.North" />.
    /// </param>
    public void PlacePartial(Int32 seed, IGrid grid, Vector3i position, Vector3i first, Vector3i last, Orientation orientation = Orientation.North)
    {
        Random? randomness = GetRandomness(seed);

        Vector3i orientedExtents = GetOrientedExtents(orientation);

        Vector3i offsetMin = (first - position).ClampComponents(Vector3i.Zero, orientedExtents);
        Vector3i offsetMax = (last + Vector3i.One - position).ClampComponents(Vector3i.Zero, orientedExtents);

        if (offsetMin == offsetMax) return;

        for (Int32 x = offsetMin.X; x < offsetMax.X; x++)
        for (Int32 y = offsetMin.Y; y < offsetMax.Y; y++)
        for (Int32 z = offsetMin.Z; z < offsetMax.Z; z++)
            PlaceContent(randomness, grid, position, orientation, (x, y, z));
    }

    private void PlaceContent(Random? randomness, IGrid grid, Vector3i position, Orientation orientation, Vector3i orientedOffset)
    {
        Single random = randomness?.NextSingle() ?? 0;

        (Content content, Boolean overwrite)? data = GetContent(GetDeOrientedOffset(orientedOffset, orientation), random);

        if (data is not {content: var content, overwrite: var overwrite}) return;

        Vector3i targetPosition = position + orientedOffset;

        if (!overwrite && grid.GetContent(targetPosition)?.IsSettable != true) return;

        grid.SetContent(content, targetPosition);
    }

    private Vector3i GetDeOrientedOffset(Vector3i offset, Orientation orientation)
    {
        return orientation switch
        {
            Orientation.North => offset,
            Orientation.East => new Vector3i(offset.Z, offset.Y, Extents.X - 1 - offset.X),
            Orientation.South => new Vector3i(Extents.X - 1 - offset.X, offset.Y, Extents.Z - 1 - offset.Z),
            Orientation.West => new Vector3i(Extents.Z - 1 - offset.Z, offset.Y, offset.X),
            _ => offset
        };
    }

    private Vector3i GetOrientedExtents(Orientation orientation)
    {
        return orientation switch
        {
            Orientation.North => Extents,
            Orientation.East => Extents.Zyx,
            Orientation.South => Extents,
            Orientation.West => Extents.Zyx,
            _ => Extents
        };
    }

    /// <summary>
    ///     Get whether an offset is within the extents of the structure.
    /// </summary>
    protected Boolean IsInExtents(Vector3i offset)
    {
        var isInExtents = true;

        isInExtents &= offset.X >= 0 && offset.X < Extents.X;
        isInExtents &= offset.Y >= 0 && offset.Y < Extents.Y;
        isInExtents &= offset.Z >= 0 && offset.Z < Extents.Z;

        return isInExtents;
    }
}
