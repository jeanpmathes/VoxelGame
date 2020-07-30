// <copyright file="Block.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using VoxelGame.Physics;
using VoxelGame.Visuals;

namespace VoxelGame.Logic
{
    /// <summary>
    /// The basic block class. Blocks are used to construct the world.
    /// </summary>
    public abstract partial class Block : IBlockBase
    {
        /// <summary>
        /// Gets the block id which can be any value from 0 to 4095.
        /// </summary>
        public uint Id { get; }

        /// <summary>
        /// Gets the localized name of the block.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// An unlocalized string that identifies this block.
        /// </summary>
        public string NamedId { get; }

        /// <summary>
        /// Gets whether this block completely fills a 1x1x1 volume or not.
        /// </summary>
        public bool IsFull { get; }

        /// <summary>
        /// Gets whether it is possible to see through this block. This will affect the rendering of this block and the blocks around him.
        /// </summary>
        public bool IsOpaque { get; }

        /// <summary>
        /// This property is only relevant for non-opaque full blocks. It decides if their faces should be rendered next to another non-opaque block.
        /// </summary>
        public bool RenderFaceAtNonOpaques { get; }

        /// <summary>
        /// Gets whether this block hinders movement.
        /// </summary>
        public bool IsSolid { get; }

        /// <summary>
        /// Gets whether the collision method should be called in case of a collision with an entity.
        /// </summary>
        public bool RecieveCollisions { get; }

        /// <summary>
        /// Gets whether this block should be checked in collision calculations even if it is not solid.
        /// </summary>
        public bool IsTrigger { get; }

        /// <summary>
        /// Gets whether this block can be replaced when placing a block.
        /// </summary>
        public bool IsReplaceable { get; }

        /// <summary>
        /// Gets whether this block responds to interactions.
        /// </summary>
        public bool IsInteractable { get; }

        /// <summary>
        /// Gets the section buffer this blocks mesh data should be stored in.
        /// </summary>
        public TargetBuffer TargetBuffer { get; }

        /// <summary>
        /// Gets whether this block is solid and full.
        /// </summary>
        public bool IsSolidAndFull => IsSolid && IsFull;

        private BoundingBox boundingBox;

        protected Block(string name, string namedId, bool isFull, bool isOpaque, bool renderFaceAtNonOpaques, bool isSolid, bool recieveCollisions, bool isTrigger, bool isReplaceable, bool isInteractable, BoundingBox boundingBox, TargetBuffer targetBuffer)
        {
            Name = name;
            NamedId = namedId;

            IsFull = isFull;
            IsOpaque = isOpaque;
            RenderFaceAtNonOpaques = renderFaceAtNonOpaques;
            IsSolid = isSolid;
            RecieveCollisions = recieveCollisions;
            IsTrigger = isTrigger;
            IsReplaceable = isReplaceable;
            IsInteractable = isInteractable;

            this.boundingBox = boundingBox;

            TargetBuffer = targetBuffer;

            if (targetBuffer == TargetBuffer.Simple && !isFull)
            {
                throw new System.ArgumentException($"TargetBuffer '{nameof(TargetBuffer.Simple)}' requires {nameof(isFull)} to be {!isFull}.", nameof(targetBuffer));
            }

            if (blockDictionary.Count < BlockLimit)
            {
                blockDictionary.Add((uint)blockDictionary.Count, this);
                namedBlockDictionary.Add(namedId, this);

                Id = (uint)(blockDictionary.Count - 1);
            }
            else
            {
                throw new System.InvalidOperationException($"Not more than {BlockLimit} blocks are allowed.");
            }
        }

        /// <summary>
        /// Called by the base block constructor, meant to setup vertex data, indices etc.
        /// </summary>
        protected virtual void Setup()
        {
        }

        /// <summary>
        /// Returns the bounding box of this block if it would be at the given position.
        /// </summary>
        /// <param name="x">The x position.</param>
        /// <param name="y">The y position.</param>
        /// <param name="z">The z position.</param>
        /// <returns>The bounding box.</returns>
        public BoundingBox GetBoundingBox(int x, int y, int z)
        {
            if (Game.World.GetBlock(x, y, z, out uint data) == this)
            {
                return GetBoundingBox(x, y, z, data);
            }
            else
            {
                return boundingBox.Translated(x, y, z);
            }
        }

        protected virtual BoundingBox GetBoundingBox(int x, int y, int z, uint data)
        {
            return boundingBox.Translated(x, y, z);
        }

        /// <summary>
        /// Returns the mesh of a block side at a certain position.
        /// </summary>
        /// <param name="side">The side of the block that is required.</param>
        /// <param name="data">The block data of the block at the position.</param>
        /// <param name="vertices">Vertices of the mesh. Every vertex is made up of 8 floats: XYZ, UV, NOP</param>
        /// <param name="indices">The indices of the mesh that determine how triangles are constructed.</param>
        /// <returns>The amount of vertices in the mesh.</returns>
        public abstract uint GetMesh(BlockSide side, uint data, out float[] vertices, out int[] textureIndices, out uint[] indices, out TintColor tint, out bool isAnimated);

        public bool Place(int x, int y, int z, Entities.PhysicsEntity? entity = null)
        {
            return Game.World.GetBlock(x, y, z, out _)?.IsReplaceable == true && Place(entity, x, y, z);
        }

        protected virtual bool Place(Entities.PhysicsEntity? entity, int x, int y, int z)
        {
            Game.World.SetBlock(this, 0, x, y, z);

            return true;
        }

        public bool Destroy(int x, int y, int z, Entities.PhysicsEntity? entity = null)
        {
            if (Game.World.GetBlock(x, y, z, out uint data) == this)
            {
                return Destroy(entity, x, y, z, data);
            }
            else
            {
                return false;
            }
        }

        protected virtual bool Destroy(Entities.PhysicsEntity? entity, int x, int y, int z, uint data)
        {
            Game.World.SetBlock(Block.Air, 0, x, y, z);

            return true;
        }

        /// <summary>
        /// This method is called when an entity collides with this block.
        /// </summary>
        /// <param name="entity">The entity that caused the collision.</param>
        /// <param name="x">The x position of the block the entity collided with.</param>
        /// <param name="y">The y position of the block the entity collided with.</param>
        /// <param name="z">The z position of the block the entity collided with.</param>
        public void EntityCollision(Entities.PhysicsEntity entity, int x, int y, int z)
        {
            if (Game.World.GetBlock(x, y, z, out uint data) == this)
            {
                EntityCollision(entity, x, y, z, data);
            }
        }

        protected virtual void EntityCollision(Entities.PhysicsEntity entity, int x, int y, int z, uint data)
        {
        }

        /// <summary>
        /// Called when a block and an entity collide.
        /// </summary>
        /// <param name="entity">The entity that collided with the block.</param>
        /// <param name="x">The x position of the block.</param>
        /// <param name="y">The y position of the block.</param>
        /// <param name="z">The z position of the block.</param>
        public void EntityInteract(Entities.PhysicsEntity entity, int x, int y, int z)
        {
            if (Game.World.GetBlock(x, y, z, out uint data) == this)
            {
                EntityInteract(entity, x, y, z, data);
            }
        }

        protected virtual void EntityInteract(Entities.PhysicsEntity entity, int x, int y, int z, uint data)
        {
        }

        /// <summary>
        /// This method is called on blocks next to a position that was changed.
        /// </summary>
        /// <param name="x">The x position of the block next to the changed position.</param>
        /// <param name="y">The y position of the block next to the changed position.</param>
        /// <param name="z">The z position of the block next to the changed position.</param>
        /// <param name="data">The data of the block next to the changed position.</param>
        /// <param name="side">The side of the block where the change happened.</param>
        internal virtual void BlockUpdate(int x, int y, int z, uint data, BlockSide side)
        {
        }

        /// <summary>
        /// This method is called randomly on some blocks every update.
        /// </summary>
        internal virtual void RandomUpdate(int x, int y, int z, uint data)
        {
        }

        public sealed override string ToString()
        {
            return NamedId;
        }

        public sealed override bool Equals(object? obj)
        {
            return ReferenceEquals(this, obj);
        }

        public sealed override int GetHashCode()
        {
            return (int)Id;
        }
    }
}