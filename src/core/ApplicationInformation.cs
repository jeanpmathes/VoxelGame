// <copyright file="ApplicationInformation.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System.Diagnostics;
using System.Threading;

namespace VoxelGame.Core;

/// <summary>
///     Information about the current application.
/// </summary>
public class ApplicationInformation
{
    private ApplicationInformation(string version)
    {
        Version = version;
        MainThread = Thread.CurrentThread;

        SetDebugMode();
    }

    /// <summary>
    ///     Information about the current game.
    /// </summary>
    public static ApplicationInformation Instance { get; private set; } = null!;

    private static bool IsInitialized { get; set; }

    /// <summary>
    ///     Get the game version.
    /// </summary>
    public string Version { get; }

    /// <summary>
    ///     Whether the application is running on a debug build.
    /// </summary>
    internal bool IsDebug { get; private set; }

    /// <summary>
    ///     Get the main thread of the application.
    /// </summary>
    private Thread MainThread { get; }

    /// <summary>
    ///     Check if the current thread is the main thread.
    /// </summary>
    public bool IsOnMainThread => Thread.CurrentThread == MainThread;

    [Conditional("DEBUG")]
    private void SetDebugMode()
    {
        IsDebug = true;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ApplicationInformation" /> class.
    /// </summary>
    /// <param name="version">The current application version.</param>
    public static void Initialize(string version)
    {
        Debug.Assert(!IsInitialized);

        Instance = new ApplicationInformation(version);

        IsInitialized = true;
    }
}
