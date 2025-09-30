// <copyright file="WorldComponent.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using VoxelGame.Core.Profiling;
using VoxelGame.Toolkit.Components;

namespace VoxelGame.Core.Logic;

/// <summary>
///     Base class for all components used in the <see cref="World" /> class.
/// </summary>
public class WorldComponent(World subject) : Component<World>(subject)
{
    /// <summary>
    ///     Called when the world becomes active.
    /// </summary>
    public virtual void OnActivate() {}

    /// <summary>
    ///     Called when the world becomes inactive.
    /// </summary>
    public virtual void OnDeactivate() {}

    /// <summary>
    ///     Called when the world is terminated, which means it begins unloading.
    ///     Disposal will happen later, when unloading is complete.
    /// </summary>
    public virtual void OnTerminate() {}

    /// <summary>
    ///     Called during <see cref="World.OnLogicUpdateInActiveState" />.
    /// </summary>
    public virtual void OnLogicUpdateInActiveState(Double deltaTime, Timer? timer) {}
}
