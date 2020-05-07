// <copyright file="FlatBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using OpenTK;
using VoxelGame.Entities;
using VoxelGame.Physics;
using VoxelGame.Rendering;
using VoxelGame.Utilities;

namespace VoxelGame.Logic.Blocks
{
    /// <summary>
    /// This class represents a block with a single face that sticks to other blocks.
    /// Data bit usage: <c>---oo</c>
    /// </summary>
    // o = orientation
    public class FlatBlock : Block
    {
#pragma warning disable CA1051 // Do not declare visible instance fields
        protected readonly float climbingVelocity;
        protected readonly float slidingVelocity;

        protected float[][] sideVertices;
        protected float[] uv;

        protected uint[] indices =
        {
            0, 1, 2,
            0, 2, 3,
            0, 2, 1,
            0, 3, 2
        };

#pragma warning restore CA1051 // Do not declare visible instance fields

        /// <summary>
        /// Creates a FlatBlock, a block with a single face that sticks to other blocks. It allows entities to climb.
        /// </summary>
        /// <param name="name">The name of the block.</param>
        /// <param name="texture">The texture to use for the block.</param>
        /// <param name="climbinVelocity"></param>
        /// <param name="slidingVelocity"></param>
        public FlatBlock(string name, string texture, float climbingVelocity, float slidingVelocity) :
            base(
                name: name,
                isFull: false,
                isOpaque: false,
                renderFaceAtNonOpaques: true,
                isSolid: false,
                recieveCollisions: true,
                isTrigger: true,
                isReplaceable: false,
                BoundingBox.Block)
        {
            this.climbingVelocity = climbingVelocity;
            this.slidingVelocity = slidingVelocity;

#pragma warning disable CA2214 // Do not call overridable methods in constructors
            this.Setup(texture);
#pragma warning restore CA2214 // Do not call overridable methods in constructors
        }

        protected virtual void Setup(string texture)
        {
            AtlasPosition atlas = Game.Atlas.GetTextureUV(Game.Atlas.GetTextureIndex(texture));

            sideVertices = new float[][]
            {
                new float[] // North
                {
                    0f, 0f, 0.99f,
                    0f, 1f, 0.99f,
                    1f, 1f, 0.99f,
                    1f, 0f, 0.99f
                },
                new float[] // East
                {
                    0.01f, 0f, 0f,
                    0.01f, 1f, 0f,
                    0.01f, 1f, 1f,
                    0.01f, 0f, 1f
                },
                new float[] // South
                {
                    0f, 0f, 0.01f,
                    0f, 1f, 0.01f,
                    1f, 1f, 0.01f,
                    1f, 0f, 0.01f
                },
                new float[] // West
                {
                    0.99f, 0f, 1f,
                    0.99f, 1f, 1f,
                    0.99f, 1f, 0f,
                    0.99f, 0f, 0f
                }
            };

            uv = new float[]
            {
                atlas.bottomLeftU, atlas.bottomLeftV,
                atlas.bottomLeftU, atlas.topRightV,
                atlas.topRightU,   atlas.topRightV,
                atlas.topRightU,   atlas.bottomLeftV
            };
        }

        public override BoundingBox GetBoundingBox(int x, int y, int z)
        {
            Game.World.GetBlock(x, y, z, out byte data);
            switch ((Orientation)(data & 0b0_0011))
            {
                case Orientation.North:
                    return new BoundingBox(new Vector3(x + 0.5f, y + 0.5f, z + 0.95f), new Vector3(0.5f, 0.5f, 0.05f));

                case Orientation.South:
                    return new BoundingBox(new Vector3(x + 0.5f, y + 0.5f, z + 0.05f), new Vector3(0.5f, 0.5f, 0.05f));

                case Orientation.West:
                    return new BoundingBox(new Vector3(x + 0.95f, y + 0.5f, z + 0.5f), new Vector3(0.05f, 0.5f, 0.5f));

                case Orientation.East:
                    return new BoundingBox(new Vector3(x + 0.05f, y + 0.5f, z + 0.5f), new Vector3(0.05f, 0.5f, 0.5f));
            }

            return new BoundingBox(new Vector3(x + 0.5f, y + 0.5f, z + 0.95f), new Vector3(0.5f, 0.5f, 0.05f));
        }

        public override bool Place(int x, int y, int z, PhysicsEntity entity)
        {
            if (Game.World.GetBlock(x, y, z, out _)?.IsReplaceable == false)
            {
                return false;
            }

            if (SideToOrientation(entity?.TargetSide ?? BlockSide.Front, out Orientation orientation))
            {
                if (orientation == Orientation.North && Game.World.GetBlock(x, y, z + 1, out _)?.IsFull == true)
                {
                    Game.World.SetBlock(this, (byte)orientation, x, y, z);

                    return true;
                }

                if (orientation == Orientation.South && Game.World.GetBlock(x, y, z - 1, out _)?.IsFull == true)
                {
                    Game.World.SetBlock(this, (byte)orientation, x, y, z);

                    return true;
                }

                if (orientation == Orientation.East && Game.World.GetBlock(x - 1, y, z, out _)?.IsFull == true)
                {
                    Game.World.SetBlock(this, (byte)orientation, x, y, z);

                    return true;
                }

                if (orientation == Orientation.West && Game.World.GetBlock(x + 1, y, z, out _)?.IsFull == true)
                {
                    Game.World.SetBlock(this, (byte)orientation, x, y, z);

                    return true;
                }

                return false;
            }
            else
            {
                return false;
            }
        }

        public override void BlockUpdate(int x, int y, int z, byte data)
        {
            Orientation orientation = (Orientation)(data & 0b0_0011);

            if (orientation == Orientation.North && (Game.World.GetBlock(x, y, z + 1, out _)?.IsFull != true))
            {
                Destroy(x, y, z, null);
            }

            if (orientation == Orientation.South && (Game.World.GetBlock(x, y, z - 1, out _)?.IsFull != true))
            {
                Destroy(x, y, z, null);
            }

            if (orientation == Orientation.East && (Game.World.GetBlock(x - 1, y, z, out _)?.IsFull != true))
            {
                Destroy(x, y, z, null);
            }

            if (orientation == Orientation.West && (Game.World.GetBlock(x + 1, y, z, out _)?.IsFull != true))
            {
                Destroy(x, y, z, null);
            }
        }

        public override uint GetMesh(BlockSide side, byte data, out float[] vertices, out uint[] indices)
        {
            float[] vert = sideVertices[data & 0b0_0011];

            vertices = new float[]
            {
                vert[0], vert[1],  vert[2],  uv[0], uv[1],
                vert[3], vert[4],  vert[5],  uv[2], uv[3],
                vert[6], vert[7],  vert[8],  uv[4], uv[5],
                vert[9], vert[10], vert[11], uv[6], uv[7],
            };
            indices = this.indices;

            return 4;
        }

        public override void OnCollision(PhysicsEntity entity, int x, int y, int z)
        {
            Game.World.GetBlock(x, y, z, out byte data);
            if ((Orientation)(data & 0b0_0011) == (-entity.Movement).ToOrientation())
            {
                // Check if entity looks up or down
                if (Vector3.CalculateAngle(entity.LookingDirection, Vector3.UnitY) < MathHelper.PiOver2)
                {
                    entity.Velocity = new Vector3(entity.Velocity.X, climbingVelocity, entity.Velocity.Z);
                }
                else
                {
                    entity.Velocity = new Vector3(entity.Velocity.X, -climbingVelocity, entity.Velocity.Z);
                }
            }
            else
            {
                entity.Velocity = new Vector3(entity.Velocity.X, MathHelper.Clamp(entity.Velocity.Y, -slidingVelocity, float.MaxValue), entity.Velocity.Z);
            }
        }

        protected static bool SideToOrientation(BlockSide side, out Orientation orientation)
        {
            switch (side)
            {
                case BlockSide.Front:
                    orientation = Orientation.South;
                    return true;

                case BlockSide.Back:
                    orientation = Orientation.North;
                    return true;

                case BlockSide.Left:
                    orientation = Orientation.West;
                    return true;

                case BlockSide.Right:
                    orientation = Orientation.East;
                    return true;

                default:
                    orientation = Orientation.North;
                    return false;
            }
        }
    }
}