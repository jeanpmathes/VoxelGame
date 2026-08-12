// <copyright file="IInputActionProvider.cs" company="VoxelGame">
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

using VoxelGame.Client.Inputs.Actions;
using VoxelGame.Client.Inputs.Internals;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Client.Inputs;

/// <summary>
///     Provides <see cref="Action" />s for <see cref="Keybind{TAction}" />s.
/// </summary>
public interface IInputActionProvider
{
    /// <summary>
    ///     Create an action for a keybind definition.
    /// </summary>
    /// <typeparam name="TAction">The type of action associated with the keybind.</typeparam>
    /// <param name="definition">The definition for which an action is created.</param>
    /// <returns>The created action, which must be disposed of when its consumer stops using it.</returns>
    public TAction Use<TAction>(Keybind<TAction> definition) where TAction : Action, IConstructible<Binding, TAction>;
}
