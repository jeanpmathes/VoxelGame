// <copyright file="IPlayerDataProvider.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using OpenTK.Mathematics;
using VoxelGame.Core.Logic;

namespace VoxelGame.UI.Providers;

/// <summary>
///     Provides data about a player.
/// </summary>
public interface IPlayerDataProvider
{
    /// <summary>
    ///     The current block/fluid mode.
    /// </summary>
    public string Mode { get; }

    /// <summary>
    ///     The current block/fluid selection.
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
    ///     The targeted fluid.
    /// </summary>
    public FluidInstance TargetFluid { get; }
}
