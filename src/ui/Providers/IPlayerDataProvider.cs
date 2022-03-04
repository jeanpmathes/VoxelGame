// <copyright file="IPlayerDataProvider.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using OpenToolkit.Mathematics;
using VoxelGame.Core.Logic;

namespace VoxelGame.UI.Providers
{
    /// <summary>
    ///     Provides data about a player.
    /// </summary>
    public interface IPlayerDataProvider
    {
        /// <summary>
        ///     The current block/liquid mode.
        /// </summary>
        public string Mode { get; }

        /// <summary>
        ///     The current block/liquid selection.
        /// </summary>
        public string Selection { get; }

        /// <summary>
        ///     The targeted position.
        /// </summary>
        public Vector3i TargetPosition { get; }

        /// <summary>
        ///     The position of the player head.
        /// </summary>
        public Vector3i HeadPosition { get; }

        /// <summary>
        ///     The targeted block.
        /// </summary>
        public BlockInstance TargetBlock { get; }

        /// <summary>
        ///     The targeted liquid.
        /// </summary>
        public LiquidInstance TargetLiquid { get; }
    }
}
