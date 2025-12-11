// <copyright file="NameTools.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
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
