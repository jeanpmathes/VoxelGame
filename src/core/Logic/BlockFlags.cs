// <copyright file="BlockInfo.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

namespace VoxelGame.Core.Logic
{
    /// <summary>
    ///     Block flags containing different options for a block.
    /// </summary>
    public record BlockFlags
    {
        /// <summary>
        ///     Whether the block is full.
        /// </summary>
        public bool IsFull { get; init; }

        /// <summary>
        ///     Whether the block is opaque.
        /// </summary>
        public bool IsOpaque { get; init; }

        /// <summary>
        ///     Whether faces are rendered at non-opaque blocks.
        /// </summary>
        public bool RenderFaceAtNonOpaques { get; init; }

        /// <summary>
        ///     Whether the block is solid.
        /// </summary>
        public bool IsSolid { get; init; }

        /// <summary>
        ///     Whether the block receives collision.
        /// </summary>
        public bool ReceiveCollisions { get; init; }

        /// <summary>
        ///     Whether the block is a trigger.
        /// </summary>
        public bool IsTrigger { get; init; }

        /// <summary>
        ///     Whether the block is replaceable.
        /// </summary>
        public bool IsReplaceable { get; init; }

        /// <summary>
        ///     Whether the block is interactable.
        /// </summary>
        public bool IsInteractable { get; init; }

        /// <summary>
        ///     Create flags for an empty block.
        /// </summary>
        public static BlockFlags Empty => new() { IsReplaceable = true };

        /// <summary>
        ///     Create flags for a basic block.
        /// </summary>
        public static BlockFlags Basic => new() { IsOpaque = true, IsSolid = true };

        /// <summary>
        ///     Create flags for a solid block.
        /// </summary>
        public static BlockFlags Solid => new() { IsSolid = true };

        /// <summary>
        ///     Create flags for a replaceable block.
        /// </summary>
        public static BlockFlags Replaceable => new() { IsReplaceable = true };

        /// <summary>
        ///     Create flags for a functional block, which is solid and allows interaction.
        /// </summary>
        public static BlockFlags Functional => new() { IsSolid = true, IsInteractable = true };

        /// <summary>
        ///     Create flags for a collider block.
        /// </summary>
        public static BlockFlags Collider => new() { IsSolid = true, ReceiveCollisions = true };

        /// <summary>
        ///     Create flags for a trigger block.
        /// </summary>
        public static BlockFlags Trigger => new() { IsTrigger = true, ReceiveCollisions = true };
    }
}