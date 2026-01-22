// <copyright file="IEventRegistry.cs" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2026 Jean Patrick Mathes
//      
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
//     
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//     
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <https://www.gnu.org/licenses/>.
// </copyright>
// <author>jeanpmathes</author>

using System;

namespace VoxelGame.Core.Behaviors.Events;

/// <summary>
///     An event registry allows defining events which one intends to publish to.
/// </summary>
public interface IEventRegistry
{
    /// <summary>
    ///     Register an event with the registry.
    /// </summary>
    /// <param name="single">Whether there can only be one subscriber to this event.</param>
    /// <typeparam name="TEventMessage">The type of the event message.</typeparam>
    /// <returns>The registered event.</returns>
    IEvent<TEventMessage> RegisterEvent<TEventMessage>(Boolean single);

    /// <summary>
    ///     Register an event with the registry, allowing multiple subscribers.
    /// </summary>
    /// <typeparam name="TEventMessage">The type of the event message.</typeparam>
    /// <returns>The registered event.</returns>
    IEvent<TEventMessage> RegisterEvent<TEventMessage>()
    {
        return RegisterEvent<TEventMessage>(single: false);
    }
}
