// <copyright file="Cactus.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Mathematics;

namespace VoxelGame.Core.Logic.Definitions.Structures;

/// <summary>
///     A simple cactus.
/// </summary>
public class Cactus : DynamicStructure
{
    /// <inheritdoc />
    public override Vector3i Extents => new(x: 1, y: 3, z: 1);

    /// <inheritdoc />
    protected override (Content content, Boolean overwrite)? GetContent(Vector3i offset)
    {
        return (new Content(Logic.Blocks.Instance.Cactus), true);
    }
}
