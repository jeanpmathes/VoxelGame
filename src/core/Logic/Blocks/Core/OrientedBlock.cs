// <copyright file="OrientedBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using System;
using VoxelGame.Core.Entities;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Blocks
{
    /// <summary>
    /// A block which can be rotated on the y axis.
    /// Data bit usage: <c>----oo</c>
    /// </summary>
    // o = orientation
    public class OrientedBlock : BasicBlock
    {
        public OrientedBlock(string name, string namedId, TextureLayout layout, bool isOpaque = true, bool renderFaceAtNonOpaques = true, bool isSolid = true) :
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

        public override uint GetMesh(BlockSide side, uint data, Liquid liquid, out float[] vertices, out int[] textureIndices, out uint[] indices, out TintColor tint, out bool isAnimated)
        {
            float[] vert = sideVertices[(int)side];

            vertices = new float[]
            {
                vert[0], vert[1], vert[2], 0f, 0f, vert[5], vert[6], vert[7],
                vert[8], vert[9], vert[10], 0f, 1f, vert[13], vert[14], vert[15],
                vert[16], vert[17], vert[18], 1f, 1f, vert[21], vert[22], vert[23],
                vert[24], vert[25], vert[26], 1f, 0f, vert[29], vert[30], vert[31]
            };

            textureIndices = sideTextureIndices[TranslateIndex(side, (Orientation)(data & 0b00_0011))];
            indices = Array.Empty<uint>();

            tint = TintColor.None;
            isAnimated = false;

            return 4;
        }

        protected override bool Place(PhysicsEntity? entity, int x, int y, int z)
        {
            Game.World.SetBlock(this, (uint)((entity?.LookingDirection.ToOrientation()) ?? Orientation.North), x, y, z);

            return true;
        }

        protected static int TranslateIndex(BlockSide side, Orientation orientation)
        {
            int index = (int)side;

            if (index < 0 || index > 5)
            {
                throw new ArgumentOutOfRangeException(nameof(side));
            }

            if (side == BlockSide.Bottom || side == BlockSide.Top)
            {
                return index;
            }

            if (((int)orientation & 0b01) == 1)
            {
                index = (3 - (index * (1 - (index & 2)))) % 5; // Rotates the index one step
            }

            if (((int)orientation & 0b10) == 2)
            {
                index = 3 - (index + 2) + ((index & 2) * 2); // Flips the index
            }

            return index;
        }
    }
}