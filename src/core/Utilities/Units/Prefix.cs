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
    /// <summary>
    ///     Describes which prefixes are allowed.
    /// </summary>
    [Flags]
    public enum AllowedPrefixes : UInt32
    {
        /// <inheritdoc cref="Yotta" />
        Yotta = 1 << 0,

        /// <inheritdoc cref="Zetta" />
        Zetta = 1 << 1,

        /// <inheritdoc cref="Exa" />
        Exa = 1 << 2,

        /// <inheritdoc cref="Peta" />
        Peta = 1 << 3,

        /// <inheritdoc cref="Tera" />
        Tera = 1 << 4,

        /// <inheritdoc cref="Giga" />
        Giga = 1 << 5,

        /// <inheritdoc cref="Mega" />
        Mega = 1 << 6,

        /// <inheritdoc cref="Kilo" />
        Kilo = 1 << 7,

        /// <inheritdoc cref="Hecto" />
        Hecto = 1 << 8,

        /// <inheritdoc cref="Deca" />
        Deca = 1 << 9,

        /// <inheritdoc cref="Unprefixed" />
        Unprefixed = 1 << 10,

        /// <inheritdoc cref="Deci" />
        Deci = 1 << 11,

        /// <inheritdoc cref="Centi" />
        Centi = 1 << 12,

        /// <inheritdoc cref="Milli" />
        Milli = 1 << 13,

        /// <inheritdoc cref="Micro" />
        Micro = 1 << 14,

        /// <inheritdoc cref="Nano" />
        Nano = 1 << 15,

        /// <inheritdoc cref="Pico" />
        Pico = 1 << 16,

        /// <inheritdoc cref="Femto" />
        Femto = 1 << 17,

        /// <inheritdoc cref="Atto" />
        Atto = 1 << 18,

        /// <inheritdoc cref="Zepto" />
        Zepto = 1 << 19,

        /// <inheritdoc cref="Yocto" />
        Yocto = 1 << 20,

        /// <summary>
        ///     All prefixes.
        /// </summary>
        All = Yotta | Zetta | Exa | Peta | Tera | Giga | Mega | Kilo | Hecto | Deca | Unprefixed | Deci | Centi | Milli | Micro | Nano | Pico | Femto | Atto | Zepto | Yocto,

        /// <summary>
        ///     No prefixes.
        /// </summary>
        None = 0
    }

    private static readonly List<Prefix> prefixes = [];

    /// <summary>
    ///     Get all prefixes, ordered from largest to smallest.
    /// </summary>
    public static IEnumerable<Prefix> All => prefixes;

    /// <summary>
    ///     Prefix for <c>10^24</c>.
    /// </summary>
    public static Prefix Yotta { get; } = Create("Y", factor: 1e24);

    /// <summary>
    ///     Prefix for <c>10^21</c>.
    /// </summary>
    public static Prefix Zetta { get; } = Create("Z", factor: 1e21);

    /// <summary>
    /// Prefix for <c>10^18</c>.
    /// </summary>
    public static Prefix Exa { get; } = Create("E", factor: 1e18);

    /// <summary>
    ///     Prefix for <c>10^15</c>.
    /// </summary>
    public static Prefix Peta { get; } = Create("P", factor: 1e15);

    /// <summary>
    ///     Prefix for <c>10^12</c>.
    /// </summary>
    public static Prefix Tera { get; } = Create("T", factor: 1e12);

    /// <summary>
    ///     Prefix for <c>10^9</c>.
    /// </summary>
    public static Prefix Giga { get; } = Create("G", factor: 1e9);

    /// <summary>
    ///     Prefix for <c>10^6</c>.
    /// </summary>
    public static Prefix Mega { get; } = Create("M", factor: 1e6);

    /// <summary>
    ///     Prefix for <c>10^3</c>.
    /// </summary>
    public static Prefix Kilo { get; } = Create("k", factor: 1e3);

    /// <summary>
    ///     Prefix for <c>10^2</c>.
    /// </summary>
    public static Prefix Hecto { get; } = Create("h", factor: 1e2);

    /// <summary>
    ///     Prefix for <c>10^1</c>.
    /// </summary>
    public static Prefix Deca { get; } = Create("da", factor: 1e1);

    /// <summary>
    ///     Prefix for <c>10^0</c>.
    /// </summary>
    public static Prefix Unprefixed { get; } = Create("", factor: 1);

    /// <summary>
    ///     Prefix for <c>10^-1</c>.
    /// </summary>
    public static Prefix Deci { get; } = Create("d", factor: 1e-1);

    /// <summary>
    ///     Prefix for <c>10^-2</c>.
    /// </summary>
    public static Prefix Centi { get; } = Create("c", factor: 1e-2);

    /// <summary>
    ///     Prefix for <c>10^-3</c>.
    /// </summary>
    public static Prefix Milli { get; } = Create("m", factor: 1e-3);

    /// <summary>
    ///     Prefix for <c>10^-6</c>.
    /// </summary>
    public static Prefix Micro { get; } = Create("μ", factor: 1e-6);

    /// <summary>
    ///     Prefix for <c>10^-9</c>.
    /// </summary>
    public static Prefix Nano { get; } = Create("n", factor: 1e-9);

    /// <summary>
    ///     Prefix for <c>10^-12</c>.
    /// </summary>
    public static Prefix Pico { get; } = Create("p", factor: 1e-12);

    /// <summary>
    ///     Prefix for <c>10^-15</c>.
    /// </summary>
    public static Prefix Femto { get; } = Create("f", factor: 1e-15);

    /// <summary>
    ///     Prefix for <c>10^-18</c>.
    /// </summary>
    public static Prefix Atto { get; } = Create("a", factor: 1e-18);

    /// <summary>
    ///     Prefix for <c>10^-21</c>.
    /// </summary>
    public static Prefix Zepto { get; } = Create("z", factor: 1e-21);

    /// <summary>
    ///     Prefix for <c>10^-24</c>.
    /// </summary>
    public static Prefix Yocto { get; } = Create("y", factor: 1e-24);

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
    /// <param name="allowed">The allowed prefixes, if no prefixes are allowed, <see cref="Unprefixed" /> is used.</param>
    /// <returns>The closest prefix.</returns>
    public static Prefix FindBest(Double value, AllowedPrefixes allowed = AllowedPrefixes.All)
    {
        if (allowed == AllowedPrefixes.None)
            return Unprefixed;

        value = Math.Abs(value);

        if (VMath.NearlyZero(value)) return Unprefixed;

        var mask = (UInt32) allowed;

        while (mask != 0)
        {
            Int32 index = BitHelper.LeastSignificantBit(mask);
            Prefix prefix = prefixes[index];

            if (prefix.Factor <= value)
                return prefix;

            mask &= ~(1u << index);
        }

        // All allowed prefixes are too large, use the smallest one.

        mask = (UInt32) allowed;

        Int32 last = BitHelper.MostSignificantBit(mask);

        return prefixes[last];
    }
}
