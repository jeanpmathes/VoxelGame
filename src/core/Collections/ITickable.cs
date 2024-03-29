﻿// <copyright file="ITickable.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using VoxelGame.Core.Logic;
using VoxelGame.Core.Serialization;

namespace VoxelGame.Core.Collections;

/// <summary>
///     An object that can receive ticks.
/// </summary>
public interface ITickable : IValue
{
    /// <summary>
    ///     Send a tick to the object.
    /// </summary>
    /// <param name="world">The world in which the tick occurs.</param>
    void Tick(World world);
}
