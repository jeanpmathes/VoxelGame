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
        private (uint vertexCount, float[][] vertices, int[] textureIndices, uint[] indices) footMesh;
        private (uint vertexCount, float[][] vertices, int[] textureIndices, uint[] indices) headMesh;

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
            BlockModel blockModel = BlockModel.Load(model);

            blockModel.PlaneSplit(Vector3.UnitZ, Vector3.UnitZ, out BlockModel head, out BlockModel foot);
            foot.Move(-Vector3.UnitZ);

            (BlockModel north, BlockModel east, BlockModel south, BlockModel west) headParts =
                head.CreateAllOrientations(rotateTopAndBottomTexture: true);

            (BlockModel north, BlockModel east, BlockModel south, BlockModel west) footParts =
                foot.CreateAllOrientations(rotateTopAndBottomTexture: true);

            AddToMeshes(headParts.north, footParts.north, index: 0);
            AddToMeshes(headParts.east, footParts.east, index: 1);
            AddToMeshes(headParts.south, footParts.south, index: 2);
            AddToMeshes(headParts.west, footParts.west, index: 3);

            void AddToMeshes(BlockModel headPart, BlockModel footPart, int index)
            {
                headPart.ToData(out float[] headVertices, out int[] headTextureIndices, out uint[] headIndices);
                footPart.ToData(out float[] footVertices, out int[] footTextureIndices, out uint[] footIndices);

                if (index == 0)
                {
                    headMesh = ((uint) head.VertexCount, new float[4][], headTextureIndices, headIndices);
                    footMesh = ((uint) foot.VertexCount, new float[4][], footTextureIndices, footIndices);
                }

                headMesh.vertices[index] = headVertices;
                footMesh.vertices[index] = footVertices;
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
                    headMesh.vertexCount,
                    headMesh.vertices[orientation],
                    headMesh.textureIndices,
                    headMesh.indices,
                    color.ToTintColor())
                : BlockMeshData.Complex(
                    footMesh.vertexCount,
                    footMesh.vertices[orientation],
                    footMesh.textureIndices,
                    footMesh.indices,
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
