// <copyright file="FlatBlock.cs" company="VoxelGame">
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
    ///     This class represents a block with a single face that sticks to other blocks.
    ///     Data bit usage: <c>----oo</c>
    /// </summary>
    // o = orientation
    public class FlatBlock : Block, IFillable
    {
        private static readonly float[][] sideVertices =
        {
            CreateSide(Orientation.North),
            CreateSide(Orientation.East),
            CreateSide(Orientation.South),
            CreateSide(Orientation.West)
        };

        private readonly float climbingVelocity;
        private readonly float slidingVelocity;

        private readonly string texture;

        private uint[] indices = null!;

        private int[] textureIndices = null!;

        /// <summary>
        ///     Creates a FlatBlock, a block with a single face that sticks to other blocks. It allows entities to climb and can
        ///     use neutral tints.
        /// </summary>
        /// <param name="name">The name of the block.</param>
        /// <param name="namedId">The unique and unlocalized name of this block.</param>
        /// <param name="texture">The texture to use for the block.</param>
        /// <param name="climbingVelocity">The velocity of players climbing the block.</param>
        /// <param name="slidingVelocity">The velocity of players sliding along the block.</param>
        internal FlatBlock(string name, string namedId, string texture, float climbingVelocity, float slidingVelocity) :
            base(
                name,
                namedId,
                BlockFlags.Trigger,
                BoundingBox.Block,
                TargetBuffer.Complex)
        {
            this.climbingVelocity = climbingVelocity;
            this.slidingVelocity = slidingVelocity;

            this.texture = texture;
        }

        private static float[] CreateSide(Orientation orientation)
        {
            return BlockModels.CreateFlatModel(orientation.ToBlockSide().Opposite(), offset: 0.01f);
        }

        protected override void Setup(ITextureIndexProvider indexProvider)
        {
            indices = BlockModels.GenerateIndexDataArray(faces: 2);
            textureIndices = BlockModels.GenerateTextureDataArray(indexProvider.GetTextureIndex(texture), length: 8);
        }

        protected override BoundingBox GetBoundingBox(uint data)
        {
            return (Orientation) (data & 0b00_0011) switch
            {
                Orientation.North => new BoundingBox(
                    new Vector3(x: 0.5f, y: 0.5f, z: 0.95f),
                    new Vector3(x: 0.45f, y: 0.5f, z: 0.05f)),
                Orientation.South => new BoundingBox(
                    new Vector3(x: 0.5f, y: 0.5f, z: 0.05f),
                    new Vector3(x: 0.45f, y: 0.5f, z: 0.05f)),
                Orientation.West => new BoundingBox(
                    new Vector3(x: 0.95f, y: 0.5f, z: 0.5f),
                    new Vector3(x: 0.05f, y: 0.5f, z: 0.45f)),
                Orientation.East => new BoundingBox(
                    new Vector3(x: 0.05f, y: 0.5f, z: 0.5f),
                    new Vector3(x: 0.05f, y: 0.5f, z: 0.45f)),
                _ => new BoundingBox(new Vector3(x: 0.5f, y: 0.5f, z: 0.95f), new Vector3(x: 0.5f, y: 0.5f, z: 0.05f))
            };
        }

        public override BlockMeshData GetMesh(BlockMeshInfo info)
        {
            return BlockMeshData.Complex(vertexCount: 8, sideVertices[info.Data & 0b00_0011], textureIndices, indices);
        }

        internal override bool CanPlace(World world, Vector3i position, PhysicsEntity? entity)
        {
            BlockSide side = entity?.TargetSide ?? BlockSide.Front;

            if (!side.IsLateral()) side = BlockSide.Back;
            var orientation = side.ToOrientation();

            return world.IsSolid(orientation.Opposite().Offset(position));
        }

        protected override void DoPlace(World world, Vector3i position, PhysicsEntity? entity)
        {
            BlockSide side = entity?.TargetSide ?? BlockSide.Front;
            if (!side.IsLateral()) side = BlockSide.Back;
            world.SetBlock(this.AsInstance((uint) side.ToOrientation()), position);
        }

        protected override void EntityCollision(PhysicsEntity entity, Vector3i position, uint data)
        {
            Vector3 forwardMovement = Vector3.Dot(entity.Movement, entity.Forward) * entity.Forward;

            if (forwardMovement.LengthSquared > 0.1f &&
                (Orientation) (data & 0b00_0011) == (-forwardMovement).ToOrientation())
                // Check if entity looks up or down
                entity.Velocity = Vector3.CalculateAngle(entity.LookingDirection, Vector3.UnitY) < MathHelper.PiOver2
                    ? new Vector3(entity.Velocity.X, climbingVelocity, entity.Velocity.Z)
                    : new Vector3(entity.Velocity.X, -climbingVelocity, entity.Velocity.Z);
            else
                entity.Velocity = new Vector3(
                    entity.Velocity.X,
                    MathHelper.Clamp(entity.Velocity.Y, -slidingVelocity, float.MaxValue),
                    entity.Velocity.Z);
        }

        internal override void BlockUpdate(World world, Vector3i position, uint data, BlockSide side)
        {
            CheckBack(world, position, side, (Orientation) (data & 0b00_0011), schedule: false);
        }

        protected void CheckBack(World world, Vector3i position, BlockSide side, Orientation blockOrientation,
            bool schedule)
        {
            if (!side.IsLateral()) return;

            if (blockOrientation != side.ToOrientation().Opposite() ||
                world.IsSolid(blockOrientation.Opposite().Offset(position))) return;

            if (schedule) ScheduleDestroy(world, position);
            else Destroy(world, position);
        }
    }
}