// <copyright file="FluidLevel.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Core.Logic.Interfaces;

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
///     Constants for fluid levels.
/// </summary>
public static class FluidLevels
{
    /// <summary>
    ///     Indicates that there is no fluid.
    /// </summary>
    public const Int32 None = -1;
}

/// <summary>
///     Extension methods for <see cref="FluidLevel" />.
/// </summary>
public static class FluidLevelExtensions
{
    /// <summary>
    ///     Get the fluid level as block height.
    /// </summary>
    public static Int32 GetBlockHeight(this FluidLevel level)
    {
        return IHeightVariable.GetBlockHeightFromFluidHeight((Int32) level);
    }

    /// <summary>
    ///     Get the texture coordinates for the fluid level.
    /// </summary>
    /// <param name="level">The fluid level.</param>
    /// <param name="skip">How much of the face to skip, in block height units.</param>
    /// <param name="flow">The flow direction.</param>
    /// <returns>The texture coordinates.</returns>
    public static (Vector2 min, Vector2 max) GetUVs(this FluidLevel level, Int32 skip, VerticalFlow flow)
    {
        Single size = IHeightVariable.GetSize(level.GetBlockHeight());
        Single skipped = IHeightVariable.GetSize(skip);

        return flow != VerticalFlow.Upwards
            ? (new Vector2(x: 0, skipped), new Vector2(x: 1, size))
            : (new Vector2(x: 0, 1 - size), new Vector2(x: 1, 1 - skipped));
    }
}
