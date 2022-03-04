// <copyright file="FieldIterationExtensions.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace VoxelGame.Manual.Utility
{
    /// <summary>
    ///     Provides functions to iterate over fields of a class.
    /// </summary>
    public static class FieldIterationExtensions
    {
        /// <summary>
        ///     Get all static fields declared in a class, that match a certain type.
        /// </summary>
        /// <param name="type">The type to get all fields from.</param>
        /// <param name="filterType">The type to filter for.</param>
        /// <returns>The found fields.</returns>
        public static IEnumerable<FieldInfo> GetStaticFields(this IReflect type, Type filterType)
        {
            return type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
                .Where(info => info.FieldType == filterType);
        }

        /// <summary>
        ///     Get the static values for all fields with a certain type, and the corresponding field documentation.
        /// </summary>
        public static IEnumerable<(T, string)> GetStaticValues<T>(this IReflect type, Documentation documentation)
        {
            return type.GetStaticFields(typeof(T))
                .Where(info => info.GetValue(obj: null) != null)
                .Select(info => ((T) info.GetValue(obj: null)!, documentation.GetFieldSummary(info)));
        }
    }
}
