// <copyright file="DoorBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System.Collections.Generic;
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
        private readonly List<BlockMesh> baseClosedMeshes = new();
        private readonly List<BlockMesh> baseOpenMeshes = new();
        private readonly List<BlockMesh> topClosedMeshes = new();
        private readonly List<BlockMesh> topOpenMeshes = new();

        internal DoorBlock(string name, string namedId, string closedModel, string openModel) :
            base(
                name,
                namedId,
                BlockFlags.Functional,
                new BoundingBox(new Vector3(x: 0.5f, y: 1f, z: 0.5f), new Vector3(x: 0.5f, y: 1f, z: 0.5f)),
                TargetBuffer.Complex)
        {
            BlockModel.Load(closedModel).PlaneSplit(
                Vector3.UnitY,
                -Vector3.UnitY,
                out BlockModel baseClosed,
                out BlockModel topClosed);

            topClosed.Move(-Vector3.UnitY);

            BlockModel.Load(openModel).PlaneSplit(
                Vector3.UnitY,
                -Vector3.UnitY,
                out BlockModel baseOpen,
                out BlockModel topOpen);

            topOpen.Move(-Vector3.UnitY);

            CreateMeshes(baseClosed, baseClosedMeshes);
            CreateMeshes(baseOpen, baseOpenMeshes);

            CreateMeshes(topClosed, topClosedMeshes);
            CreateMeshes(topOpen, topOpenMeshes);

            static void CreateMeshes(BlockModel model, ICollection<BlockMesh> meshList)
            {
                (BlockModel north, BlockModel east, BlockModel south, BlockModel west) =
                    model.CreateAllOrientations(rotateTopAndBottomTexture: true);

                meshList.Add(north.GetMesh());
                meshList.Add(east.GetMesh());
                meshList.Add(south.GetMesh());
                meshList.Add(west.GetMesh());
            }
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

            if (isClosed)
            {
                var index = (int) orientation;

                BlockMesh mesh = isBase ? baseClosedMeshes[index] : topClosedMeshes[index];

                return mesh.GetComplexMeshData();
            }
            else
            {
                Orientation openOrientation = isLeftSided ? orientation.Opposite() : orientation;
                var index = (int) openOrientation;

                BlockMesh mesh = isBase ? baseOpenMeshes[index] : topOpenMeshes[index];

                return mesh.GetComplexMeshData();
            }
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