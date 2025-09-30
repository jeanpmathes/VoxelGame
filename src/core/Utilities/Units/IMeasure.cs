// <copyright file="IUnit.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;

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
    ///     All allowed prefixes for the measure.
    /// </summary>
    static abstract Prefix.AllowedPrefixes Prefixes { get; }

    /// <summary>
    ///     The value of the measures.
    /// </summary>
    Double Value { get; }

    /// <summary>
    ///     Get the string representation of the measure.
    /// </summary>
    /// <param name="measure">The measure to convert to string.</param>
    /// <param name="format">The format provider to use.</param>
    /// <typeparam name="T">The type of the measure.</typeparam>
    /// <returns>The string representation of the measure.</returns>
    public static String ToString<T>(T measure, IFormatProvider? format) where T : IMeasure
    {
        Prefix prefix = Prefix.FindBest(measure.Value, T.Prefixes);

        return String.Create(format, $"{measure.Value / prefix.Factor:F2} {prefix.Symbol}{T.Unit.Symbol}");
    }
}
