// <copyright file="FlatBlock.cs" company="VoxelGame">
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
    /// This class represents a block with a single face that sticks to other blocks.
    /// Data bit usage: <c>---oo</c>
    /// </summary>
    // o = orientation
    public class FlatBlock : Block
    {
        private protected readonly float climbingVelocity;
        private protected readonly float slidingVelocity;

        private protected float[][] sideVertices = null!;
        private protected int[] textureIndices = null!;

        private protected uint[] indices =
        {
            0, 2, 1,
            0, 3, 2,
            4, 6, 5,
            4, 7, 6
        };

        /// <summary>
        /// Creates a FlatBlock, a block with a single face that sticks to other blocks. It allows entities to climb and can use neutral tints.
        /// </summary>
        /// <param name="name">The name of the block.</param>
        /// <param name="texture">The texture to use for the block.</param>
        /// <param name="climbingVelocity"></param>
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
                isInteractable: false,
                BoundingBox.Block,
                TargetBuffer.Complex)
        {
            this.climbingVelocity = climbingVelocity;
            this.slidingVelocity = slidingVelocity;

#pragma warning disable CA2214 // Do not call overridable methods in constructors
            this.Setup(texture);
#pragma warning restore CA2214 // Do not call overridable methods in constructors
        }

        protected virtual void Setup(string texture)
        {
            sideVertices = new float[][]
            {
                new float[] // North
                {
                    1f, 0f, 0.99f, 1f, 0f, 0f, 0f, -1f,
                    1f, 1f, 0.99f, 1f, 1f, 0f, 0f, -1f,
                    0f, 1f, 0.99f, 0f, 1f, 0f, 0f, -1f,
                    0f, 0f, 0.99f, 0f, 0f, 0f, 0f, -1f,

                    0f, 0f, 0.99f, 0f, 0f, 0f, 0f, 1f,
                    0f, 1f, 0.99f, 0f, 1f, 0f, 0f, 1f,
                    1f, 1f, 0.99f, 1f, 1f, 0f, 0f, 1f,
                    1f, 0f, 0.99f, 1f, 0f, 0f, 0f, 1f
                },
                new float[] // East
                {
                    0.01f, 0f, 1f, 1f, 0f, 1f, 0f, 0f,
                    0.01f, 1f, 1f, 1f, 1f, 1f, 0f, 0f,
                    0.01f, 1f, 0f, 0f, 1f, 1f, 0f, 0f,
                    0.01f, 0f, 0f, 0f, 0f, 1f, 0f, 0f,

                    0.01f, 0f, 0f, 0f, 0f, -1f, 0f, 0f,
                    0.01f, 1f, 0f, 0f, 1f, -1f, 0f, 0f,
                    0.01f, 1f, 1f, 1f, 1f, -1f, 0f, 0f,
                    0.01f, 0f, 1f, 1f, 0f, -1f, 0f, 0f
                },
                new float[] // South
                {
                    0f, 0f, 0.01f, 0f, 0f, 0f, 0f, 1f,
                    0f, 1f, 0.01f, 0f, 1f, 0f, 0f, 1f,
                    1f, 1f, 0.01f, 1f, 1f, 0f, 0f, 1f,
                    1f, 0f, 0.01f, 1f, 0f, 0f, 0f, 1f,

                    1f, 0f, 0.01f, 1f, 0f, 0f, 0f, -1f,
                    1f, 1f, 0.01f, 1f, 1f, 0f, 0f, -1f,
                    0f, 1f, 0.01f, 0f, 1f, 0f, 0f, -1f,
                    0f, 0f, 0.01f, 0f, 0f, 0f, 0f, -1f
                },
                new float[] // West
                {
                    0.99f, 0f, 0f, 1f, 0f, -1f, 0f, 0f,
                    0.99f, 1f, 0f, 1f, 1f, -1f, 0f, 0f,
                    0.99f, 1f, 1f, 0f, 1f, -1f, 0f, 0f,
                    0.99f, 0f, 1f, 0f, 0f, -1f, 0f, 0f,

                    0.99f, 0f, 1f, 0f, 0f, 1f, 0f, 0f,
                    0.99f, 1f, 1f, 0f, 1f, 1f, 0f, 0f,
                    0.99f, 1f, 0f, 1f, 1f, 1f, 0f, 0f,
                    0.99f, 0f, 0f, 1f, 0f, 1f, 0f, 0f
                }
            };

            int tex = Game.BlockTextureArray.GetTextureIndex(texture);
            textureIndices = new int[] { tex, tex, tex, tex, tex, tex, tex, tex };
        }

        protected override BoundingBox GetBoundingBox(int x, int y, int z, byte data)
        {
            return ((Orientation)(data & 0b0_0011)) switch
            {
                Orientation.North => new BoundingBox(new Vector3(x + 0.5f, y + 0.5f, z + 0.95f), new Vector3(0.45f, 0.5f, 0.05f)),
                Orientation.South => new BoundingBox(new Vector3(x + 0.5f, y + 0.5f, z + 0.05f), new Vector3(0.45f, 0.5f, 0.05f)),
                Orientation.West => new BoundingBox(new Vector3(x + 0.95f, y + 0.5f, z + 0.5f), new Vector3(0.05f, 0.5f, 0.45f)),
                Orientation.East => new BoundingBox(new Vector3(x + 0.05f, y + 0.5f, z + 0.5f), new Vector3(0.05f, 0.5f, 0.45f)),
                _ => new BoundingBox(new Vector3(x + 0.5f, y + 0.5f, z + 0.95f), new Vector3(0.5f, 0.5f, 0.05f)),
            };
        }

        public override uint GetMesh(BlockSide side, byte data, out float[] vertices, out int[] textureIndices, out uint[] indices, out TintColor tint)
        {
            vertices = sideVertices[data & 0b0_0011];
            textureIndices = this.textureIndices;
            indices = this.indices;

            tint = TintColor.None;

            return 8;
        }

        protected override bool Place(int x, int y, int z, bool? replaceable, PhysicsEntity? entity)
        {
            if (replaceable != true)
            {
                return false;
            }

            if (SideToOrientation(entity?.TargetSide ?? BlockSide.Front, out Orientation orientation))
            {
                if (orientation == Orientation.North && Game.World.GetBlock(x, y, z + 1, out _)?.IsSolidAndFull == true)
                {
                    Game.World.SetBlock(this, (byte)orientation, x, y, z);

                    return true;
                }

                if (orientation == Orientation.South && Game.World.GetBlock(x, y, z - 1, out _)?.IsSolidAndFull == true)
                {
                    Game.World.SetBlock(this, (byte)orientation, x, y, z);

                    return true;
                }

                if (orientation == Orientation.East && Game.World.GetBlock(x - 1, y, z, out _)?.IsSolidAndFull == true)
                {
                    Game.World.SetBlock(this, (byte)orientation, x, y, z);

                    return true;
                }

                if (orientation == Orientation.West && Game.World.GetBlock(x + 1, y, z, out _)?.IsSolidAndFull == true)
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

        protected override void EntityCollision(PhysicsEntity entity, int x, int y, int z, byte data)
        {
            Vector3 forwardMovement = Vector3.Dot(entity.Movement, entity.Forward) * entity.Forward;

            if (forwardMovement.LengthSquared > 0.1f && (Orientation)(data & 0b0_0011) == (-forwardMovement).ToOrientation())
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

        internal override void BlockUpdate(int x, int y, int z, byte data)
        {
            Orientation orientation = (Orientation)(data & 0b0_0011);

            if (orientation == Orientation.North && (Game.World.GetBlock(x, y, z + 1, out _)?.IsSolidAndFull != true))
            {
                Destroy(x, y, z, null);
            }

            if (orientation == Orientation.South && (Game.World.GetBlock(x, y, z - 1, out _)?.IsSolidAndFull != true))
            {
                Destroy(x, y, z, null);
            }

            if (orientation == Orientation.East && (Game.World.GetBlock(x - 1, y, z, out _)?.IsSolidAndFull != true))
            {
                Destroy(x, y, z, null);
            }

            if (orientation == Orientation.West && (Game.World.GetBlock(x + 1, y, z, out _)?.IsSolidAndFull != true))
            {
                Destroy(x, y, z, null);
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