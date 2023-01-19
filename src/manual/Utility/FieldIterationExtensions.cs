// <copyright file="FieldIterationExtensions.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace VoxelGame.Manual.Utility;

/// <summary>
///     Provides functions to iterate over fields of a class.
/// </summary>
public static class FieldIterationExtensions
{
    /// <summary>
    ///     Get all properties declared in a class, that match a certain type.
    /// </summary>
    /// <param name="type">The type to get all properties from.</param>
    /// <param name="filterType">The type to filter for.</param>
    /// <returns>The found properties.</returns>
    public static IEnumerable<PropertyInfo> GetProperties(this IReflect type, Type filterType)
    {
        return type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(info => info.PropertyType == filterType);
    }

    /// <summary>
    ///     Get the values for all fields with a certain type, and the corresponding field documentation.
    /// </summary>
    public static IEnumerable<(T, string)> GetValues<T>(this object obj, Documentation documentation)
    {
        return obj.GetType().GetProperties(typeof(T))
            .Where(info => info.GetValue(obj) != null)
            .Select(info => ((T) info.GetValue(obj)!, documentation.GetPropertySummary(info)));
    }
}


