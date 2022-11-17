// <copyright file="Tree.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using OpenTK.Mathematics;
using VoxelGame.Core.Utilities;

namespace VoxelGame.Core.Logic.Structures;

/// <summary>
///     A dynamically created tree structure.
/// </summary>
public class Tree : DynamicStructure
{
    /// <inheritdoc />
    public override Vector3i Extents { get; } = new(x: 5, y: 8, z: 5);

    /// <inheritdoc />
    protected override Content? GetContent(Vector3i offset)
    {
        if (offset is {X: 2, Z: 2} and {Y: >= 0 and < 6})
            return new Content(Block.Specials.Log.GetInstance(Axis.Y), FluidInstance.Default);

        Vector3i crownCenter = new(x: 2, y: 5, z: 2);

        const float radiusSquared = 2.5f * 2.5f;
        float distanceSquared = Vector3.DistanceSquared(offset, crownCenter);

        if (distanceSquared > radiusSquared) return null;

        float closeness = 1 - distanceSquared / radiusSquared;

        if (closeness < 0.25f * Random.NextSingle()) return null;

        return new Content(Block.Leaves);
    }
}
