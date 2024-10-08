// <copyright file="IUpdate.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;

namespace VoxelGame.Core.Updates;

/// <summary>
///     A process that can be updated and will complete at some point.
/// </summary>
public interface IUpdate
{
    /// <summary>
    ///     Whether the process is still running.
    ///     If not, it will no longer be updated.
    /// </summary>
    public Boolean IsRunning { get; }

    /// <summary>
    ///     Update the process.
    /// </summary>
    public void Update();
}
