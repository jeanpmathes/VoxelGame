// <copyright file="Status.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

namespace VoxelGame.Core.Updates;

/// <summary>
///     Represents the status of an operation or class.
/// </summary>
public enum Status
{
    /// <summary>
    ///     The operation is still working.
    /// </summary>
    Running,

    /// <summary>
    ///     The operation is done and was successful.
    /// </summary>
    Ok,

    /// <summary>
    ///     The operation is done and failed.
    /// </summary>
    Error
}
