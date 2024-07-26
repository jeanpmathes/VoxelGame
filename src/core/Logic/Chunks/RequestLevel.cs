// <copyright file="RequestLevel.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

namespace VoxelGame.Core.Logic.Chunks;

/// <summary>
///     Indicates the level at which a chunk is requested.
/// </summary>
public enum RequestLevel
{
    /// <summary>
    ///     The chunk is not requested.
    ///     This is not a valid level for a request.
    /// </summary>
    None,

    /// <summary>
    ///     The chunk is requested to be loaded (or generated).
    ///     Neighbors of higher request levels will have this level.
    ///     This is not a valid level for a request.
    /// </summary>
    Loaded,

    /// <summary>
    ///     The chunk is requested to be active.
    ///     Neighbors of this level will have at least the level <see cref="Loaded" />.
    /// </summary>
    Active
}
