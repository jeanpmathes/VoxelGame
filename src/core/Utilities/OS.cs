// <copyright file="OS.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
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

    [LoggerMessage(EventId = Events.OS, Level = LogLevel.Error, Message = "Failed to fill {File} with: {Text}")]
    private static partial void LogFailedToFillFile(ILogger logger, IOException e, String file, String text);

    [LoggerMessage(EventId = Events.OS, Level = LogLevel.Debug, Message = "File to start not found: {File}")]
    private static partial void LogFileToStartNotFound(ILogger logger, String file);

    [LoggerMessage(EventId = Events.OS, Level = LogLevel.Debug, Message = "Failed to start file: {File}")]
    private static partial void LogFailedToStartFile(ILogger logger, Win32Exception e, String file);

    #endregion LOGGING
}
