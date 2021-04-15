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

        public override BlockMeshData GetMesh(BlockMeshInfo info)
        {
            float[] v = sideVertices[(int)info.Side];

            float[] vertices =
            {
                v[0], v[1], v[2], 0f, 0f, v[5], v[6], v[7],
                v[8], v[9], v[10], 0f, 1f, v[13], v[14], v[15],
                v[16], v[17], v[18], 1f, 1f, v[21], v[22], v[23],
                v[24], v[25], v[26], 1f, 0f, v[29], v[30], v[31]
            };

            return BlockMeshData.Basic(vertices, sideTextureIndices[TranslateIndex(info.Side, (Orientation)(info.Data & 0b00_0011))]);
        }

        protected override void DoPlace(int x, int y, int z, PhysicsEntity? entity)
        {
            Game.World.SetBlock(this, (uint)(entity?.LookingDirection.ToOrientation() ?? Orientation.North), x, y, z);
        }

        private static int TranslateIndex(BlockSide side, Orientation orientation)
        {
            var index = (int)side;

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