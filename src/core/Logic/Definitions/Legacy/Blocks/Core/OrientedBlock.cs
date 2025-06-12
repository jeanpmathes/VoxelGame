// <copyright file="OrientedBlock.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Core.Actors;
using VoxelGame.Core.Logic.Elements;
using VoxelGame.Core.Logic.Elements.Legacy;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Visuals;
using VoxelGame.Core.Visuals.Meshables;

namespace VoxelGame.Core.Logic.Definitions.Legacy.Blocks;

/// <summary>
///     A block which can be rotated on the y-axis.
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

    private static Side TranslateSide(Side side, Orientation orientation)
    {
        if (side is Side.Bottom or Side.Top)
            return side;

        if (orientation is Orientation.West or Orientation.East)
            side = side.Rotate(Axis.Y);

        if (orientation is Orientation.South or Orientation.West)
            side = side.Opposite();

        return side;
    }

    /// <inheritdoc />
    protected override ISimple.MeshData GetMeshData(BlockMeshInfo info)
    {
        return ISimple.CreateData(
            sideTextureIndices[TranslateSide(info.Side, (Orientation) (info.Data & 0b00_0011))],
            isTextureRotated: false);
    }
}
