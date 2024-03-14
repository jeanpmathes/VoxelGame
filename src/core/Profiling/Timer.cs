// <copyright file="Timer.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace VoxelGame.Core.Profiling;

/// <summary>
///     Measures the time it takes to execute a block of code.
/// </summary>
public sealed class Timer(string name, TimingStyle style, Profile? profile, IDisposable? disposable) : IDisposable
{
    private readonly Stopwatch stopwatch = new();

    /// <summary>
    ///     Gets the style of the measurement.
    /// </summary>
    public TimingStyle Style => style;

    private string Name => name;

    /// <inheritdoc />
    public void Dispose()
    {
        if (profile != null)
        {
            stopwatch.Stop();
            profile.FinishTimingMeasurement(name, stopwatch.Elapsed.TotalMilliseconds);
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
    ///     If an disposable object is passed, an (inactive) timer will be returned, even if no profiler is available.
    /// </returns>
    public static Timer? Start(string name, TimingStyle style = TimingStyle.Reoccurring, Profile? profiler = null, Timer? containing = null, IDisposable? other = null)
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
    ///     Start a sub-timer and measure the time it takes to execute the sub-block.
    ///     The sub-timer will be associated with the same profiler as the parent timer and use the same style.
    /// </summary>
    /// <param name="sub">The name of the sub-timer.</param>
    /// <param name="other">Another disposable object to dispose when the timer is disposed.</param>
    /// <returns>An active timer, or null if no profiling is running.</returns>
    public Timer? StartSub(string sub, IDisposable? other = null)
    {
        return Start(sub, style, profile, this, other);
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
    public static Timer? BeginTimedScoped(this ILogger logger, string name, TimingStyle style = TimingStyle.Reoccurring)
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
    public static Timer? BeginTimedSubScoped(this ILogger logger, string name, Timer containing)
    {
        return Timer.Start(name, containing.Style, Profile.Instance, other: logger.BeginScope(name));
    }
}
