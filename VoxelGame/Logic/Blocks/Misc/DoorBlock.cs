// <copyright file="DoorBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using OpenToolkit.Mathematics;
using VoxelGame.Entities;
using VoxelGame.Physics;
using VoxelGame.Utilities;
using VoxelGame.Visuals;

namespace VoxelGame.Logic.Blocks
{
    /// <summary>
    /// A two units high block that can be opened and closed.
    /// Data bit usage: <c>csboo</c>
    /// </summary>
    // c = closed
    // s = side
    // b = base
    // o = orientation
    public class DoorBlock : Block
    {
        private protected float[][] verticesTop = new float[8][];
        private protected float[][] verticesBase = new float[8][];

        private protected int[] texIndicesTop = null!;
        private protected int[] texIndicesBase = null!;

        private protected uint[] indicesTop = null!;
        private protected uint[] indicesBase = null!;

        private protected uint vertexCountTop;
        private protected uint vertexCountBase;

        private protected string closed, open;

        public DoorBlock(string name, string closed, string open) :
            base(
                name,
                isFull: false,
                isOpaque: false,
                renderFaceAtNonOpaques: true,
                isSolid: true,
                recieveCollisions: false,
                isTrigger: false,
                isReplaceable: false,
                isInteractable: true,
                new BoundingBox(new Vector3(0.5f, 1f, 0.5f), new Vector3(0.5f, 1f, 0.5f)),
                TargetBuffer.Complex)
        {
            this.closed = closed;
            this.open = open;
        }

        protected override void Setup()
        {
            BlockModel.Load(closed).PlaneSplit(Vector3.UnitY, -Vector3.UnitY, out BlockModel baseClosed, out BlockModel topClosed);
            topClosed.Move(-Vector3.UnitY);

            BlockModel.Load(open).PlaneSplit(Vector3.UnitY, -Vector3.UnitY, out BlockModel baseOpen, out BlockModel topOpen);
            topOpen.Move(-Vector3.UnitY);

            for (int i = 0; i < 4; i++)
            {
                if (i == 0)
                {
                    baseClosed.ToData(out verticesBase[i], out texIndicesBase, out indicesBase);
                    topClosed.ToData(out verticesTop[i], out texIndicesTop, out indicesTop);

                    baseOpen.ToData(out verticesBase[i + 4], out _, out _);
                    topOpen.ToData(out verticesTop[i + 4], out _, out _);
                }
                else
                {
                    baseClosed.RotateY(1);
                    baseClosed.ToData(out verticesBase[i], out _, out _);
                    topClosed.RotateY(1);
                    topClosed.ToData(out verticesTop[i], out _, out _);

                    baseOpen.RotateY(1);
                    baseOpen.ToData(out verticesBase[i + 4], out _, out _);
                    topOpen.RotateY(1);
                    topOpen.ToData(out verticesTop[i + 4], out _, out _);
                }
            }

            vertexCountTop = (uint)topClosed.VertexCount;
            vertexCountBase = (uint)baseClosed.VertexCount;
        }

        protected override BoundingBox GetBoundingBox(int x, int y, int z, byte data)
        {
            Orientation orientation = (Orientation)(data & 0b0_0011);

            // Check if door is open and if the door is left sided.
            if ((data & 0b1_0000) != 0)
            {
                orientation = ((data & 0b0_1000) == 0) ? orientation.Rotate() : orientation.Rotate().Invert();
            }

            return orientation switch
            {
                Orientation.North => new BoundingBox(new Vector3(0.5f, 0.5f, 0.9375f) + new Vector3(x, y, z), new Vector3(0.5f, 0.5f, 0.0625f)),
                Orientation.East => new BoundingBox(new Vector3(0.0625f, 0.5f, 0.5f) + new Vector3(x, y, z), new Vector3(0.0625f, 0.5f, 0.5f)),
                Orientation.South => new BoundingBox(new Vector3(0.5f, 0.5f, 0.0625f) + new Vector3(x, y, z), new Vector3(0.5f, 0.5f, 0.0625f)),
                Orientation.West => new BoundingBox(new Vector3(0.9375f, 0.5f, 0.5f) + new Vector3(x, y, z), new Vector3(0.0625f, 0.5f, 0.5f)),
                _ => new BoundingBox(new Vector3(0.5f, 0.5f, 0.5f) + new Vector3(x, y, z), new Vector3(0.5f, 0.5f, 0.5f))
            };
        }

        public override uint GetMesh(BlockSide side, byte data, out float[] vertices, out int[] textureIndices, out uint[] indices, out TintColor tint)
        {
            Orientation orientation = (Orientation)(data & 0b0_0011);
            bool isBase = (data & 0b0_0100) == 0;
            bool isLeftSided = (data & 0b0_1000) == 0;
            bool isClosed = (data & 0b1_0000) == 0;

            if (isBase)
            {
                vertices = verticesBase[isClosed ? (int)orientation : 4 + (isLeftSided ? (int)orientation.Invert() : (int)orientation)];

                textureIndices = texIndicesBase;
                indices = indicesBase;
                tint = TintColor.None;

                return vertexCountBase;
            }
            else
            {
                vertices = verticesTop[isClosed ? (int)orientation : 4 + (isLeftSided ? (int)orientation.Invert() : (int)orientation)];

                textureIndices = texIndicesTop;
                indices = indicesTop;
                tint = TintColor.None;

                return vertexCountTop;
            }
        }

        protected override bool Place(int x, int y, int z, bool? replaceable, PhysicsEntity? entity)
        {
            if (replaceable != true || Game.World.GetBlock(x, y + 1, z, out _)?.IsReplaceable != true || Game.World.GetBlock(x, y - 1, z, out _)?.IsSolidAndFull != true)
            {
                return false;
            }

            Orientation orientation = entity?.LookingDirection.ToOrientation() ?? Orientation.North;
            BlockSide side = entity?.TargetSide ?? BlockSide.Top;

            bool isLeftSided;

            if (side == BlockSide.Top)
            {
                // Choose side according to neighboring doors to form a double door.
                Block neighbour;
                byte data;

                switch (orientation)
                {
                    case Orientation.North:
                        neighbour = Game.World.GetBlock(x - 1, y, z, out data) ?? Block.AIR;
                        break;

                    case Orientation.East:
                        neighbour = Game.World.GetBlock(x, y, z - 1, out data) ?? Block.AIR;
                        break;

                    case Orientation.South:
                        neighbour = Game.World.GetBlock(x + 1, y, z, out data) ?? Block.AIR;
                        break;

                    case Orientation.West:
                        neighbour = Game.World.GetBlock(x, y, z + 1, out data) ?? Block.AIR;
                        break;

                    default:
                        neighbour = Block.AIR;
                        data = 0;
                        break;
                }

                isLeftSided = neighbour != this || (data & 0b0_1011) != (int)orientation;
            }
            else
            {
                isLeftSided =
                    (orientation == Orientation.North && side != BlockSide.Left) ||
                    (orientation == Orientation.East && side != BlockSide.Back) ||
                    (orientation == Orientation.South && side != BlockSide.Right) ||
                    (orientation == Orientation.West && side != BlockSide.Front);
            }

            Game.World.SetBlock(this, (byte)((isLeftSided ? 0b0000 : 0b1000) | 0b0000 | (int)orientation), x, y, z);
            Game.World.SetBlock(this, (byte)((isLeftSided ? 0b0000 : 0b1000) | 0b0100 | (int)orientation), x, y + 1, z);

            return true;
        }

        protected override bool Destroy(int x, int y, int z, byte data, PhysicsEntity? entity)
        {
            Game.World.SetBlock(Block.AIR, 0, x, y, z);
            Game.World.SetBlock(Block.AIR, 0, x, y + ((data & 0b0_0100) == 0 ? 1 : -1), z);

            return true;
        }

        protected override void EntityInteract(PhysicsEntity entity, int x, int y, int z, byte data)
        {
            bool isBase = (data & 0b0_0100) == 0;

            if (!entity.BoundingBox.Intersects(new BoundingBox(new Vector3(0.5f, 1f, 0.5f) + new Vector3(x, isBase ? y : y - 1, z), new Vector3(0.5f, 1f, 0.5f))))
            {
                Game.World.SetBlock(this, (byte)(data ^ 0b1_0000), x, y, z);
                Game.World.SetBlock(this, (byte)(data ^ 0b1_0100), x, y + (isBase ? 1 : -1), z);

                // Open a neighboring door, if available.
                switch (((data & 0b0_1000) == 0) ? ((Orientation)(data & 0b0_0011)).Invert() : (Orientation)(data & 0b0_0011))
                {
                    case Orientation.North:
                        Block neighbour = Game.World.GetBlock(x - 1, y, z, out byte neighbourData) ?? Block.AIR;

                        if (neighbour == this && (data & 0b1_1011) == ((neighbourData ^ 0b0_1000) & 0b1_1011))
                        {
                            neighbour.EntityInteract(entity, x - 1, y, z);
                        }

                        break;

                    case Orientation.East:
                        neighbour = Game.World.GetBlock(x, y, z - 1, out neighbourData) ?? Block.AIR;

                        if (neighbour == this && (data & 0b1_1011) == ((neighbourData ^ 0b0_1000) & 0b1_1011))
                        {
                            neighbour.EntityInteract(entity, x, y, z - 1);
                        }

                        break;

                    case Orientation.South:
                        neighbour = Game.World.GetBlock(x + 1, y, z, out neighbourData) ?? Block.AIR;

                        if (neighbour == this && (data & 0b1_1011) == ((neighbourData ^ 0b0_1000) & 0b1_1011))
                        {
                            neighbour.EntityInteract(entity, x + 1, y, z);
                        }

                        break;

                    case Orientation.West:
                        neighbour = Game.World.GetBlock(x, y, z + 1, out neighbourData) ?? Block.AIR;

                        if (neighbour == this && (data & 0b1_1011) == ((neighbourData ^ 0b0_1000) & 0b1_1011))
                        {
                            neighbour.EntityInteract(entity, x, y, z + 1);
                        }

                        break;
                }
            }
        }

        internal override void BlockUpdate(int x, int y, int z, byte data, BlockSide side)
        {
            if (side == BlockSide.Bottom && (data & 0b0_0100) == 0 && Game.World.GetBlock(x, y - 1, z, out _)?.IsSolidAndFull != true)
            {
                Destroy(x, y, z, null);
            }
        }
    }
}