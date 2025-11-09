// <copyright file="Unit.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using VoxelGame.Annotations.Attributes;
using VoxelGame.Annotations.Definitions;

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
    [GenerateMeasure(
        "Length",
        "Meters",
        AllowedPrefixes.Kilo | AllowedPrefixes.Unprefixed | AllowedPrefixes.Milli | AllowedPrefixes.Micro | AllowedPrefixes.Nano,
        MeasureSummary = "A measure for length.",
        ValueSummary = "Get the length, in meters.")]
    public static Unit Meter { get; } = new("m");

    /// <summary>
    ///     A unit of density, see <see cref="Density" />.
    /// </summary>
    [GenerateMeasure(
        "Density",
        "KilogramsPerCubicMeter",
        AllowedPrefixes.Kilo | AllowedPrefixes.Unprefixed,
        MeasureSummary = "A measure for density.",
        ValueSummary = "Get the density, in kilograms per cubic meter.")]
    public static Unit KilogramPerCubicMeter { get; } = new("kg/m³");

    /// <summary>
    ///     A unit of data size, see <see cref="Memory" />.
    /// </summary>
    [GenerateMeasure(
        "Memory",
        "Bytes",
        AllowedPrefixes.Unprefixed | AllowedPrefixes.Kilo | AllowedPrefixes.Mega | AllowedPrefixes.Giga | AllowedPrefixes.Tera
        | AllowedPrefixes.Peta | AllowedPrefixes.Exa | AllowedPrefixes.Zetta | AllowedPrefixes.Yotta,
        MeasureSummary = "A measure for computer memory.",
        ValueSummary = "Get the memory, in bytes.")]
    public static Unit Byte { get; } = new("B");

    /// <summary>
    ///     A unit of temperature, see <see cref="Temperature" />.
    /// </summary>
    [GenerateMeasure(
        "Temperature",
        "DegreesCelsius",
        AllowedPrefixes.None,
        MeasureSummary = "A measure for temperature.",
        ValueSummary = "Get the temperature in degrees Celsius.")]
    public static Unit Celsius { get; } = new("°C");

    /// <summary>
    ///     A unit of time, see <see cref="Duration" />.
    /// </summary>
    [GenerateMeasure(
        "Duration",
        "Seconds",
        AllowedPrefixes.Unprefixed | AllowedPrefixes.Milli | AllowedPrefixes.Micro | AllowedPrefixes.Nano,
        MeasureSummary = "A real-time duration. Do not use this for time spans or time intervals.",
        ValueSummary = "Get the duration, in seconds.")]
    public static Unit Second { get; } = new("s");

    /// <summary>
    ///     A unit of viscosity, see <see cref="Viscosity" />.
    /// </summary>
    [GenerateMeasure(
        "Viscosity",
        "PascalSeconds",
        AllowedPrefixes.Milli | AllowedPrefixes.Unprefixed,
        MeasureSummary = "A measure for dynamic viscosity.",
        ValueSummary = "Get the dynamic viscosity, in pascal seconds.")]
    public static Unit PascalSecond { get; } = new("Pa·s");
}
