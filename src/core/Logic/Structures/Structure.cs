﻿// <copyright file="Structure.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using OpenTK.Mathematics;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Core.Logic.Structures;

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
    /// <returns>The content at the given offset. Can be null if the structure does not contain anything at the given offset.</returns>
    public abstract Content? GetContent(Vector3i offset);

    /// <summary>
    ///     Place the structure in the world at the given position.
    /// </summary>
    /// <param name="world">The world to place the structure in.</param>
    /// <param name="position">The position to place the structure at.</param>
    /// <param name="orientation">The orientation to place with. Structures are exported with orientation <see cref="Orientation.North"/>.</param>
    public void Place(World world, Vector3i position, Orientation orientation = Orientation.North)
    {
        for (var x = 0; x < Extents.X; x++)
        for (var y = 0; y < Extents.Y; y++)
        for (var z = 0; z < Extents.Z; z++)
        {
            var offset = new Vector3i(x, y, z);
            Content? content = GetContent(offset);

            if (content == null) continue;

            Vector3i targetPosition = orientation switch
            {
                Orientation.North => position + offset,
                Orientation.East => position + new Vector3i(Extents.Z - 1 - offset.Z, offset.Y, offset.X),
                Orientation.South => position + new Vector3i(Extents.X - 1 - offset.X, offset.Y, Extents.Z - 1 - offset.Z),
                Orientation.West => position + new Vector3i(offset.Z, offset.Y, Extents.X - 1 - offset.X),
                _ => position + offset
            };

            world.SetContent(content.Value, targetPosition);
        }
    }
}