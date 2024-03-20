// <copyright file="IUnit.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

namespace VoxelGame.Core.Utilities.Units;

/// <summary>
///     Base interface for all measures.
/// </summary>
public interface IMeasure
{
    /// <summary>
    ///     The unit of the measures.
    /// </summary>
    static abstract Unit Unit { get; }

    /// <summary>
    ///     The value of the measures.
    /// </summary>
    double Value { get; }

    /// <summary>
    ///     If set, only this prefix will be used for the measure.
    /// </summary>
    Prefix? Prefix => null;

    /// <summary>
    /// Get the string representation of the measure.
    /// </summary>
    /// <param name="measure">The measure to convert to string.</param>
    /// <typeparam name="T">The type of the measure.</typeparam>
    /// <returns>The string representation of the measure.</returns>
    public static string ToString<T>(T measure) where T : IMeasure
    {
        Prefix prefix = measure.Prefix ?? Prefix.FindBest(measure.Value);

        return $"{measure.Value / prefix.Factor:F2} {prefix.Symbol}{T.Unit.Symbol}";
    }
}
