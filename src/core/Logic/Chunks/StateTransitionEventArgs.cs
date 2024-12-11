// <copyright file="StateTransition.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;

namespace VoxelGame.Core.Logic.Chunks;

/// <summary>
///     Arguments for a state transition event.
/// </summary>
public class StateTransitionEventArgs : EventArgs
{
    /// <summary>
    ///     The old state of the chunk.
    /// </summary>
    public required ChunkState? OldState { get; init; }

    /// <summary>
    ///     The new state of the chunk.
    /// </summary>
    public required ChunkState? NewState { get; init; }
}
