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
    /// This class represents a simple block that is completely filled. <see cref="BasicBlock"/>s themselves do not have much function, but the class can be extended easily.
    /// Data bit usage: <c>------</c>
    /// </summary>
    public class BasicBlock : Block, IOverlayTextureProvider
    {
        private protected float[][] sideVertices = null!;
        private protected int[][] sideTextureIndices = null!;

        private protected TextureLayout layout;

        public virtual int TextureIdentifier => layout.Bottom;

        public BasicBlock(string name, string namedId, TextureLayout layout, bool isOpaque = true, bool renderFaceAtNonOpaques = true, bool isSolid = true, bool recieveCollisions = false, bool isTrigger = false, bool isInteractable = false) :
            base(
                name,
                namedId,
                isFull: true,
                isOpaque,
                renderFaceAtNonOpaques,
                isSolid,
                recieveCollisions,
                isTrigger,
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