// <copyright file="Unit.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;

namespace VoxelGame.Core.Utilities.Units;

/// <summary>
///     A unit of measure.
/// </summary>
public record Unit(String Symbol)
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

    /// <summary>
    ///     A unit of temperature, see <see cref="Temperature" />.
    /// </summary>
    public static Unit Celsius { get; } = new("°C");

    /// <summary>
    ///     A unit of time, see <see cref="Duration" />.
    /// </summary>
    public static Unit Second { get; } = new("s");
}
