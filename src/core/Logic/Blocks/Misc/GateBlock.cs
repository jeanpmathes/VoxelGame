// <copyright file="GateBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Visuals;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Utilities;
using OpenToolkit.Mathematics;
using VoxelGame.Core.Entities;

namespace VoxelGame.Core.Logic.Blocks
{
    /// <summary>
    /// A simple gate that can be used in fences and walls. It can be opened and closed.
    /// Data bit usage: <c>---coo</c>
    /// </summary>
    public class GateBlock : Block, IConnectable, IFlammable, IFillable
    {
        private float[][] verticesClosed = null!;
        private float[][] verticesOpen = null!;

        private int[] texIndicesClosed = null!;
        private int[] texIndicesOpen = null!;

        private uint[] indicesClosed = null!;
        private uint[] indicesOpen = null!;

        private uint vertexCountClosed;
        private uint vertexCountOpen;

        private readonly string closed;
        private readonly string open;

        internal GateBlock(string name, string namedId, string closed, string open) :
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
                BoundingBox.Block,
                TargetBuffer.Complex)
        {
            this.closed = closed;
            this.open = open;
        }

        protected override void Setup()
        {
            verticesClosed = new float[4][];
            verticesOpen = new float[4][];

            BlockModel closedModel = BlockModel.Load(this.closed);
            BlockModel openModel = BlockModel.Load(this.open);

            for (var i = 0; i < 4; i++)
            {
                if (i == 0)
                {
                    closedModel.ToData(out verticesClosed[i], out texIndicesClosed, out indicesClosed);
                    openModel.ToData(out verticesOpen[i], out texIndicesOpen, out indicesOpen);
                }
                else
                {
                    closedModel.RotateY(1, false);
                    closedModel.ToData(out verticesClosed[i], out _, out _);
                    openModel.RotateY(1, false);
                    openModel.ToData(out verticesOpen[i], out _, out _);
                }
            }

            vertexCountClosed = (uint)closedModel.VertexCount;
            vertexCountOpen = (uint)openModel.VertexCount;
        }

        protected override BoundingBox GetBoundingBox(uint data)
        {
            bool isClosed = (data & 0b00_0100) == 0;

            return ((Orientation)(data & 0b00_0011)) switch
            {
                Orientation.North => NorthSouth(0.375f),
                Orientation.East => WestEast(0.625f),
                Orientation.South => NorthSouth(0.625f),
                Orientation.West => WestEast(0.375f),
                _ => NorthSouth(0.375f),
            };
            BoundingBox NorthSouth(float offset)
            {
                if (isClosed)
                {
                    return new BoundingBox(new Vector3(0.96875f, 0.71875f, 0.5f), new Vector3(0.03125f, 0.15625f, 0.125f),
                    new BoundingBox(new Vector3(0.96875f, 0.28125f, 0.5f), new Vector3(0.03125f, 0.15625f, 0.125f)),
                    new BoundingBox(new Vector3(0.03125f, 0.71875f, 0.5f), new Vector3(0.03125f, 0.15625f, 0.125f)),
                    new BoundingBox(new Vector3(0.03125f, 0.28125f, 0.5f), new Vector3(0.03125f, 0.15625f, 0.125f)),
                    // Moving parts.
                    new BoundingBox(new Vector3(0.75f, 0.71875f, 0.5f), new Vector3(0.1875f, 0.09375f, 0.0625f)),
                    new BoundingBox(new Vector3(0.75f, 0.28125f, 0.5f), new Vector3(0.1875f, 0.09375f, 0.0625f)),
                    new BoundingBox(new Vector3(0.25f, 0.71875f, 0.5f), new Vector3(0.1875f, 0.09375f, 0.0625f)),
                    new BoundingBox(new Vector3(0.25f, 0.28125f, 0.5f), new Vector3(0.1875f, 0.09375f, 0.0625f)));
                }
                else
                {
                    return new BoundingBox(new Vector3(0.96875f, 0.71875f, 0.5f), new Vector3(0.03125f, 0.15625f, 0.125f),
                    new BoundingBox(new Vector3(0.96875f, 0.28125f, 0.5f), new Vector3(0.03125f, 0.15625f, 0.125f)),
                    new BoundingBox(new Vector3(0.03125f, 0.71875f, 0.5f), new Vector3(0.03125f, 0.15625f, 0.125f)),
                    new BoundingBox(new Vector3(0.03125f, 0.28125f, 0.5f), new Vector3(0.03125f, 0.15625f, 0.125f)),
                    // Moving parts.
                    new BoundingBox(new Vector3(0.875f, 0.71875f, offset), new Vector3(0.0625f, 0.09375f, 0.1875f)),
                    new BoundingBox(new Vector3(0.875f, 0.28125f, offset), new Vector3(0.0625f, 0.09375f, 0.1875f)),
                    new BoundingBox(new Vector3(0.125f, 0.71875f, offset), new Vector3(0.0625f, 0.09375f, 0.1875f)),
                    new BoundingBox(new Vector3(0.125f, 0.28125f, offset), new Vector3(0.0625f, 0.09375f, 0.1875f)));
                }
            }

            BoundingBox WestEast(float offset)
            {
                if (isClosed)
                {
                    return new BoundingBox(new Vector3(0.5f, 0.71875f, 0.96875f), new Vector3(0.125f, 0.15625f, 0.03125f),
                    new BoundingBox(new Vector3(0.5f, 0.28125f, 0.96875f), new Vector3(0.125f, 0.15625f, 0.03125f)),
                    new BoundingBox(new Vector3(0.5f, 0.71875f, 0.03125f), new Vector3(0.125f, 0.15625f, 0.03125f)),
                    new BoundingBox(new Vector3(0.5f, 0.28125f, 0.03125f), new Vector3(0.125f, 0.15625f, 0.03125f)),
                    // Moving parts.
                    new BoundingBox(new Vector3(0.5f, 0.71875f, 0.75f), new Vector3(0.0625f, 0.09375f, 0.1875f)),
                    new BoundingBox(new Vector3(0.5f, 0.28125f, 0.75f), new Vector3(0.0625f, 0.09375f, 0.1875f)),
                    new BoundingBox(new Vector3(0.5f, 0.71875f, 0.25f), new Vector3(0.0625f, 0.09375f, 0.1875f)),
                    new BoundingBox(new Vector3(0.5f, 0.28125f, 0.25f), new Vector3(0.0625f, 0.09375f, 0.1875f)));
                }
                else
                {
                    return new BoundingBox(new Vector3(0.5f, 0.71875f, 0.96875f), new Vector3(0.125f, 0.15625f, 0.03125f),
                    new BoundingBox(new Vector3(0.5f, 0.28125f, 0.96875f), new Vector3(0.125f, 0.15625f, 0.03125f)),
                    new BoundingBox(new Vector3(0.5f, 0.71875f, 0.03125f), new Vector3(0.125f, 0.15625f, 0.03125f)),
                    new BoundingBox(new Vector3(0.5f, 0.28125f, 0.03125f), new Vector3(0.125f, 0.15625f, 0.03125f)),
                    // Moving parts.
                    new BoundingBox(new Vector3(offset, 0.71875f, 0.875f), new Vector3(0.1875f, 0.09375f, 0.0625f)),
                    new BoundingBox(new Vector3(offset, 0.28125f, 0.875f), new Vector3(0.1875f, 0.09375f, 0.0625f)),
                    new BoundingBox(new Vector3(offset, 0.71875f, 0.125f), new Vector3(0.1875f, 0.09375f, 0.0625f)),
                    new BoundingBox(new Vector3(offset, 0.28125f, 0.125f), new Vector3(0.1875f, 0.09375f, 0.0625f)));
                }
            }
        }

        public override BlockMeshData GetMesh(BlockMeshInfo info)
        {
            return (info.Data & 0b00_0100) == 0
                ? new BlockMeshData(vertexCountClosed, verticesClosed[info.Data & 0b00_0011], texIndicesClosed, indicesClosed) // Open.
                : new BlockMeshData(vertexCountOpen, verticesOpen[info.Data & 0b00_0011], texIndicesOpen, indicesOpen); // Closed.
        }

        internal override bool CanPlace(int x, int y, int z, PhysicsEntity? entity)
        {
            Orientation orientation = entity?.LookingDirection.ToOrientation() ?? Orientation.North;

            bool connectX = (Game.World.GetBlock(x + 1, y, z, out _) is IConnectable east && east.IsConnectable(BlockSide.Left, x + 1, y, z)) || (Game.World.GetBlock(x - 1, y, z, out _) is IConnectable west && west.IsConnectable(BlockSide.Right, x - 1, y, z));
            bool connectZ = (Game.World.GetBlock(x, y, z + 1, out _) is IConnectable south && south.IsConnectable(BlockSide.Back, x, y, z + 1)) || (Game.World.GetBlock(x, y, z - 1, out _) is IConnectable north && north.IsConnectable(BlockSide.Front, x, y, z - 1));

            if (orientation.IsZ() && !connectX)
            {
                return connectZ;
            }
            else if (orientation.IsX() && !connectZ && !connectX)
            {
                return false;
            }

            return true;
        }

        protected override void DoPlace(int x, int y, int z, PhysicsEntity? entity)
        {
            Orientation orientation = entity?.LookingDirection.ToOrientation() ?? Orientation.North;

            bool connectX = (Game.World.GetBlock(x + 1, y, z, out _) is IConnectable east && east.IsConnectable(BlockSide.Left, x + 1, y, z)) || (Game.World.GetBlock(x - 1, y, z, out _) is IConnectable west && west.IsConnectable(BlockSide.Right, x - 1, y, z));
            bool connectZ = (Game.World.GetBlock(x, y, z + 1, out _) is IConnectable south && south.IsConnectable(BlockSide.Back, x, y, z + 1)) || (Game.World.GetBlock(x, y, z - 1, out _) is IConnectable north && north.IsConnectable(BlockSide.Front, x, y, z - 1));

            if (orientation.IsZ() && !connectX)
            {
                orientation = orientation.Rotate();
            }
            else if (orientation.IsX() && !connectZ)
            {
                orientation = orientation.Rotate();
            }

            Game.World.SetBlock(this, (uint)orientation, x, y, z);
        }

        protected override void EntityInteract(PhysicsEntity entity, int x, int y, int z, uint data)
        {
            Orientation orientation = (Orientation)(data & 0b00_0011);
            bool isClosed = (data & 0b00_0100) == 0;

            // Check if orientation has to be inverted.
            if (isClosed && Vector2.Dot(orientation.ToVector().Xz, entity.Position.Xz - new Vector2(x + 0.5f, z + 0.5f)) < 0)
            {
                orientation = orientation.Invert();
            }

            Vector3 center = isClosed ? new Vector3(0.5f, 0.5f, 0.5f) + (-orientation.ToVector() * 0.09375f) : new Vector3(0.5f, 0.5f, 0.5f);
            float closedOffset = isClosed ? 0.09375f : 0f;
            Vector3 extents = (orientation == Orientation.North || orientation == Orientation.South) ? new Vector3(0.5f, 0.375f, 0.125f + closedOffset) : new Vector3(0.125f + closedOffset, 0.375f, 0.5f);

            if (entity.BoundingBox.Intersects(new BoundingBox(center + new Vector3(x, y, z), extents)))
            {
                return;
            }

            Game.World.SetBlock(this, (uint)((isClosed ? 0b00_0100 : 0b00_0000) | (int)orientation.Invert()), x, y, z);
        }

        internal override void BlockUpdate(int x, int y, int z, uint data, BlockSide side)
        {
            var orientation = (Orientation)(data & 0b00_0011);

            switch (side)
            {
                case BlockSide.Left:
                case BlockSide.Right:

                    if (orientation.IsZ() &&
                        !((Game.World.GetBlock(x + 1, y, z, out _) is IConnectable east && east.IsConnectable(BlockSide.Left, x + 1, y, z)) ||
                        (Game.World.GetBlock(x - 1, y, z, out _) is IConnectable west && west.IsConnectable(BlockSide.Right, x - 1, y, z))))
                    {
                        Destroy(x, y, z);
                    }

                    break;

                case BlockSide.Front:
                case BlockSide.Back:

                    if (orientation.IsX() &&
                        !((Game.World.GetBlock(x, y, z + 1, out _) is IConnectable south && south.IsConnectable(BlockSide.Back, x, y, z + 1)) ||
                        (Game.World.GetBlock(x, y, z - 1, out _) is IConnectable north && north.IsConnectable(BlockSide.Front, x, y, z - 1))))
                    {
                        Destroy(x, y, z);
                    }

                    break;
            }
        }

        public virtual bool IsConnectable(BlockSide side, int x, int y, int z)
        {
            if (Game.World.GetBlock(x, y, z, out uint data) == this)
            {
                var orientation = (Orientation)(data & 0b00_0011);

                return orientation switch
                {
                    Orientation.North => side == BlockSide.Left || side == BlockSide.Right,
                    Orientation.East => side == BlockSide.Front || side == BlockSide.Back,
                    Orientation.South => side == BlockSide.Left || side == BlockSide.Right,
                    Orientation.West => side == BlockSide.Front || side == BlockSide.Back,
                    _ => false
                };
            }
            else
            {
                return false;
            }
        }
    }
}