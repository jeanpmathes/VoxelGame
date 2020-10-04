// <copyright file="BasicBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using System;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Blocks
{
    /// <summary>
    /// This class represents a simple block that is completely filled. It is used for basic blocks with no functions that make up most of the world.
    /// Data bit usage: <c>------</c>
    /// </summary>
    public class BasicBlock : Block
    {
        private protected float[][] sideVertices = null!;
        private protected int[][] sideTextureIndices = null!;

        private protected TextureLayout layout;

        public BasicBlock(string name, string namedId, TextureLayout layout, bool isOpaque = true, bool renderFaceAtNonOpaques = true, bool isSolid = true, bool isInteractable = false) :
            base(
                name,
                namedId,
                isFull: true,
                isOpaque,
                renderFaceAtNonOpaques,
                isSolid,
                recieveCollisions: false,
                isTrigger: false,
                isReplaceable: false,
                isInteractable,
                BoundingBox.Block,
                TargetBuffer.Simple)
        {
            this.layout = layout;
        }

        protected override void Setup()
        {
            sideVertices = BlockModel.CubeVertices();

            sideTextureIndices = layout.GetTexIndexArrays();
        }

        public override uint GetMesh(BlockSide side, uint data, Liquid liquid, out float[] vertices, out int[] textureIndices, out uint[] indices, out TintColor tint, out bool isAnimated)
        {
            vertices = sideVertices[(int)side];
            textureIndices = sideTextureIndices[(int)side];
            indices = Array.Empty<uint>();

            tint = TintColor.None;
            isAnimated = false;

            return 4;
        }
    }
}