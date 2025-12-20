// <copyright file="Chance.cs" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2025 Jean Patrick Mathes
//      
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
//     
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//     
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <https://www.gnu.org/licenses/>.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Diagnostics.CodeAnalysis;

namespace VoxelGame.Core.Utilities;

/// <summary>
///     Describes a probability of something occurring.
/// </summary>
public readonly struct Chance : IEquatable<Chance>, IComparable<Chance>
{
    private readonly Byte value;

    private Chance(Byte value)
    {
        this.value = value;
    }

    /// <summary>
    ///     Creates a new chance from a percentage value between 0 and 1.
    /// </summary>
    /// <param name="percentage">The percentage value, will be clamped between 0 and 1.</param>
    /// <returns>>The created chance.</returns>
    public static Chance FromPercentage(Double percentage)
    {
        return new Chance((Byte) (Math.Clamp(percentage, min: 0.0, max: 1.0) * 100.0));
    }

    /// <summary>
    ///     Gets a chance that is impossible (0%).
    /// </summary>
    public static Chance Impossible => new(value: 0);

    /// <summary>
    ///     Gets a chance that is certain (100%).
    /// </summary>
    public static Chance Certain => new(value: 100);

    /// <summary>
    ///     Gets a chance representing a coin toss (50%).
    /// </summary>
    public static Chance CoinToss => new(value: 50);

    /// <summary>
    ///     Check if a roll of 0-99 passes this chance.
    /// </summary>
    /// <param name="roll">The roll to check.</param>
    /// <returns>><c>true</c> if the roll passes the chance, <c>false</c> otherwise.</returns>
    public Boolean Passes(Int32 roll)
    {
        return roll < value;
    }

    #region EQUALITY

    /// <inheritdoc />
    public override Boolean Equals([NotNullWhen(returnValue: true)] Object? obj)
    {
        return obj is Chance other && Equals(other);
    }

    /// <inheritdoc />
    public Boolean Equals(Chance other)
    {
        return value == other.value;
    }

    /// <inheritdoc />
    public override Int32 GetHashCode()
    {
        return value.GetHashCode();
    }

    /// <summary>
    ///     The equality operator.
    /// </summary>
    public static Boolean operator ==(Chance left, Chance right)
    {
        return left.Equals(right);
    }

    /// <summary>
    ///     The inequality operator.
    /// </summary>
    public static Boolean operator !=(Chance left, Chance right)
    {
        return !left.Equals(right);
    }

    #endregion EQUALITY

    #region COMPARABLE

    /// <inheritdoc />
    public Int32 CompareTo(Chance other)
    {
        return value.CompareTo(other.value);
    }

    /// <summary>
    ///     Comparison operator for less than.
    /// </summary>
    public static Boolean operator <(Chance left, Chance right)
    {
        return left.CompareTo(right) < 0;
    }

    /// <summary>
    ///     Comparison operator for less than or equal to.
    /// </summary>
    public static Boolean operator <=(Chance left, Chance right)
    {
        return left.CompareTo(right) <= 0;
    }

    /// <summary>
    ///     Comparison operator for greater than.
    /// </summary>
    public static Boolean operator >(Chance left, Chance right)
    {
        return left.CompareTo(right) > 0;
    }

    /// <summary>
    ///     Comparison operator for greater than or equal to.
    /// </summary>
    public static Boolean operator >=(Chance left, Chance right)
    {
        return left.CompareTo(right) >= 0;
    }

    #endregion COMPARABLE
}
