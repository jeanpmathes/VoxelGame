// <copyright file="Structure.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using OpenTK.Mathematics;
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
    ///     Whether the structure can be placed in the current state.
    /// </summary>
    /// <returns>True if the structure can be placed, false otherwise.</returns>
    public abstract bool IsPlaceable { get; }

    /// <summary>
    ///     Get the content of the structure at the given offset.
    /// </summary>
    /// <param name="offset">The offset, must be within the extents.</param>
    /// <returns>The content at the given offset and a bool indicating whether to overwrite blocks. Can be null if the structure does not contain anything at the given offset.</returns>
    protected abstract (Content content, bool overwrite)? GetContent(Vector3i offset);

    /// <summary>
    ///     Pass the structure a seed to generate its content.
    ///     Only some structures need a seed.
    /// </summary>
    /// <param name="seed">The seed.</param>
    public virtual void SetStructureSeed(int seed) {}

    /// <summary>
    ///     Place the structure in a grid at the given position.
    ///     Only call this method if <see cref="IsPlaceable" /> is true.
    /// </summary>
    /// <param name="grid">The grid to place the structure in.</param>
    /// <param name="position">The position to place the structure at.</param>
    /// <param name="orientation">The orientation to place with. Structures are exported with orientation <see cref="Orientation.North"/>.</param>
    public void Place(IGrid grid, Vector3i position, Orientation orientation = Orientation.North)
    {
        Vector3i orientedExtents = GetOrientedExtents(orientation);

        for (var x = 0; x < orientedExtents.X; x++)
        for (var y = 0; y < orientedExtents.Y; y++)
        for (var z = 0; z < orientedExtents.Z; z++)
        {
            PlaceContent(grid, position, orientation, (x, y, z));
        }
    }

    /// <summary>
    ///     Place only a part of the structure in a grid into a given area.
    ///     Only call this method if <see cref="IsPlaceable" /> is true.
    ///     If the area is larger then needed, the rest will be ignored.
    /// </summary>
    /// <param name="grid">The grid to place the structure in.</param>
    /// <param name="position">The position to place the structure at.</param>
    /// <param name="first">The first block of the area to place in.</param>
    /// <param name="last">The last block of the area to place in.</param>
    /// <param name="orientation">The orientation to place with. Structures are exported with orientation <see cref="Orientation.North" />.</param>
    public void PlacePartial(IGrid grid, Vector3i position, Vector3i first, Vector3i last, Orientation orientation = Orientation.North)
    {
        Vector3i orientedExtents = GetOrientedExtents(orientation);

        Vector3i offsetMin = VMath.ClampComponents(first - position, Vector3i.Zero, orientedExtents);
        Vector3i offsetMax = VMath.ClampComponents(last + Vector3i.One - position, Vector3i.Zero, orientedExtents);

        if (offsetMin == offsetMax) return;

        for (int x = offsetMin.X; x < offsetMax.X; x++)
        for (int y = offsetMin.Y; y < offsetMax.Y; y++)
        for (int z = offsetMin.Z; z < offsetMax.Z; z++)
            PlaceContent(grid, position, orientation, (x, y, z));
    }

    private void PlaceContent(IGrid grid, Vector3i position, Orientation orientation, Vector3i orientedOffset)
    {
        (Content content, bool overwrite)? data = GetContent(GetDeOrientedOffset(orientedOffset, orientation));

        if (data is not {content: var content, overwrite: var overwrite}) return;

        Vector3i targetPosition = position + orientedOffset;

        if (!overwrite && grid.GetContent(targetPosition)?.IsReplaceable != true) return;

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
    protected bool IsInExtents(Vector3i offset)
    {
        var isInExtents = true;

        isInExtents &= offset.X >= 0 && offset.X < Extents.X;
        isInExtents &= offset.Y >= 0 && offset.Y < Extents.Y;
        isInExtents &= offset.Z >= 0 && offset.Z < Extents.Z;

        return isInExtents;
    }
}

