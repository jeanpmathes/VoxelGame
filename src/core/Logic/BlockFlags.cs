// <copyright file="BlockInfo.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

namespace VoxelGame.Core.Logic
{
    public record BlockFlags
    {
        public bool IsFull { get; init; }
        public bool IsOpaque { get; init; }
        public bool RenderFaceAtNonOpaques { get; init; }
        public bool IsSolid { get; init; }
        public bool ReceiveCollisions { get; init; }
        public bool IsTrigger { get; init; }
        public bool IsReplaceable { get; init; }
        public bool IsInteractable { get; init; }

        public static BlockFlags Empty => new() { IsReplaceable = true };
        public static BlockFlags Basic => new() { IsOpaque = true, IsSolid = true };
        public static BlockFlags Solid => new() { IsSolid = true };
        public static BlockFlags Replaceable => new() { IsReplaceable = true };
        public static BlockFlags Functional => new() { IsSolid = true, IsInteractable = true };
        public static BlockFlags Collider => new() { IsSolid = true, ReceiveCollisions = true };
        public static BlockFlags Trigger => new() { IsTrigger = true, ReceiveCollisions = true };
    }
}