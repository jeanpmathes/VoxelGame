// <copyright file="RotatedBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using OpenToolkit.Mathematics;
using VoxelGame.Core.Entities;
using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Blocks
{
    /// <summary>
    ///     A block which can be rotated to be oriented on different axis. The y axis is the default orientation.
    ///     Data bit usage: <c>----aa</c>
    /// </summary>
    // a: axis
    public class RotatedBlock : BasicBlock, IFlammable
    {
        internal RotatedBlock(string name, string namedId, BlockFlags flags, TextureLayout layout) :
            base(
                name,
                namedId,
                flags,
                layout) {}

        /// <inheritdoc />
        public override BlockMeshData GetMesh(BlockMeshInfo info)
        {
            Axis axis = ToAxis(info.Data);

            // Check if the texture has to be rotated.
            bool rotated = axis == Axis.X && info.Side != BlockSide.Left && info.Side != BlockSide.Right ||
                           axis == Axis.Z && info.Side is BlockSide.Left or BlockSide.Right;

            return BlockMeshData.Basic(sideTextureIndices[TranslateIndex(info.Side, axis)], rotated);
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
}
