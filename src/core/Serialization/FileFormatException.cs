// <copyright file="FileFormatException.cs" company="VoxelGame">
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
