// <copyright file="Application.cs" company="VoxelGame">
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
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using VoxelGame.Annotations.Attributes;
using VoxelGame.Toolkit.Components;
using VoxelGame.Toolkit.Utilities;
using Timer = VoxelGame.Core.Profiling.Timer;

namespace VoxelGame.Core.App;

/// <summary>
///     Represents the running application.
///     There will always be exactly one instance of this class.
/// </summary>
[ComponentSubject(typeof(ApplicationComponent))]
public abstract partial class Application : Composed<Application, ApplicationComponent>
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
        OnInitializationComponents(timer);
    }

    /// <inheritdoc cref="OnInitialization" />
    [ComponentEvent(nameof(ApplicationComponent.OnInitialization))]
    private partial void OnInitializationComponents(Timer? timer);

    /// <summary>
    ///     Called on initialization of the application.
    /// </summary>
    /// <param name="timer">A timer used for profiling.</param>
    protected virtual void OnInitialization(Timer? timer) {}

    /// <inheritdoc cref="OnLogicUpdate" />
    protected void DoLogicUpdate(Double delta, Timer? timer)
    {
        OnLogicUpdate(delta, timer);
        OnLogicUpdateComponents(delta, timer);
    }

    /// <inheritdoc cref="OnLogicUpdate" />
    [ComponentEvent(nameof(ApplicationComponent.OnLogicUpdate))]
    private partial void OnLogicUpdateComponents(Double delta, Timer? timer);

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
        OnRenderUpdateComponents(delta, timer);
    }

    /// <inheritdoc cref="OnRenderUpdate" />
    [ComponentEvent(nameof(ApplicationComponent.OnRenderUpdate))]
    private partial void OnRenderUpdateComponents(Double delta, Timer? timer);

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
        OnDestroyComponents(timer);
    }

    [ComponentEvent(nameof(ApplicationComponent.OnDestroy))]
    private partial void OnDestroyComponents(Timer? timer);

    /// <summary>
    ///     Called when the application is destroyed.
    /// </summary>
    /// <param name="timer">A timer used for profiling.</param>
    protected virtual void OnDestroy(Timer? timer) {}
}
