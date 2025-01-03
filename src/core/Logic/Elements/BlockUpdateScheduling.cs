// <copyright file="BlockUpdateScheduling.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Core.Collections;
using VoxelGame.Core.Logic.Chunks;
using VoxelGame.Core.Serialization;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Core.Logic.Elements;

public partial class Block
{
    private const Int32 ScheduledDestroyOffset = 5;

    /// <summary>
    ///     Schedules an update according to the given update offset.
    ///     Note that the system does not guarantee that the update will be executed exactly at the given offset, as chunks
    ///     could
    ///     be inactive.
    /// </summary>
    /// <param name="world">The world in which the block is.</param>
    /// <param name="position">The position of the block an update should be scheduled for.</param>
    /// <param name="updateOffset">The offset in cycles to when the block should be updated. Must be greater than 0.</param>
    protected void ScheduleUpdate(World world, Vector3i position, UInt32 updateOffset)
    {
        Chunk? chunk = world.GetActiveChunk(position);
        chunk?.ScheduleBlockUpdate(new BlockUpdate(position, this, UpdateOperation.Update), updateOffset);
    }

    /// <summary>
    ///     Schedule the destruction of this block.
    /// </summary>
    /// <param name="world">The world in which the block is located.</param>
    /// <param name="position">The position of the block that will be scheduled to be destroyed.</param>
    protected void ScheduleDestroy(World world, Vector3i position)
    {
        Chunk? chunk = world.GetActiveChunk(position);
        chunk?.ScheduleBlockUpdate(new BlockUpdate(position, this, UpdateOperation.Destroy), ScheduledDestroyOffset);
    }

    internal enum UpdateOperation
    {
        Update,
        Destroy
    }

    internal struct BlockUpdate(Vector3i position, IBlockBase target, UpdateOperation operation) : IUpdateable, IEquatable<BlockUpdate>
    {
        private Int32 x = position.X;
        private Int32 y = position.Y;
        private Int32 z = position.Z;

        private UInt32 target = target.ID;
        private UpdateOperation operation = operation;

        public void Update(World world)
        {
            BlockInstance? potentialBlock = world.GetBlock((x, y, z));

            if (potentialBlock is not {} block) return;
            if (block.Block.ID != target) return;

            switch (operation)
            {
                case UpdateOperation.Update:
                    block.Block.ScheduledUpdate(world, (x, y, z), block.Data);

                    break;

                case UpdateOperation.Destroy:
                    block.Block.Destroy(world, (x, y, z));

                    break;

                default: throw Exceptions.UnsupportedEnumValue(operation);
            }
        }

        public void Serialize(Serializer serializer)
        {
            serializer.Serialize(ref x);
            serializer.Serialize(ref y);
            serializer.Serialize(ref z);
            serializer.Serialize(ref target);
            serializer.Serialize(ref operation);
        }

        public Boolean Equals(BlockUpdate other)
        {
            return (x, y, z, target, operation) == (other.x, other.y, other.z, other.target, other.operation);
        }

        public override Boolean Equals(Object? obj)
        {
            return obj is BlockUpdate other && Equals(other);
        }

#pragma warning disable S2328
        public override Int32 GetHashCode()
        {
            return HashCode.Combine(x, y, z, target, (Int32) operation);
        }
#pragma warning restore S2328

        public static Boolean operator ==(BlockUpdate left, BlockUpdate right)
        {
            return left.Equals(right);
        }

        public static Boolean operator !=(BlockUpdate left, BlockUpdate right)
        {
            return !(left == right);
        }
    }
}
