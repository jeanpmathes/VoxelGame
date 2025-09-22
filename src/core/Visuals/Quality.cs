// <copyright file="Quality.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;

namespace VoxelGame.Core.Visuals;

/// <summary>
///     Different quality levels to select resources and routines.
/// </summary>
public enum Quality
{
    /// <summary>
    ///     Low quality. Performance is the most important.
    /// </summary>
    Low,

    /// <summary>
    ///     Medium quality. Performance and visual quality are equally important.
    /// </summary>
    Medium,

    /// <summary>
    ///     High quality. Visual quality is prioritized.
    /// </summary>
    High,

    /// <summary>
    ///     Ultra quality. Visual quality is the most important.
    /// </summary>
    Ultra
}

/// <summary>
///     Utility class for quality.
/// </summary>
public static class Qualities
{
    /// <summary>
    ///     The number of quality levels.
    /// </summary>
    public const Int32 Count = 4;

    /// <summary>
    ///     Get all quality levels.
    /// </summary>
    /// <returns></returns>
    public static IEnumerable<Quality> All()
    {
        yield return Quality.Low;
        yield return Quality.Medium;
        yield return Quality.High;
        yield return Quality.Ultra;
    }

    /// <summary>
    ///     The name of the quality as string.
    /// </summary>
    public static String Name(this Quality quality)
    {
        return quality.ToStringFast();
    }
}
