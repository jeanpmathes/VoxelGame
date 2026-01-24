// <copyright file = "SupportedLanguage.cs" company = "VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Collections.Generic;
using System.Globalization;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Client.Application;

/// <summary>
///     All languages supported by the client.
/// </summary>
public enum SupportedLanguage
{
    /// <summary>
    ///     The english language.
    /// </summary>
    English,

    /// <summary>
    ///     The german language.
    /// </summary>
    German
}

/// <summary>
///     Utilities for <see cref="SupportedLanguage" />.
/// </summary>
public static class SupportedLanguages
{
    /// <summary>
    ///     Get all supported languages.
    /// </summary>
    public static IEnumerable<SupportedLanguage> All { get; } = Enum.GetValues<SupportedLanguage>();

    /// <summary>
    ///     Get the <see cref="CultureInfo" /> for the given <see cref="SupportedLanguage" />.
    /// </summary>
    /// <param name="language">The language.</param>
    /// <returns>The culture info.</returns>
    public static CultureInfo ToCultureInfo(this SupportedLanguage language)
    {
        return language switch
        {
            SupportedLanguage.English => new CultureInfo("en-US"),
            SupportedLanguage.German => new CultureInfo("de-DE"),
            _ => throw Exceptions.UnsupportedEnumValue(language)
        };
    }

    /// <summary>
    ///     Get a readable string for the given <see cref="SupportedLanguage" />.
    ///     The string is in the language itself.
    /// </summary>
    /// <param name="language">The language.</param>
    /// <returns>The readable string.</returns>
    public static String ToReadableString(this SupportedLanguage language)
    {
        return language switch
        {
            SupportedLanguage.English => "English",
            SupportedLanguage.German => "Deutsch",
            _ => throw Exceptions.UnsupportedEnumValue(language)
        };
    }
}
