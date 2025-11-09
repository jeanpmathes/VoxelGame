// <copyright file="RequestLevel.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;

namespace VoxelGame.Core.Logic.Chunks;

/// <summary>
///     The request level of a chunk.
///     Requests are made with the highest level and spread outwards along the sides.
///     This means not all 26 neighbors around a chunk receive the same level on spreading.
/// </summary>
public readonly struct RequestLevel : IEquatable<RequestLevel>, IComparable<RequestLevel>
{
    private readonly Byte level;

    private RequestLevel(Byte level)
    {
        this.level = level;
    }

    /// <summary>
    ///     The lowest level of a request.
    ///     Chunks with this level are not loaded.
    ///     Unloaded chunks have this level implicitly.
    /// </summary>
    private const Byte UnloadedLevel = 0;

    /// <summary>
    ///     All chunks with this level or higher are loaded.
    /// </summary>
    private const Byte LoadedLevel = 1;

    /// <summary>
    ///     All chunks with this level or higher can activate.
    ///     It must be at least <see cref="LoadedLevel" /> + 3 to guarantee that all 26 neighbors are loaded.
    ///     Because levels only spread along sides, the corner neighbors are three levels away.
    /// </summary>
    private const Byte ActiveLevel = LoadedLevel + 3;

    /// <summary>
    ///     All chunks with this level or higher are simulated.
    ///     If an active chunk is not simulated, neither blocks nor entities in it are updated.
    /// </summary>
    private const Byte SimulatedLevel = ActiveLevel + 3;

    /// <summary>
    ///     This is the highest level a request can have.
    ///     It must be at least <see cref="ActiveLevel" />.
    ///     The value determines to loading distance.
    /// </summary>
    private const Byte MaxLevel = SimulatedLevel + 3;

    /// <summary>
    ///     The range (in manhattan distance) that requests spread.
    ///     This is always <see cref="MaxLevel" /> - 1.
    ///     Consider a max level of 1, then all neighbors have level 0 and are not loaded.
    ///     As such, the range in that case is 0.
    /// </summary>
    public const Int32 Range = MaxLevel - 1;

    /// <summary>
    ///     The highest level a request can have.
    /// </summary>
    public static RequestLevel Highest => new(MaxLevel);

    /// <summary>
    ///     The lowest level a request can have.
    /// </summary>
    public static RequestLevel Lowest => new(UnloadedLevel);

    /// <summary>
    ///     Whether the chunk should be loaded.
    /// </summary>
    public Boolean IsLoaded => level >= LoadedLevel;

    /// <summary>
    ///     Whether the chunk should be active.
    /// </summary>
    public Boolean IsActive => level >= ActiveLevel;

    /// <summary>
    ///     Whether the chunk should be simulated.
    /// </summary>
    public Boolean IsSimulated => level >= SimulatedLevel;

    #region EQUALITY

    /// <summary>
    ///     Tests if this request level is equal to another.
    /// </summary>
    public Boolean Equals(RequestLevel other)
    {
        return level == other.level;
    }

    /// <inheritdoc />
    public override Boolean Equals(Object? obj)
    {
        return obj is RequestLevel other && Equals(other);
    }

    /// <inheritdoc />
    public override Int32 GetHashCode()
    {
        return level.GetHashCode();
    }

    /// <summary>
    ///     The equality operator.
    /// </summary>
    public static Boolean operator ==(RequestLevel left, RequestLevel right)
    {
        return left.Equals(right);
    }

    /// <summary>
    ///     The inequality operator.
    /// </summary>
    public static Boolean operator !=(RequestLevel left, RequestLevel right)
    {
        return !left.Equals(right);
    }
    
    /// <inheritdoc />
    public Int32 CompareTo(RequestLevel other)
    {
        return level.CompareTo(other.level);
    }

    #endregion EQUALITY

    #region OPERATORS

    /// <summary>
    ///     Subtract a value from the request level.
    /// </summary>
    public static RequestLevel operator -(RequestLevel level, Int32 value)
    {
        return level.Subtract(value);
    }

    private RequestLevel Subtract(Int32 value)
    {
        return value > level ? Lowest : new RequestLevel((Byte) (level - value));
    }

    /// <summary>
    ///     Compare two request levels.
    /// </summary>
    public static Boolean operator <(RequestLevel left, RequestLevel right)
    {
        return left.level < right.level;
    }

    /// <summary>
    ///     Compare two request levels.
    /// </summary>
    public static Boolean operator >(RequestLevel left, RequestLevel right)
    {
        return left.level > right.level;
    }

    /// <summary>
    ///     Compare two request levels.
    /// </summary>
    public static Boolean operator <=(RequestLevel left, RequestLevel right)
    {
        return left.level <= right.level;
    }

    /// <summary>
    ///     Compare two request levels.
    /// </summary>
    public static Boolean operator >=(RequestLevel left, RequestLevel right)
    {
        return left.level >= right.level;
    }

    #endregion OPERATORS

    /// <inheritdoc />
    public override String ToString()
    {
        return $"RequestLevel({level})";
    }
}
