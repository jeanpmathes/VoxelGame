// <copyright file="NameTools.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Text;

namespace VoxelGame.Generators.Utilities;

/// <summary>
/// Tools to help working with names.
/// </summary>
public static class NameTools
{
    /// <summary>
    /// Sanitize a string so it can be used for IO, e.g. as file name.
    /// </summary>
    /// <param name="str">The string to sanitize.</param>
    /// <returns>The sanitized string.</returns>
    public static String SanitizeForIO(String str)
    {
        StringBuilder sb = new();
        
        foreach (Char c in str)
        {
            if (Char.IsLetterOrDigit(c) || c is '-' or '.' or '_')
            {
                sb.Append(c);
            }
            else
            {
                sb.Append('_');
            }
        }
        
        return sb.ToString();
    }
    
    /// <summary>
    /// Sanitize a string so it can be used in XML documentation references.
    /// </summary>
    /// <param name="str">The string to sanitize.</param>
    /// <returns>The sanitized string.</returns>
    public static String SanitizeForDocumentationReference(String str)
        => str.Replace(oldChar: '<', newChar: '{').Replace(oldChar: '>', newChar: '}');
}
