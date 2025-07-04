// <copyright file="IUpdateable.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Core.Logic;
using VoxelGame.Core.Serialization;

namespace VoxelGame.Core.Collections;

/// <summary>
///     A world object that can receive updates, used by the <see cref="ScheduledUpdateManager{T,TMaxScheduledUpdatesPerLogicUpdate}"/>.
/// </summary>
public interface IUpdateable : IValue
{
    /// <summary>
    ///     Send an update to the object.
    /// </summary>
    /// <param name="world">The world in which the update occurs.</param>
    void Update(World world);
}
