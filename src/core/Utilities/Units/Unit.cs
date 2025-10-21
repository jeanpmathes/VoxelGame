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
public record Unit(String Symbol) // todo: add a code generator for measure classes
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
    ///     A unit of density, see <see cref="Density" />.
    /// </summary>
    public static Unit KilogramPerCubicMeter { get; } = new("kg/m³");

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

    /// <summary>
    ///     A unit of viscosity, see <see cref="Viscosity" />.
    /// </summary>
    public static Unit PascalSecond { get; } = new("Pa·s");
}
