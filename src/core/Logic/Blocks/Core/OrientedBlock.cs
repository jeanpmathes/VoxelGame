// <copyright file="OrientedBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Core.Entities;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Blocks
{
    /// <summary>
    ///     A block which can be rotated on the y axis.
    ///     Data bit usage: <c>----oo</c>
    /// </summary>
    // o: orientation
    public class OrientedBlock : BasicBlock
    {
        internal OrientedBlock(string name, string namedId, BlockFlags flags, TextureLayout layout) :
            base(
                name,
                namedId,
                flags,
                layout) {}

        /// <inheritdoc />
        public override BlockMeshData GetMesh(BlockMeshInfo info)
        {
            return BlockMeshData.Basic(
                sideTextureIndices[TranslateIndex(info.Side, (Orientation) (info.Data & 0b00_0011))],
                isTextureRotated: false);
        }

        /// <inheritdoc />
        protected override void DoPlace(World world, Vector3i position, PhysicsEntity? entity)
        {
            world.SetBlock(
                this.AsInstance((uint) (entity?.LookingDirection.ToOrientation() ?? Orientation.North)),
                position);
        }

        private static int TranslateIndex(BlockSide side, Orientation orientation)
        {
            var index = (int) side;

            if (index is < 0 or > 5) throw new ArgumentOutOfRangeException(nameof(side));

            if (side is BlockSide.Bottom or BlockSide.Top) return index;

            if (((int) orientation & 0b01) == 1)
                index = (3 - index * (1 - (index & 2))) % 5; // Rotates the index one step

            if (((int) orientation & 0b10) == 2) index = 3 - (index + 2) + (index & 2) * 2; // Flips the index

            return index;
        }
    }
}
