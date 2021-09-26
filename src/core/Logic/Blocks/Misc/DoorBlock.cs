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
    ///     A two units high block that can be opened and closed.
    ///     Data bit usage: <c>-csboo</c>
    /// </summary>
    // c = closed
    // s = side
    // b = base
    // o = orientation
    public class DoorBlock : Block, IFillable
    {
        private readonly string closed;
        private readonly string open;
        private readonly float[][] verticesBase = new float[8][];
        private readonly float[][] verticesTop = new float[8][];
        private uint[] indicesBase = null!;

        private uint[] indicesTop = null!;
        private int[] texIndicesBase = null!;

        private int[] texIndicesTop = null!;
        private uint vertexCountBase;

        private uint vertexCountTop;

        internal DoorBlock(string name, string namedId, string closed, string open) :
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
                new BoundingBox(new Vector3(x: 0.5f, y: 1f, z: 0.5f), new Vector3(x: 0.5f, y: 1f, z: 0.5f)),
                TargetBuffer.Complex)
        {
            this.closed = closed;
            this.open = open;
        }

        protected override void Setup(ITextureIndexProvider indexProvider)
        {
            BlockModel.Load(closed).PlaneSplit(
                Vector3.UnitY,
                -Vector3.UnitY,
                out BlockModel baseClosed,
                out BlockModel topClosed);

            topClosed.Move(-Vector3.UnitY);

            BlockModel.Load(open).PlaneSplit(
                Vector3.UnitY,
                -Vector3.UnitY,
                out BlockModel baseOpen,
                out BlockModel topOpen);

            topOpen.Move(-Vector3.UnitY);

            for (var i = 0; i < 4; i++)
                if (i == 0)
                {
                    baseClosed.ToData(out verticesBase[i], out texIndicesBase, out indicesBase);
                    topClosed.ToData(out verticesTop[i], out texIndicesTop, out indicesTop);

                    baseOpen.ToData(out verticesBase[i + 4], out _, out _);
                    topOpen.ToData(out verticesTop[i + 4], out _, out _);
                }
                else
                {
                    baseClosed.RotateY(rotations: 1);
                    baseClosed.ToData(out verticesBase[i], out _, out _);
                    topClosed.RotateY(rotations: 1);
                    topClosed.ToData(out verticesTop[i], out _, out _);

                    baseOpen.RotateY(rotations: 1);
                    baseOpen.ToData(out verticesBase[i + 4], out _, out _);
                    topOpen.RotateY(rotations: 1);
                    topOpen.ToData(out verticesTop[i + 4], out _, out _);
                }

            vertexCountTop = (uint) topClosed.VertexCount;
            vertexCountBase = (uint) baseClosed.VertexCount;
        }

        protected override BoundingBox GetBoundingBox(uint data)
        {
            var orientation = (Orientation) (data & 0b00_0011);

            // Check if door is open and if the door is left sided.
            if ((data & 0b01_0000) != 0)
                orientation = (data & 0b00_1000) == 0 ? orientation.Rotate() : orientation.Rotate().Opposite();

            return orientation switch
            {
                Orientation.North => new BoundingBox(
                    new Vector3(x: 0.5f, y: 0.5f, z: 0.9375f),
                    new Vector3(x: 0.5f, y: 0.5f, z: 0.0625f)),
                Orientation.East => new BoundingBox(
                    new Vector3(x: 0.0625f, y: 0.5f, z: 0.5f),
                    new Vector3(x: 0.0625f, y: 0.5f, z: 0.5f)),
                Orientation.South => new BoundingBox(
                    new Vector3(x: 0.5f, y: 0.5f, z: 0.0625f),
                    new Vector3(x: 0.5f, y: 0.5f, z: 0.0625f)),
                Orientation.West => new BoundingBox(
                    new Vector3(x: 0.9375f, y: 0.5f, z: 0.5f),
                    new Vector3(x: 0.0625f, y: 0.5f, z: 0.5f)),
                _ => new BoundingBox(new Vector3(x: 0.5f, y: 0.5f, z: 0.5f), new Vector3(x: 0.5f, y: 0.5f, z: 0.5f))
            };
        }

        public override BlockMeshData GetMesh(BlockMeshInfo info)
        {
            var orientation = (Orientation) (info.Data & 0b00_0011);
            bool isBase = (info.Data & 0b00_0100) == 0;
            bool isLeftSided = (info.Data & 0b00_1000) == 0;
            bool isClosed = (info.Data & 0b01_0000) == 0;

            Orientation openOrientation = isLeftSided ? orientation.Opposite() : orientation;

            int index = isClosed ? (int) orientation : 4 + (int) openOrientation;

            return isBase
                ? BlockMeshData.Complex(vertexCountBase, verticesBase[index], texIndicesBase, indicesBase)
                : BlockMeshData.Complex(vertexCountTop, verticesTop[index], texIndicesTop, indicesTop);
        }

        internal override bool CanPlace(World world, Vector3i position, PhysicsEntity? entity)
        {
            return world.GetBlock(position.Above(), out _)?.IsReplaceable == true &&
                   world.HasSolidGround(position, solidify: true);
        }

        protected override void DoPlace(World world, Vector3i position, PhysicsEntity? entity)
        {
            Orientation orientation = entity?.LookingDirection.ToOrientation() ?? Orientation.North;
            BlockSide side = entity?.TargetSide ?? BlockSide.Top;

            bool isLeftSided;

            if (side == BlockSide.Top)
            {
                // Choose side according to neighboring doors to form a double door.

                Orientation toNeighbor = orientation.Rotate().Opposite();
                Vector3i neighborPosition = toNeighbor.Offset(position);

                Block neighbor = world.GetBlock(neighborPosition, out uint data) ?? Air;
                isLeftSided = neighbor != this || (data & 0b00_1011) != (int) orientation;
            }
            else
            {
                isLeftSided =
                    orientation == Orientation.North && side != BlockSide.Left ||
                    orientation == Orientation.East && side != BlockSide.Back ||
                    orientation == Orientation.South && side != BlockSide.Right ||
                    orientation == Orientation.West && side != BlockSide.Front;
            }

            world.SetBlock(this, (uint) ((isLeftSided ? 0b0000 : 0b1000) | (int) orientation), position);

            world.SetBlock(
                this,
                (uint) ((isLeftSided ? 0b0000 : 0b1000) | 0b0100 | (int) orientation),
                position.Above());
        }

        internal override void DoDestroy(World world, Vector3i position, uint data, PhysicsEntity? entity)
        {
            bool isBase = (data & 0b00_0100) == 0;

            world.SetDefaultBlock(position);
            world.SetDefaultBlock(position + (isBase ? Vector3i.UnitY : -Vector3i.UnitY));
        }

        protected override void EntityInteract(PhysicsEntity entity, Vector3i position, uint data)
        {
            bool isBase = (data & 0b00_0100) == 0;
            Vector3i otherPosition = position + (isBase ? Vector3i.UnitY : -Vector3i.UnitY);

            if (entity.BoundingBox.Intersects(
                new BoundingBox(
                    new Vector3(x: 0.5f, y: 1f, z: 0.5f) + otherPosition.ToVector3(),
                    new Vector3(x: 0.5f, y: 1f, z: 0.5f)))) return;

            entity.World.SetBlock(this, data ^ 0b1_0000, position);
            entity.World.SetBlock(this, data ^ 0b1_0100, otherPosition);

            // Open a neighboring door, if available.
            bool isLeftSided = (data & 0b00_1000) == 0;
            var orientation = (Orientation) (data & 0b00_0011);
            orientation = isLeftSided ? orientation.Opposite() : orientation;

            Orientation toNeighbor = orientation.Rotate().Opposite();

            OpenNeighbor(toNeighbor.Offset(position));

            void OpenNeighbor(Vector3i neighborPosition)
            {
                Block neighbor = entity.World.GetBlock(neighborPosition, out uint neighborData) ?? Air;

                if (neighbor == this && (data & 0b01_1011) == ((neighborData ^ 0b00_1000) & 0b01_1011))
                    neighbor.EntityInteract(entity, neighborPosition);
            }
        }

        internal override void BlockUpdate(World world, Vector3i position, uint data, BlockSide side)
        {
            if (side == BlockSide.Bottom && (data & 0b00_0100) == 0 && !world.HasSolidGround(position))
                Destroy(world, position);
        }
    }
}