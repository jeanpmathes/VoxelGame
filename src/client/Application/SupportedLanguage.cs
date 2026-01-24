// <copyright file="SupportedLanguage.cs" company="VoxelGame">
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
    ///     The English language.
    /// </summary>
    English,

    /// <summary>
    ///     The German language.
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
