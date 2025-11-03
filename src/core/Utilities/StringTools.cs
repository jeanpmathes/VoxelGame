// <copyright file="StringTools.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
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

        for (var index = 0; index < text.Length; index++)
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
}
