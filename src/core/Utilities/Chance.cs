// <copyright file = "Chance.cs" company = "VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Diagnostics.CodeAnalysis;

namespace VoxelGame.Core.Utilities;

/// <summary>
/// Describes a probability of something occurring.
/// </summary>
public readonly struct Chance : IEquatable<Chance>, IComparable<Chance>
{
    private readonly Byte value;
    
    private Chance(Byte value) => this.value = value;
    
    /// <summary>
    /// Creates a new chance from a percentage value between 0 and 1.
    /// </summary>
    /// <param name="percentage">The percentage value, will be clamped between 0 and 1.</param>
    /// <returns>>The created chance.</returns>
    public static Chance FromPercentage(Double percentage)
    {
        return new Chance((Byte) (Math.Clamp(percentage, min: 0.0, max: 1.0) * 100.0));
    }
    
    /// <summary>
    /// Gets a chance that is impossible (0%).
    /// </summary>
    public static Chance Impossible => new(0);
 
    /// <summary>
    /// Gets a chance that is certain (100%).
    /// </summary>
    public static Chance Certain => new(100);
    
    /// <summary>
    /// Gets a chance representing a coin toss (50%).
    /// </summary>
    public static Chance CoinToss => new(50);
    
    /// <summary>
    /// Check if a roll of 0-99 passes this chance.
    /// </summary>
    /// <param name="roll">The roll to check.</param>
    /// <returns>><c>true</c> if the roll passes the chance, <c>false</c> otherwise.</returns>
    public Boolean Passes(Int32 roll)
    {
        return roll < value;
    }

    #region EQUALITY
    
    /// <inheritdoc />
    public override Boolean Equals([NotNullWhen(true)] Object? obj)
    {
        return obj is Chance other && Equals(other);
    }
    
    /// <inheritdoc />
    public override Int32 GetHashCode()
    {
        return value.GetHashCode();
    }

    /// <inheritdoc />
    public Boolean Equals(Chance other)
    {
        return value == other.value;
    }
    
    /// <inheritdoc />
    public Int32 CompareTo(Chance other)
    {
        return value.CompareTo(other.value);
    }
    
    /// <summary>
    /// The equality operator.
    /// </summary>
    public static Boolean operator ==(Chance left, Chance right)
    {
        return left.Equals(right);
    }
    
    /// <summary>
    /// The inequality operator.
    /// </summary>
    public static Boolean operator !=(Chance left, Chance right)
    {
        return !left.Equals(right);
    }
    
    #endregion EQUALITY
}
