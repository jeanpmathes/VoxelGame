// <copyright file="IUpdateableProcess.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;

namespace VoxelGame.Core.Updates;

/// <summary>
///     A process that can be updated and will complete at some point.
///     Should be used in combination with <see cref="UpdateDispatch" />.
/// </summary>
public interface IUpdateableProcess
{
    /// <summary>
    ///     Whether the process is currently running.
    ///     If not, it will no longer be updated by the <see cref="UpdateDispatch" />.
    /// </summary>
    Boolean IsRunning { get; }

    /// <summary>
    ///     Is called by <see cref="UpdateDispatch" /> to update the process.
    /// </summary>
    void Update();

    /// <summary>
    ///     Attempt to cancel the process.
    ///     Canceled process can either ignore the cancellation, or stop to enter a failed state.
    /// </summary>
    void Cancel();
}
