// <copyright file="VerticalFlow.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using OpenToolkit.Mathematics;

namespace VoxelGame.Core.Logic
{
    /// <summary>
    ///     The vertical flow direction of a fluid.
    /// </summary>
    public enum VerticalFlow
    {
        /// <summary>
        ///     Flows up.
        /// </summary>
        Upwards,

        /// <summary>
        ///     Does not flow.
        /// </summary>
        Static,

        /// <summary>
        ///     Flows down.
        /// </summary>
        Downwards
    }

    /// <summary>
    ///     Extension methods for <see cref="VerticalFlow" />.
    /// </summary>
    public static class VerticalFlowExtensions
    {
        /// <summary>
        ///     Get the flow as a direction vector.
        /// </summary>
        public static Vector3i Direction(this VerticalFlow flow)
        {
            return flow switch
            {
                VerticalFlow.Upwards => (0, 1, 0),
                VerticalFlow.Static => (0, 0, 0),
                VerticalFlow.Downwards => (0, -1, 0),
                _ => (0, 0, 0)
            };
        }

        /// <summary>
        ///     Get the flow encoded as a bit for shaders.
        ///     When encoded, static is ignored.
        /// </summary>
        public static int GetBit(this VerticalFlow flow)
        {
            return flow switch
            {
                VerticalFlow.Upwards => 1,
                VerticalFlow.Static => 0,
                VerticalFlow.Downwards => 0,
                _ => 0
            };
        }

        /// <summary>
        ///     Get the opposite flow.
        /// </summary>
        public static VerticalFlow Opposite(this VerticalFlow flow)
        {
            return flow switch
            {
                VerticalFlow.Upwards => VerticalFlow.Downwards,
                VerticalFlow.Static => VerticalFlow.Static,
                VerticalFlow.Downwards => VerticalFlow.Upwards,
                _ => VerticalFlow.Static
            };
        }

        /// <summary>
        ///     Get the <see cref="BlockSide" /> trough which the flows exists a block.
        /// </summary>
        public static BlockSide ExitSide(this VerticalFlow flow)
        {
            return flow switch
            {
                VerticalFlow.Upwards => BlockSide.Top,
                VerticalFlow.Static => BlockSide.All,
                VerticalFlow.Downwards => BlockSide.Bottom,
                _ => BlockSide.All
            };
        }

        /// <summary>
        ///     Get the <see cref="BlockSide" /> trough which the flows enters a block.
        /// </summary>
        public static BlockSide EntrySide(this VerticalFlow flow)
        {
            return flow.ExitSide().Opposite();
        }
    }
}