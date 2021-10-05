// <copyright file="VerticalFlow.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using OpenToolkit.Mathematics;

namespace VoxelGame.Core.Logic
{
    public enum VerticalFlow
    {
        Upwards,
        Static,
        Downwards
    }

    public static class VerticalFlowExtensions
    {
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

        public static BlockSide EntrySide(this VerticalFlow flow)
        {
            return flow.ExitSide().Opposite();
        }
    }
}