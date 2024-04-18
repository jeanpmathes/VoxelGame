// <copyright file="OrientedBlock.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Core.Actors;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Visuals;
using VoxelGame.Core.Visuals.Meshables;

namespace VoxelGame.Core.Logic.Definitions.Blocks;

/// <summary>
///     A block which can be rotated on the y axis.
///     Data bit usage: <c>----oo</c>
/// </summary>
// o: orientation
public class OrientedBlock : BasicBlock
{
    internal OrientedBlock(String name, String namedID, BlockFlags flags, TextureLayout layout) :
        base(
            name,
            namedID,
            flags,
            layout) {}

    /// <inheritdoc />
    protected override void DoPlace(World world, Vector3i position, PhysicsActor? actor)
    {
        world.SetBlock(
            this.AsInstance((UInt32) (actor?.Head.Forward.ToOrientation() ?? Orientation.North)),
            position);
    }

    private static Int32 TranslateIndex(BlockSide side, Orientation orientation)
    {
        var index = (Int32) side;

        if (index is < 0 or > 5) throw new ArgumentOutOfRangeException(nameof(side));

        if (side is BlockSide.Bottom or BlockSide.Top) return index;

        if (((Int32) orientation & 0b01) == 1)
            index = (3 - index * (1 - (index & 2))) % 5; // Rotates the index one step

        if (((Int32) orientation & 0b10) == 2) index = 3 - (index + 2) + (index & 2) * 2; // Flips the index

        return index;
    }

    /// <inheritdoc />
    protected override ISimple.MeshData GetMeshData(BlockMeshInfo info)
    {
        return ISimple.CreateData(
            sideTextureIndices[TranslateIndex(info.Side, (Orientation) (info.Data & 0b00_0011))],
            isTextureRotated: false);
    }
}
