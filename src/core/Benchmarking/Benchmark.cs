// <copyright file="Benchmark.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using VoxelGame.Core.Collections;
using VoxelGame.Core.Collections.Properties;
using VoxelGame.Core.Utilities.Units;

namespace VoxelGame.Core.Benchmarking;

/// <summary>
///     Storage for performance reports.
/// </summary>
public class Benchmark
{
    private readonly ConcurrentDictionary<string, TimingMeasurement> measurements = new();

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
        List<Property> grouped = [null!];
        List<Property> ungrouped = [];

        foreach ((string key, TimingMeasurement measurement) in measurements)
            measurement.GenerateReport(key, grouped, ungrouped);

        grouped[index: 0] = new Group("Ungrouped", ungrouped);

        return new Group(nameof(Benchmark), grouped);
    }

    /// <summary>
    ///     Add a new measurement of a duration, i.e. the time it took to execute a block of code.
    /// </summary>
    /// <param name="key">The unique key of the operation.</param>
    /// <param name="duration">The duration of the operation, in milliseconds.</param>
    /// <param name="style">The style of the measurement.</param>
    public void AddDurationMeasurement(string key, double duration, TimingStyle style)
    {
        measurements.GetOrAdd(key, _ => new TimingMeasurement()).Add(duration, style);
    }

    private sealed class TimingMeasurement
    {
        private CircularTimeBuffer? reoccurring;
        private double once;

        public void Add(double duration, TimingStyle style)
        {
            switch (style)
            {
                case TimingStyle.Reoccurring:
                    reoccurring ??= new CircularTimeBuffer(capacity: 200);
                    reoccurring.Write(duration);

                    break;
                case TimingStyle.Once:
                    once = duration;
                    reoccurring = null;

                    break;
                default:
                    throw new InvalidEnumArgumentException(nameof(style), (int) style, typeof(TimingStyle));
            }
        }

        public void GenerateReport(string name, ICollection<Property> grouped, ICollection<Property> ungrouped)
        {
            if (reoccurring != null)
                grouped.Add(new Group(name,
                [
                    new Measure("Average", new Duration {Milliseconds = reoccurring.Average}),
                    new Measure("Minimum", new Duration {Milliseconds = reoccurring.Min}),
                    new Measure("Maximum", new Duration {Milliseconds = reoccurring.Max})
                ]));
            else ungrouped.Add(new Measure(name, new Duration {Milliseconds = once}));
        }
    }
}
