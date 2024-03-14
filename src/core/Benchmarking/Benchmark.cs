// <copyright file="Benchmark.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System.Collections.Concurrent;
using System.Collections.Generic;
using VoxelGame.Core.Collections;
using VoxelGame.Core.Collections.Properties;

namespace VoxelGame.Core.Benchmarking;

/// <summary>
///     Storage for performance reports.
/// </summary>
public class Benchmark
{
    private readonly ConcurrentDictionary<string, Measurement> measurements = new();

    /// <summary>
    ///     Global instance to add reports to if no local instance is available.
    /// </summary>
    public static Benchmark? Instance { get; private set; }

    /// <summary>
    ///     Create a global instance if none exists.
    /// </summary>
    public static void CreateGlobalInstance()
    {
        if (Instance != null) return;

        Instance = new Benchmark();
    }

    /// <summary>
    ///     Generate a property-based report of the current performance.
    /// </summary>
    /// <returns>The report.</returns>
    public Property GenerateReport()
    {
        List<Message> timings = [];

        foreach ((string key, Measurement measurement) in measurements) timings.Add(new Message(key, measurement.ToString()));

        return new Group(nameof(Benchmark), timings);
    }

    /// <summary>
    ///     Add a new measurement of a duration, i.e. the time it took to execute a block of code.
    /// </summary>
    /// <param name="key">The unique key of the operation.</param>
    /// <param name="duration">The duration of the operation, in milliseconds.</param>
    public void AddDurationMeasurement(string key, double duration)
    {
        measurements.GetOrAdd(key, _ => new Measurement()).Add(duration);
    }

    private sealed class Measurement
    {
        private readonly CircularTimeBuffer buffer = new(capacity: 200);

        public void Add(double duration)
        {
            buffer.Write(duration);
        }

        public override string ToString()
        {
            return $"r.avg.: {buffer.Average:F3}ms r.max: {buffer.Max:F3}ms r.min: {buffer.Min:F3}ms";
        }
    }
}
