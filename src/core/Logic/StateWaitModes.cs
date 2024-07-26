// <copyright file="StateWaitModes.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using VoxelGame.Core.Updates;

namespace VoxelGame.Core.Logic;

/// <summary>
/// </summary>
[Flags]
public enum StateWaitModes
{
    /// <summary>
    ///     The state is not waiting for anything.
    ///     It will receive updates as normal.
    /// </summary>
    None = 0,

    /// <summary>
    ///     The state is waiting for the completion of an action, wrapped in a  <see cref="Future" />.
    /// </summary>
    WaitForCompletion = 1 << 0,

    /// <summary>
    ///     The state is waiting for any neighbors to become usable.
    /// </summary>
    WaitForNeighborUsability = 1 << 1,

    /// <summary>
    ///     The state is waiting for a transition request to be made.
    /// </summary>
    WaitForRequest = 1 << 2,

    /// <summary>
    ///     The state is waiting for resources of the chunk to be released
    ///     that is currently being used by another state or operation.
    ///     This mode is used internally to wait for the resources this state needs to acquire.
    /// </summary>
    WaitForResource = 1 << 3
}
