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
                isInteractable: false)
        {
        }

        public override uint GetMesh(BlockSide side, uint data, out float[] vertices, out int[] textureIndices, out uint[] indices, out TintColor tint, out bool isAnimated)
        {
            Axis axis = ToAxis(data);

            float[] vert = sideVertices[(int)side];

            // Check if the texture has to be rotated.
            if ((axis == Axis.X && (side != BlockSide.Left && side != BlockSide.Right)) || (axis == Axis.Z && (side == BlockSide.Left || side == BlockSide.Right)))
            {
                // Texture rotation.
                vertices = new float[]
                {
                    vert[0], vert[1], vert[2], 0f, 1f, vert[5], vert[6], vert[7],
                    vert[8], vert[9], vert[10], 1f, 1f, vert[13], vert[14], vert[15],
                    vert[16], vert[17], vert[18], 1f, 0f, vert[21], vert[22], vert[23],
                    vert[24], vert[25], vert[26], 0f, 0f, vert[29], vert[30], vert[31]
                };
            }
            else
            {
                // No texture rotation.
                vertices = new float[]
                {
                    vert[0], vert[1], vert[2], 0f, 0f, vert[5], vert[6], vert[7],
                    vert[8], vert[9], vert[10], 0f, 1f, vert[13], vert[14], vert[15],
                    vert[16], vert[17], vert[18], 1f, 1f, vert[21], vert[22], vert[23],
                    vert[24], vert[25], vert[26], 1f, 0f, vert[29], vert[30], vert[31]
                };
            }

            textureIndices = sideTextureIndices[TranslateIndex(side, axis)];
            indices = Array.Empty<uint>();

            tint = TintColor.None;
            isAnimated = false;

            return 4;
        }

        protected override bool Place(PhysicsEntity? entity, int x, int y, int z)
        {
            Game.World.SetBlock(this, (uint)ToAxis(entity?.TargetSide ?? BlockSide.Front), x, y, z);

            return true;
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
            int index = (int)side;

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