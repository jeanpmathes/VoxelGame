// <copyright file="ProfilerConfiguration.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

namespace VoxelGame.Core.Profiling;

/// <summary>
///     How the profiler should be configured.
/// </summary>
public enum ProfilerConfiguration
{
    /// <summary>
    ///     No profiling is done.
    ///     Has negligible performance impact.
    /// </summary>
    Disabled,

    /// <summary>
    ///     Basic profiling is done.
    ///     Can have a noticeable performance impact, but the application remains usable.
    /// </summary>
    Basic,

    /// <summary>
    ///     Full profiling, including lifetime tracking.
    ///     The results are summarized in a report file when the application is closed.
    ///     Will have a significant performance impact.
    /// </summary>
    Full
}
