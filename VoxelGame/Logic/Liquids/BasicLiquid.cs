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

        private protected uint[] indices = null!;

        public BasicLiquid(string name, string namedId) :
            base(
                name,
                namedId,
                isRendered: true)
        {
        }

        protected override void Setup()
        {
            indices = new uint[]
            {
                0, 2, 1,
                0, 3, 2
            };
        }

        public override uint GetMesh(BlockSide side, LiquidLevel level, bool isStatic, out float[] vertices, out int[] textureIndices, out uint[] indices, out TintColor tint)
        {
            float height = ((int)level + 1) * 0.125f;

            switch (side)
            {
                case BlockSide.Front:
                case BlockSide.Back:
                case BlockSide.Left:
                case BlockSide.Right:

                    vertices = new float[32];
                    Array.Copy(BasicLiquid.vertices[(int)side], vertices, 32);
                    vertices[9] = vertices[12] = vertices[17] = vertices[20] = height;

                    textureIndices = new int[] { 0, 0, 0, 0 };

                    break;

                case BlockSide.Bottom:

                    vertices = BasicLiquid.vertices[4];
                    textureIndices = new int[] { 0, 0, 0, 0 };

                    break;

                case BlockSide.Top:

                    vertices = new float[32];
                    Array.Copy(BasicLiquid.vertices[5], vertices, 32);
                    vertices[1] = vertices[9] = vertices[17] = vertices[25] = height;

                    textureIndices = new int[] { 0, 0, 0, 0 };

                    break;

                default:
                    throw new ArgumentException("Only the six sides are valid arguments.", nameof(side));
            }

            indices = this.indices;
            tint = TintColor.None;

            return 4;
        }
    }
}