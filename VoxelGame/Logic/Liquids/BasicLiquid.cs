// <copyright file="BasicLiquid.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using VoxelGame.Visuals;

namespace VoxelGame.Logic.Liquids
{
    public class BasicLiquid : Liquid
    {
        protected readonly static float[][] vertices = new float[][]
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

        private protected TextureLayout movingLayout;
        private protected TextureLayout staticLayout;

        private protected int[][] movingTex = null!;
        private protected int[][] staticTex = null!;

        private protected uint[] indices = null!;

        public BasicLiquid(string name, string namedId, TextureLayout movingLayout, TextureLayout staticLayout) :
            base(
                name,
                namedId,
                isRendered: true)
        {
            this.movingLayout = movingLayout;
            this.staticLayout = staticLayout;
        }

        protected override void Setup()
        {
            indices = new uint[]
            {
                0, 2, 1,
                0, 3, 2
            };

            movingTex = SetupTex(movingLayout);
            staticTex = SetupTex(staticLayout);

            static int[][] SetupTex(TextureLayout layout)
            {
                return new int[][]
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
        }

        public override uint GetMesh(LiquidLevel level, BlockSide side, int sideHeight, bool isStatic, out float[] vertices, out int[] textureIndices, out uint[] indices, out TintColor tint)
        {
            float start = (sideHeight + 1) * 0.125f;
            float height = ((int)level + 1) * 0.125f;

            switch (side)
            {
                case BlockSide.Front:
                case BlockSide.Back:
                case BlockSide.Left:
                case BlockSide.Right:

                    vertices = new float[32];
                    Array.Copy(BasicLiquid.vertices[(int)side], vertices, 32);
                    vertices[1] = vertices[25] = vertices[4] = vertices[28] = start;
                    vertices[9] = vertices[12] = vertices[17] = vertices[20] = height;

                    break;

                case BlockSide.Bottom:

                    vertices = BasicLiquid.vertices[4];

                    break;

                case BlockSide.Top:

                    vertices = new float[32];
                    Array.Copy(BasicLiquid.vertices[5], vertices, 32);
                    vertices[1] = vertices[9] = vertices[17] = vertices[25] = height;

                    break;

                default:
                    throw new ArgumentException("Only the six sides are valid arguments.", nameof(side));
            }

            textureIndices = isStatic ? staticTex[(int)side] : movingTex[(int)side];

            indices = this.indices;
            tint = TintColor.None;

            return 4;
        }
    }
}