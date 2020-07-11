// <copyright file="FenceBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using OpenToolkit.Mathematics;
using System;
using VoxelGame.Logic.Interfaces;
using VoxelGame.Physics;
using VoxelGame.Visuals;

namespace VoxelGame.Logic.Blocks
{
    /// <summary>
    /// This class represents a block which connects to blocks with the <see cref="IConnectable"/> interface. The texture and indices of the BlockModels are ignored.
    /// Data bit usage: <c>-nesw</c>
    /// </summary>
    // n = connected north
    // e = connected east
    // s = connected south
    // w = connected west
    public class FenceBlock : Block, IConnectable
    {
        private protected uint postVertCount;
        private protected uint extensionVertCount;

        private protected float[] postVertices = null!;

        private protected float[] northVertices = null!;
        private protected float[] eastVertices = null!;
        private protected float[] southVertices = null!;
        private protected float[] westVertices = null!;

        private protected int[][] textureIndices = null!;

        private protected uint[][] indices = null!;

        private protected string texture, post, extension;

        public FenceBlock(string name, string namedId, string texture, string post, string extension) :
            base(
                name,
                namedId,
                isFull: false,
                isOpaque: false,
                renderFaceAtNonOpaques: true,
                isSolid: true,
                recieveCollisions: false,
                isTrigger: false,
                isReplaceable: false,
                isInteractable: false,
                new BoundingBox(new Vector3(0.5f, 0.5f, 0.5f), new Vector3(0.1875f, 0.5f, 0.1875f)),
                TargetBuffer.Complex)
        {
            this.texture = texture;
            this.post = post;
            this.extension = extension;
        }

        protected override void Setup()
        {
            BlockModel post = BlockModel.Load(this.post);
            BlockModel extension = BlockModel.Load(this.extension);

            postVertCount = (uint)post.VertexCount;
            extensionVertCount = (uint)extension.VertexCount;

            post.ToData(out postVertices, out _, out _);

            extension.RotateY(0, false);
            extension.ToData(out northVertices, out _, out _);

            extension.RotateY(1, false);
            extension.ToData(out eastVertices, out _, out _);

            extension.RotateY(1, false);
            extension.ToData(out southVertices, out _, out _);

            extension.RotateY(1, false);
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

                for (int f = 0; f < post.Quads.Length + (i * extension.Quads.Length); f++)
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

        protected override BoundingBox GetBoundingBox(int x, int y, int z, byte data)
        {
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

        public override uint GetMesh(BlockSide side, byte data, out float[] vertices, out int[] textureIndices, out uint[] indices, out TintColor tint)
        {
            bool north = (data & 0b0_1000) != 0;
            bool east = (data & 0b0_0100) != 0;
            bool south = (data & 0b0_0010) != 0;
            bool west = (data & 0b0_0001) != 0;

            int extensions = (north ? 1 : 0) + (east ? 1 : 0) + (south ? 1 : 0) + (west ? 1 : 0);
            uint vertCount = (uint)(postVertCount + (extensions * extensionVertCount));

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

        protected override bool Place(Entities.PhysicsEntity? entity, int x, int y, int z)
        {
            byte data = 0;
            // Check the neighboring blocks
            if (Game.World.GetBlock(x, y, z - 1, out _) is IConnectable north && north.IsConnetable(BlockSide.Front, x, y, z - 1))
                data |= 0b0_1000;
            if (Game.World.GetBlock(x + 1, y, z, out _) is IConnectable east && east.IsConnetable(BlockSide.Left, x + 1, y, z))
                data |= 0b0_0100;
            if (Game.World.GetBlock(x, y, z + 1, out _) is IConnectable south && south.IsConnetable(BlockSide.Back, x, y, z + 1))
                data |= 0b0_0010;
            if (Game.World.GetBlock(x - 1, y, z, out _) is IConnectable west && west.IsConnetable(BlockSide.Right, x - 1, y, z))
                data |= 0b0_0001;

            Game.World.SetBlock(this, data, x, y, z);

            return true;
        }

        internal override void BlockUpdate(int x, int y, int z, byte data, BlockSide side)
        {
            byte newData = data;

            // Check the changed block
            switch (side)
            {
                case BlockSide.Back:

                    if (Game.World.GetBlock(x, y, z - 1, out _) is IConnectable north && north.IsConnetable(BlockSide.Front, x, y, z - 1))
                    {
                        newData |= 0b0_1000;
                    }
                    else
                    {
                        newData &= 0b1_0111;
                    }

                    break;

                case BlockSide.Right:

                    if (Game.World.GetBlock(x + 1, y, z, out _) is IConnectable east && east.IsConnetable(BlockSide.Left, x + 1, y, z))
                    {
                        newData |= 0b0_0100;
                    }
                    else
                    {
                        newData &= 0b1_1011;
                    }

                    break;

                case BlockSide.Front:

                    if (Game.World.GetBlock(x, y, z + 1, out _) is IConnectable south && south.IsConnetable(BlockSide.Back, x, y, z + 1))
                    {
                        newData |= 0b0_0010;
                    }
                    else
                    {
                        newData &= 0b1_1101;
                    }

                    break;

                case BlockSide.Left:

                    if (Game.World.GetBlock(x - 1, y, z, out _) is IConnectable west && west.IsConnetable(BlockSide.Right, x - 1, y, z))
                    {
                        newData |= 0b0_0001;
                    }
                    else
                    {
                        newData &= 0b1_1110;
                    }

                    break;
            }

            if (newData != data)
            {
                Game.World.SetBlock(this, newData, x, y, z);
            }
        }
    }
}