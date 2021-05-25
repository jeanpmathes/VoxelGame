// <copyright file="BedBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System.Diagnostics;
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
        private readonly float[][] verticesHead = new float[4][];
        private readonly float[][] verticesEnd = new float[4][];

        private int[] texIndicesHead = null!;
        private int[] texIndicesEnd = null!;

        private uint[] indicesHead = null!;
        private uint[] indicesEnd = null!;

        private uint vertexCountHead;
        private uint vertexCountEnd;

        private readonly string model;

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
                new BoundingBox(new Vector3(0.5f, 0.21875f, 0.5f), new Vector3(0.5f, 0.21875f, 0.5f)),
                TargetBuffer.Complex)
        {
            this.model = model;
        }

        protected override void Setup(ITextureIndexProvider indexProvider)
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

        protected override BoundingBox GetBoundingBox(uint data)
        {
            bool isBase = (data & 0b1) == 1;
            Orientation orientation = (Orientation)((data & 0b00_0110) >> 1);

            BoundingBox[] legs = new BoundingBox[2];

            switch (isBase ? orientation : orientation.Invert())
            {
                case Orientation.North:

                    legs[0] = new BoundingBox(new Vector3(0.09375f, 0.09375f, 0.09375f), new Vector3(0.09375f, 0.09375f, 0.09375f));
                    legs[1] = new BoundingBox(new Vector3(0.90625f, 0.09375f, 0.09375f), new Vector3(0.09375f, 0.09375f, 0.09375f));

                    break;

                case Orientation.East:

                    legs[0] = new BoundingBox(new Vector3(0.90625f, 0.09375f, 0.09375f), new Vector3(0.09375f, 0.09375f, 0.09375f));
                    legs[1] = new BoundingBox(new Vector3(0.90625f, 0.09375f, 0.90625f), new Vector3(0.09375f, 0.09375f, 0.09375f));

                    break;

                case Orientation.South:

                    legs[0] = new BoundingBox(new Vector3(0.09375f, 0.09375f, 0.90625f), new Vector3(0.09375f, 0.09375f, 0.09375f));
                    legs[1] = new BoundingBox(new Vector3(0.90625f, 0.09375f, 0.90625f), new Vector3(0.09375f, 0.09375f, 0.09375f));

                    break;

                case Orientation.West:

                    legs[0] = new BoundingBox(new Vector3(0.09375f, 0.09375f, 0.09375f), new Vector3(0.09375f, 0.09375f, 0.09375f));
                    legs[1] = new BoundingBox(new Vector3(0.09375f, 0.09375f, 0.90625f), new Vector3(0.09375f, 0.09375f, 0.09375f));

                    break;
            }

            return new BoundingBox(new Vector3(0.5f, 0.3125f, 0.5f), new Vector3(0.5f, 0.125f, 0.5f), legs);
        }

        public override BlockMeshData GetMesh(BlockMeshInfo info)
        {
            bool isHead = (info.Data & 0b1) == 1;
            int orientation = (int)((info.Data & 0b00_0110) >> 1);
            BlockColor color = (BlockColor)((info.Data & 0b11_1000) >> 3);

            return isHead
                ? new BlockMeshData(vertexCountHead, verticesHead[orientation], texIndicesHead, indicesHead, color.ToTintColor())
                : new BlockMeshData(vertexCountEnd, verticesEnd[orientation], texIndicesEnd, indicesEnd, color.ToTintColor());
        }

        internal override bool CanPlace(World world, int x, int y, int z, PhysicsEntity? entity)
        {
            if (!world.HasSolidGround(x, y, z))
            {
                return false;
            }

            return (entity?.LookingDirection.ToOrientation() ?? Orientation.North) switch
            {
                Orientation.North => world.GetBlock(x, y, z - 1, out _)?.IsReplaceable == true && world.HasSolidGround(x, y, z - 1),
                Orientation.East => world.GetBlock(x + 1, y, z, out _)?.IsReplaceable == true && world.HasSolidGround(x + 1, y, z),
                Orientation.South => world.GetBlock(x, y, z + 1, out _)?.IsReplaceable == true && world.HasSolidGround(x, y, z + 1),
                Orientation.West => world.GetBlock(x - 1, y, z, out _)?.IsReplaceable == true && world.HasSolidGround(x - 1, y, z),
                _ => false,
            };
        }

        protected override void DoPlace(World world, int x, int y, int z, PhysicsEntity? entity)
        {
            switch (entity?.LookingDirection.ToOrientation() ?? Orientation.North)
            {
                case Orientation.North:

                    world.SetBlock(this, (int)Orientation.North << 1, x, y, z);
                    world.SetBlock(this, ((int)Orientation.North << 1) | 1, x, y, z - 1);

                    world.SetSpawnPosition(new Vector3(x, 1024f, z));

                    break;

                case Orientation.East:

                    world.SetBlock(this, (int)Orientation.East << 1, x, y, z);
                    world.SetBlock(this, ((int)Orientation.East << 1) | 1, x + 1, y, z);

                    world.SetSpawnPosition(new Vector3(x, 1024f, z));

                    break;

                case Orientation.South:

                    world.SetBlock(this, (int)Orientation.South << 1, x, y, z);
                    world.SetBlock(this, ((int)Orientation.South << 1) | 1, x, y, z + 1);

                    world.SetSpawnPosition(new Vector3(x, 1024f, z));

                    break;

                case Orientation.West:

                    world.SetBlock(this, (int)Orientation.West << 1, x, y, z);
                    world.SetBlock(this, ((int)Orientation.West << 1) | 1, x - 1, y, z);

                    world.SetSpawnPosition(new Vector3(x, 1024f, z));

                    break;
            }
        }

        internal override void DoDestroy(World world, int x, int y, int z, uint data, PhysicsEntity? entity)
        {
            bool isHead = (data & 0b1) == 1;

            switch ((Orientation)((data & 0b00_0110) >> 1))
            {
                case Orientation.North:

                    isHead = !isHead;

                    world.SetDefaultBlock(x, y, z);
                    world.SetDefaultBlock(x, y, z - (isHead ? 1 : -1));
                    break;

                case Orientation.East:

                    world.SetDefaultBlock(x, y, z);
                    world.SetDefaultBlock(x - (isHead ? 1 : -1), y, z);
                    break;

                case Orientation.South:

                    world.SetDefaultBlock(x, y, z);
                    world.SetDefaultBlock(x, y, z - (isHead ? 1 : -1));
                    break;

                case Orientation.West:

                    isHead = !isHead;

                    world.SetDefaultBlock(x, y, z);
                    world.SetDefaultBlock(x - (isHead ? 1 : -1), y, z);
                    break;
            }
        }

        protected override void EntityInteract(PhysicsEntity entity, int x, int y, int z, uint data)
        {
            bool isHead = (data & 0b1) == 1;

            switch ((Orientation)((data & 0b00_0110) >> 1))
            {
                case Orientation.North:

                    isHead = !isHead;

                    entity.World.SetBlock(this, data + 0b00_1000 & 0b11_1111, x, y, z);
                    entity.World.SetBlock(this, (data + 0b00_1000 & 0b11_1111) ^ 0b00_0001, x, y, z - (isHead ? 1 : -1));
                    break;

                case Orientation.East:

                    entity.World.SetBlock(this, data + 0b00_1000 & 0b11_1111, x, y, z);
                    entity.World.SetBlock(this, (data + 0b00_1000 & 0b11_1111) ^ 0b00_0001, x - (isHead ? 1 : -1), y, z);
                    break;

                case Orientation.South:

                    entity.World.SetBlock(this, data + 0b00_1000 & 0b11_1111, x, y, z);
                    entity.World.SetBlock(this, (data + 0b00_1000 & 0b11_1111) ^ 0b00_0001, x, y, z - (isHead ? 1 : -1));
                    break;

                case Orientation.West:

                    isHead = !isHead;

                    entity.World.SetBlock(this, data + 0b00_1000 & 0b01_1111, x, y, z);
                    entity.World.SetBlock(this, (data + 0b00_1000 & 0b01_1111) ^ 0b00_0001, x - (isHead ? 1 : -1), y, z);
                    break;
            }
        }

        internal override void BlockUpdate(World world, int x, int y, int z, uint data, BlockSide side)
        {
            if (side == BlockSide.Bottom && !world.HasSolidGround(x, y, z))
            {
                Destroy(world, x, y, z);
            }
        }
    }
}