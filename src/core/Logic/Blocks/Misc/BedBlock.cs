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
    ///     A block that is two blocks long and allows setting the spawn point.
    ///     Data bit usage: <c>cccoop</c>
    /// </summary>
    // c = color
    // o = orientation
    // p = position
    public class BedBlock : Block, IFlammable, IFillable
    {
        private readonly string model;
        private readonly float[][] verticesEnd = new float[4][];
        private readonly float[][] verticesHead = new float[4][];
        private uint[] indicesEnd = null!;

        private uint[] indicesHead = null!;
        private int[] texIndicesEnd = null!;

        private int[] texIndicesHead = null!;
        private uint vertexCountEnd;

        private uint vertexCountHead;

        internal BedBlock(string name, string namedId, string model) :
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
                new BoundingBox(new Vector3(x: 0.5f, y: 0.21875f, z: 0.5f), new Vector3(x: 0.5f, y: 0.21875f, z: 0.5f)),
                TargetBuffer.Complex)
        {
            this.model = model;
        }

        protected override void Setup(ITextureIndexProvider indexProvider)
        {
            BlockModel blockModel = BlockModel.Load(model);

            blockModel.PlaneSplit(Vector3.UnitZ, Vector3.UnitZ, out BlockModel top, out BlockModel bottom);
            bottom.Move(-Vector3.UnitZ);

            vertexCountHead = (uint) top.VertexCount;
            vertexCountEnd = (uint) bottom.VertexCount;

            for (var i = 0; i < 4; i++)
                if (i == 0)
                {
                    top.ToData(out verticesHead[i], out texIndicesHead, out indicesHead);
                    bottom.ToData(out verticesEnd[i], out texIndicesEnd, out indicesEnd);
                }
                else
                {
                    top.RotateY(rotations: 1);
                    top.ToData(out verticesHead[i], out _, out _);

                    bottom.RotateY(rotations: 1);
                    bottom.ToData(out verticesEnd[i], out _, out _);
                }
        }

        protected override BoundingBox GetBoundingBox(uint data)
        {
            bool isBase = (data & 0b1) == 1;
            var orientation = (Orientation) ((data & 0b00_0110) >> 1);

            BoundingBox[] legs = new BoundingBox[2];

            switch (isBase ? orientation : orientation.Opposite())
            {
                case Orientation.North:

                    legs[0] = new BoundingBox(
                        new Vector3(x: 0.09375f, y: 0.09375f, z: 0.09375f),
                        new Vector3(x: 0.09375f, y: 0.09375f, z: 0.09375f));

                    legs[1] = new BoundingBox(
                        new Vector3(x: 0.90625f, y: 0.09375f, z: 0.09375f),
                        new Vector3(x: 0.09375f, y: 0.09375f, z: 0.09375f));

                    break;

                case Orientation.East:

                    legs[0] = new BoundingBox(
                        new Vector3(x: 0.90625f, y: 0.09375f, z: 0.09375f),
                        new Vector3(x: 0.09375f, y: 0.09375f, z: 0.09375f));

                    legs[1] = new BoundingBox(
                        new Vector3(x: 0.90625f, y: 0.09375f, z: 0.90625f),
                        new Vector3(x: 0.09375f, y: 0.09375f, z: 0.09375f));

                    break;

                case Orientation.South:

                    legs[0] = new BoundingBox(
                        new Vector3(x: 0.09375f, y: 0.09375f, z: 0.90625f),
                        new Vector3(x: 0.09375f, y: 0.09375f, z: 0.09375f));

                    legs[1] = new BoundingBox(
                        new Vector3(x: 0.90625f, y: 0.09375f, z: 0.90625f),
                        new Vector3(x: 0.09375f, y: 0.09375f, z: 0.09375f));

                    break;

                case Orientation.West:

                    legs[0] = new BoundingBox(
                        new Vector3(x: 0.09375f, y: 0.09375f, z: 0.09375f),
                        new Vector3(x: 0.09375f, y: 0.09375f, z: 0.09375f));

                    legs[1] = new BoundingBox(
                        new Vector3(x: 0.09375f, y: 0.09375f, z: 0.90625f),
                        new Vector3(x: 0.09375f, y: 0.09375f, z: 0.09375f));

                    break;
            }

            return new BoundingBox(
                new Vector3(x: 0.5f, y: 0.3125f, z: 0.5f),
                new Vector3(x: 0.5f, y: 0.125f, z: 0.5f),
                legs);
        }

        public override BlockMeshData GetMesh(BlockMeshInfo info)
        {
            bool isHead = (info.Data & 0b1) == 1;
            var orientation = (int) ((info.Data & 0b00_0110) >> 1);
            var color = (BlockColor) ((info.Data & 0b11_1000) >> 3);

            return isHead
                ? BlockMeshData.Complex(
                    vertexCountHead,
                    verticesHead[orientation],
                    texIndicesHead,
                    indicesHead,
                    color.ToTintColor())
                : BlockMeshData.Complex(
                    vertexCountEnd,
                    verticesEnd[orientation],
                    texIndicesEnd,
                    indicesEnd,
                    color.ToTintColor());
        }

        internal override bool CanPlace(World world, Vector3i position, PhysicsEntity? entity)
        {
            if (!world.HasSolidGround(position, solidify: true)) return false;

            Orientation orientation = entity?.LookingDirection.ToOrientation() ?? Orientation.North;
            Vector3i otherPosition = orientation.Offset(position);

            return world.GetBlock(otherPosition, out _)?.IsReplaceable == true &&
                   world.HasSolidGround(otherPosition, solidify: true);
        }

        protected override void DoPlace(World world, Vector3i position, PhysicsEntity? entity)
        {
            Orientation orientation = entity?.LookingDirection.ToOrientation() ?? Orientation.North;
            Vector3i otherPosition = orientation.Offset(position);

            world.SetBlock(this, (uint) orientation << 1, position);
            world.SetBlock(this, (uint) (((int) orientation << 1) | 1), otherPosition);

            world.SetSpawnPosition(new Vector3(position.X, y: 1024f, position.Z));
        }

        internal override void DoDestroy(World world, Vector3i position, uint data, PhysicsEntity? entity)
        {
            bool isHead = (data & 0b1) == 1;
            var orientation = (Orientation) ((data & 0b00_0110) >> 1);
            Orientation placementOrientation = isHead ? orientation.Opposite() : orientation;

            world.SetDefaultBlock(position);
            world.SetDefaultBlock(placementOrientation.Offset(position));
        }

        protected override void EntityInteract(PhysicsEntity entity, Vector3i position, uint data)
        {
            bool isHead = (data & 0b1) == 1;

            var orientation = (Orientation) ((data & 0b00_0110) >> 1);
            Orientation placementOrientation = isHead ? orientation.Opposite() : orientation;

            entity.World.SetBlock(this, (data + 0b00_1000) & 0b11_1111, position);

            entity.World.SetBlock(
                this,
                ((data + 0b00_1000) & 0b11_1111) ^ 0b00_0001,
                placementOrientation.Offset(position));
        }

        internal override void BlockUpdate(World world, Vector3i position, uint data, BlockSide side)
        {
            if (side == BlockSide.Bottom && !world.HasSolidGround(position)) Destroy(world, position);
        }
    }
}