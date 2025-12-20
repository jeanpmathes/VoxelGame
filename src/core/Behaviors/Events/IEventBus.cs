// <copyright file="IEventBus.cs" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2025 Jean Patrick Mathes
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
///     An event bus allows subscribing to events.
/// </summary>
public interface IEventBus
{
    /// <summary>
    ///     Subscribe to an event with a handler.
    /// </summary>
    /// <param name="handler">The event handler.</param>
    /// <typeparam name="TEventMessage">The type of event message to subscribe to.</typeparam>
    void Subscribe<TEventMessage>(Action<TEventMessage> handler);
}
