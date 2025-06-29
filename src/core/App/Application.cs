// <copyright file="Application.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using VoxelGame.Toolkit.Components;
using VoxelGame.Toolkit.Utilities;
using Timer = VoxelGame.Core.Profiling.Timer;

namespace VoxelGame.Core.App;

/// <summary>
///     Represents the running application.
///     There will always be exactly one instance of this class.
/// </summary>
public abstract class Application : Composed<Application, ApplicationComponent>
{
    /// <summary>
    ///     Create a new application instance.
    ///     This constructor must be called exactly once at the start of the application.
    /// </summary>
    /// <param name="version">The version of the application.</param>
    protected Application(Version version)
    {
        if (PrivateInstance != null)
            throw Exceptions.InvalidOperation("Cannot create multiple application instances.");

        PrivateInstance = this;

        Version = version;
        MainThread = Thread.CurrentThread;

        SetDebugMode();
    }

    private static Application? PrivateInstance { get; set; }

    /// <summary>
    ///     Get the current application instance.
    /// </summary>
    public static Application Instance => PrivateInstance!;

    /// <summary>
    ///     Get the application version.
    /// </summary>
    public Version Version { get; }

    /// <summary>
    ///     Whether the application is running on a debug build.
    /// </summary>
    public Boolean IsDebug { get; private set; }

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
    ///     Ensure that the current thread is the main thread.
    /// </summary>
    /// <returns>True if the current thread is the main thread.</returns>
    [Conditional("DEBUG")]
    public static void ThrowIfNotOnMainThread(Object @object, [CallerMemberName] String operation = "")
    {
        if (PrivateInstance == null || Instance.IsOnMainThread)
            return;

        Debug.Fail($"Attempted to perform operation '{operation}' with object '{@object}' from non-main thread");
    }

    /// <inheritdoc cref="OnInitialization" />
    protected void DoInitialization(Timer? timer)
    {
        OnInitialization(timer);

        foreach (ApplicationComponent component in Components)
            component.OnInitialization(timer);
    }

    /// <summary>
    ///     Called on initialization of the application.
    /// </summary>
    /// <param name="timer">A timer used for profiling.</param>
    protected virtual void OnInitialization(Timer? timer) {}

    /// <inheritdoc cref="OnLogicUpdate" />
    protected void DoLogicUpdate(Double delta, Timer? timer)
    {
        OnLogicUpdate(delta, timer);

        foreach (ApplicationComponent component in Components)
            component.OnLogicUpdate(delta, timer);
    }

    /// <summary>
    ///     Called for each fixed update step.
    /// </summary>
    /// <param name="delta">The time since the last update in seconds.</param>
    /// <param name="timer">A timer used for profiling.</param>
    protected virtual void OnLogicUpdate(Double delta, Timer? timer) {}

    /// <inheritdoc cref="OnRenderUpdate" />
    protected void DoRenderUpdate(Double delta, Timer? timer)
    {
        OnRenderUpdate(delta, timer);

        foreach (ApplicationComponent component in Components)
            component.OnRenderUpdate(delta, timer);
    }

    /// <summary>
    ///     Called for each render update step.
    ///     An application can potentially not render, which would mean that this is never called.
    /// </summary>
    /// <param name="delta">The time since the last render in seconds.</param>
    /// <param name="timer">A timer used for profiling.</param>
    protected virtual void OnRenderUpdate(Double delta, Timer? timer) {}

    /// <inheritdoc cref="OnDestroy" />
    protected void DoDestroy(Timer? timer)
    {
        OnDestroy(timer);

        foreach (ApplicationComponent component in Components)
            component.OnDestroy(timer);
    }

    /// <summary>
    ///     Called when the application is destroyed.
    /// </summary>
    /// <param name="timer">A timer used for profiling.</param>
    protected virtual void OnDestroy(Timer? timer) {}
}
