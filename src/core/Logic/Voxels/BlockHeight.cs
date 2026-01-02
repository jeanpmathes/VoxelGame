// <copyright file="BlockHeight.cs" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2026 Jean Patrick Mathes
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
using System.Globalization;
using System.Numerics;
using System.Runtime.InteropServices;

namespace VoxelGame.Core.Logic.Voxels;

/// <summary>
///     Represents the height of <see cref="Behaviors.Height.PartialHeight" /> blocks.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public readonly struct BlockHeight : IEquatable<BlockHeight>, IComparable<BlockHeight>, IComparisonOperators<BlockHeight, BlockHeight, Boolean>
{
    private const Int32 NoneValue = -1;
    private const Int32 MinValue = 0;
    private const Int32 MaxValue = 15; // Don't even think about changing this without checking implicit dependencies, e.g. from StoredHeight8 and StoredHeight16, or FluidLevel.

    private readonly SByte value;

    private BlockHeight(Int32 value)
    {
        this.value = (SByte) value;
    }

    /// <summary>
    ///     Gets the lowest height that still results in a visible block.
    /// </summary>
    public static BlockHeight Minimum { get; } = new(MinValue);

    /// <summary>
    ///     Gets the highest height which represents a full block.
    /// </summary>
    public static BlockHeight Maximum { get; } = new(MaxValue);

    /// <summary>
    ///     Gets the height representing half of a block.
    /// </summary>
    public static BlockHeight Half { get; } = new(MaxValue / 2);

    /// <summary>
    ///     Gets a sentinel value representing the absence of a height.
    /// </summary>
    public static BlockHeight None { get; } = new(NoneValue);

    /// <summary>
    ///     Gets whether this value represents the absence of height.
    /// </summary>
    public Boolean IsNone => value == NoneValue;

    /// <summary>
    ///     Gets whether this value represents a full block.
    /// </summary>
    public Boolean IsFull => value == MaxValue;

    /// <summary>
    ///     Get the ratio of this height to the maximum height of a block.
    ///     As a block is exactly 1 meter tall, this is also the height in meters.
    /// </summary>
    public Double Ratio => IsNone ? 0.0 : ComputeRatio(value);

    /// <summary>
    ///     Get the ratio of a given height to the maximum height of a block.
    ///     Note that using this is not equivalent to using <see cref="FromInt32" /> first, as that method clamps the value.
    /// </summary>
    /// <param name="height">The height to get the ratio for.</param>
    /// <returns>>The ratio of the given height.</returns>
    public static Double ComputeRatio(Int32 height)
    {
        return (Double) (height + 1) / (MaxValue + 1);
    }

    /// <summary>
    ///     Create a height value from an integer representation.
    /// </summary>
    /// <param name="value">The integer representation.</param>
    /// <returns>The created height value.</returns>
    public static BlockHeight FromInt32(Int32 value)
    {
        return new BlockHeight(Math.Clamp(value, MinValue, MaxValue));
    }

    /// <summary>
    ///     Try to create a height value from an integer representation.
    /// </summary>
    public static Boolean TryFromInt32(Int32 value, out BlockHeight height)
    {
        if (value is < NoneValue or > MaxValue)
        {
            height = None;

            return false;
        }

        height = FromInt32(value);

        return true;
    }

    /// <summary>
    ///     Get the integer representation of this height value.
    /// </summary>
    public Int32 ToInt32()
    {
        return value;
    }

    /// <summary>
    ///     Adds a delta to the given height.
    /// </summary>
    public static BlockHeight operator +(BlockHeight height, Int32 delta)
    {
        return FromInt32(height.ToInt32() + delta);
    }

    /// <summary>
    ///     Subtracts a delta from the given height.
    /// </summary>
    public static BlockHeight operator -(BlockHeight height, Int32 delta)
    {
        return FromInt32(height.ToInt32() - delta);
    }

    /// <inheritdoc cref="op_Addition" />
    public static BlockHeight Add(BlockHeight height, Int32 delta)
    {
        return height + delta;
    }

    /// <inheritdoc cref="op_Subtraction" />
    public static BlockHeight Subtract(BlockHeight height, Int32 delta)
    {
        return height - delta;
    }

    /// <inheritdoc />
    public override String ToString()
    {
        return IsNone ? nameof(None) : value.ToString(CultureInfo.InvariantCulture);
    }

    #region EQUALITY AND COMPARISON

    /// <inheritdoc />
    public Boolean Equals(BlockHeight other)
    {
        return value == other.value;
    }

    /// <inheritdoc />
    public override Boolean Equals([NotNullWhen(returnValue: true)] Object? obj)
    {
        return obj is BlockHeight other && Equals(other);
    }

    /// <inheritdoc />
    public override Int32 GetHashCode()
    {
        return value.GetHashCode();
    }

    /// <inheritdoc />
    public Int32 CompareTo(BlockHeight other)
    {
        return value.CompareTo(other.value);
    }

    /// <summary>
    ///     The equality operator.
    /// </summary>
    public static Boolean operator ==(BlockHeight left, BlockHeight right)
    {
        return left.Equals(right);
    }

    /// <summary>
    ///     The inequality operator.
    /// </summary>
    public static Boolean operator !=(BlockHeight left, BlockHeight right)
    {
        return !left.Equals(right);
    }

    /// <summary>
    ///     The greater than operator.
    /// </summary>
    public static Boolean operator >(BlockHeight left, BlockHeight right)
    {
        return left.value > right.value;
    }

    /// <summary>
    ///     The greater than or equal operator.
    /// </summary>
    public static Boolean operator >=(BlockHeight left, BlockHeight right)
    {
        return left.value >= right.value;
    }

    /// <summary>
    ///     The less than operator.
    /// </summary>
    public static Boolean operator <(BlockHeight left, BlockHeight right)
    {
        return left.value < right.value;
    }

    /// <summary>
    ///     The less than or equal operator.
    /// </summary>
    public static Boolean operator <=(BlockHeight left, BlockHeight right)
    {
        return left.value <= right.value;
    }

    #endregion EQUALITY AND COMPARISON
}
