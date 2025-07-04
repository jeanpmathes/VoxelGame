﻿// <copyright file="ApplicationInformation.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

namespace VoxelGame.Core;

/// <summary>
///     Information about the current application.
/// </summary>
public class ApplicationInformation
{
    private ApplicationInformation(String version)
    {
        Version = version;
        MainThread = Thread.CurrentThread;

        SetDebugMode();
    }

    /// <summary>
    ///     Information about the current application.
    /// </summary>
    public static ApplicationInformation Instance { get; private set; } = null!;

    private static Boolean IsInitialized { get; set; }

    /// <summary>
    ///     Get the game version.
    /// </summary>
    public String Version { get; }

    /// <summary>
    ///     Whether the application is running on a debug build.
    /// </summary>
    internal Boolean IsDebug { get; private set; }

    /// <summary>
    ///     Get the main thread of the application.
    /// </summary>
    private Thread MainThread { get; }

    /// <summary>
    ///     Check if the current thread is the main thread.
    /// </summary>
    public Boolean IsOnMainThread => Thread.CurrentThread == MainThread;

    [Conditional("DEBUG")]
    private void SetDebugMode()
    {
        IsDebug = true;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ApplicationInformation" /> class.
    /// </summary>
    /// <param name="version">The current application version.</param>
    public static void Initialize(String version)
    {
        Debug.Assert(!IsInitialized);

        Instance = new ApplicationInformation(version);

        IsInitialized = true;
    }

    /// <summary>
    ///     Ensure that the current thread is the main thread.
    /// </summary>
    /// <returns>True if the current thread is the main thread.</returns>
    [Conditional("DEBUG")]
    public static void ThrowIfNotOnMainThread(Object @object, [CallerMemberName] String operation = "")
    {
        if (!IsInitialized || Instance.IsOnMainThread)
            return;

        Debug.Fail($"Attempted to perform operation '{operation}' with object '{@object}' from non-main thread");
    }
}
