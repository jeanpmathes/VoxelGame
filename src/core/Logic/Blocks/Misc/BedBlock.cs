// <copyright file="BedBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using OpenToolkit.Mathematics;
using VoxelGame.Core.Entities;
using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Blocks
{
    /// <summary>
    /// A block that is two blocks long and allows setting the spawn point.
    /// Data bit usage: <c>cccoop</c>
    /// </summary>
    // c = color
    // o = orientation
    // p = position
    public class BedBlock : Block, IFlammable, IFillable
    {
        private protected float[][] verticesHead = new float[4][];
        private protected float[][] verticesEnd = new float[4][];

        private protected int[] texIndicesHead = null!;
        private protected int[] texIndicesEnd = null!;

        private protected uint[] indicesHead = null!;
        private protected uint[] indicesEnd = null!;

        private protected uint vertexCountHead;
        private protected uint vertexCountEnd;

        private protected string model;

        public BedBlock(string name, string namedId, string model) :
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
                isInteractable: true,
                new BoundingBox(new Vector3(0.5f, 0.21875f, 0.5f), new Vector3(0.5f, 0.21875f, 0.5f)),
                TargetBuffer.Complex)
        {
            this.model = model;
        }

        protected override void Setup()
        {
            BlockModel blockModel = BlockModel.Load(this.model);

            blockModel.PlaneSplit(Vector3.UnitZ, Vector3.UnitZ, out BlockModel top, out BlockModel bottom);
            bottom.Move(-Vector3.UnitZ);

            vertexCountHead = (uint)top.VertexCount;
            vertexCountEnd = (uint)bottom.VertexCount;

            for (int i = 0; i < 4; i++)
            {
                if (i == 0)
                {
                    top.ToData(out verticesHead[i], out texIndicesHead, out indicesHead);
                    bottom.ToData(out verticesEnd[i], out texIndicesEnd, out indicesEnd);
                }
                else
                {
                    top.RotateY(1);
                    top.ToData(out verticesHead[i], out _, out _);

                    bottom.RotateY(1);
                    bottom.ToData(out verticesEnd[i], out _, out _);
                }
            }
        }

        protected override BoundingBox GetBoundingBox(int x, int y, int z, uint data)
        {
            bool isBase = (data & 0b1) == 1;
            Orientation orientation = (Orientation)((data & 0b00_0110) >> 1);

            BoundingBox[] legs = new BoundingBox[2];

            switch (isBase ? orientation : orientation.Invert())
            {
                case Orientation.North:

                    legs[0] = new BoundingBox(new Vector3(0.09375f, 0.09375f, 0.09375f) + new Vector3(x, y, z), new Vector3(0.09375f, 0.09375f, 0.09375f));
                    legs[1] = new BoundingBox(new Vector3(0.90625f, 0.09375f, 0.09375f) + new Vector3(x, y, z), new Vector3(0.09375f, 0.09375f, 0.09375f));

                    break;

                case Orientation.East:

                    legs[0] = new BoundingBox(new Vector3(0.90625f, 0.09375f, 0.09375f) + new Vector3(x, y, z), new Vector3(0.09375f, 0.09375f, 0.09375f));
                    legs[1] = new BoundingBox(new Vector3(0.90625f, 0.09375f, 0.90625f) + new Vector3(x, y, z), new Vector3(0.09375f, 0.09375f, 0.09375f));

                    break;

                case Orientation.South:

                    legs[0] = new BoundingBox(new Vector3(0.09375f, 0.09375f, 0.90625f) + new Vector3(x, y, z), new Vector3(0.09375f, 0.09375f, 0.09375f));
                    legs[1] = new BoundingBox(new Vector3(0.90625f, 0.09375f, 0.90625f) + new Vector3(x, y, z), new Vector3(0.09375f, 0.09375f, 0.09375f));

                    break;

                case Orientation.West:

                    legs[0] = new BoundingBox(new Vector3(0.09375f, 0.09375f, 0.09375f) + new Vector3(x, y, z), new Vector3(0.09375f, 0.09375f, 0.09375f));
                    legs[1] = new BoundingBox(new Vector3(0.09375f, 0.09375f, 0.90625f) + new Vector3(x, y, z), new Vector3(0.09375f, 0.09375f, 0.09375f));

                    break;
            }

            return new BoundingBox(new Vector3(0.5f, 0.3125f, 0.5f) + new Vector3(x, y, z), new Vector3(0.5f, 0.125f, 0.5f), legs);
        }

        public override uint GetMesh(BlockSide side, uint data, out float[] vertices, out int[] textureIndices, out uint[] indices, out TintColor tint, out bool isAnimated)
        {
            bool isHead = (data & 0b1) == 1;
            int orientation = (int)((data & 0b00_0110) >> 1);
            BlockColor color = (BlockColor)((data & 0b11_1000) >> 3);

            if (isHead)
            {
                vertices = verticesHead[orientation];
                textureIndices = texIndicesHead;
                indices = indicesHead;

                tint = color.ToTintColor();
                isAnimated = false;

                return vertexCountHead;
            }
            else
            {
                vertices = verticesEnd[orientation];
                textureIndices = texIndicesEnd;
                indices = indicesEnd;

                tint = color.ToTintColor();
                isAnimated = false;

                return vertexCountEnd;
            }
        }

        protected override bool Place(PhysicsEntity? entity, int x, int y, int z)
        {
            if (Game.World.GetBlock(x, y - 1, z, out _)?.IsSolidAndFull != true)
            {
                return false;
            }

            switch (entity?.LookingDirection.ToOrientation() ?? Orientation.North)
            {
                case Orientation.North:

                    if (Game.World.GetBlock(x, y, z - 1, out _)?.IsReplaceable != true || Game.World.GetBlock(x, y - 1, z - 1, out _)?.IsSolidAndFull != true)
                    {
                        return false;
                    }

                    Game.World.SetBlock(this, (int)Orientation.North << 1, x, y, z);
                    Game.World.SetBlock(this, ((int)Orientation.North << 1) | 1, x, y, z - 1);

                    Game.World.SetSpawnPosition(new Vector3(x, 1024f, z));

                    return true;

                case Orientation.East:

                    if (Game.World.GetBlock(x + 1, y, z, out _)?.IsReplaceable != true || Game.World.GetBlock(x + 1, y - 1, z, out _)?.IsSolidAndFull != true)
                    {
                        return false;
                    }

                    Game.World.SetBlock(this, (int)Orientation.East << 1, x, y, z);
                    Game.World.SetBlock(this, ((int)Orientation.East << 1) | 1, x + 1, y, z);

                    Game.World.SetSpawnPosition(new Vector3(x, 1024f, z));

                    return true;

                case Orientation.South:

                    if (Game.World.GetBlock(x, y, z + 1, out _)?.IsReplaceable != true || Game.World.GetBlock(x, y - 1, z + 1, out _)?.IsSolidAndFull != true)
                    {
                        return false;
                    }

                    Game.World.SetBlock(this, (int)Orientation.South << 1, x, y, z);
                    Game.World.SetBlock(this, ((int)Orientation.South << 1) | 1, x, y, z + 1);

                    Game.World.SetSpawnPosition(new Vector3(x, 1024f, z));

                    return true;

                case Orientation.West:

                    if (Game.World.GetBlock(x - 1, y, z, out _)?.IsReplaceable != true || Game.World.GetBlock(x - 1, y - 1, z, out _)?.IsSolidAndFull != true)
                    {
                        return false;
                    }

                    Game.World.SetBlock(this, (int)Orientation.West << 1, x, y, z);
                    Game.World.SetBlock(this, ((int)Orientation.West << 1) | 1, x - 1, y, z);

                    Game.World.SetSpawnPosition(new Vector3(x, 1024f, z));

                    return true;

                default:
                    return false;
            }
        }

        protected override bool Destroy(PhysicsEntity? entity, int x, int y, int z, uint data)
        {
            bool isHead = (data & 0b1) == 1;

            switch ((Orientation)((data & 0b00_0110) >> 1))
            {
                case Orientation.North:

                    isHead = !isHead;

                    Game.World.SetBlock(Block.Air, 0, x, y, z);
                    Game.World.SetBlock(Block.Air, 0, x, y, z - (isHead ? 1 : -1));
                    return true;

                case Orientation.East:

                    Game.World.SetBlock(Block.Air, 0, x, y, z);
                    Game.World.SetBlock(Block.Air, 0, x - (isHead ? 1 : -1), y, z);
                    return true;

                case Orientation.South:

                    Game.World.SetBlock(Block.Air, 0, x, y, z);
                    Game.World.SetBlock(Block.Air, 0, x, y, z - (isHead ? 1 : -1));
                    return true;

                case Orientation.West:

                    isHead = !isHead;

                    Game.World.SetBlock(Block.Air, 0, x, y, z);
                    Game.World.SetBlock(Block.Air, 0, x - (isHead ? 1 : -1), y, z);
                    return true;

                default:
                    return false;
            }
        }

        protected override void EntityInteract(PhysicsEntity entity, int x, int y, int z, uint data)
        {
            bool isHead = (data & 0b1) == 1;

            switch ((Orientation)((data & 0b00_0110) >> 1))
            {
                case Orientation.North:

                    isHead = !isHead;

                    Game.World.SetBlock(this, data + 0b00_1000 & 0b11_1111, x, y, z);
                    Game.World.SetBlock(this, (data + 0b00_1000 & 0b11_1111) ^ 0b00_0001, x, y, z - (isHead ? 1 : -1));
                    break;

                case Orientation.East:

                    Game.World.SetBlock(this, data + 0b00_1000 & 0b11_1111, x, y, z);
                    Game.World.SetBlock(this, (data + 0b00_1000 & 0b11_1111) ^ 0b00_0001, x - (isHead ? 1 : -1), y, z);
                    break;

                case Orientation.South:

                    Game.World.SetBlock(this, data + 0b00_1000 & 0b11_1111, x, y, z);
                    Game.World.SetBlock(this, (data + 0b00_1000 & 0b11_1111) ^ 0b00_0001, x, y, z - (isHead ? 1 : -1));
                    break;

                case Orientation.West:

                    isHead = !isHead;

                    Game.World.SetBlock(this, data + 0b00_1000 & 0b01_1111, x, y, z);
                    Game.World.SetBlock(this, (data + 0b00_1000 & 0b01_1111) ^ 0b00_0001, x - (isHead ? 1 : -1), y, z);
                    break;
            }
        }

        internal override void BlockUpdate(int x, int y, int z, uint data, BlockSide side)
        {
            if (side == BlockSide.Bottom && Game.World.GetBlock(x, y - 1, z, out _)?.IsSolidAndFull != true)
            {
                Destroy(x, y, z);
            }
        }
    }
}