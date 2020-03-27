﻿// <copyright file="BasicBlock.cs" company="VoxelGame">
//     All rights reserved.
// </copyright>
// <author>pershingthesecond</author>
using System;
using VoxelGame.Physics;
using VoxelGame.Rendering;

namespace VoxelGame.Logic
{
    /// <summary>
    /// This class represents a simple block that is completely filled. It is used for basic blocks with no functions that make up most of the world.
    /// </summary>
    public class BasicBlock : Block
    {
#pragma warning disable CA1051 // Do not declare visible instance fields
        protected float[][] sideVertices;

        protected uint[] indices =
        {
            0, 2, 1,
            0, 3, 2
        };

#pragma warning restore CA1051 // Do not declare visible instance fields

        public BasicBlock(string name, bool isOpaque, bool renderFaceAtNonOpaques, (int front, int back, int left, int right, int bottom, int top) sideIndices, bool isSolid) : base(name, true, isOpaque, isSolid, false, false, BoundingBox.Block)
        {
            RenderFaceAtNonOpaques = renderFaceAtNonOpaques;

#pragma warning disable CA2214 // Do not call overridable methods in constructors
            this.Setup(sideIndices);
#pragma warning restore CA2214 // Do not call overridable methods in constructors
        }

        protected virtual void Setup((int front, int back, int left, int right, int bottom, int top) sideIndices)
        {
            int textureIndex = Game.Atlas.GetTextureIndex(Name);

            if (textureIndex == -1)
            {
                throw new Exception($"No texture '{Name}' found!");
            }

            AtlasPosition[] sideUVs =
            {
                Game.Atlas.GetTextureUV(textureIndex + sideIndices.front),
                Game.Atlas.GetTextureUV(textureIndex + sideIndices.back),
                Game.Atlas.GetTextureUV(textureIndex + sideIndices.left),
                Game.Atlas.GetTextureUV(textureIndex + sideIndices.right),
                Game.Atlas.GetTextureUV(textureIndex + sideIndices.bottom),
                Game.Atlas.GetTextureUV(textureIndex + sideIndices.top)
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

        public override uint GetMesh(BlockSide side, ushort data, out float[] vertices, out uint[] indices)
        {
            vertices = sideVertices[(int)side];
            indices = this.indices;

            return 4;
        }

        public override void OnCollision(Entities.PhysicsEntity entity, int x, int y, int z)
        {
            throw new NotImplementedException();
        }
    }
}