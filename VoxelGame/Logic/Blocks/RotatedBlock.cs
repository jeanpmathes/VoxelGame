﻿// <copyright file="RotatedBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using System;
using VoxelGame.Entities;
using VoxelGame.Rendering;

namespace VoxelGame.Logic.Blocks
{
    /// <summary>
    /// A block which can be rotated to be oriented on different axis. The y axis is the default orientation.
    /// Data bit usage: <c>---aa</c>
    /// </summary>
    // a = axis
    public class RotatedBlock : BasicBlock
    {
#pragma warning disable CA1051 // Do not declare visible instance fields
        protected float[][] sideNormals;
        protected int[] texIndices;

#pragma warning restore CA1051 // Do not declare visible instance fields

        public RotatedBlock(string name, TextureLayout layout, bool isOpaque, bool renderFaceAtNonOpaques, bool isSolid) :
            base(
                name,
                layout,
                isOpaque,
                renderFaceAtNonOpaques,
                isSolid)
        {
        }

        protected override void Setup(TextureLayout layout)
        {
            sideVertices = new float[][]
            {
                new float[] // Front face
                {
                    0f, 0f, 1f,
                    0f, 1f, 1f,
                    1f, 1f, 1f,
                    1f, 0f, 1f
                },
                new float[] // Back face
                {
                    1f, 0f, 0f,
                    1f, 1f, 0f,
                    0f, 1f, 0f,
                    0f, 0f, 0f
                },
                new float[] // Left face
                {
                    0f, 0f, 0f,
                    0f, 1f, 0f,
                    0f, 1f, 1f,
                    0f, 0f, 1f
                },
                new float[] // Right face
                {
                    1f, 0f, 1f,
                    1f, 1f, 1f,
                    1f, 1f, 0f,
                    1f, 0f, 0f
                },
                new float[] // Bottom face
                {
                    0f, 0f, 0f,
                    0f, 0f, 1f,
                    1f, 0f, 1f,
                    1f, 0f, 0f
                },
                new float[] // Top face
                {
                    0f, 1f, 1f,
                    0f, 1f, 0f,
                    1f, 1f, 0f,
                    1f, 1f, 1f
                }
            };

            sideNormals = new float[][]
            {
                new float[] // Front face
                {
                    0f, 0f, 1f
                },
                new float[] // Back face
                {
                    0f, 0f, -1f
                },
                new float[] // Left face
                {
                    -1f, 0f, 0f
                },
                new float[] // Right face
                {
                    1f, 0f, 0f
                },
                new float[] // Bottom face
                {
                    0f, -1f, 0f
                },
                new float[] // Top face
                {
                    0f, 1f, 0f
                }
            };

            texIndices = new int[]
            {
                layout.Front,
                layout.Back,
                layout.Left,
                layout.Right,
                layout.Bottom,
                layout.Top
            };
        }

        public override uint GetMesh(BlockSide side, byte data, out float[] vertices, out int[] textureIndices, out uint[] indices, out TintColor tint)
        {
            Axis axis = ToAxis(data);

            float[] vert = sideVertices[(int)side];
            float[] norms = sideNormals[(int)side];
            int tex = texIndices[TranslateIndex(side, axis)];

            // Check if the texture has to be rotated
            if ((axis == Axis.X && (side != BlockSide.Left && side != BlockSide.Right)) || (axis == Axis.Z && (side == BlockSide.Left || side == BlockSide.Right)))
            {
                // Texture rotation
                vertices = new float[]
                {
                    vert[0], vert[1],  vert[2], 0f, 1f, norms[0], norms[1], norms[2],
                    vert[3], vert[4],  vert[5], 1f, 1f, norms[0], norms[1], norms[2],
                    vert[6], vert[7],  vert[8], 1f, 0f, norms[0], norms[1], norms[2],
                    vert[9], vert[10], vert[11], 0f, 0f, norms[0], norms[1], norms[2]
                };
            }
            else
            {
                // No texture rotation
                vertices = new float[]
                {
                    vert[0], vert[1],  vert[2], 0f, 0f, norms[0], norms[1], norms[2],
                    vert[3], vert[4],  vert[5], 0f, 1f, norms[0], norms[1], norms[2],
                    vert[6], vert[7],  vert[8], 1f, 1f, norms[0], norms[1], norms[2],
                    vert[9], vert[10], vert[11], 1f, 0f, norms[0], norms[1], norms[2]
                };
            }

            textureIndices = new int[] { tex, tex, tex, tex };
            indices = Array.Empty<uint>();
            tint = TintColor.None;

            return 4;
        }

        public override bool Place(int x, int y, int z, PhysicsEntity entity)
        {
            if (Game.World.GetBlock(x, y, z, out _)?.IsReplaceable != true)
            {
                return false;
            }

            Game.World.SetBlock(this, (byte)ToAxis(entity?.TargetSide ?? BlockSide.Front), x, y, z);

            return true;
        }

        protected enum Axis
        {
            X, // East-West
            Y, // Up-Down
            Z  // North-South
        }

        protected static Axis ToAxis(BlockSide side)
        {
            switch (side)
            {
                case BlockSide.Front:
                case BlockSide.Back:
                    return Axis.Z;

                case BlockSide.Left:
                case BlockSide.Right:
                    return Axis.X;

                case BlockSide.Bottom:
                case BlockSide.Top:
                    return Axis.Y;

                default:
                    throw new ArgumentOutOfRangeException(nameof(side));
            }
        }

        protected static Axis ToAxis(byte data)
        {
            return (Axis)(data & 0b0_0011);
        }

        protected static int TranslateIndex(BlockSide side, Axis axis)
        {
            int index = (int)side;

            if (axis == Axis.X && side != BlockSide.Front && side != BlockSide.Back)
            {
                index = 7 - index;
            }

            if (axis == Axis.Z && side != BlockSide.Left && side != BlockSide.Right)
            {
                index = 5 - index;
            }

            return index;
        }
    }
}