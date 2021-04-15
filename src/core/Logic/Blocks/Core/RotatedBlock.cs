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
        public RotatedBlock(string name, string namedId, TextureLayout layout, bool isOpaque = true, bool renderFaceAtNonOpaques = true, bool isSolid = true) :
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

            float[] v = sideVertices[(int)info.Side];
            float[] vertices;

            // Check if the texture has to be rotated.
            if ((axis == Axis.X && (info.Side != BlockSide.Left && info.Side != BlockSide.Right)) || (axis == Axis.Z && (info.Side == BlockSide.Left || info.Side == BlockSide.Right)))
            {
                // Texture rotation.
                vertices = new[]
                {
                    v[0], v[1], v[2], 0f, 1f, v[5], v[6], v[7],
                    v[8], v[9], v[10], 1f, 1f, v[13], v[14], v[15],
                    v[16], v[17], v[18], 1f, 0f, v[21], v[22], v[23],
                    v[24], v[25], v[26], 0f, 0f, v[29], v[30], v[31]
                };
            }
            else
            {
                // No texture rotation.
                vertices = new[]
                {
                    v[0], v[1], v[2], 0f, 0f, v[5], v[6], v[7],
                    v[8], v[9], v[10], 0f, 1f, v[13], v[14], v[15],
                    v[16], v[17], v[18], 1f, 1f, v[21], v[22], v[23],
                    v[24], v[25], v[26], 1f, 0f, v[29], v[30], v[31]
                };
            }

            return BlockMeshData.Basic(vertices, sideTextureIndices[TranslateIndex(info.Side, axis)]);
        }

        protected override void DoPlace(int x, int y, int z, PhysicsEntity? entity)
        {
            Game.World.SetBlock(this, (uint)ToAxis(entity?.TargetSide ?? BlockSide.Front), x, y, z);
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