// <copyright file="NameTools.cs" company="VoxelGame">
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
using System.Text;

namespace VoxelGame.SourceGenerators.Utilities;

/// <summary>
///     Tools to help working with names.
/// </summary>
public static class NameTools
{
    /// <summary>
    ///     Sanitize a string so it can be used for IO, e.g. as file name.
    /// </summary>
    /// <param name="str">The string to sanitize.</param>
    /// <returns>The sanitized string.</returns>
    public static String SanitizeForIO(String str)
    {
        StringBuilder sb = new();

        var skip = 0;

        const String globalPrefix = "global::";

        if (str.StartsWith(globalPrefix, StringComparison.Ordinal))
            skip = globalPrefix.Length;

        foreach (Char c in str)
        {
            if (skip > 0)
            {
                skip -= 1;

                continue;
            }

            if (Char.IsLetterOrDigit(c) || c is '-' or '.' or '_') sb.Append(c);
            else sb.Append(value: '_');
        }

        return sb.ToString();
    }

    /// <summary>
    ///     Sanitize a string so it can be used in XML documentation references.
    /// </summary>
    /// <param name="str">The string to sanitize.</param>
    /// <returns>The sanitized string.</returns>
    public static String SanitizeForDocumentationReference(String str)
    {
        return str.Replace(oldChar: '<', newChar: '{').Replace(oldChar: '>', newChar: '}');
    }

    /// <summary>
    ///     Convert a <c>PascalCase</c> string to <c>camelCase</c>.
    /// </summary>
    /// <param name="str">The string to convert.</param>
    /// <returns>The converted string.</returns>
    public static String ConvertPascalCaseToCamelCase(String str)
    {
        if (String.IsNullOrEmpty(str) || Char.IsLower(str, index: 0))
            return str;

        if (str.Length == 1)
            return str.ToLowerInvariant();

        return Char.ToLowerInvariant(str[index: 0]) + str.Substring(startIndex: 1);
    }
}
