﻿// <copyright file="BasicBlock.cs" company="VoxelGame">
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

        public override uint GetMesh(BlockSide side, byte data, out float[] vertices, out int[] textureIndices, out uint[] indices, out TintColor tint, out bool isAnimated)
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