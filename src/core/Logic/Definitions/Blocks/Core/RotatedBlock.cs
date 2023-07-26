// <copyright file="RotatedBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using OpenTK.Mathematics;
using VoxelGame.Core.Entities;
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
    internal RotatedBlock(string name, string namedId, BlockFlags flags, TextureLayout layout) :
        base(
            name,
            namedId,
            flags,
            layout) {}

    /// <summary>
    ///     Get a instance of this block rotated to the given axis.
    /// </summary>
    /// <param name="axis">The axis to rotate to.</param>
    /// <returns>The rotated block.</returns>
    public BlockInstance GetInstance(Axis axis)
    {
        return this.AsInstance((byte) axis);
    }

    /// <inheritdoc />
    protected override ISimple.MeshData GetMeshData(BlockMeshInfo info)
    {
        Axis axis = ToAxis(info.Data);

        // Check if the texture has to be rotated.
        bool isLeftOrRightSide = info.Side is BlockSide.Left or BlockSide.Right;
        bool onXAndRotated = axis == Axis.X && !isLeftOrRightSide;
        bool onZAndRotated = axis == Axis.Z && isLeftOrRightSide;

        bool rotated = onXAndRotated || onZAndRotated;

        return ISimple.CreateData(sideTextureIndices[TranslateIndex(info.Side, axis)], rotated);
    }

    /// <inheritdoc />
    protected override void DoPlace(World world, Vector3i position, PhysicsEntity? entity)
    {
        world.SetBlock(this.AsInstance((uint) (entity?.TargetSide ?? BlockSide.Front).Axis()), position);
    }

    private static Axis ToAxis(uint data)
    {
        return (Axis) (data & 0b00_0011);
    }

    private static int TranslateIndex(BlockSide side, Axis axis)
    {
        var index = (int) side;

        index = axis switch
        {
            Axis.X when side != BlockSide.Front && side != BlockSide.Back => 7 - index,
            Axis.Z when side != BlockSide.Left && side != BlockSide.Right => 5 - index,
            _ => index
        };

        return index;
    }
}
