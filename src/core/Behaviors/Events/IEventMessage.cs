// <copyright file="IEvent.cs" company="VoxelGame">
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

using VoxelGame.Core.Collections;

namespace VoxelGame.Core.Behaviors.Events;

/// <summary>
///     Core interface for event messages, with pooling support.
/// </summary>
public interface IEventMessage<TSelf> where TSelf : class, IEventMessage<TSelf>, new()
{
    /// <summary>
    ///     A simple pool for this event message type.
    /// </summary>
    static SimpleObjectPool<TSelf> Pool { get; } = new();
}
