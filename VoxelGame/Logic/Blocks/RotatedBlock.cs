// <copyright file="RotatedBlock.cs" company="VoxelGame">
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
    /// Data bit usage: <c>uuuaa</c>
    /// </summary>
    // u = unused
    // a = axis
    public class RotatedBlock : BasicBlock
    {
        private float[][] sideUVs;

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
            Axis axis = ToAxis(data);

            float[] vert = sideVertices[(int)side];
            float[] uv = sideUVs[TranslateIndex(side, axis)];

            // Check if the texture has to be rotated
            if ((axis == Axis.X && (side != BlockSide.Left && side != BlockSide.Right)) || (axis == Axis.Z && (side == BlockSide.Left || side == BlockSide.Right)))
            {
                // Texture rotation
                vertices = new float[]
                {
                    vert[0], vert[1],  vert[2],  uv[2], uv[3],
                    vert[3], vert[4],  vert[5],  uv[4], uv[5],
                    vert[6], vert[7],  vert[8],  uv[6], uv[7],
                    vert[9], vert[10], vert[11], uv[0], uv[1],
                };
            }
            else
            {
                // No texture rotation
                vertices = new float[]
                {
                    vert[0], vert[1],  vert[2],  uv[0], uv[1],
                    vert[3], vert[4],  vert[5],  uv[2], uv[3],
                    vert[6], vert[7],  vert[8],  uv[4], uv[5],
                    vert[9], vert[10], vert[11], uv[6], uv[7],
                };
            }


            indices = this.indices;

            return 4;
        }

        public override bool Place(int x, int y, int z, PhysicsEntity entity)
        {
            if (Game.World.GetBlock(x, y, z, out _)?.IsReplaceable == false)
            {
                return false;
            }

            Game.World.SetBlock(this, (byte)ToAxis(entity.TargetSide), x, y, z);

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
