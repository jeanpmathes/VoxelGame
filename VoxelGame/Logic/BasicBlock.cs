using System;
using OpenTK;
using OpenTK.Graphics.OpenGL4;

using VoxelGame.Rendering;

namespace VoxelGame.Logic
{
    public class BasicBlock : Block
    {
        protected float[][] sideVertices;
        protected uint[] indicies =
        {
            0, 2, 1,
            0, 3, 2
        };

        public BasicBlock(string name, bool isSolid, Shader shader, Tuple<int, int, int, int, int, int> sideIndices) : base(name, isSolid)
        {
            Setup(shader, sideIndices);
        }

        protected virtual void Setup(Shader shader, Tuple<int, int, int, int, int, int> sideIndices)
        {
            int textureIndex = Game.Atlas.GetTextureIndex(Name);

            if (textureIndex == -1)
            {
                throw new Exception($"No texture '{Name}' found!");
            }

            AtlasPosition[] sideUVs =
            {
                Game.Atlas.GetTextureUV(textureIndex + sideIndices.Item1),
                Game.Atlas.GetTextureUV(textureIndex + sideIndices.Item2),
                Game.Atlas.GetTextureUV(textureIndex + sideIndices.Item3),
                Game.Atlas.GetTextureUV(textureIndex + sideIndices.Item4),
                Game.Atlas.GetTextureUV(textureIndex + sideIndices.Item5),
                Game.Atlas.GetTextureUV(textureIndex + sideIndices.Item6)
            };

            sideVertices = new float[][]
            {
                new float[] // Front face
                {
                    0f, 0f, 1f, sideUVs[0].bottomLeftU, sideUVs[0].bottomLeftV,
                    0f, 1f, 1f, sideUVs[0].bottomLeftU, sideUVs[0].topRightV,
                    1f, 1f, 1f, sideUVs[0].topRightU, sideUVs[0].topRightV,
                    1f, 0f, 1f, sideUVs[0].topRightU, sideUVs[0].bottomLeftV
                },
                new float[] // Back face
                {
                    1f, 0f, 0f, sideUVs[1].bottomLeftU, sideUVs[1].bottomLeftV,
                    1f, 1f, 0f, sideUVs[1].bottomLeftU, sideUVs[1].topRightV,
                    0f, 1f, 0f, sideUVs[1].topRightU, sideUVs[1].topRightV,
                    0f, 0f, 0f, sideUVs[1].topRightU, sideUVs[1].bottomLeftV
                },
                new float[] // Left face
                {
                    0f, 0f, 0f, sideUVs[2].bottomLeftU, sideUVs[2].bottomLeftV,
                    0f, 1f, 0f, sideUVs[2].bottomLeftU, sideUVs[2].topRightV,
                    0f, 1f, 1f, sideUVs[2].topRightU, sideUVs[2].topRightV,
                    0f, 0f, 1f, sideUVs[2].topRightU, sideUVs[2].bottomLeftV
                },
                new float[] // Right face
                {
                    1f, 0f, 1f, sideUVs[3].bottomLeftU, sideUVs[3].bottomLeftV,
                    1f, 1f, 1f, sideUVs[3].bottomLeftU, sideUVs[3].topRightV,
                    1f, 1f, 0f, sideUVs[3].topRightU, sideUVs[3].topRightV,
                    1f, 0f, 0f, sideUVs[3].topRightU, sideUVs[3].bottomLeftV
                },
                new float[] // Bottom top
                {
                    0f, 0f, 0f, sideUVs[4].bottomLeftU, sideUVs[4].bottomLeftV,
                    0f, 0f, 1f, sideUVs[4].bottomLeftU, sideUVs[4].topRightV,
                    1f, 0f, 1f, sideUVs[4].topRightU, sideUVs[4].topRightV,
                    1f, 0f, 0f, sideUVs[4].topRightU, sideUVs[4].bottomLeftV
                },
                new float[] // Top face
                {
                    0f, 1f, 1f, sideUVs[5].bottomLeftU, sideUVs[5].bottomLeftV,
                    0f, 1f, 0f, sideUVs[5].bottomLeftU, sideUVs[5].topRightV,
                    1f, 1f, 0f, sideUVs[5].topRightU, sideUVs[5].topRightV,
                    1f, 1f, 1f, sideUVs[5].topRightU, sideUVs[5].bottomLeftV
                }
            };
        }

        public override uint GetMesh(BlockSide side, out float[] vertecies, out uint[] indicies)
        {
            vertecies = sideVertices[(int)side];
            indicies = this.indicies;

            return 4;
        }
    }
}
