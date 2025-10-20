// <copyright file="FluidLevel.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Numerics;
using Vector2 = OpenTK.Mathematics.Vector2;

namespace VoxelGame.Core.Logic.Voxels;

/// <summary>
///     The level or amount of fluid. A position is split into 8 equal parts.
/// </summary>
public readonly struct FluidLevel : IEquatable<FluidLevel>, IComparable<FluidLevel>, IComparisonOperators<FluidLevel, FluidLevel, Boolean>
{
    private const Int32 NoneValue = -1;
    private const Int32 MinValue = 0;
    private const Int32 MaxValue = 7;

    private readonly SByte value;

    private FluidLevel(Int32 value)
    {
        this.value = (SByte) value;
    }

    /// <summary>
    ///     Represents a fluid volume of 125L.
    /// </summary>
    public static FluidLevel One { get; } = new(value: 0);

    /// <summary>
    ///     Represents a fluid volume of 250L.
    /// </summary>
    public static FluidLevel Two { get; } = new(value: 1);

    /// <summary>
    ///     Represents a fluid volume of 375L.
    /// </summary>
    public static FluidLevel Three { get; } = new(value: 2);

    /// <summary>
    ///     Represents a fluid volume of 500L.
    /// </summary>
    public static FluidLevel Four { get; } = new(value: 3);

    /// <summary>
    ///     Represents a fluid volume of 625L.
    /// </summary>
    public static FluidLevel Five { get; } = new(value: 4);

    /// <summary>
    ///     Represents a fluid volume of 750L.
    /// </summary>
    public static FluidLevel Six { get; } = new(value: 5);

    /// <summary>
    ///     Represents a fluid volume of 875L.
    /// </summary>
    public static FluidLevel Seven { get; } = new(value: 6);

    /// <summary>
    ///     Represents a fluid volume of 1000L.
    /// </summary>
    public static FluidLevel Eight { get; } = new(value: 7);
    
    /// <summary>
    ///     Represents a full block of fluid.
    /// </summary>
    public static FluidLevel Full => Eight;

    /// <summary>
    ///     Represents the absence of fluid.
    /// </summary>
    public static FluidLevel None { get; } = new(value: NoneValue);
    
    /// <summary>
    ///     Gets whether this level represents a full block of fluid.
    /// </summary>
    public Boolean IsFull => value == MaxValue;

    /// <summary>
    ///     Create a <see cref="FluidLevel" /> from an integer value.
    /// </summary>
    /// <param name="value">The integer representation of the level.</param>
    /// <returns>The created level.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="value" /> is outside the valid range.</exception>
    public static FluidLevel FromInt32(Int32 value)
    {
        return new FluidLevel(Math.Clamp(value, MinValue, MaxValue));
    }

    /// <summary>
    ///     Try to create a <see cref="FluidLevel" /> from an integer value.
    /// </summary>
    public static Boolean TryFromInt32(Int32 value, out FluidLevel level)
    {
        if (value is < MinValue or > MaxValue)
        {
            level = None;
            return false;
        }

        level = new FluidLevel(value);
        
        return true;
    }

    /// <summary>
    /// Get the integer representation of this fluid level.
    /// </summary>
    public Int32 ToInt32() => value;
    
    /// <summary>
    /// Get the fraction of a full block this fluid level represents.
    /// </summary>
    public Double Fraction => value != NoneValue ? (value + 1) / 8.0 : 0.0;
    
    /// <summary>
    ///     Get the fluid level as block height, or <see cref="BlockHeight.None"/> if there is no fluid.
    /// </summary>
    public BlockHeight GetBlockHeight()
    {
        return value == NoneValue ? BlockHeight.None : BlockHeight.FromInt32(value * (BlockHeight.Maximum.ToInt32() / MaxValue) + 1);
    }

    /// <summary>
    ///     Get the texture coordinates for the fluid level.
    /// </summary>
    /// <param name="neighbor">The neighboring fluid level.</param>
    /// <param name="flow">The flow direction.</param>
    /// <returns>The texture coordinates.</returns>
    public (Vector2 min, Vector2 max) GetUVs(FluidLevel neighbor, VerticalFlow flow)
    {
        var size = (Single) GetBlockHeight().Ratio;
        var skipped = (Single) neighbor.GetBlockHeight().Ratio;

        return flow != VerticalFlow.Upwards
            ? (new Vector2(x: 0, skipped), new Vector2(x: 1, size))
            : (new Vector2(x: 0, 1 - size), new Vector2(x: 1, 1 - skipped));
    }

    /// <inheritdoc />
    public Boolean Equals(FluidLevel other)
    {
        return value == other.value;
    }

    #region EQUALITY

    /// <inheritdoc />
    public override Boolean Equals(Object? obj)
    {
        return obj is FluidLevel other && Equals(other);
    }

    /// <inheritdoc />
    public override Int32 GetHashCode()
    {
        return value.GetHashCode();
    }

    /// <inheritdoc />
    public Int32 CompareTo(FluidLevel other)
    {
        return value.CompareTo(other.value);
    }

    /// <summary>
    /// The equality operator.
    /// </summary>
    public static Boolean operator ==(FluidLevel left, FluidLevel right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// The inequality operator.
    /// </summary>
    public static Boolean operator !=(FluidLevel left, FluidLevel right)
    {
        return !left.Equals(right);
    }
    
    #endregion EQUALITY
    
    /// <inheritdoc />
    public override String ToString()
    {
        return value switch
        {
            NoneValue => nameof(None),
            0 => nameof(One),
            1 => nameof(Two),
            2 => nameof(Three),
            3 => nameof(Four),
            4 => nameof(Five),
            5 => nameof(Six),
            6 => nameof(Seven),
            7 => nameof(Eight),
            _ => $"Invalid({value})"
        };
    }
    
    #region COMPARISON

    /// <summary>
    ///     The less-than operator.
    /// </summary>
    public static Boolean operator <(FluidLevel left, FluidLevel right)
    {
        return left.value < right.value;
    }

    /// <summary>
    ///     The greater-than operator.
    /// </summary>
    public static Boolean operator >(FluidLevel left, FluidLevel right)
    {
        return left.value > right.value;
    }

    /// <summary>
    /// The less-than-or-equal operator.
    /// </summary>
    public static Boolean operator <=(FluidLevel left, FluidLevel right)
    {
        return left.value <= right.value;
    }

    /// <summary>
    /// The greater-than-or-equal operator.
    /// </summary>
    public static Boolean operator >=(FluidLevel left, FluidLevel right)
    {
        return left.value >= right.value;
    }

    #endregion COMPARISON
    
    #region MATH
    
    /// <summary>
    /// Get the maximum of two fluid levels.
    /// </summary>
    public static FluidLevel Max(FluidLevel left, FluidLevel right) => left >= right ? left : right;

    /// <summary>
    /// The addition operator.
    /// </summary>
    public static FluidLevel operator +(FluidLevel left, FluidLevel right)
    {
        Int32 result = left.value + right.value + 1; // Because levels are 0-indexed.
        
        return FromInt32(result);
    }
    
    /// <summary>
    /// The subtraction operator.
    /// </summary>
    public static FluidLevel operator -(FluidLevel left, FluidLevel right)
    {
        Int32 result = left.value - right.value - 1; // Because levels are 0-indexed.
        
        return result < MinValue ? None : FromInt32(result);
    }
    
    #endregion MATH
}
