// <copyright file="OrientedBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using System;
using VoxelGame.Entities;
using VoxelGame.Rendering;
using VoxelGame.Utilities;

namespace VoxelGame.Logic.Blocks
{
    /// <summary>
    /// A block which can be rotated on the y axis.
    /// Data bit usage: <c>---oo</c>
    /// </summary>
    // o = orientation
    public class OrientedBlock : BasicBlock
    {
#pragma warning disable CA1051 // Do not declare visible instance fields
        protected float[][] sideNormals;
        protected int[] texIndices;
#pragma warning restore CA1051 // Do not declare visible instance fields

        public OrientedBlock(string name, TextureLayout layout, bool isOpaque, bool renderFaceAtNonOpaques, bool isSolid) :
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
            float[] vert = sideVertices[(int)side];
            float[] norms = sideNormals[(int)side];
            int tex = texIndices[TranslateIndex(side, (Orientation)(data & 0b0_0011))];

            vertices = new float[]
            {
                vert[0], vert[1],  vert[2], 0f, 0f, norms[0], norms[1], norms[2],
                vert[3], vert[4],  vert[5], 0f, 1f, norms[0], norms[1], norms[2],
                vert[6], vert[7],  vert[8], 1f, 1f, norms[0], norms[1], norms[2],
                vert[9], vert[10], vert[11], 1f, 0f, norms[0], norms[1], norms[2]
            };

            textureIndices = new int[] { tex, tex, tex, tex };
            indices = this.indices;
            tint = TintColor.None;

            return 4;
        }

        public override bool Place(int x, int y, int z, PhysicsEntity entity)
        {
            if (Game.World.GetBlock(x, y, z, out _)?.IsReplaceable == false)
            {
                return false;
            }

            Game.World.SetBlock(this, (byte)entity?.LookingDirection.ToOrientation(), x, y, z);

            return true;
        }

        protected static int TranslateIndex(BlockSide side, Orientation orientation)
        {
            int index = (int)side;

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