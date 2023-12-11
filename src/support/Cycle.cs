// <copyright file="Cycle.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

namespace VoxelGame.Support;

/// <summary>
///     The cycles of a game loop.
/// </summary>
internal enum Cycle
{
    /// <summary>
    ///     The update cycle, in which logic is performed, updating render data.
    /// </summary>
    Update,

    /// <summary>
    ///     The render cycle, in which all render data is drawn to the screen.
    /// </summary>
    Render
}
