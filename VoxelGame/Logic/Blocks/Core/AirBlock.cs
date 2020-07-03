// <copyright file="AirBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using System;
using VoxelGame.Physics;

namespace VoxelGame.Logic.Blocks
{
    /// <summary>
    /// AirBlocks are blocks that have no collision and are not rendered. They are used for the air block that stands for the absence of other blocks.
    /// </summary>
    public class AirBlock : Block
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AirBlock"/> class.
        /// </summary>
        /// <param name="name">The unique name of this block</param>
        public AirBlock(string name) :
            base(
                name: name,
                isFull: false,
                isOpaque: false,
                renderFaceAtNonOpaques: false,
                isSolid: false,
                recieveCollisions: false,
                isTrigger: false,
                isReplaceable: true,
                isInteractable: false,
                BoundingBox.Block,
                Visuals.TargetBuffer.NotRendered)
        {
        }

        public override uint GetMesh(BlockSide side, byte data, out float[] vertices, out int[] textureIndices, out uint[] indicies, out Visuals.TintColor tint)
        {
            vertices = Array.Empty<float>();
            textureIndices = Array.Empty<int>();
            indicies = Array.Empty<uint>();
            tint = Visuals.TintColor.None;

            return 0;
        }

        protected override bool Place(int x, int y, int z, bool? replaceable, Entities.PhysicsEntity? entity)
        {
            return false;
        }

        protected override bool Destroy(int x, int y, int z, byte data, Entities.PhysicsEntity? entity)
        {
            return false;
        }
    }
}