// <copyright file="OS.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using Microsoft.Extensions.Logging;
using VoxelGame.Logging;

namespace VoxelGame.Client.Utilities;

/// <summary>
///     Utility functions related to operations that happen outside of the game itself.
/// </summary>
public class OS
{
    private static readonly ILogger logger = LoggingHelper.CreateLogger<OS>();

    private OS() {}

    /// <summary>
    ///     Get the OS instance.
    /// </summary>
    public static OS Instance { get; } = new();

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
            logger.LogDebug(Events.OS, "File to start not found: {File}", path);
        }
        catch (Win32Exception e)
        {
            logger.LogDebug(Events.OS, e, "Failed to start file: {File}", path);
        }
    }
}


