// <copyright file="DoorBlock.cs" company="VoxelGame">
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
    /// A two units high block that can be opened and closed.
    /// Data bit usage: <c>-csboo</c>
    /// </summary>
    // c = closed
    // s = side
    // b = base
    // o = orientation
    public class DoorBlock : Block, IFillable
    {
        private readonly float[][] verticesTop = new float[8][];
        private readonly float[][] verticesBase = new float[8][];

        private int[] texIndicesTop = null!;
        private int[] texIndicesBase = null!;

        private uint[] indicesTop = null!;
        private uint[] indicesBase = null!;

        private uint vertexCountTop;
        private uint vertexCountBase;

        private readonly string closed;
        private readonly string open;

        public DoorBlock(string name, string namedId, string closed, string open) :
            base(
                name,
                namedId,
                isFull: false,
                isOpaque: false,
                renderFaceAtNonOpaques: true,
                isSolid: true,
                receiveCollisions: false,
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

        protected override BoundingBox GetBoundingBox(int x, int y, int z, uint data)
        {
            Orientation orientation = (Orientation)(data & 0b00_0011);

            // Check if door is open and if the door is left sided.
            if ((data & 0b01_0000) != 0)
            {
                orientation = ((data & 0b00_1000) == 0) ? orientation.Rotate() : orientation.Rotate().Invert();
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

        public override BlockMeshData GetMesh(BlockMeshInfo info)
        {
            Orientation orientation = (Orientation)(info.Data & 0b00_0011);
            bool isBase = (info.Data & 0b00_0100) == 0;
            bool isLeftSided = (info.Data & 0b00_1000) == 0;
            bool isClosed = (info.Data & 0b01_0000) == 0;

            Orientation openOrientation = isLeftSided ? orientation.Invert() : orientation;

            int index = isClosed ? (int)orientation : 4 + (int)openOrientation;

            return isBase
                ? new BlockMeshData(vertexCountBase, verticesBase[index], texIndicesBase, indicesBase)
                : new BlockMeshData(vertexCountTop, verticesTop[index], texIndicesTop, indicesTop);
        }

        protected override bool Place(PhysicsEntity? entity, int x, int y, int z)
        {
            if (Game.World.GetBlock(x, y + 1, z, out _)?.IsReplaceable != true || !Game.World.HasSolidGround(x, y, z))
            {
                return false;
            }

            Orientation orientation = entity?.LookingDirection.ToOrientation() ?? Orientation.North;
            BlockSide side = entity?.TargetSide ?? BlockSide.Top;

            bool isLeftSided;

            if (side == BlockSide.Top)
            {
                // Choose side according to neighboring doors to form a double door.
                Block neighbor;
                uint data;

                switch (orientation)
                {
                    case Orientation.North:
                        neighbor = Game.World.GetBlock(x - 1, y, z, out data) ?? Block.Air;
                        break;

                    case Orientation.East:
                        neighbor = Game.World.GetBlock(x, y, z - 1, out data) ?? Block.Air;
                        break;

                    case Orientation.South:
                        neighbor = Game.World.GetBlock(x + 1, y, z, out data) ?? Block.Air;
                        break;

                    case Orientation.West:
                        neighbor = Game.World.GetBlock(x, y, z + 1, out data) ?? Block.Air;
                        break;

                    default:
                        neighbor = Block.Air;
                        data = 0;
                        break;
                }

                isLeftSided = neighbor != this || (data & 0b00_1011) != (int)orientation;
            }
            else
            {
                isLeftSided =
                    (orientation == Orientation.North && side != BlockSide.Left) ||
                    (orientation == Orientation.East && side != BlockSide.Back) ||
                    (orientation == Orientation.South && side != BlockSide.Right) ||
                    (orientation == Orientation.West && side != BlockSide.Front);
            }

            Game.World.SetBlock(this, (uint)((isLeftSided ? 0b0000 : 0b1000) | (int)orientation), x, y, z);
            Game.World.SetBlock(this, (uint)((isLeftSided ? 0b0000 : 0b1000) | 0b0100 | (int)orientation), x, y + 1, z);

            return true;
        }

        protected override bool Destroy(PhysicsEntity? entity, int x, int y, int z, uint data)
        {
            Game.World.SetDefaultBlock(x, y, z);
            Game.World.SetDefaultBlock(x, y + ((data & 0b00_0100) == 0 ? 1 : -1), z);

            return true;
        }

        protected override void EntityInteract(PhysicsEntity entity, int x, int y, int z, uint data)
        {
            bool isBase = (data & 0b00_0100) == 0;

            if (entity.BoundingBox.Intersects(new BoundingBox(new Vector3(0.5f, 1f, 0.5f) + new Vector3(x, isBase ? y : y - 1, z), new Vector3(0.5f, 1f, 0.5f))))
            {
                return;
            }

            Game.World.SetBlock(this, data ^ 0b1_0000, x, y, z);
            Game.World.SetBlock(this, data ^ 0b1_0100, x, y + (isBase ? 1 : -1), z);

            // Open a neighboring door, if available.
            switch (((data & 0b00_1000) == 0) ? ((Orientation)(data & 0b00_0011)).Invert() : (Orientation)(data & 0b00_0011))
            {
                case Orientation.North:

                    OpenNeighbor(x - 1, y, z);
                    break;

                case Orientation.East:

                    OpenNeighbor(x, y, z - 1);
                    break;

                case Orientation.South:

                    OpenNeighbor(x + 1, y, z);
                    break;

                case Orientation.West:

                    OpenNeighbor(x, y, z + 1);
                    break;
            }

            void OpenNeighbor(int x, int y, int z)
            {
                Block neighbor = Game.World.GetBlock(x, y, z, out uint neighborData) ?? Block.Air;

                if (neighbor == this && (data & 0b01_1011) == ((neighborData ^ 0b00_1000) & 0b01_1011))
                {
                    neighbor.EntityInteract(entity, x, y, z);
                }
            }
        }

        internal override void BlockUpdate(int x, int y, int z, uint data, BlockSide side)
        {
            if (side == BlockSide.Bottom && (data & 0b00_0100) == 0 && !Game.World.HasSolidGround(x, y, z))
            {
                Destroy(x, y, z);
            }
        }
    }
}