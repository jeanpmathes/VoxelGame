﻿// <copyright file="FileFormatException.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.IO;

namespace VoxelGame.Core.Serialization;

/// <summary>
///     Thrown when a file is not in the expected format.
/// </summary>
public sealed class FileFormatException : IOException
{
    /// <summary>
    ///     Creates a new instance of the <see cref="FileFormatException" /> class.
    /// </summary>
    /// <param name="file">The file that is not in the expected format.</param>
    /// <param name="info">Additional information about the exception.</param>
    public FileFormatException(String file, String? info = null) : base(FormatMessage(file, info)) {}

    private static String FormatMessage(String file, String? info)
    {
        var message = $"File '{file}' is not in the expected format";

        if (info != null) message += $": {info}";
        else message += '.';

        return message;
    }
}
