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
    /// Data bit usage: <c>------</c>
    /// </summary>
    public class AirBlock : Block
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AirBlock"/> class.
        /// </summary>
        /// <param name="name">The unique name of this block</param>
        public AirBlock(string name, string namedId) :
            base(
                name: name,
                namedId,
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

        public override uint GetMesh(BlockSide side, uint data, out float[] vertices, out int[] textureIndices, out uint[] indices, out Visuals.TintColor tint, out bool isAnimated)
        {
            vertices = Array.Empty<float>();
            textureIndices = Array.Empty<int>();
            indices = Array.Empty<uint>();

            tint = Visuals.TintColor.None;
            isAnimated = false;

            return 0;
        }

        protected override bool Place(Entities.PhysicsEntity? entity, int x, int y, int z)
        {
            return false;
        }

        protected override bool Destroy(Entities.PhysicsEntity? entity, int x, int y, int z, uint data)
        {
            return false;
        }
    }
}