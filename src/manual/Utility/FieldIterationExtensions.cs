// <copyright file="FieldIterationExtensions.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.Linq;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Manual.Utility;

/// <summary>
///     Provides functions to iterate over fields of a class.
/// </summary>
public static class FieldIterationExtensions
{
    /// <summary>
    ///     Get the values for all fields with a certain type, and the corresponding field documentation.
    /// </summary>
    public static IEnumerable<(T, String)> GetDocumentedValues<T>(this Object obj, Documentation documentation) where T : class
    {
        return Reflections.GetPropertiesOfType<T>(obj)
            .Where(info => info.GetValue(obj) != null)
            .Select(info => ((T) info.GetValue(obj)!, documentation.GetPropertySummary(info)));
    }
}
