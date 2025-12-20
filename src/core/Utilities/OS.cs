// <copyright file="OS.cs" company="VoxelGame">
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
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using Microsoft.Extensions.Logging;
using VoxelGame.Logging;

namespace VoxelGame.Core.Utilities;

/// <summary>
///     Utility functions related to operations that happen outside of the game itself.
/// </summary>
public static partial class OS
{
    /// <summary>
    ///     Show a text in a text editor.
    ///     This will save the text to a temporary file and open it in the default text editor.
    /// </summary>
    /// <param name="title">A title for the text. Will be used as part of the file name.</param>
    /// <param name="text">The text to show.</param>
    public static void Show(String title, String text)
    {
        DirectoryInfo directory = FileSystem.CreateTemporaryDirectory();
        FileInfo file = directory.GetFile($"{title}.txt");

        try
        {
            file.WriteAllText(text);
        }
        catch (IOException e)
        {
            LogFailedToFillFile(logger, e, file.FullName, text);

            return;
        }

        Start(file);
    }

    /// <summary>
    ///     Start a process or launch a file.
    /// </summary>
    /// <param name="path">The path to launch.</param>
    public static void Start(FileSystemInfo path)
    {
        try
        {
            ProcessStartInfo info = new()
            {
                FileName = path.FullName,
                UseShellExecute = true
            };

            Process.Start(info);
        }
        catch (FileNotFoundException)
        {
            LogFileToStartNotFound(logger, path.FullName);
        }
        catch (Win32Exception e)
        {
            LogFailedToStartFile(logger, e, path.FullName);
        }
    }

    #region LOGGING

    private static readonly ILogger logger = LoggingHelper.CreateLogger(nameof(OS));

    [LoggerMessage(EventId = LogID.OS + 0, Level = LogLevel.Error, Message = "Failed to fill {File} with: {Text}")]
    private static partial void LogFailedToFillFile(ILogger logger, IOException e, String file, String text);

    [LoggerMessage(EventId = LogID.OS + 1, Level = LogLevel.Debug, Message = "File to start not found: {File}")]
    private static partial void LogFileToStartNotFound(ILogger logger, String file);

    [LoggerMessage(EventId = LogID.OS + 2, Level = LogLevel.Debug, Message = "Failed to start file: {File}")]
    private static partial void LogFailedToStartFile(ILogger logger, Win32Exception e, String file);

    #endregion LOGGING
}
