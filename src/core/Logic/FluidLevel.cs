// <copyright file="FluidLevel.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

namespace VoxelGame.Core.Logic;

/// <summary>
///     The level or amount of fluid. A position is split into 8 equal parts.
/// </summary>
public enum FluidLevel
{
    /// <summary>
    ///     One, or 125L.
    /// </summary>
    One = 0,

    /// <summary>
    ///     Two, or 250L.
    /// </summary>
    Two = 1,

    /// <summary>
    ///     Three, or 375L.
    /// </summary>
    Three = 2,

    /// <summary>
    ///     Four, or 500L.
    /// </summary>
    Four = 3,

    /// <summary>
    ///     Five, or 625L.
    /// </summary>
    Five = 4,

    /// <summary>
    ///     Six, or 750L.
    /// </summary>
    Six = 5,

    /// <summary>
    ///     Seven, or 875L.
    /// </summary>
    Seven = 6,

    /// <summary>
    ///     Eight, or 1000L.
    /// </summary>
    Eight = 7
}

/// <summary>
///     Extension methods for <see cref="FluidLevel" />.
/// </summary>
public static class FluidLevelExtensions
{
    /// <summary>
    ///     Get the fluid level as block height.
    /// </summary>
    public static int GetBlockHeight(this FluidLevel level)
    {
        return (int) level * 2 + 1;
    }
}
