// <copyright file="StringTools.cs" company="VoxelGame">
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
using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace VoxelGame.Core.Utilities;

/// <summary>
///     Utility class for string operations.
/// </summary>
public static class StringTools
{
    /// <summary>
    ///     Shortens the text to the specified length and adds an ellipsis if the text is longer than the specified length.
    /// </summary>
    /// <param name="text">The text to shorten.</param>
    /// <param name="maxLength">The maximum length of the text. Must be greater than 3.</param>
    /// <returns>The shortened text.</returns>
    public static String Ellipsis(this String text, Int32 maxLength)
    {
        const String ellipsis = "...";

        Debug.Assert(maxLength > ellipsis.Length);

        return text.Length <= maxLength
            ? text
            : String.Concat(text.AsSpan(start: 0, maxLength - ellipsis.Length), ellipsis);
    }

    /// <summary>
    ///     Convert a pascal case string to a snake case string.
    /// </summary>
    /// <param name="text">The pascal case string to convert.</param>
    /// <returns>The resulting snake case string.</returns>
    public static String PascalCaseToSnakeCase(this String text)
    {
        StringBuilder builder = new(text.Length);

        for (Int32 index = 0; index < text.Length; index++)
        {
            Char c = text[index];

            if (Char.IsUpper(c))
            {
                if (index > 0) builder.Append(value: '_');

                #pragma warning disable S4040 // This is not string normalization.
                builder.Append(Char.ToLower(c, CultureInfo.InvariantCulture));
                #pragma warning restore S4040
            }
            else
            {
                builder.Append(c);
            }
        }

        return builder.ToString();
    }

    /// <summary>
    ///     Get the Levenshtein edit distance between two strings.
    /// </summary>
    /// <param name="source">The source string.</param>
    /// <param name="target">The target string.</param>
    /// <returns>The amount of edits required to transform one string into the other.</returns>
    public static Int32 LevenshteinDistance(String source, String target)
    {
        if (source.Length == 0) return target.Length;
        if (target.Length == 0) return source.Length;

        Int32[,] distances = new Int32[source.Length + 1, target.Length + 1];

        for (Int32 i = 0; i <= source.Length; i++) distances[i, 0] = i;
        for (Int32 j = 0; j <= target.Length; j++) distances[0, j] = j;

        for (Int32 i = 1; i <= source.Length; i++)
        for (Int32 j = 1; j <= target.Length; j++)
        {
            Int32 cost = source[i - 1] == target[j - 1] ? 0 : 1;

            distances[i, j] = Math.Min(
                Math.Min(
                    distances[i - 1, j] + 1,
                    distances[i, j - 1] + 1),
                distances[i - 1, j - 1] + cost);
        }

        return distances[source.Length, target.Length];
    }
}
