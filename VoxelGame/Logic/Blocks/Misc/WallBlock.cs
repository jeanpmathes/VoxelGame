// <copyright file="WallBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using OpenToolkit.Mathematics;
using System;
using VoxelGame.Logic.Interfaces;
using VoxelGame.Physics;
using VoxelGame.Utilities;
using VoxelGame.Visuals;

namespace VoxelGame.Logic.Blocks
{
    /// <summary>
    /// This class represents a wall block which connects to blocks with the <see cref="IConnectable"/> interface. When connecting in a straight line, no post is used and indices are not ignored, else indices are ignored.
    /// Data bit usage: <c>-nesw</c>
    /// </summary>
    // n = connected north
    // e = connected east
    // s = connected south
    // w = connected west
    public class WallBlock : Block, IConnectable
    {
        private protected uint postVertCount;
        private protected uint extensionVertCount;
        private protected uint straightVertCount;

        private protected float[] postVertices = null!;

        private protected float[] northVertices = null!;
        private protected float[] eastVertices = null!;
        private protected float[] southVertices = null!;
        private protected float[] westVertices = null!;

        private protected int[][] textureIndices = null!;
        private protected uint[][] indices = null!;

        private protected float[] extensionStraightZVertices = null!;
        private protected float[] extensionStraightXVertices = null!;

        private protected int[] texIndicesStraight = null!;
        private protected uint[] indicesStraight = null!;

        private protected string texture, post, extension, extensionStraight;

        public WallBlock(string name, string namedId, string texture, string post, string extension, string extensionStraight) :
            base(
                name: name,
                namedId,
                isFull: false,
                isOpaque: false,
                renderFaceAtNonOpaques: true,
                isSolid: true,
                recieveCollisions: false,
                isTrigger: false,
                isReplaceable: false,
                isInteractable: false,
                new BoundingBox(new Vector3(0.5f, 0.5f, 0.5f), new Vector3(0.25f, 0.5f, 0.25f)),
                TargetBuffer.Complex)
        {
            this.texture = texture;
            this.post = post;
            this.extension = extension;
            this.extensionStraight = extensionStraight;
        }

        protected override void Setup()
        {
            BlockModel postModel = BlockModel.Load(this.post);
            BlockModel extensionModel = BlockModel.Load(this.extension);
            BlockModel extensionStraightModel = BlockModel.Load(this.extensionStraight);

            postVertCount = (uint)postModel.VertexCount;
            extensionVertCount = (uint)extensionModel.VertexCount;
            straightVertCount = (uint)extensionStraightModel.VertexCount;

            postModel.ToData(out postVertices, out _, out _);

            extensionModel.RotateY(0, false);
            extensionModel.ToData(out northVertices, out _, out _);

            extensionModel.RotateY(1, false);
            extensionModel.ToData(out eastVertices, out _, out _);

            extensionModel.RotateY(1, false);
            extensionModel.ToData(out southVertices, out _, out _);

            extensionModel.RotateY(1, false);
            extensionModel.ToData(out westVertices, out _, out _);

            int tex = Game.BlockTextureArray.GetTextureIndex(texture);

            textureIndices = new int[5][];

            for (int i = 0; i < 5; i++)
            {
                int[] texInd = new int[postModel.VertexCount + (i * extensionModel.VertexCount)];

                for (int v = 0; v < texInd.Length; v++)
                {
                    texInd[v] = tex;
                }

                textureIndices[i] = texInd;
            }

            indices = new uint[5][];

            for (int i = 0; i < 5; i++)
            {
                uint[] ind = new uint[(postModel.Quads.Length * 6) + (i * extensionModel.Quads.Length * 6)];

                for (int f = 0; f < postModel.Quads.Length + (i * extensionModel.Quads.Length); f++)
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

            extensionStraightModel.RotateY(0, false);
            extensionStraightModel.ToData(out extensionStraightZVertices, out texIndicesStraight, out indicesStraight);

            extensionStraightModel.RotateY(1, false);
            extensionStraightModel.ToData(out extensionStraightXVertices, out _, out _);

            for (int i = 0; i < texIndicesStraight.Length; i++)
            {
                texIndicesStraight[i] = tex;
            }
        }

        protected override BoundingBox GetBoundingBox(int x, int y, int z, byte data)
        {
            bool north = (data & 0b0_1000) != 0;
            bool east = (data & 0b0_0100) != 0;
            bool south = (data & 0b0_0010) != 0;
            bool west = (data & 0b0_0001) != 0;

            bool straightZ = north && south && !east && !west;
            bool straightX = !north && !south && east && west;

            if (straightZ)
            {
                return new BoundingBox(new Vector3(0.5f, 0.46875f, 0.5f) + new Vector3(x, y, z), new Vector3(0.1875f, 0.46875f, 0.5f));
            }
            else if (straightX)
            {
                return new BoundingBox(new Vector3(0.5f, 0.46875f, 0.5f) + new Vector3(x, y, z), new Vector3(0.5f, 0.46875f, 0.1875f));
            }
            else
            {
                int extensions = BitHelper.CountSetBits(data & 0b1111);

                BoundingBox[] children = new BoundingBox[extensions];
                extensions = 0;

                if (north)
                {
                    children[extensions] = new BoundingBox(new Vector3(0.5f, 0.46875f, 0.125f) + new Vector3(x, y, z), new Vector3(0.1875f, 0.46875f, 0.125f));
                    extensions++;
                }

                if (east)
                {
                    children[extensions] = new BoundingBox(new Vector3(0.875f, 0.46875f, 0.5f) + new Vector3(x, y, z), new Vector3(0.125f, 0.46875f, 0.1875f));
                    extensions++;
                }

                if (south)
                {
                    children[extensions] = new BoundingBox(new Vector3(0.5f, 0.46875f, 0.875f) + new Vector3(x, y, z), new Vector3(0.1875f, 0.46875f, 0.125f));
                    extensions++;
                }

                if (west)
                {
                    children[extensions] = new BoundingBox(new Vector3(0.125f, 0.46875f, 0.5f) + new Vector3(x, y, z), new Vector3(0.125f, 0.46875f, 0.1875f));
                }

                return new BoundingBox(new Vector3(0.5f, 0.5f, 0.5f) + new Vector3(x, y, z), new Vector3(0.25f, 0.5f, 0.25f), children);
            }
        }

        public override uint GetMesh(BlockSide side, byte data, out float[] vertices, out int[] textureIndices, out uint[] indices, out TintColor tint, out bool isAnimated)
        {
            bool north = (data & 0b0_1000) != 0;
            bool east = (data & 0b0_0100) != 0;
            bool south = (data & 0b0_0010) != 0;
            bool west = (data & 0b0_0001) != 0;

            bool straightZ = north && south && !east && !west;
            bool straightX = !north && !south && east && west;

            if (straightZ || straightX)
            {
                vertices = straightZ ? extensionStraightZVertices : extensionStraightXVertices;
                textureIndices = texIndicesStraight;
                indices = indicesStraight;

                tint = TintColor.None;
                isAnimated = false;

                return straightVertCount;
            }

            int extensions = BitHelper.CountSetBits(data & 0b1111);
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
            isAnimated = false;

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

            switch (side)
            {
                case BlockSide.Back:

                    newData = CheckNeighbour(x, y, z - 1, BlockSide.Front, 0b0_1000, newData);
                    break;

                case BlockSide.Right:

                    newData = CheckNeighbour(x + 1, y, z, BlockSide.Left, 0b0_0100, newData);
                    break;

                case BlockSide.Front:

                    newData = CheckNeighbour(x, y, z + 1, BlockSide.Back, 0b0_0010, newData);
                    break;

                case BlockSide.Left:

                    newData = CheckNeighbour(x - 1, y, z, BlockSide.Right, 0b0_0001, newData);
                    break;
            }

            if (newData != data)
            {
                Game.World.SetBlock(this, newData, x, y, z);
            }

            static byte CheckNeighbour(int x, int y, int z, BlockSide side, byte mask, byte newData)
            {
                if (Game.World.GetBlock(x, y, z, out _) is IConnectable neighbour && neighbour.IsConnetable(side, x, y, z))
                {
                    newData |= mask;
                }
                else
                {
                    newData = (byte)(newData & ~mask);
                }

                return newData;
            }
        }
    }
}