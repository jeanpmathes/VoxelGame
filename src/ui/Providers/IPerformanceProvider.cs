// <copyright file="IPerformanceProvider.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

namespace VoxelGame.UI.Providers
{
    public interface IPerformanceProvider
    {
        public double FPS { get; }
        public double UPS { get; }
    }
}