// <copyright file="General.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;

namespace VoxelGame.Core.Utilities;

/// <summary>
///     General utilities.
/// </summary>
public static class General
{
    /// <summary>
    ///     Performs an action on an object and returns the object.
    /// </summary>
    public static T With<T>(this T obj, Action<T> action)
    {
        action(obj);

        return obj;
    }

    /// <summary>
    ///     Signifies an implication. If a is true, b must be true as well.
    /// </summary>
    public static Boolean Implies(this Boolean a, Boolean b)
    {
        return !a || b;
    }
}
