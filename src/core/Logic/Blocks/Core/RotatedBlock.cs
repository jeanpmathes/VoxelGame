// <copyright file="RotatedBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using System;
using VoxelGame.Core.Entities;
using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Blocks
{
    /// <summary>
    /// A block which can be rotated to be oriented on different axis. The y axis is the default orientation.
    /// Data bit usage: <c>----aa</c>
    /// </summary>
    // a = axis
    public class RotatedBlock : BasicBlock, IFlammable
    {
        internal RotatedBlock(string name, string namedId, TextureLayout layout, bool isOpaque = true, bool renderFaceAtNonOpaques = true, bool isSolid = true) :
            base(
                name,
                namedId,
                layout,
                isOpaque,
                renderFaceAtNonOpaques,
                isSolid,
                receiveCollisions: false,
                isTrigger: false,
                isInteractable: false)
        {
        }

        public override BlockMeshData GetMesh(BlockMeshInfo info)
        {
            Axis axis = ToAxis(info.Data);

            // Check if the texture has to be rotated.
            bool rotated = (axis == Axis.X && (info.Side != BlockSide.Left && info.Side != BlockSide.Right)) ||
                           (axis == Axis.Z && (info.Side == BlockSide.Left || info.Side == BlockSide.Right));

            return BlockMeshData.Basic(sideTextureIndices[TranslateIndex(info.Side, axis)], rotated);
        }

        protected override void DoPlace(World world, int x, int y, int z, PhysicsEntity? entity)
        {
            world.SetBlock(this, (uint)ToAxis(entity?.TargetSide ?? BlockSide.Front), x, y, z);
        }

        protected enum Axis
        {
            X, // East-West
            Y, // Up-Down
            Z  // North-South
        }

        protected static Axis ToAxis(BlockSide side)
        {
            switch (side)
            {
                case BlockSide.Front:
                case BlockSide.Back:
                    return Axis.Z;

                case BlockSide.Left:
                case BlockSide.Right:
                    return Axis.X;

                case BlockSide.Bottom:
                case BlockSide.Top:
                    return Axis.Y;

                default:
                    throw new ArgumentOutOfRangeException(nameof(side));
            }
        }

        protected static Axis ToAxis(uint data)
        {
            return (Axis)(data & 0b00_0011);
        }

        protected static int TranslateIndex(BlockSide side, Axis axis)
        {
            var index = (int)side;

            if (axis == Axis.X && side != BlockSide.Front && side != BlockSide.Back)
            {
                index = 7 - index;
            }

            if (axis == Axis.Z && side != BlockSide.Left && side != BlockSide.Right)
            {
                index = 5 - index;
            }

            return index;
        }
    }
}