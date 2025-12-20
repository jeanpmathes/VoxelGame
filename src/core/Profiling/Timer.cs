// <copyright file="Timer.cs" company="VoxelGame">
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
using Microsoft.Extensions.Logging;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Utilities.Units;

namespace VoxelGame.Core.Profiling;

/// <summary>
///     Measures the time it takes to execute a block of code.
/// </summary>
public sealed class Timer : IDisposable
{
    private readonly Stopwatch stopwatch = new();

    private IDisposable? disposable;

    private Timer(String name, TimingStyle style, Profile? profile, IDisposable? disposable)
    {
        Name = name;
        Style = style;
        Profile = profile;

        this.disposable = disposable;
    }

    private String Name { get; }

    private TimingStyle Style { get; }

    private Profile? Profile { get; }

    /// <summary>
    ///     Gets the elapsed time.
    /// </summary>
    public Duration Elapsed => new() {Milliseconds = stopwatch.Elapsed.TotalMilliseconds};

    /// <inheritdoc />
    public void Dispose()
    {
        if (Profile != null)
        {
            stopwatch.Stop();
            Profile.FinishTimingMeasurement(Name, stopwatch.Elapsed.TotalMilliseconds);
        }

        disposable?.Dispose();
    }

    /// <summary>
    ///     Creates a new instance of the <see cref="Timer" /> class.
    /// </summary>
    /// <param name="name">The name of the measurement. Must be unique for this call in the source code.</param>
    /// <param name="style">How the timing is measured and evaluated.</param>
    /// <param name="profiler">
    ///     The profiler the measurement is associated with.
    ///     If no profiler is provided, the global profiler is used.
    /// </param>
    /// <param name="containing">The containing timer, if any.</param>
    /// <param name="other">Another disposable object to dispose when the timer is disposed.</param>
    /// <returns>
    ///     An active timer, or null if both the passed profiler and the global benchmark are null.
    ///     If a disposable object is passed, an (inactive) timer will be returned, even if no profiler is available.
    /// </returns>
    public static Timer? Start(String name, TimingStyle style = TimingStyle.Reoccurring, Profile? profiler = null, Timer? containing = null, IDisposable? other = null)
    {
        profiler ??= Profile.Instance;

        if (profiler == null)
            return other != null ? new Timer(name, style, profiler, other) : null;

        Timer timer = new(name, style, profiler, other);

        profiler.PrepareTimingMeasurement(name, containing?.Name, style);
        timer.stopwatch.Start();

        return timer;
    }

    /// <summary>
    ///     Start a timer that is not associated with any profiler.
    /// </summary>
    /// <param name="onCompletion">The action to execute when the timer is disposed.</param>
    /// <returns>The timer.</returns>
    public static Timer Start(Action<Duration> onCompletion)
    {
        Timer timer = new("", TimingStyle.Once, profile: null, disposable: null);

        timer.disposable = new Disposer(() => onCompletion(timer.Elapsed));
        timer.stopwatch.Start();

        return timer;
    }

    /// <summary>
    ///     Start a sub-timer and measure the time it takes to execute the sub-block.
    ///     The sub-timer will be associated with the same profiler as the parent timer and use the same style.
    /// </summary>
    /// <param name="sub">The name of the sub-timer.</param>
    /// <param name="other">Another disposable object to dispose when the timer is disposed.</param>
    /// <returns>An active timer, or null if no profiling is running.</returns>
    public Timer? StartSub(String sub, IDisposable? other = null)
    {
        return Start(sub, Style, Profile, this, other);
    }
}

/// <summary>
///     Extension methods for the <see cref="Timer" /> class.
/// </summary>
public static class TimerExtensions
{
    /// <summary>
    ///     Begin a new logging scope and measure the time it takes to execute the block.
    /// </summary>
    /// <param name="logger">The logger to use.</param>
    /// <param name="name">The name of the scope.</param>
    /// <param name="style">How the timing is measured and evaluated.</param>
    /// <returns>The timer, or null if no profiling is running.</returns>
    public static Timer? BeginTimedScoped(this ILogger logger, String name, TimingStyle style = TimingStyle.Reoccurring)
    {
        return Timer.Start(name, style, Profile.Instance, other: logger.BeginScope(name));
    }

    /// <summary>
    ///     Begin a new logging scope and measure the time it takes to execute the sub-block.
    /// </summary>
    /// <param name="logger">The logger to use.</param>
    /// <param name="name">The name of the sub-scope.</param>
    /// <param name="containing">The parent timer containing this sub-scope.</param>
    /// <returns>The timer, or null if no profiling is running.</returns>
    public static Timer? BeginTimedSubScoped(this ILogger logger, String name, Timer? containing)
    {
        IDisposable? scope = logger.BeginScope(name);

        return containing?.StartSub(name, scope) ?? Timer.Start(name, other: scope);
    }
}
