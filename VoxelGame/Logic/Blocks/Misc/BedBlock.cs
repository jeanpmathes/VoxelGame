// <copyright file="BedBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using OpenToolkit.Mathematics;
using VoxelGame.Entities;
using VoxelGame.Visuals;
using VoxelGame.Utilities;
using VoxelGame.Physics;

namespace VoxelGame.Logic.Blocks
{
    /// <summary>
    /// A block that is two blocks long and allows setting the spawn point.
    /// Data bit usage: <c>ccoop</c>
    /// </summary>
    // c = color
    // o = orientation
    // p = position
    public class BedBlock : Block
    {
        private protected float[][] topVertices = new float[4][];
        private protected float[][] bottomVertices = new float[4][];

        private protected int[] topTextureIndices = null!;
        private protected int[] bottomTextureIndices = null!;

        private protected uint[] topIndices = null!;
        private protected uint[] bottomIndices = null!;

        private protected uint topVertCount;
        private protected uint bottomVertCount;

        public BedBlock(string name, string model) :
            base(
                name,
                isFull: false,
                isOpaque: false,
                renderFaceAtNonOpaques: true,
                isSolid: true,
                recieveCollisions: false,
                isTrigger: false,
                isReplaceable: false,
                new BoundingBox(new Vector3(0.5f, 0.21875f, 0.5f), new Vector3(0.5f, 0.21875f, 0.5f)),
                TargetBuffer.Complex)
        {
#pragma warning disable CA2214 // Do not call overridable methods in constructors
            Setup(BlockModel.Load(model));
#pragma warning restore CA2214 // Do not call overridable methods in constructors
        }

        public virtual void Setup(BlockModel model)
        {
            model.PlaneSplit(Vector3.UnitZ, Vector3.UnitZ, out BlockModel top, out BlockModel bottom);
            bottom.Move(-Vector3.UnitZ);

            topVertCount = (uint)top.VertexCount;
            bottomVertCount = (uint)bottom.VertexCount;

            for (int i = 0; i < 4; i++)
            {
                if (i == 0)
                {
                    top.ToData(out topVertices[i], out topTextureIndices, out topIndices);
                    bottom.ToData(out bottomVertices[i], out bottomTextureIndices, out bottomIndices);
                }
                else
                {
                    top.RotateY(1, true);
                    top.ToData(out topVertices[i], out _, out _);

                    bottom.RotateY(1, true);
                    bottom.ToData(out bottomVertices[i], out _, out _);
                }
            }
        }

        protected override BoundingBox GetBoundingBox(int x, int y, int z, byte data)
        {
            bool isBase = (data & 0b1) == 1;
            Orientation orientation = (Orientation)((data & 0b0_0110) >> 1);

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

        public override uint GetMesh(BlockSide side, byte data, out float[] vertices, out int[] textureIndices, out uint[] indices, out TintColor tint)
        {
            bool isBase = (data & 0b1) == 1;
            int orientation = (data & 0b0_0110) >> 1;
            BlockColor color = (BlockColor)((data & 0b1_1000) >> 3);

            if (isBase)
            {
                vertices = topVertices[orientation];
                textureIndices = topTextureIndices;
                indices = topIndices;

                tint = color.ToTintColor();

                return topVertCount;
            }
            else
            {
                vertices = bottomVertices[orientation];
                textureIndices = bottomTextureIndices;
                indices = bottomIndices;

                tint = color.ToTintColor();

                return bottomVertCount;
            }
        }

        protected override bool Place(int x, int y, int z, bool? replaceable, PhysicsEntity? entity)
        {
            if (replaceable != true || Game.World.GetBlock(x, y - 1, z, out _)?.IsSolidAndFull != true)
            {
                return false;
            }

            int colorData = (x & 0b11) << 3;

            switch (entity?.LookingDirection.ToOrientation() ?? Orientation.North)
            {
                case Orientation.North:

                    if (Game.World.GetBlock(x, y, z - 1, out _)?.IsReplaceable != true || Game.World.GetBlock(x, y - 1, z - 1, out _)?.IsSolidAndFull != true)
                    {
                        return false;
                    }

                    Game.World.SetBlock(this, (byte)(colorData | ((int)Orientation.North << 1) | 0), x, y, z);
                    Game.World.SetBlock(this, (byte)(colorData | ((int)Orientation.North << 1) | 1), x, y, z - 1);

                    return true;

                case Orientation.East:

                    if (Game.World.GetBlock(x + 1, y, z, out _)?.IsReplaceable != true || Game.World.GetBlock(x + 1, y - 1, z, out _)?.IsSolidAndFull != true)
                    {
                        return false;
                    }

                    Game.World.SetBlock(this, (byte)(colorData | ((int)Orientation.East << 1) | 0), x, y, z);
                    Game.World.SetBlock(this, (byte)(colorData | ((int)Orientation.East << 1) | 1), x + 1, y, z);

                    return true;

                case Orientation.South:

                    if (Game.World.GetBlock(x, y, z + 1, out _)?.IsReplaceable != true || Game.World.GetBlock(x, y - 1, z + 1, out _)?.IsSolidAndFull != true)
                    {
                        return false;
                    }

                    Game.World.SetBlock(this, (byte)(colorData | ((int)Orientation.South << 1) | 0), x, y, z);
                    Game.World.SetBlock(this, (byte)(colorData | ((int)Orientation.South << 1) | 1), x, y, z + 1);

                    return true;

                case Orientation.West:

                    if (Game.World.GetBlock(x - 1, y, z, out _)?.IsReplaceable != true || Game.World.GetBlock(x - 1, y - 1, z, out _)?.IsSolidAndFull != true)
                    {
                        return false;
                    }

                    Game.World.SetBlock(this, (byte)(colorData | ((int)Orientation.West << 1) | 0), x, y, z);
                    Game.World.SetBlock(this, (byte)(colorData | ((int)Orientation.West << 1) | 1), x - 1, y, z);

                    return true;

                default:
                    return false;
            }
        }

        protected override bool Destroy(int x, int y, int z, byte data, PhysicsEntity? entity)
        {
            bool isBase = (data & 0b1) == 1;

            switch ((Orientation)((data & 0b0_0110) >> 1))
            {
                case Orientation.North:

                    isBase = !isBase;
                    goto case Orientation.South;

                case Orientation.East:

                    Game.World.SetBlock(Block.AIR, 0, x, y, z);
                    Game.World.SetBlock(Block.AIR, 0, x - (isBase ? 1 : -1), y, z);
                    return true;

                case Orientation.South:

                    Game.World.SetBlock(Block.AIR, 0, x, y, z);
                    Game.World.SetBlock(Block.AIR, 0, x, y, z - (isBase ? 1 : -1));
                    return true;

                case Orientation.West:

                    isBase = !isBase;
                    goto case Orientation.East;

                default:
                    return false;
            }
        }

        internal override void BlockUpdate(int x, int y, int z, byte data)
        {
            if (Game.World.GetBlock(x, y - 1, z, out _)?.IsSolidAndFull != true)
            {
                Destroy(x, y, z, null);
            }
        }
    }
}