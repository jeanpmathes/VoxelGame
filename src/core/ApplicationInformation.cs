// <copyright file="ApplicationInformation.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System.Diagnostics;
using System.Runtime.CompilerServices;
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
    ///     Get the main thread of the application.
    /// </summary>
    private Thread MainThread { get; }

    /// <summary>
    ///     Ensure that the current thread is the main thread.
    /// </summary>
    /// <returns>True if the current thread is the main thread.</returns>
    [Conditional("DEBUG")]
    public void EnsureMainThread(object @object, [CallerMemberName] string operation = "")
    {
        if (Thread.CurrentThread == MainThread) return;

        Debug.Fail($"Attempted to perform operation '{operation}' with object '{@object}' from non-main thread");
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

