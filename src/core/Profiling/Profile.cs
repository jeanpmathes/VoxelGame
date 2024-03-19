// <copyright file="Profile.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System.Collections.Concurrent;
using System.Collections.Generic;
using VoxelGame.Core.Collections;
using VoxelGame.Core.Collections.Properties;
using VoxelGame.Core.Utilities.Units;

namespace VoxelGame.Core.Profiling;

/// <summary>
///     Storage for performance profiling measurements.
/// </summary>
public class Profile
{
    private readonly ConcurrentDictionary<string, TimingMeasurement> measurements = new();

    /// <summary>
    ///     Global instance to add reports to if no local instance is available.
    /// </summary>
    public static Profile? Instance { get; private set; }

    /// <summary>
    ///     Create a global instance if none exists.
    /// </summary>
    public static void CreateGlobalInstance()
    {
        if (Instance != null) return;

        Instance = new Profile();
    }

    /// <summary>
    ///     Generate a property-based report of the current performance.
    /// </summary>
    /// <returns>The report.</returns>
    public Property GenerateReport()
    {
        List<Property> content = [];

        foreach (TimingMeasurement measurement in measurements.Values)
            if (measurement.IsRoot)
                content.Add(measurement.GenerateReport());

        return new Group(nameof(Profile), content);
    }

    /// <summary>
    /// Prepare a timing measurement for later completion.
    /// This can be called multiple times with the same key, but only the first call will have an effect.
    /// </summary>
    /// <param name="key">The unique key of the measurement.</param>
    /// <param name="parent">The unique key of the parent measurement, if any.</param>
    /// <param name="style">The style of the measurement.</param>
    public void PrepareTimingMeasurement(string key, string? parent, TimingStyle style)
    {
        bool added = measurements.TryAdd(key, new TimingMeasurement(key, style, parent == null));

        if (added && parent != null)
            measurements[parent].AddChild(measurements[key]);
    }

    /// <summary>
    ///     Finish a timing measurement and add the duration to the measurement.
    ///     The measurement must have been prepared before.
    /// </summary>
    /// <param name="key">The unique key of the measurement.</param>
    /// <param name="duration">The duration of the measurement.</param>
    public void FinishTimingMeasurement(string key, double duration)
    {
        measurements[key].AddTiming(duration);
    }

    private sealed class TimingMeasurement(string name, TimingStyle style, bool isRoot)
    {
        private readonly List<TimingMeasurement> children = [];

        private readonly object timingLock = new();
        private readonly object childrenLock = new();
        private CircularTimeBuffer? reoccurring;
        private double once;

        public bool IsRoot => isRoot;

        public void AddTiming(double duration)
        {
            lock (timingLock)
            {
                if (style == TimingStyle.Reoccurring && reoccurring == null)
                    reoccurring = new CircularTimeBuffer(capacity: 200);

                if (reoccurring != null)
                    reoccurring.Write(duration);
                else once = duration;
            }
        }

        public void AddChild(TimingMeasurement child)
        {
            lock (childrenLock)
            {
                children.Add(child);
            }
        }

        public Property GenerateReport()
        {
            List<Property> content = [];

            lock (timingLock)
            {
                if (reoccurring != null)
                {
                    content.Add(new Measure("Average", new Duration {Milliseconds = reoccurring.Average}));
                    content.Add(new Measure("Minimum", new Duration {Milliseconds = reoccurring.Min}));
                    content.Add(new Measure("Maximum", new Duration {Milliseconds = reoccurring.Max}));
                }
                else
                {
                    content.Add(new Measure("Time", new Duration {Milliseconds = once}));
                }
            }

            lock (childrenLock)
            {
                foreach (TimingMeasurement child in children)
                    content.Add(child.GenerateReport());
            }

            return new Group(name, content);
        }
    }
}
