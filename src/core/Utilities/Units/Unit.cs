﻿// <copyright file="Unit.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

namespace VoxelGame.Core.Utilities.Units;

/// <summary>
///     A unit of measure.
/// </summary>
public record Unit(string Symbol)
{
    /// <summary>
    ///     No unit.
    /// </summary>
    public static Unit None { get; } = new("");

    /// <summary>
    ///     A unit of distance, see <see cref="Length" />.
    /// </summary>
    public static Unit Meter { get; } = new("m");

    /// <summary>
    ///     A unit of data size, see <see cref="Memory" />.
    /// </summary>
    public static Unit Byte { get; } = new("B");
}
