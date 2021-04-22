// <copyright file="FlatBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
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
    /// This class represents a block with a single face that sticks to other blocks.
    /// Data bit usage: <c>----oo</c>
    /// </summary>
    // o = orientation
    public class FlatBlock : Block, IFillable
    {
        private readonly float climbingVelocity;
        private readonly float slidingVelocity;

        private float[][] sideVertices = null!;
        private int[] textureIndices = null!;

        private uint[] indices = null!;

        private readonly string texture;

        /// <summary>
        /// Creates a FlatBlock, a block with a single face that sticks to other blocks. It allows entities to climb and can use neutral tints.
        /// </summary>
        /// <param name="name">The name of the block.</param>
        /// <param name="namedId">The unique and unlocalized name of this block.</param>
        /// <param name="texture">The texture to use for the block.</param>
        /// <param name="climbingVelocity"></param>
        /// <param name="slidingVelocity"></param>
        internal FlatBlock(string name, string namedId, string texture, float climbingVelocity, float slidingVelocity) :
            base(
                name,
                namedId,
                isFull: false,
                isOpaque: false,
                renderFaceAtNonOpaques: true,
                isSolid: false,
                receiveCollisions: true,
                isTrigger: true,
                isReplaceable: false,
                isInteractable: false,
                BoundingBox.Block,
                TargetBuffer.Complex)
        {
            this.climbingVelocity = climbingVelocity;
            this.slidingVelocity = slidingVelocity;

            this.texture = texture;
        }

        protected override void Setup()
        {
            sideVertices = new[]
            {
                new[] // North
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
                new[] // East
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
                new[] // South
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
                new[] // West
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

            int tex = Game.BlockTextures.GetTextureIndex(texture);
            textureIndices = new[] { tex, tex, tex, tex, tex, tex, tex, tex };

            indices = new uint[]
            {
                0, 2, 1,
                0, 3, 2,
                4, 6, 5,
                4, 7, 6
            };
        }

        protected override BoundingBox GetBoundingBox(int x, int y, int z, uint data)
        {
            return ((Orientation)(data & 0b00_0011)) switch
            {
                Orientation.North => new BoundingBox(new Vector3(x + 0.5f, y + 0.5f, z + 0.95f), new Vector3(0.45f, 0.5f, 0.05f)),
                Orientation.South => new BoundingBox(new Vector3(x + 0.5f, y + 0.5f, z + 0.05f), new Vector3(0.45f, 0.5f, 0.05f)),
                Orientation.West => new BoundingBox(new Vector3(x + 0.95f, y + 0.5f, z + 0.5f), new Vector3(0.05f, 0.5f, 0.45f)),
                Orientation.East => new BoundingBox(new Vector3(x + 0.05f, y + 0.5f, z + 0.5f), new Vector3(0.05f, 0.5f, 0.45f)),
                _ => new BoundingBox(new Vector3(x + 0.5f, y + 0.5f, z + 0.95f), new Vector3(0.5f, 0.5f, 0.05f)),
            };
        }

        public override BlockMeshData GetMesh(BlockMeshInfo info)
        {
            return new BlockMeshData(8, sideVertices[info.Data & 0b00_0011], textureIndices, indices);
        }

        internal override bool CanPlace(int x, int y, int z, PhysicsEntity? entity)
        {
            if (SideToOrientation(entity?.TargetSide ?? BlockSide.Front, out Orientation orientation))
            {
                switch (orientation)
                {
                    case Orientation.North:
                        return Game.World.IsSolid(x, y, z + 1);

                    case Orientation.East:
                        return Game.World.IsSolid(x - 1, y, z);

                    case Orientation.South:
                        return Game.World.IsSolid(x, y, z - 1);

                    case Orientation.West:
                        return Game.World.IsSolid(x + 1, y, z);

                    default:
                        return false;
                }
            }
            else
            {
                return false;
            }
        }

        protected override void DoPlace(int x, int y, int z, PhysicsEntity? entity)
        {
            if (SideToOrientation(entity?.TargetSide ?? BlockSide.Front, out Orientation orientation))
            {
                Game.World.SetBlock(this, (uint)orientation, x, y, z);
            }
            else
            {
                Debug.Fail("Should be able to place.");
            }
        }

        protected override void EntityCollision(PhysicsEntity entity, int x, int y, int z, uint data)
        {
            Vector3 forwardMovement = Vector3.Dot(entity.Movement, entity.Forward) * entity.Forward;

            if (forwardMovement.LengthSquared > 0.1f && (Orientation)(data & 0b00_0011) == (-forwardMovement).ToOrientation())
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

        internal override void BlockUpdate(int x, int y, int z, uint data, BlockSide side)
        {
            CheckBack(x, y, z, side, (Orientation)(data & 0b00_0011), schedule: false);
        }

        protected void CheckBack(int x, int y, int z, BlockSide side, Orientation blockOrientation, bool schedule)
        {
            switch (side)
            {
                case BlockSide.Front:

                    Check(x, y, z + 1, Orientation.North);
                    break;

                case BlockSide.Back:

                    Check(x, y, z - 1, Orientation.South);
                    break;

                case BlockSide.Left:

                    Check(x - 1, y, z, Orientation.East);
                    break;

                case BlockSide.Right:

                    Check(x + 1, y, z, Orientation.West);
                    break;
            }

            void Check(int bx, int by, int bz, Orientation orientation)
            {
                if (blockOrientation == orientation && !Game.World.IsSolid(bx, by, bz))
                {
                    if (schedule) ScheduleDestroy(x, y, z);
                    else Destroy(x, y, z);
                }
            }
        }

        private static bool SideToOrientation(BlockSide side, out Orientation orientation)
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