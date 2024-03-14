// <copyright file="Timer.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace VoxelGame.Core.Benchmarking;

/// <summary>
///     Measures the time it takes to execute a block of code.
/// </summary>
public sealed class Timer(string name, Benchmark benchmark, IDisposable? disposable) : IDisposable
{
    private readonly Stopwatch stopwatch = new();

    /// <inheritdoc />
    public void Dispose()
    {
        stopwatch.Stop();
        disposable?.Dispose();

        benchmark.AddDurationMeasurement(name, stopwatch.Elapsed.TotalMilliseconds);
    }

    /// <summary>
    ///     Creates a new instance of the <see cref="Timer" /> class.
    /// </summary>
    /// <param name="name">The name of the measurement. Must be unique for this call in the source code.</param>
    /// <param name="benchmark">
    ///     The benchmark the measurement is associated with.
    ///     If no benchmark is provided, the global benchmark is used.
    /// </param>
    /// <param name="other">Another disposable object to dispose when the timer is disposed.</param>
    /// <returns>An active timer, or null if both the passed benchmark and the global benchmark are null.</returns>
    public static Timer? Start(string name, Benchmark? benchmark = null, IDisposable? other = null)
    {
        benchmark ??= Benchmark.Instance;

        if (benchmark == null)
            return null;

        Timer timer = new(name, benchmark, other);

        timer.stopwatch.Start();

        return timer;
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
    /// <returns>An object to dispose to end the scope and the measurement.</returns>
    public static IDisposable? BeginTimedScoped(this ILogger logger, string name)
    {
        return Timer.Start(name, Benchmark.Instance, logger.BeginScope(name));
    }
}
