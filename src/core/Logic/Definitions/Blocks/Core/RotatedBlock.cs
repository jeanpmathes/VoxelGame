// <copyright file="RotatedBlock.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Core.Actors;
using VoxelGame.Core.Logic.Elements;
using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Visuals;
using VoxelGame.Core.Visuals.Meshables;

namespace VoxelGame.Core.Logic.Definitions.Blocks;

/// <summary>
///     A block which can be rotated to be oriented on different axis. The y axis is the default orientation.
///     Data bit usage: <c>----aa</c>
/// </summary>
// a: axis
public class RotatedBlock : BasicBlock, ICombustible
{
    internal RotatedBlock(String name, String namedID, BlockFlags flags, TextureLayout layout) :
        base(
            name,
            namedID,
            flags,
            layout) {}

    /// <summary>
    ///     Get a instance of this block rotated to the given axis.
    /// </summary>
    /// <param name="axis">The axis to rotate to.</param>
    /// <returns>The rotated block.</returns>
    public BlockInstance GetInstance(Axis axis)
    {
        return this.AsInstance((Byte) axis);
    }

    /// <inheritdoc />
    protected override ISimple.MeshData GetMeshData(BlockMeshInfo info)
    {
        Axis axis = ToAxis(info.Data);

        // Check if the texture has to be rotated.
        Boolean isLeftOrRightSide = info.Side is BlockSide.Left or BlockSide.Right;
        Boolean onXAndRotated = axis == Axis.X && !isLeftOrRightSide;
        Boolean onZAndRotated = axis == Axis.Z && isLeftOrRightSide;

        Boolean rotated = onXAndRotated || onZAndRotated;

        return ISimple.CreateData(sideTextureIndices[TranslateSide(info.Side, axis)], rotated);
    }

    /// <inheritdoc />
    protected override void DoPlace(World world, Vector3i position, PhysicsActor? actor)
    {
        world.SetBlock(this.AsInstance((UInt32) (actor?.TargetSide ?? BlockSide.Front).Axis()), position);
    }

    private static Axis ToAxis(UInt32 data)
    {
        return (Axis) (data & 0b00_0011);
    }

    private static BlockSide TranslateSide(BlockSide side, Axis axis) // todo: improve
    {
        var index = (Int32) side;

        index = axis switch
        {
            Axis.X when side != BlockSide.Front && side != BlockSide.Back => 7 - index,
            Axis.Z when side != BlockSide.Left && side != BlockSide.Right => 5 - index,
            _ => index
        };

        return (BlockSide) index;
    }
}
