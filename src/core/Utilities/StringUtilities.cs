// <copyright file="StringUtilities.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Diagnostics;

namespace VoxelGame.Core.Utilities;

/// <summary>
///     Utility class for string operations.
/// </summary>
public static class StringUtilities
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
}
