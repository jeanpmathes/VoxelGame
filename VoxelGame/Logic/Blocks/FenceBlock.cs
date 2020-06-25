﻿// <copyright file="FenceBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using OpenToolkit.Mathematics;
using System;
using VoxelGame.Logic.Interfaces;
using VoxelGame.Physics;
using VoxelGame.Rendering;

namespace VoxelGame.Logic.Blocks
{
    /// <summary>
    /// This class represents a block which connects to blocks with the <see cref="IFenceConnectable"/> interface. The texture and indices of the BlockModels are ignored.
    /// Data bit usage: <c>-nesw</c>
    /// </summary>
    // n = connected north
    // e = connected east
    // s = connected south
    // w = connected west
    public class FenceBlock : Block, IFenceConnectable
    {
#pragma warning disable CA1051 // Do not declare visible instance fields
        protected float[] postVertices = null!;

        protected float[] northVertices = null!;
        protected float[] eastVertices = null!;
        protected float[] southVertices = null!;
        protected float[] westVertices = null!;

        protected int[][] textureIndices = null!;

        protected uint[][] indices = null!;

#pragma warning restore CA1051 // Do not declare visible instance fields

        public FenceBlock(string name, string texture, string post, string extension) :
            base(
                name: name,
                isFull: false,
                isOpaque: false,
                renderFaceAtNonOpaques: true,
                isSolid: true,
                recieveCollisions: false,
                isTrigger: false,
                isReplaceable: false,
                new BoundingBox(new Vector3(0.5f, 0.5f, 0.5f), new Vector3(0.1875f, 0.5f, 0.1875f)),
                TargetBuffer.Complex)
        {
#pragma warning disable CA2214 // Do not call overridable methods in constructors
            this.Setup(texture, BlockModel.Load(post), BlockModel.Load(extension));
#pragma warning restore CA2214 // Do not call overridable methods in constructors
        }

        protected void Setup(string texture, BlockModel post, BlockModel extension)
        {
            post.ToData(out postVertices, out _, out _);

            extension.RotateY(0);
            extension.ToData(out northVertices, out _, out _);

            extension.RotateY(1);
            extension.ToData(out eastVertices, out _, out _);

            extension.RotateY(1);
            extension.ToData(out southVertices, out _, out _);

            extension.RotateY(1);
            extension.ToData(out westVertices, out _, out _);

            int tex = Game.BlockTextureArray.GetTextureIndex(texture);

            textureIndices = new int[5][];
            // Generate texture indices
            for (int i = 0; i < 5; i++)
            {
                int[] texInd = new int[post.VertexCount + (i * extension.VertexCount)];

                for (int v = 0; v < texInd.Length; v++)
                {
                    texInd[v] = tex;
                }

                textureIndices[i] = texInd;
            }

            indices = new uint[5][];
            // Generate indices
            for (int i = 0; i < 5; i++)
            {
                uint[] ind = new uint[(post.Quads.Length * 6) + (i * extension.Quads.Length * 6)];

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

        public override bool Place(int x, int y, int z, Entities.PhysicsEntity? entity)
        {
            if (Game.World.GetBlock(x, y, z, out _)?.IsReplaceable != true)
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

        public override uint GetMesh(BlockSide side, byte data, out float[] vertices, out int[] textureIndices, out uint[] indices, out TintColor tint)
        {
            bool north = (data & 0b0_1000) != 0;
            bool east = (data & 0b0_0100) != 0;
            bool south = (data & 0b0_0010) != 0;
            bool west = (data & 0b0_0001) != 0;

            int extensions = (north ? 1 : 0) + (east ? 1 : 0) + (south ? 1 : 0) + (west ? 1 : 0);
            uint vertCount = (uint)(24 + (extensions * 32));

            vertices = new float[vertCount * 8];
            textureIndices = this.textureIndices[extensions];
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

            tint = TintColor.None;

            return vertCount;
        }
    }
}