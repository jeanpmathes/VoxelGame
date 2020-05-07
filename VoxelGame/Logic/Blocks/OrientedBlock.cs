// <copyright file="OrientedBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using System;
using VoxelGame.Entities;
using VoxelGame.Logic.Interfaces;
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
        protected float[][] sideUVs;
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
            AtlasPosition[] uvs =
            {
                Game.Atlas.GetTextureUV(layout.Front),
                Game.Atlas.GetTextureUV(layout.Back),
                Game.Atlas.GetTextureUV(layout.Left),
                Game.Atlas.GetTextureUV(layout.Right),
                Game.Atlas.GetTextureUV(layout.Bottom),
                Game.Atlas.GetTextureUV(layout.Top)
            };

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

            sideUVs = new float[][]
            {
                new float[] // Front face
                {
                    uvs[0].bottomLeftU, uvs[0].bottomLeftV,
                    uvs[0].bottomLeftU, uvs[0].topRightV,
                    uvs[0].topRightU, uvs[0].topRightV,
                    uvs[0].topRightU, uvs[0].bottomLeftV
                },
                new float[] // Back face
                {
                    uvs[1].bottomLeftU, uvs[1].bottomLeftV,
                    uvs[1].bottomLeftU, uvs[1].topRightV,
                    uvs[1].topRightU, uvs[1].topRightV,
                    uvs[1].topRightU, uvs[1].bottomLeftV
                },
                new float[] // Left face
                {
                    uvs[2].bottomLeftU, uvs[2].bottomLeftV,
                    uvs[2].bottomLeftU, uvs[2].topRightV,
                    uvs[2].topRightU, uvs[2].topRightV,
                    uvs[2].topRightU, uvs[2].bottomLeftV
                },
                new float[] // Right face
                {
                    uvs[3].bottomLeftU, uvs[3].bottomLeftV,
                    uvs[3].bottomLeftU, uvs[3].topRightV,
                    uvs[3].topRightU, uvs[3].topRightV,
                    uvs[3].topRightU, uvs[3].bottomLeftV
                },
                new float[] // Bottom face
                {
                    uvs[4].bottomLeftU, uvs[4].bottomLeftV,
                    uvs[4].bottomLeftU, uvs[4].topRightV,
                    uvs[4].topRightU, uvs[4].topRightV,
                    uvs[4].topRightU, uvs[4].bottomLeftV
                },
                new float[] // Top face
                {
                    uvs[5].bottomLeftU, uvs[5].bottomLeftV,
                    uvs[5].bottomLeftU, uvs[5].topRightV,
                    uvs[5].topRightU, uvs[5].topRightV,
                    uvs[5].topRightU, uvs[5].bottomLeftV
                }
            };
        }

        public override uint GetMesh(BlockSide side, byte data, out float[] vertices, out uint[] indices)
        {
            float[] vert = sideVertices[(int)side];
            float[] uv = sideUVs[TranslateIndex(side, (Orientation)(data & 0b0_0011))];

            vertices = new float[]
            {
                vert[0], vert[1],  vert[2],  uv[0], uv[1],
                vert[3], vert[4],  vert[5],  uv[2], uv[3],
                vert[6], vert[7],  vert[8],  uv[4], uv[5],
                vert[9], vert[10], vert[11], uv[6], uv[7],
            };
            indices = this.indices;

            return 4;
        }

        public override bool Place(int x, int y, int z, PhysicsEntity entity)
        {
            if (Game.World.GetBlock(x, y, z, out _)?.IsReplaceable == false)
            {
                return false;
            }

            Game.World.SetBlock(this, (byte)entity.LookingDirection.ToOrientation(), x, y, z);

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
                index = 3 - (index + 2) + (index & 2) * 2; // Flips the index
            }

            return index;
        }
    }
}