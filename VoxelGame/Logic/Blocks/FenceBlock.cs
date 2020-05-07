﻿// <copyright file="FenceBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using OpenTK;
using System;
using VoxelGame.Entities;
using VoxelGame.Logic.Interfaces;
using VoxelGame.Physics;
using VoxelGame.Rendering;

namespace VoxelGame.Logic.Blocks
{
    /// <summary>
    /// This class represents a block which connects to blocks with the <see cref="IFenceConnectable"/> interface.
    /// Data bit usage: <c>-nesw</c>
    /// </summary>
    // n = connected north
    // e = connected east
    // s = connected south
    // w = connected west
    public class FenceBlock : Block, IFenceConnectable
    {
#pragma warning disable CA1051 // Do not declare visible instance fields
        protected float[] postVertices;

        protected float[] northVertices;
        protected float[] eastVertices;
        protected float[] southVertices;
        protected float[] westVertices;

        protected uint[][] indices;

#pragma warning restore CA1051 // Do not declare visible instance fields

        public FenceBlock(string name, string texture) :
            base(
                name: name,
                isFull: false,
                isOpaque: false,
                renderFaceAtNonOpaques: true,
                isSolid: true,
                recieveCollisions: false,
                isTrigger: false,
                isReplaceable: false,
                new BoundingBox(new Vector3(0.5f, 0.5f, 0.5f), new Vector3(0.1875f, 0.5f, 0.1875f)))
        {
#pragma warning disable CA2214 // Do not call overridable methods in constructors
            this.Setup(texture);
#pragma warning restore CA2214 // Do not call overridable methods in constructors
        }

        protected void Setup(string texture)
        {
            AtlasPosition uv = Game.Atlas.GetTextureUV(Game.Atlas.GetTextureIndex(texture));
            float pixelSize = (uv.topRightU - uv.bottomLeftU) / 16f;

            postVertices = new float[]
            {
                // Front
                0.3125f, 0f, 0.6875f, uv.bottomLeftU + (5 * pixelSize), uv.bottomLeftV,
                0.3125f, 1f, 0.6875f, uv.bottomLeftU + (5 * pixelSize), uv.topRightV,
                0.6875f, 1f, 0.6875f, uv.topRightU - (5 * pixelSize), uv.topRightV,
                0.6875f, 0f, 0.6875f, uv.topRightU - (5 * pixelSize), uv.bottomLeftV,
                // Back
                0.6875f, 0f, 0.3125f, uv.bottomLeftU + (5 * pixelSize), uv.bottomLeftV,
                0.6875f, 1f, 0.3125f, uv.bottomLeftU + (5 * pixelSize), uv.topRightV,
                0.3125f, 1f, 0.3125f, uv.topRightU - (5 * pixelSize), uv.topRightV,
                0.3125f, 0f, 0.3125f, uv.topRightU - (5 * pixelSize), uv.bottomLeftV,
                // Left
                0.3125f, 0f, 0.3125f, uv.bottomLeftU + (5 * pixelSize), uv.bottomLeftV,
                0.3125f, 1f, 0.3125f, uv.bottomLeftU + (5 * pixelSize), uv.topRightV,
                0.3125f, 1f, 0.6875f, uv.topRightU - (5 * pixelSize), uv.topRightV,
                0.3125f, 0f, 0.6875f, uv.topRightU - (5 * pixelSize), uv.bottomLeftV,
                // Right
                0.6875f, 0f, 0.6875f, uv.bottomLeftU + (5 * pixelSize), uv.bottomLeftV,
                0.6875f, 1f, 0.6875f, uv.bottomLeftU + (5 * pixelSize), uv.topRightV,
                0.6875f, 1f, 0.3125f, uv.topRightU - (5 * pixelSize), uv.topRightV,
                0.6875f, 0f, 0.3125f, uv.topRightU - (5 * pixelSize), uv.bottomLeftV,
                // Bottom
                0.3125f, 0f, 0.3125f, uv.bottomLeftU + (5 * pixelSize), uv.bottomLeftV + (5 * pixelSize),
                0.3125f, 0f, 0.6875f, uv.bottomLeftU + (5 * pixelSize), uv.topRightV - (5 * pixelSize),
                0.6875f, 0f, 0.6875f, uv.topRightU - (5 * pixelSize), uv.topRightV - (5 * pixelSize),
                0.6875f, 0f, 0.3125f, uv.topRightU - (5 * pixelSize), uv.bottomLeftV + (5 * pixelSize),
                // Top
                0.3125f, 1f, 0.6875f, uv.bottomLeftU + (5 * pixelSize), uv.bottomLeftV + (5 * pixelSize),
                0.3125f, 1f, 0.3125f, uv.bottomLeftU + (5 * pixelSize), uv.topRightV - (5 * pixelSize),
                0.6875f, 1f, 0.3125f, uv.topRightU - (5 * pixelSize), uv.topRightV - (5 * pixelSize),
                0.6875f, 1f, 0.6875f, uv.topRightU - (5 * pixelSize), uv.bottomLeftV + (5 * pixelSize),
            };

            northVertices = new float[]
            {
                // Low extension
                0.375f, 0.125f, 0f, uv.bottomLeftU, uv.bottomLeftV + (2 * pixelSize),
                0.375f, 0.4375f, 0f, uv.bottomLeftU, uv.topRightV - (9 * pixelSize),
                0.375f, 0.4375f, 0.3125f, uv.topRightU - (11 * pixelSize), uv.topRightV - (9 * pixelSize),
                0.375f, 0.125f, 0.3125f, uv.topRightU - (11 * pixelSize), uv.bottomLeftV + (2 * pixelSize),

                0.625f, 0.125f, 0.3125f, uv.bottomLeftU + (11 * pixelSize), uv.bottomLeftV + (2 * pixelSize),
                0.625f, 0.4375f, 0.3125f, uv.bottomLeftU + (11 * pixelSize), uv.topRightV - (9 * pixelSize),
                0.625f, 0.4375f, 0f, uv.topRightU, uv.topRightV - (9 * pixelSize),
                0.625f, 0.125f, 0f, uv.topRightU, uv.bottomLeftV + (2 * pixelSize),

                0.375f, 0.125f, 0f, uv.bottomLeftU + (6 * pixelSize), uv.bottomLeftV + (11 * pixelSize),
                0.375f, 0.125f, 0.3125f, uv.bottomLeftU + (6 * pixelSize), uv.topRightV,
                0.625f, 0.125f, 0.3125f, uv.topRightU - (6 * pixelSize), uv.topRightV,
                0.625f, 0.125f, 0f, uv.topRightU - (6 * pixelSize), uv.bottomLeftV + (11 * pixelSize),

                0.375f, 0.4375f, 0.3125f, uv.bottomLeftU + (6 * pixelSize), uv.bottomLeftV + (11 * pixelSize),
                0.375f, 0.4375f, 0f, uv.bottomLeftU + (6 * pixelSize), uv.topRightV,
                0.625f, 0.4375f, 0f, uv.topRightU - (6 * pixelSize), uv.topRightV,
                0.625f, 0.4375f, 0.3125f, uv.topRightU - (6 * pixelSize), uv.bottomLeftV + (11 * pixelSize),

                // High extension
                0.375f, 0.5625f, 0f, uv.bottomLeftU, uv.bottomLeftV + (9 * pixelSize),
                0.375f, 0.875f, 0f, uv.bottomLeftU, uv.topRightV - (2 * pixelSize),
                0.375f, 0.875f, 0.3125f, uv.topRightU - (11 * pixelSize), uv.topRightV - (2 * pixelSize),
                0.375f, 0.5625f, 0.3125f, uv.topRightU - (11 * pixelSize), uv.bottomLeftV + (9 * pixelSize),

                0.625f, 0.5625f, 0.3125f, uv.bottomLeftU + (11 * pixelSize), uv.bottomLeftV + (9 * pixelSize),
                0.625f, 0.875f, 0.3125f, uv.bottomLeftU + (11 * pixelSize), uv.topRightV - (2 * pixelSize),
                0.625f, 0.875f, 0f, uv.topRightU, uv.topRightV - (2 * pixelSize),
                0.625f, 0.5625f, 0f, uv.topRightU, uv.bottomLeftV + (9 * pixelSize),

                0.375f, 0.5625f, 0f, uv.bottomLeftU + (6 * pixelSize), uv.bottomLeftV + (11 * pixelSize),
                0.375f, 0.5625f, 0.3125f, uv.bottomLeftU + (6 * pixelSize), uv.topRightV,
                0.625f, 0.5625f, 0.3125f, uv.topRightU - (6 * pixelSize), uv.topRightV,
                0.625f, 0.5625f, 0f, uv.topRightU - (6 * pixelSize), uv.bottomLeftV + (11 * pixelSize),

                0.375f, 0.875f, 0.3125f, uv.bottomLeftU + (6 * pixelSize), uv.bottomLeftV + (11 * pixelSize),
                0.375f, 0.875f, 0f, uv.bottomLeftU + (6 * pixelSize), uv.topRightV,
                0.625f, 0.875f, 0f, uv.topRightU - (6 * pixelSize), uv.topRightV,
                0.625f, 0.875f, 0.3125f, uv.topRightU - (6 * pixelSize), uv.bottomLeftV + (11 * pixelSize),
            };

            eastVertices = new float[]
            {
                // Low extension
                0.6875f, 0.125f, 0.625f, uv.bottomLeftU + (11 * pixelSize), uv.bottomLeftV + (2 * pixelSize),
                0.6875f, 0.4375f, 0.625f, uv.bottomLeftU + (11 * pixelSize), uv.topRightV - (9 * pixelSize),
                1f, 0.4375f, 0.625f, uv.topRightU, uv.topRightV - (9 * pixelSize),
                1f, 0.125f, 0.625f, uv.topRightU, uv.bottomLeftV + (2 * pixelSize),

                1f, 0.125f, 0.375f, uv.bottomLeftU, uv.bottomLeftV + (2 * pixelSize),
                1f, 0.4375f, 0.375f, uv.bottomLeftU, uv.topRightV - (9 * pixelSize),
                0.6875f, 0.4375f, 0.375f, uv.topRightU - (11 * pixelSize), uv.topRightV - (9 * pixelSize),
                0.6875f, 0.125f, 0.375f, uv.topRightU - (11 * pixelSize), uv.bottomLeftV + (2 * pixelSize),

                0.6875f, 0.125f, 0.375f, uv.bottomLeftU + (11 * pixelSize), uv.bottomLeftV + (6 * pixelSize),
                0.6875f, 0.125f, 0.625f, uv.bottomLeftU + (11 * pixelSize), uv.topRightV  - (6 * pixelSize),
                1f, 0.125f, 0.625f, uv.topRightU, uv.topRightV  - (6 * pixelSize),
                1f, 0.125f, 0.375f, uv.topRightU, uv.bottomLeftV + (6 * pixelSize),

                0.6875f, 0.4375f, 0.625f, uv.bottomLeftU + (11 * pixelSize), uv.bottomLeftV + (6 * pixelSize),
                0.6875f, 0.4375f, 0.375f, uv.bottomLeftU + (11 * pixelSize), uv.topRightV  - (6 * pixelSize),
                1f, 0.4375f, 0.375f, uv.topRightU, uv.topRightV  - (6 * pixelSize),
                1f, 0.4375f, 0.625f, uv.topRightU, uv.bottomLeftV + (6 * pixelSize),

                // High extension
                0.6875f, 0.5625f, 0.625f, uv.bottomLeftU, uv.bottomLeftV + (9 * pixelSize),
                0.6875f, 0.875f, 0.625f, uv.bottomLeftU, uv.topRightV - (2 * pixelSize),
                1f, 0.875f, 0.625f, uv.topRightU - (11 * pixelSize), uv.topRightV - (2 * pixelSize),
                1f, 0.5625f, 0.625f, uv.topRightU - (11 * pixelSize), uv.bottomLeftV + (9 * pixelSize),

                1f, 0.5625f, 0.375f, uv.bottomLeftU + (11 * pixelSize), uv.bottomLeftV + (9 * pixelSize),
                1f, 0.875f, 0.375f, uv.bottomLeftU + (11 * pixelSize), uv.topRightV - (2 * pixelSize),
                0.6875f, 0.875f, 0.375f, uv.topRightU, uv.topRightV - (2 * pixelSize),
                0.6875f, 0.5625f, 0.375f, uv.topRightU, uv.bottomLeftV + (9 * pixelSize),

                0.6875f, 0.5625f, 0.375f, uv.bottomLeftU + (11 * pixelSize), uv.bottomLeftV + (6 * pixelSize),
                0.6875f, 0.5625f, 0.625f, uv.bottomLeftU + (11 * pixelSize), uv.topRightV  - (6 * pixelSize),
                1f, 0.5625f, 0.625f, uv.topRightU, uv.topRightV  - (6 * pixelSize),
                1f, 0.5625f, 0.375f, uv.topRightU, uv.bottomLeftV + (6 * pixelSize),

                0.6875f, 0.875f, 0.625f, uv.bottomLeftU + (11 * pixelSize), uv.bottomLeftV + (6 * pixelSize),
                0.6875f, 0.875f, 0.375f, uv.bottomLeftU + (11 * pixelSize), uv.topRightV  - (6 * pixelSize),
                1f, 0.875f, 0.375f, uv.topRightU, uv.topRightV  - (6 * pixelSize),
                1f, 0.875f, 0.625f, uv.topRightU, uv.bottomLeftV + (6 * pixelSize),
            };

            southVertices = new float[]
            {
                // Low extension
                0.375f, 0.125f, 0.6875f, uv.bottomLeftU + (11 * pixelSize), uv.bottomLeftV + (2 * pixelSize),
                0.375f, 0.4375f, 0.6875f, uv.bottomLeftU + (11 * pixelSize), uv.topRightV - (9 * pixelSize),
                0.375f, 0.4375f, 1f, uv.topRightU, uv.topRightV - (9 * pixelSize),
                0.375f, 0.125f, 1f, uv.topRightU, uv.bottomLeftV + (2 * pixelSize),

                0.625f, 0.125f, 1f, uv.bottomLeftU, uv.bottomLeftV + (2 * pixelSize),
                0.625f, 0.4375f, 1f, uv.bottomLeftU, uv.topRightV - (9 * pixelSize),
                0.625f, 0.4375f, 0.6875f, uv.topRightU - (11 * pixelSize), uv.topRightV - (9 * pixelSize),
                0.625f, 0.125f, 0.6875f, uv.topRightU - (11 * pixelSize), uv.bottomLeftV + (2 * pixelSize),

                0.375f, 0.125f, 0.6875f, uv.bottomLeftU + (6 * pixelSize), uv.bottomLeftV,
                0.375f, 0.125f, 1f, uv.bottomLeftU + (6 * pixelSize), uv.topRightV - (11 * pixelSize),
                0.625f, 0.125f, 1f, uv.topRightU - (6 * pixelSize), uv.topRightV - (11 * pixelSize),
                0.625f, 0.125f, 0.6875f, uv.topRightU - (6 * pixelSize), uv.bottomLeftV,

                0.375f, 0.4375f, 1f, uv.bottomLeftU + (6 * pixelSize), uv.bottomLeftV,
                0.375f, 0.4375f, 0.6875f, uv.bottomLeftU + (6 * pixelSize), uv.topRightV - (11 * pixelSize),
                0.625f, 0.4375f, 0.6875f, uv.topRightU - (6 * pixelSize), uv.topRightV - (11 * pixelSize),
                0.625f, 0.4375f, 1f, uv.topRightU - (6 * pixelSize), uv.bottomLeftV,

                // High extension
                0.375f, 0.5625f, 0.6875f, uv.bottomLeftU + (11 * pixelSize), uv.bottomLeftV + (9 * pixelSize),
                0.375f, 0.875f, 0.6875f, uv.bottomLeftU + (11 * pixelSize), uv.topRightV - (2 * pixelSize),
                0.375f, 0.875f, 1f, uv.topRightU, uv.topRightV - (2 * pixelSize),
                0.375f, 0.5625f, 1f, uv.topRightU, uv.bottomLeftV + (9 * pixelSize),

                0.625f, 0.5625f, 1f, uv.bottomLeftU, uv.bottomLeftV + (9 * pixelSize),
                0.625f, 0.875f, 1f, uv.bottomLeftU, uv.topRightV - (2 * pixelSize),
                0.625f, 0.875f, 0.6875f, uv.topRightU - (11 * pixelSize), uv.topRightV - (2 * pixelSize),
                0.625f, 0.5625f, 0.6875f, uv.topRightU - (11 * pixelSize), uv.bottomLeftV + (9 * pixelSize),

                0.375f, 0.5625f, 0.6875f, uv.bottomLeftU + (6 * pixelSize), uv.bottomLeftV,
                0.375f, 0.5625f, 1f, uv.bottomLeftU + (6 * pixelSize), uv.topRightV - (11 * pixelSize),
                0.625f, 0.5625f, 1f, uv.topRightU - (6 * pixelSize), uv.topRightV - (11 * pixelSize),
                0.625f, 0.5625f, 0.6875f, uv.topRightU - (6 * pixelSize), uv.bottomLeftV,

                0.375f, 0.875f, 1f, uv.bottomLeftU + (6 * pixelSize), uv.bottomLeftV,
                0.375f, 0.875f, 0.6875f, uv.bottomLeftU + (6 * pixelSize), uv.topRightV - (11 * pixelSize),
                0.625f, 0.875f, 0.6875f, uv.topRightU - (6 * pixelSize), uv.topRightV - (11 * pixelSize),
                0.625f, 0.875f, 1f, uv.topRightU - (6 * pixelSize), uv.bottomLeftV,
            };

            westVertices = new float[]
            {
                // Low extension
                0f, 0.125f, 0.625f, uv.bottomLeftU, uv.bottomLeftV + (2 * pixelSize),
                0f, 0.4375f, 0.625f, uv.bottomLeftU, uv.topRightV - (9 * pixelSize),
                0.3125f, 0.4375f, 0.625f,uv.topRightU - (11 * pixelSize), uv.topRightV - (9 * pixelSize),
                0.3125f, 0.125f, 0.625f, uv.topRightU - (11 * pixelSize), uv.bottomLeftV + (2 * pixelSize),

                0.3125f, 0.125f, 0.375f, uv.bottomLeftU + (11 * pixelSize), uv.bottomLeftV + (2 * pixelSize),
                0.3125f, 0.4375f, 0.375f, uv.bottomLeftU + (11 * pixelSize), uv.topRightV - (9 * pixelSize),
                0f, 0.4375f, 0.375f, uv.topRightU, uv.topRightV - (9 * pixelSize),
                0f, 0.125f, 0.375f, uv.topRightU, uv.bottomLeftV + (2 * pixelSize),

                0f, 0.125f, 0.375f, uv.bottomLeftU, uv.bottomLeftV + (6 * pixelSize),
                0f, 0.125f, 0.625f, uv.bottomLeftU, uv.topRightV - (6 * pixelSize),
                0.3125f, 0.125f, 0.625f, uv.topRightU - (11 * pixelSize), uv.topRightV - (6 * pixelSize),
                0.3125f, 0.125f, 0.375f, uv.topRightU - (11 * pixelSize), uv.bottomLeftV + (6 * pixelSize),

                0f, 0.4375f, 0.625f, uv.bottomLeftU, uv.bottomLeftV + (6 * pixelSize),
                0f, 0.4375f, 0.375f, uv.bottomLeftU, uv.topRightV - (6 * pixelSize),
                0.3125f, 0.4375f, 0.375f, uv.topRightU - (11 * pixelSize), uv.topRightV - (6 * pixelSize),
                0.3125f, 0.4375f, 0.625f, uv.topRightU - (11 * pixelSize), uv.bottomLeftV + (6 * pixelSize),

                // High extension
                0f, 0.5625f, 0.625f, uv.bottomLeftU + (11 * pixelSize), uv.bottomLeftV + (9 * pixelSize),
                0f, 0.875f, 0.625f, uv.bottomLeftU + (11 * pixelSize), uv.topRightV - (2 * pixelSize),
                0.3125f, 0.875f, 0.625f, uv.topRightU, uv.topRightV - (2 * pixelSize),
                0.3125f, 0.5625f, 0.625f, uv.topRightU, uv.bottomLeftV + (9 * pixelSize),

                0.3125f, 0.5625f, 0.375f, uv.bottomLeftU, uv.bottomLeftV + (9 * pixelSize),
                0.3125f, 0.875f, 0.375f, uv.bottomLeftU, uv.topRightV - (2 * pixelSize),
                0f, 0.875f, 0.375f, uv.topRightU - (11 * pixelSize), uv.topRightV - (2 * pixelSize),
                0f, 0.5625f, 0.375f, uv.topRightU - (11 * pixelSize), uv.bottomLeftV + (9 * pixelSize),

                0f, 0.5625f, 0.375f, uv.bottomLeftU, uv.bottomLeftV + (6 * pixelSize),
                0f, 0.5625f, 0.625f, uv.bottomLeftU, uv.topRightV - (6 * pixelSize),
                0.3125f, 0.5625f, 0.625f, uv.topRightU - (11 * pixelSize), uv.topRightV - (6 * pixelSize),
                0.3125f, 0.5625f, 0.375f, uv.topRightU - (11 * pixelSize), uv.bottomLeftV + (6 * pixelSize),

                0f, 0.875f, 0.625f, uv.bottomLeftU, uv.bottomLeftV + (6 * pixelSize),
                0f, 0.875f, 0.375f, uv.bottomLeftU, uv.topRightV - (6 * pixelSize),
                0.3125f, 0.875f, 0.375f, uv.topRightU - (11 * pixelSize), uv.topRightV - (6 * pixelSize),
                0.3125f, 0.875f, 0.625f, uv.topRightU - (11 * pixelSize), uv.bottomLeftV + (6 * pixelSize),
            };

            indices = new uint[5][];
            // Generate indices
            for (int i = 0; i < 5; i++)
            {
                uint[] ind = new uint[36 + (i * 48)];

                for (int f = 0; f < 6 + (i * 8); f++)
                {
                    uint offset = (uint)(f * 4);

                    ind[(f * 6) + 0] = 0 + offset;
                    ind[(f * 6) + 1] = 2 + offset;
                    ind[(f * 6) + 2] = 1 + offset;
                    ind[(f * 6) + 3] = 0 + offset;
                    ind[(f * 6) + 4] = 3 + offset;
                    ind[(f * 6) + 5] = 2 + offset;
                }

                indices[i] = ind;
            }
        }

        public override BoundingBox GetBoundingBox(int x, int y, int z)
        {
            Game.World.GetBlock(x, y, z, out byte data);

            bool north = (data & 0b0_1000) != 0;
            bool east = (data & 0b0_0100) != 0;
            bool south = (data & 0b0_0010) != 0;
            bool west = (data & 0b0_0001) != 0;

            int extensions = (north ? 1 : 0) + (east ? 1 : 0) + (south ? 1 : 0) + (west ? 1 : 0);

            BoundingBox[] children = new BoundingBox[2 * extensions];
            extensions = 0;

            if (north)
            {
                children[extensions] = new BoundingBox(new Vector3(0.5f, 0.28125f, 0.15625f) + new Vector3(x, y, z), new Vector3(0.125f, 0.15625f, 0.15625f));
                children[extensions + 1] = new BoundingBox(new Vector3(0.5f, 0.71875f, 0.15625f) + new Vector3(x, y, z), new Vector3(0.125f, 0.15625f, 0.15625f));
                extensions += 2;
            }

            if (east)
            {
                children[extensions] = new BoundingBox(new Vector3(0.84375f, 0.28125f, 0.5f) + new Vector3(x, y, z), new Vector3(0.15625f, 0.15625f, 0.125f));
                children[extensions + 1] = new BoundingBox(new Vector3(0.84375f, 0.71875f, 0.5f) + new Vector3(x, y, z), new Vector3(0.15625f, 0.15625f, 0.125f));
                extensions += 2;
            }

            if (south)
            {
                children[extensions] = new BoundingBox(new Vector3(0.5f, 0.28125f, 0.84375f) + new Vector3(x, y, z), new Vector3(0.125f, 0.15625f, 0.15625f));
                children[extensions + 1] = new BoundingBox(new Vector3(0.5f, 0.71875f, 0.84375f) + new Vector3(x, y, z), new Vector3(0.125f, 0.15625f, 0.15625f));
                extensions += 2;
            }

            if (west)
            {
                children[extensions] = new BoundingBox(new Vector3(0.15625f, 0.28125f, 0.5f) + new Vector3(x, y, z), new Vector3(0.15625f, 0.15625f, 0.125f));
                children[extensions + 1] = new BoundingBox(new Vector3(0.15625f, 0.71875f, 0.5f) + new Vector3(x, y, z), new Vector3(0.15625f, 0.15625f, 0.125f));
            }

            return new BoundingBox(new Vector3(0.5f, 0.5f, 0.5f) + new Vector3(x, y, z), new Vector3(0.1875f, 0.5f, 0.1875f), children);
        }

        public override bool Place(int x, int y, int z, Entities.PhysicsEntity entity)
        {
            if (Game.World.GetBlock(x, y, z, out _)?.IsReplaceable == false)
            {
                return false;
            }

            byte data = 0;
            // Check the neighboring blocks
            if (Game.World.GetBlock(x, y, z - 1, out _) is IFenceConnectable) // North
                data |= 0b0_1000;
            if (Game.World.GetBlock(x + 1, y, z, out _) is IFenceConnectable) // East
                data |= 0b0_0100;
            if (Game.World.GetBlock(x, y, z + 1, out _) is IFenceConnectable) // South
                data |= 0b0_0010;
            if (Game.World.GetBlock(x - 1, y, z, out _) is IFenceConnectable) // West
                data |= 0b0_0001;

            Game.World.SetBlock(this, data, x, y, z);

            return true;
        }

        public override void BlockUpdate(int x, int y, int z, byte data)
        {
            byte newData = 0;
            // Check the neighboring blocks
            if (Game.World.GetBlock(x, y, z - 1, out _) is IFenceConnectable) // North
                newData |= 0b0_1000;
            if (Game.World.GetBlock(x + 1, y, z, out _) is IFenceConnectable) // East
                newData |= 0b0_0100;
            if (Game.World.GetBlock(x, y, z + 1, out _) is IFenceConnectable) // South
                newData |= 0b0_0010;
            if (Game.World.GetBlock(x - 1, y, z, out _) is IFenceConnectable) // West
                newData |= 0b0_0001;

            if (newData != data)
            {
                Game.World.SetBlock(this, newData, x, y, z);
            }
        }

        public override uint GetMesh(BlockSide side, byte data, out float[] vertices, out uint[] indices)
        {
            bool north = (data & 0b0_1000) != 0;
            bool east = (data & 0b0_0100) != 0;
            bool south = (data & 0b0_0010) != 0;
            bool west = (data & 0b0_0001) != 0;

            int extensions = (north ? 1 : 0) + (east ? 1 : 0) + (south ? 1 : 0) + (west ? 1 : 0);
            uint vertCount = (uint)(24 + (extensions * 32));

            vertices = new float[vertCount * 5];
            indices = this.indices[extensions];

            // Combine the required vertices into one array
            int position = 0;
            Array.Copy(postVertices, 0, vertices, 0, postVertices.Length);
            position += postVertices.Length;

            if (north)
            {
                Array.Copy(northVertices, 0, vertices, position, northVertices.Length);
                position += northVertices.Length;
            }

            if (east)
            {
                Array.Copy(eastVertices, 0, vertices, position, eastVertices.Length);
                position += eastVertices.Length;
            }

            if (south)
            {
                Array.Copy(southVertices, 0, vertices, position, southVertices.Length);
                position += southVertices.Length;
            }

            if (west)
            {
                Array.Copy(westVertices, 0, vertices, position, westVertices.Length);
            }

            return vertCount;
        }

        public override void OnCollision(PhysicsEntity entity, int x, int y, int z)
        {
        }
    }
}