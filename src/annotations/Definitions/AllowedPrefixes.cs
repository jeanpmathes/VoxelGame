// <copyright file = "AllowedPrefixes.cs" company = "VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;

namespace VoxelGame.Annotations.Definitions;

/// <summary>
///     Flags describing which prefixes are supported by a generated measure.
/// </summary>
[Flags]
public enum AllowedPrefixes : UInt32
{
    /// <summary>
    ///     Prefix for <c>10^24</c>.
    /// </summary>
    Yotta = 1 << 0,

    /// <summary>
    ///     Prefix for <c>10^21</c>.
    /// </summary>
    Zetta = 1 << 1,

    /// <summary>
    ///     Prefix for <c>10^18</c>.
    /// </summary>
    Exa = 1 << 2,

    /// <summary>
    ///     Prefix for <c>10^15</c>.
    /// </summary>
    Peta = 1 << 3,

    /// <summary>
    ///     Prefix for <c>10^12</c>.
    /// </summary>
    Tera = 1 << 4,

    /// <summary>
    ///     Prefix for <c>10^9</c>.
    /// </summary>
    Giga = 1 << 5,

    /// <summary>
    ///     Prefix for <c>10^6</c>.
    /// </summary>
    Mega = 1 << 6,

    /// <summary>
    ///     Prefix for <c>10^3</c>.
    /// </summary>
    Kilo = 1 << 7,

    /// <summary>
    ///     Prefix for <c>10^2</c>.
    /// </summary>
    Hecto = 1 << 8,

    /// <summary>
    ///     Prefix for <c>10^1</c>.
    /// </summary>
    Deca = 1 << 9,

    /// <summary>
    ///     Prefix for <c>10^0</c>.
    /// </summary>
    Unprefixed = 1 << 10,

    /// <summary>
    ///     Prefix for <c>10^-1</c>.
    /// </summary>
    Deci = 1 << 11,

    /// <summary>
    ///     Prefix for <c>10^-2</c>.
    /// </summary>
    Centi = 1 << 12,

    /// <summary>
    ///     Prefix for <c>10^-3</c>.
    /// </summary>
    Milli = 1 << 13,

    /// <summary>
    ///     Prefix for <c>10^-6</c>.
    /// </summary>
    Micro = 1 << 14,

    /// <summary>
    ///     Prefix for <c>10^-9</c>.
    /// </summary>
    Nano = 1 << 15,

    /// <summary>
    ///     Prefix for <c>10^-12</c>.
    /// </summary>
    Pico = 1 << 16,

    /// <summary>
    ///     Prefix for <c>10^-15</c>.
    /// </summary>
    Femto = 1 << 17,

    /// <summary>
    ///     Prefix for <c>10^-18</c>.
    /// </summary>
    Atto = 1 << 18,

    /// <summary>
    ///     Prefix for <c>10^-21</c>.
    /// </summary>
    Zepto = 1 << 19,

    /// <summary>
    ///     Prefix for <c>10^-24</c>.
    /// </summary>
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
