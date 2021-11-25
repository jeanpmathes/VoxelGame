// <copyright file="IPlayerDataProvider.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using OpenToolkit.Mathematics;
using VoxelGame.Core.Logic;

namespace VoxelGame.UI.Providers
{
    public interface IPlayerDataProvider
    {
        public string Mode { get; }
        public string Selection { get; }

        public Vector3i TargetPosition { get; }
        public Vector3i HeadPosition { get; }

        public BlockInstance TargetBlock { get; }
        public LiquidInstance TargetLiquid { get; }
    }
}
