// <copyright file="BasicBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using System;
using VoxelGame.Physics;
using VoxelGame.Visuals;

namespace VoxelGame.Logic.Blocks
{
    /// <summary>
    /// This class represents a simple block that is completely filled. It is used for basic blocks with no functions that make up most of the world.
    /// </summary>
    public class BasicBlock : Block
    {
        private protected float[][] sideVertices = null!;
        private protected int[][] sideTextureIndices = null!;

        public BasicBlock(string name, TextureLayout layout, bool isOpaque, bool renderFaceAtNonOpaques, bool isSolid) :
            base(
                name: name,
                isFull: true,
                isOpaque,
                renderFaceAtNonOpaques,
                isSolid,
                recieveCollisions: false,
                isTrigger: false,
                isReplaceable: false,
                BoundingBox.Block,
                TargetBuffer.Simple)
        {
#pragma warning disable CA2214 // Do not call overridable methods in constructors
            this.Setup(layout);
#pragma warning restore CA2214 // Do not call overridable methods in constructors
        }

        protected virtual void Setup(TextureLayout layout)
        {
            sideVertices = new float[][]
            {
                new float[] // Front face
                {
                    0f, 0f, 1f, 0f, 0f, 0f, 0f, 1f,
                    0f, 1f, 1f, 0f, 1f, 0f, 0f, 1f,
                    1f, 1f, 1f, 1f, 1f, 0f, 0f, 1f,
                    1f, 0f, 1f, 1f, 0f, 0f, 0f, 1f
                },
                new float[] // Back face
                {
                    1f, 0f, 0f, 0f, 0f, 0f, 0f, -1f,
                    1f, 1f, 0f, 0f, 1f, 0f, 0f, -1f,
                    0f, 1f, 0f, 1f, 1f, 0f, 0f, -1f,
                    0f, 0f, 0f, 1f, 0f, 0f, 0f, -1f
                },
                new float[] // Left face
                {
                    0f, 0f, 0f, 0f, 0f, -1f, 0f, 0f,
                    0f, 1f, 0f, 0f, 1f, -1f, 0f, 0f,
                    0f, 1f, 1f, 1f, 1f, -1f, 0f, 0f,
                    0f, 0f, 1f, 1f, 0f, -1f, 0f, 0f
                },
                new float[] // Right face
                {
                    1f, 0f, 1f, 0f, 0f, 1f, 0f, 0f,
                    1f, 1f, 1f, 0f, 1f, 1f, 0f, 0f,
                    1f, 1f, 0f, 1f, 1f, 1f, 0f, 0f,
                    1f, 0f, 0f, 1f, 0f, 1f, 0f, 0f
                },
                new float[] // Bottom face
                {
                    0f, 0f, 0f, 0f, 0f, 0f, -1f, 0f,
                    0f, 0f, 1f, 0f, 1f, 0f, -1f, 0f,
                    1f, 0f, 1f, 1f, 1f, 0f, -1f, 0f,
                    1f, 0f, 0f, 1f, 0f, 0f, -1f, 0f
                },
                new float[] // Top face
                {
                    0f, 1f, 1f, 0f, 0f, 0f, 1f, 0f,
                    0f, 1f, 0f, 0f, 1f, 0f, 1f, 0f,
                    1f, 1f, 0f, 1f, 1f, 0f, 1f, 0f,
                    1f, 1f, 1f, 1f, 0f, 0f, 1f, 0f
                }
            };

            sideTextureIndices = new int[][]
            {
                new int[]
                {
                    layout.Front, layout.Front, layout.Front, layout.Front
                },
                new int[]
                {
                    layout.Back, layout.Back, layout.Back, layout.Back
                },
                new int[]
                {
                    layout.Left, layout.Left, layout.Left, layout.Left
                },
                new int[]
                {
                    layout.Right, layout.Right, layout.Right, layout.Right
                },
                new int[]
                {
                    layout.Bottom, layout.Bottom, layout.Bottom, layout.Bottom
                },
                new int[]
                {
                    layout.Top, layout.Top, layout.Top, layout.Top
                }
            };
        }

        public override uint GetMesh(BlockSide side, byte data, out float[] vertices, out int[] textureIndices, out uint[] indices, out TintColor tint)
        {
            vertices = sideVertices[(int)side];
            textureIndices = sideTextureIndices[(int)side];
            indices = Array.Empty<uint>();

            tint = TintColor.None;

            return 4;
        }
    }
}