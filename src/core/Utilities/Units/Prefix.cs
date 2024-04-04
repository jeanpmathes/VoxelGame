// <copyright file="Prefix.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;

namespace VoxelGame.Core.Utilities.Units;

/// <summary>
///     A unit prefix.
/// </summary>
public record Prefix(String Symbol, Double Factor)
{
    private static readonly List<Prefix> prefixes = new();

    /// <summary>
    ///     Get all prefixes, ordered from largest to smallest.
    /// </summary>
    public static IEnumerable<Prefix> All => prefixes;

    private static Prefix Create(String symbol, Double factor)
    {
        Prefix prefix = new(symbol, factor);

        prefixes.Add(prefix);

        return prefix;
    }

    /// <summary>
    ///     Find the best prefix, resulting in a good human-readable representation.
    /// </summary>
    /// <param name="value">The value to find the best prefix for.</param>
    /// <returns>The closest prefix.</returns>
    public static Prefix FindBest(Double value)
    {
        value = Math.Abs(value);

        if (VMath.NearlyZero(value)) return None;
        if (value > Exa.Factor) return Exa;
        if (value < Atto.Factor) return Atto;

        foreach (Prefix prefix in prefixes)
        {
            if (prefix.Factor > value) continue;

            return prefix;
        }

        return None;
    }

#pragma warning disable CS1591 // Understandable without documentation.
    public static Prefix Exa { get; } = Create("E", factor: 1e18);
    public static Prefix Peta { get; } = Create("P", factor: 1e15);
    public static Prefix Tera { get; } = Create("T", factor: 1e12);
    public static Prefix Giga { get; } = Create("G", factor: 1e9);
    public static Prefix Mega { get; } = Create("M", factor: 1e6);
    public static Prefix Kilo { get; } = Create("k", factor: 1e3);
    public static Prefix None { get; } = Create("", factor: 1);
    public static Prefix Centi { get; } = Create("c", factor: 1e-2);
    public static Prefix Milli { get; } = Create("m", factor: 1e-3);
    public static Prefix Micro { get; } = Create("μ", factor: 1e-6);
    public static Prefix Nano { get; } = Create("n", factor: 1e-9);
    public static Prefix Pico { get; } = Create("p", factor: 1e-12);
    public static Prefix Femto { get; } = Create("f", factor: 1e-15);
    public static Prefix Atto { get; } = Create("a", factor: 1e-18);
#pragma warning restore CS1591 // Understandable without documentation.
}
